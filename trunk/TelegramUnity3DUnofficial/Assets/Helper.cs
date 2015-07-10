using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;

public static class Helper
{
	public static byte[] RandomData(int pBitLength)
	{
		if (pBitLength % 8 == 0) 
		{
			int pByteLen = pBitLength / 8;
			RandomNumberGenerator pRandom = RandomNumberGenerator.Create ();
			byte[] pData = new byte[pByteLen];
			pRandom.GetBytes (pData);
			return pData;
		}
		else
		{
			return null;
		}
	}
	public static Math.BigInteger RandomNum(int pBitLength)
	{
		byte[] pData = RandomData (pBitLength);
		if (pData != null) 
		{
			return new Math.BigInteger(pData);
		}
		else
		{
			return -1;
		}
	}
	public static void SetData(out byte[] pData, byte[] pDataToSet, int pStart)
	{
		int pEnd = pDataToSet.Length + pStart;
		if(pEnd < pData.Length)
		{
			for(int x = pStart, i  = 0; x < pEnd; x++, i++)
			{
				pData[x] = pDataToSet[i];
			}
		}
		else
		{
			throw new System.Exception("Data to set is too long");
		}
	}
	public static byte[] GetData(byte[] pData, int pStart, int pLength)
	{
		int pEnd = pStart + pLength;
		if(pEnd < pData.Length)
		{
			byte[] pResult = new byte[pLength];
			for(int x = pStart, i = 0; x < pEnd; x++, i++)
			{
				pResult[i] = pData[x];
			}
			return pResult;
		}
		else
		{
			return null;
		}
	}
	private static TimeSpan UnixTimeSpan()
	{
		DateTime CetTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "UTC");
		DateTime CetTimeStart = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UTC");
		return (CetTime - CetTimeStart);
	}
	public static Int64 TimeNowUnix()
	{
		TimeSpan span = UnixTimeSpan();
		return Convert.ToInt64(System.Math.Floor(span.TotalSeconds));
	}
	public static string RandomNumStr(int pBitLength)
	{
		return RandomNum (pBitLength).ToString ();
	}
	public static PQPair DecomposeToPrimeFactors(Math.BigInteger pNum)
	{
		Math.BigInteger pOriginalNum = pNum;
		List<Math.BigInteger> pPrimes = new List<Math.BigInteger> ();
		for( ; pNum > 1; pNum--)
		{
			bool pIsPrime = true;
			for(Math.BigInteger i = 2; i < pNum - 1; i++)
			{
				if(pNum % i == 0)
				{
					pIsPrime = false;
					break;
				}
			}
			if(pIsPrime)
			{
				for(Math.BigInteger q = 0; q < pPrimes.Count; q++)
				{
					Math.BigInteger pProduct = pPrimes[q] * pNum;
					if(pProduct == pOriginalNum)
					{
						return new PQPair(pNum, pPrimes[q]);
					}
				}
				pPrimes.Insert(0, pNum);

			}
		}
	}
	public class PQPair
	{
		public Math.BigInteger P;
		public Math.BigInteger Q;
		public PQPair(Math.BigInteger pP, Math.BigInteger pQ)
		{
			this.P = pP;
			this.Q = pQ;
		}
	}
}
