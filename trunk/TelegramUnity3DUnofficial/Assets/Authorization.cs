using UnityEngine;
using System.Collections;
using System;

public class Authorization 
{
	private const long pAppId = 41190;
	private Math.BigInteger ClientNonce;
	private long pMessageId = 0;
	public void StartDHExchange()
	{
		byte[] pAuthRequest = new byte[40];
		Helper.SetData(pAuthRequest, BitConverter.GetBytes(pAppId), 0); // auth_key_id
		long pUnixStamp = Helper.TimeNowUnix ();
		Helper.SetData(pAuthRequest, BitConverter.GetBytes(pUnixStamp), 8); // message_id
		Helper.SetData(pAuthRequest, BitConverter.GetBytes((int)20), 16); // message length
		Helper.SetData(pAuthRequest, BitConverter.GetBytes((int)60469778), 20); // message length
		Helper.SetData(pAuthRequest, Helper.RandomNum(128).getBytes(), 24); // nonce

		// send

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

	}
}
