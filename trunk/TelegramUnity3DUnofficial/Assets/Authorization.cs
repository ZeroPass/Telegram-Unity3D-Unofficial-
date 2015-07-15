using UnityEngine;
using System.Collections;
using System;

public class Authorization 
{
	private const long pAppId = 41190;
	private Math.BigInteger ClientNonce;
	public bool StartDHExchange()
	{
		byte[] pAppIdData = BitConverter.GetBytes (pAppId);

		// #1
		byte[] pAuthRequest = new byte[40];
		Helper.SetData(ref pAuthRequest, pAppIdData, 0); // auth_key_id
		long pUnixStamp = Helper.TimeNowUnix ();
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes(pUnixStamp), 8); // message_id
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes((int)20), 16); // message length
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes((int)60469778), 20); // message length
		byte[] pClientNonce = Helper.RandomNum (128).getBytes ();
		Helper.SetData(ref pAuthRequest, pClientNonce, 24); // nonce
		// send

		// #2
		byte[] pResponse = new byte[84];
		long pAuthKeyId = BitConverter.ToInt64 (pResponse, 0);
		long pMessageId = BitConverter.ToInt64 (pResponse, 8);
		int pMessageLen = BitConverter.ToInt32 (pResponse, 16);
		int pResPQ = BitConverter.ToInt32 (pResponse, 20);
		Math.BigInteger pNonce = new Math.BigInteger(Helper.GetData (pResponse, 24, 16));
		Math.BigInteger pServerNonce = new Math.BigInteger(Helper.GetData (pResponse, 40, 16));
		Math.BigInteger pPq = new Math.BigInteger(Helper.GetData (pResponse, 57, 8));
		int pVector = BitConverter.ToInt32 (pResponse, 68);
		int pCount = BitConverter.ToInt32 (pResponse, 72);
		long[] pFingerprints = new long[pCount];
		for(short x = 0; x < pCount; x++)
		{
			pFingerprints[x] = BitConverter.ToInt64(pResponse, 76 + x * 8);
		}

		// #3 pq decomposed to primes
		Helper.PQPair pPqDecomposed = Helper.DecomposeToPrimeFactors (pPq);

		// #4 client presents proof to server
		byte[] pClientProofPlain = new byte[96];
		Helper.SetData(ref pClientProofPlain, BitConverter.GetBytes(0x83c95aec), 0);
		Helper.SetData (ref pClientProofPlain, pPq.getBytes(), 5); // 1 denoting byte, 3 padding at the end = length 12 (1 + 8 + 3)
		Helper.SetData (ref pClientProofPlain, pPqDecomposed.P.getBytes(), 17); // 1 + 4 (p) + 3
		Helper.SetData (ref pClientProofPlain, pPqDecomposed.Q.getBytes(), 25); // 1 + 4 (q) + 3
		Helper.SetData (ref pClientProofPlain, pClientNonce, 32);
		Helper.SetData (ref pClientProofPlain, pServerNonce.getBytes(), 48);
		byte[] pClientProofNonce = Helper.RandomNum (256).getBytes (); // new nonce
		Helper.SetData (ref pClientProofPlain, pClientProofNonce, 64);
		byte[] pDataSha1 = Helper.GetSha1 (pClientProofPlain);
		byte[] pClientProofEncrypted = new byte[255];
		Helper.SetData (ref pClientProofEncrypted, pDataSha1, 0); // length 64
		Helper.SetData (ref pClientProofEncrypted, pClientProofPlain, 64);
		pClientProofEncrypted = Helper.GetEncryptedFromPublicRSA (pClientProofEncrypted, BitConverter.GetBytes(pFingerprints [0])); // length 256

		// Request to Start Diffie-Hellman Key Exchange
		byte[] pHdExchange = new byte[340];
		Helper.SetData(ref pHdExchange, pAppIdData, 0); // auth_key_id
		pUnixStamp = Helper.TimeNowUnix ();
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(pUnixStamp), 8); // message_id
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(pHdExchange.Length), 16); // message_length
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(0xd712e4be), 20); // req_DH_params
		Helper.SetData (ref pHdExchange, pClientNonce, 24);
		Helper.SetData (ref pHdExchange, pServerNonce.getBytes(), 40);
		Helper.SetData (ref pHdExchange, BitConverter.GetBytes (pPqDecomposed.P.IntValue()), 56); // 1 + 4 (p) + 3
		Helper.SetData (ref pHdExchange, BitConverter.GetBytes (pPqDecomposed.Q.IntValue()), 64); // 1 + 4 (q) + 3
		Helper.SetData (ref pHdExchange, BitConverter.GetBytes(pFingerprints[0]), 72);
		Helper.SetData (ref pHdExchange, pClientProofEncrypted, 80);

		// #5 server response (DH OK - d0e8075c, DH FAILED - 79cb045d)
		byte[] pServerProofResponse = new byte[652];
		int pResponseState = BitConverter.ToInt32(pServerProofResponse, 20);

		if(pResponseState == 0x79cb045d) // failed
		{
			return false;
		}

		long pAuthKeyIdS1 = BitConverter.ToInt64 (pServerProofResponse, 0);
		long pMessageIdS1 = BitConverter.ToInt64 (pServerProofResponse, 8);
		pMessageLen = BitConverter.ToInt32 (pServerProofResponse, 16);
		byte[] pClientNonceCheck = Helper.GetData (pServerProofResponse, 24, 16);
		byte[] pServerNonceCheck = Helper.GetData (pServerProofResponse, 40, 16);
		byte[] pEncryptedAnswer = Helper.GetData(pServerProofResponse, 56, 592);

		byte[] newNonceHash = Helper.GetData(Helper.GetSha1 (pClientProofNonce), 0, 16);

		return true;
	}
}
