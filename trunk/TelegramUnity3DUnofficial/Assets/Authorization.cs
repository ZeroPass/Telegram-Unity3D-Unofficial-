using System.Collections;
using System;

public class Authorization 
{
	private TTCPClient pTcpClient = null;
	private const long pAppId = 41190;
	private Math.BigInteger ClientNonce;
	private Math.BigInteger pAuthorizationKey = -1;
	public byte[] AuthorizationKey
	{
		get
		{
			return pAuthorizationKey.getBytes();
		}
	}
	public Authorization(TTCPClient pClient)
	{
		this.pTcpClient = pClient;
		this.StartDHExchange ();
	}
    private void CheckNonce(Math.BigInteger pNum0, Math.BigInteger pNum1)
    {
        if(pNum0 != pNum1)
        {
            throw new Exception("Invalid nonce");
        }
    }
    public void CheckNonce(byte[] pNum0, byte[] pNum1)
    {
        if(!Helper.DataEquals(pNum0, pNum1))
        {
            throw new Exception("Invalid nonce");
        }
    }
	public bool StartDHExchange()
	{
        byte[] pAppIdData = BitConverter.GetBytes((long)0);

		// #1
		byte[] pAuthRequest = new byte[40];
		Helper.SetData(ref pAuthRequest, pAppIdData, 0); // auth_key_id
		long pUnixStamp = Helper.TimeNowUnix ();
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes(pUnixStamp), 8); // message_id
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes((int)20), 16); // message length
		Helper.SetData(ref pAuthRequest, BitConverter.GetBytes((int)60469778), 20);
		byte[] pClientNonce = Helper.RandomNum (128).getBytes ();
        Math.BigInteger pClientNonceNum = new Math.BigInteger(pClientNonce);
		Helper.SetData(ref pAuthRequest, pClientNonce, 24); // nonce

		// #2
		byte[] pResponse = pTcpClient.Send (pAuthRequest);  // new byte[84];
        System.Threading.Thread.Sleep(1500);
        return true;

		long pAuthKeyId = BitConverter.ToInt64 (pResponse, 0);
		long pMessageId = BitConverter.ToInt64 (pResponse, 8);
		int pMessageLen = BitConverter.ToInt32 (pResponse, 16);
		int pResPQ = BitConverter.ToInt32 (pResponse, 20);
		Math.BigInteger pNonce = new Math.BigInteger(Helper.GetData (pResponse, 24, 16));
        CheckNonce(pClientNonceNum, pNonce);
        byte[] pServerNonceBytes = Helper.GetData(pResponse, 40, 16);
        Math.BigInteger pServerNonce = new Math.BigInteger(pServerNonceBytes);
		Math.BigInteger pPq = new Math.BigInteger(Helper.GetData (pResponse, 57, 8));
		int pVector = BitConverter.ToInt32 (pResponse, 68);
		int pCount = BitConverter.ToInt32 (pResponse, 72);
		ulong[] pFingerprints = new ulong[pCount];
		for(short x = 0; x < pCount; x++)
		{
			pFingerprints[x] = BitConverter.ToUInt64(pResponse, 76 + x * 8);
		}

		// #3 pq decomposed to primes
		Helper.PQPair pPqDecomposed = Helper.DecomposeToPrimeFactors (pPq);
		Math.BigInteger pBottomLimit = 2 ^ 2047;
		Math.BigInteger pTopLimit = 2 ^ 2048;
		if(!(pBottomLimit < pPqDecomposed.P && pPqDecomposed.P < pTopLimit) || !Helper.IsPrime(pPqDecomposed.P) || !Helper.IsPrime((pPqDecomposed.P - 1) / 2))
		{
			return false;
		}
		// #4 client presents proof to server
		byte[] pClientProofPlain = new byte[96];
		Helper.SetData(ref pClientProofPlain, BitConverter.GetBytes(0x83c95aec), 0);
		Helper.SetData(ref pClientProofPlain, pPq.getBytes(), 5); // 1 denoting byte, 3 padding at the end = length 12 (1 + 8 + 3)
		Helper.SetData(ref pClientProofPlain, pPqDecomposed.P.getBytes(), 17); // 1 + 4 (p) + 3
		Helper.SetData(ref pClientProofPlain, pPqDecomposed.Q.getBytes(), 25); // 1 + 4 (q) + 3
		Helper.SetData(ref pClientProofPlain, pClientNonce, 32);
		Helper.SetData(ref pClientProofPlain, pServerNonce.getBytes(), 48);
		byte[] pClientProofNewNonce = Helper.RandomNum (256).getBytes (); // new nonce new_nonce newnonce
		Helper.SetData(ref pClientProofPlain, pClientProofNewNonce, 64);
		byte[] pDataSha1 = Helper.GetSha1 (pClientProofPlain);
		byte[] pClientProofEncrypted = new byte[255];
		Helper.SetData(ref pClientProofEncrypted, pDataSha1, 0); // length 64
		Helper.SetData(ref pClientProofEncrypted, pClientProofPlain, 64);

        MTProto.ServerInfo.PublicRSAKey pServerPublicKey = MTProto.ServerInfo.PublicKeys.GetKeyByFingerprint(pFingerprints[0]);
        if(pServerPublicKey == null)
        {
            throw new Exception("Server public key not found");
        }
        pClientProofEncrypted = pServerPublicKey.Encrypt(pClientProofEncrypted); // length 256

		// Request to Start Diffie-Hellman Key Exchange
		byte[] pHdExchange = new byte[340];
		Helper.SetData(ref pHdExchange, pAppIdData, 0); // auth_key_id
		pUnixStamp = Helper.TimeNowUnix ();
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(pUnixStamp), 8); // message_id
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(pHdExchange.Length), 16); // message_length
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(0xd712e4be), 20); // req_DH_params
		Helper.SetData(ref pHdExchange, pClientNonce, 24);
		Helper.SetData(ref pHdExchange, pServerNonce.getBytes(), 40);
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes (pPqDecomposed.P.IntValue()), 56); // 1 + 4 (p) + 3
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes (pPqDecomposed.Q.IntValue()), 64); // 1 + 4 (q) + 3
		Helper.SetData(ref pHdExchange, BitConverter.GetBytes(pFingerprints[0]), 72);
		Helper.SetData(ref pHdExchange, pClientProofEncrypted, 80);

		// #5 server response (DH OK - d0e8075c, DH FAILED - 79cb045d)
		byte[] pServerProofResponse = pTcpClient.Send(pHdExchange); // new byte[652];
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
        CheckNonce(pClientNonce, pClientNonceCheck);
        CheckNonce(pServerNonce, new Math.BigInteger(pServerNonceCheck));
		byte[] pEncryptedAnswer = Helper.GetData(pServerProofResponse, 56, 596);

		// decrypt answer
		byte[] newNonceHash = Helper.GetData(Helper.GetSha1 (pClientProofNewNonce), 0, 16);
		byte[] tmpAesKey = new byte[1024];
		Helper.SetData(ref tmpAesKey, pClientProofNewNonce, 0);
		Helper.SetData(ref tmpAesKey, pServerNonce.getBytes(), pClientProofNewNonce.Length);
		Helper.SetData(ref tmpAesKey, pClientProofNewNonce, pClientProofNewNonce.Length + pServerNonce.dataLength);
		Helper.SetData(ref tmpAesKey, pClientProofNewNonce, pClientProofNewNonce.Length + pServerNonce.dataLength + pClientProofNewNonce.Length);
		byte[] pSha1ns = new byte[20];
		byte[] pSha1sn = new byte[20];
		byte[] pSha1nn = new byte[20];

		pSha1ns = Helper.GetSha1 (Helper.GetData (tmpAesKey, 0, pClientProofNewNonce.Length + pServerNonce.dataLength));
		pSha1sn = Helper.GetSha1 (Helper.GetData (tmpAesKey, pClientProofNewNonce.Length, pClientProofNewNonce.Length + pServerNonce.dataLength));
		pSha1nn = Helper.GetSha1 (Helper.GetData (tmpAesKey, pClientProofNewNonce.Length + pServerNonce.dataLength, pClientProofNewNonce.Length + pClientProofNewNonce.Length));

		byte[] pAesKey = new byte[32];
		byte[] pAesIV = new byte[32];

		Helper.SetData(ref pAesKey, pSha1ns, 0);
		Helper.SetData(ref pAesKey, Helper.GetData (pSha1sn, 0, 12), 20);

		Helper.SetData(ref pAesIV, Helper.GetData (pSha1sn, 12, 8), 0);
		Helper.SetData(ref pAesIV, pSha1nn, 8);
		Helper.SetData(ref pAesIV, Helper.GetData(pClientProofNewNonce, 0, 4), 28);

		byte[] pDecryptedAnswer = Helper.Aes256IgeDecrypt(pEncryptedAnswer, pAesKey, pAesIV);
		long pAuthKeyId2 = BitConverter.ToInt64 (pDecryptedAnswer, 0);
		byte[] pOriginalClientNonce = Helper.GetData(pDecryptedAnswer, 4, 16);
		byte[] pOriginalServerNonce = Helper.GetData (pDecryptedAnswer, 20, 16);
		int pG = BitConverter.ToInt32(pDecryptedAnswer, 36);
		Math.BigInteger pDhPrime = new Math.BigInteger(Helper.GetData(pDecryptedAnswer, 40, 260));
		Math.BigInteger pGa = new Math.BigInteger(Helper.GetData(pDecryptedAnswer, 300, 260));
		long pServerTime = BitConverter.ToInt32 (pDecryptedAnswer, 560);
		bool pGCyclicSubgroup = (pPqDecomposed.P % 8 == 7 && pG == 2 || pPqDecomposed.P % 3 == 2 && pG == 3 || 
		                        (pPqDecomposed.P % 5 == 1 || pPqDecomposed.P % 5 == 4) && pG == 5 ||
		                        (pPqDecomposed.P % 24 == 19 || pPqDecomposed.P % 24 == 23 ) && pG == 6 ||
		                         (pPqDecomposed.P % 7 == 3 || pPqDecomposed.P % 7 == 5 || pPqDecomposed.P % 7 == 6) && pG == 7);
	
		if(pDhPrime != pPqDecomposed.P || !pGCyclicSubgroup)
		{
			return false;
		}

		// #6 Random number b is computed
		Math.BigInteger pB = new Math.BigInteger(Helper.RandomData (256));
		Math.BigInteger pGb = (pG ^ pB) % pDhPrime;
		Math.BigInteger pMinusOnePrime = pDhPrime - 1;
		Math.BigInteger pBottomPrimeLimit = 2 ^ (2048 - 64);
		Math.BigInteger pTopPrimeLimit = pDhPrime - pBottomPrimeLimit;
		if(!(pG > 1 && pGa > 1 && pGb > 1 && pG < pMinusOnePrime && pGa < pMinusOnePrime && pGb < pMinusOnePrime &&
		     (pGa > pBottomLimit && pGa < pTopPrimeLimit || pGa < pBottomLimit && pGa > pTopPrimeLimit) ||
		     (pGb > pBottomLimit && pGb < pTopPrimeLimit || pGb < pBottomLimit && pGb > pTopPrimeLimit)))
		{
			return false;
		}
		byte[] pClientEncrypedData = new byte[336];
		Helper.SetData(ref pClientEncrypedData, BitConverter.GetBytes ((int)0x6643b654), 0);
		Helper.SetData(ref pClientEncrypedData, pClientNonce, 4);
		Helper.SetData(ref pClientEncrypedData, BitConverter.GetBytes(0L), 36);
		Helper.SetData(ref pClientEncrypedData, pGb.getBytes(), 44);

		int pDataWithHashLen = pClientEncrypedData.Length + 20;
		pDataWithHashLen += pClientEncrypedData.Length % 16; // must be divisible by 16
		byte[] pDataWithHash = new byte[pDataWithHashLen];
		Helper.SetData(ref pDataWithHash, Helper.GetSha1 (pClientEncrypedData), 0);
		Helper.SetData(ref pDataWithHash, pClientEncrypedData, 20);
		pClientEncrypedData = Helper.Aes256IgeEncrypt(pDataWithHash, pAesKey, pAesIV);

		// request
		byte[] pEncryptedRequest = new byte[396];
		Helper.SetData(ref pEncryptedRequest, pAppIdData, 0);
		Helper.SetData(ref pEncryptedRequest, BitConverter.GetBytes (Helper.TimeNowUnix ()), 8);
		Helper.SetData(ref pEncryptedRequest, BitConverter.GetBytes(pEncryptedRequest.Length), 16);
		Helper.SetData(ref pEncryptedRequest, BitConverter.GetBytes((uint)0xf5045f1f), 20);
		Helper.SetData(ref pEncryptedRequest, pOriginalClientNonce, 24);
		Helper.SetData(ref pEncryptedRequest, pOriginalServerNonce, 40);
		Helper.SetData(ref pEncryptedRequest, pClientEncrypedData, 56);

		// #7 Computing auth_key using formula
		Math.BigInteger pAuthKey = (pGa ^ pB) % pDhPrime;
		this.pAuthorizationKey = pAuthKey;
		// #8 this is done on the server side "The server checks whether there already is another key with the same auth_key_hash and responds in one of the following ways"
		// byte[] pAuthKeyHash = Helper.GetData (Helper.GetSha1 (pAuthKey.getBytes ()), 0, 8);

		// #9 DH key exchange complete
		byte[] pAuthKeyAuxHash = Helper.GetData (Helper.GetSha1 (pAuthKey.getBytes ()), 0, 8); // 64 higher-order bits of SHA1(auth_key)
		byte[] pServerResponse = pTcpClient.Send(pEncryptedRequest); // new byte[52];
		int pGenStatus = BitConverter.ToInt32 (pServerResponse, 0);
		byte[] pFirstClientNonce = Helper.GetData (pServerResponse, 4, 16);
		byte[] pFirstServerNonce = Helper.GetData (pServerResponse, 20, 16);
        CheckNonce(pClientNonce, pFirstClientNonce);
        CheckNonce(pServerNonceBytes, pFirstServerNonce);
		byte[] pNewNonceHash123 = Helper.GetData (pServerResponse, 36, 16);
        byte[] pComputedNewNonceHash = ComputeNewNonceHash(pClientProofNewNonce, 1, pAuthKeyAuxHash);
		if (pGenStatus != 0x3bcbf734 && !Helper.DataEquals(pNewNonceHash123, pComputedNewNonceHash)) 
		{
            return false;
		}
		return true;
	}
    private byte[] ComputeNewNonceHash(byte[] pNewNonce, byte pReplyNum, byte[] pAuthKeyAuxHash)
    {
        var pArr = new byte[33 + pAuthKeyAuxHash.Length];
        Helper.SetData(ref pArr, pNewNonce, 9);
        pArr[0] = pReplyNum;
        Helper.SetData(ref pArr, pAuthKeyAuxHash, 1);
        return Helper.GetData(Helper.GetSha1(pArr), 0, 16);
    }
}
