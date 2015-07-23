using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using System.IO;

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
	public static void SetData(ref byte[] pData, byte[] pDataToSet, int pStart)
	{
		int pEnd = pDataToSet.Length + pStart;
		if(pEnd <= pData.Length)
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
	public static byte[] CombineData(byte[] pData0, byte[] pData1)
	{
		int pLen = pData0.Length + pData1.Length;
		byte[] pNewData = new byte[pLen];
		for(int x = 0, i = 0; x < pLen; x++, i++)
		{
			if(pData0.Length == x)
			{
				pData0 = pData1;
				i = 0;
			}
			pNewData[x] = pData0[i];
		}
		return pNewData;
	}
	public static byte[] AddDataPadding(byte[] pData, int pUntilIsDivisibleBy)
	{
		int pModulu = pData.Length % pUntilIsDivisibleBy;
		if(pModulu != 0)
		{
			byte[] pPaddedData = new byte[pData.Length + pModulu];
			for(int x = 0; x < pData.Length; x++)
			{
				pPaddedData[x] = pData[x];
			}
			return pPaddedData;
		}
		else
		{
			return pData;
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
    public static bool DataEquals(byte[] pData0, byte[] pData1)
    {
        if(pData0 != null && pData1 != null && pData0.Length == pData1.Length)
        {
            for(int x = 0; x < pData0.Length; x++)
            {
                if(pData0[x] != pData1[x])
                {
                    return false;
                }
            }
        }
        return false;
    }
	public static byte[] GetSha1(byte[] pData)
	{
		using (var sha = new SHA1CryptoServiceProvider())
		{
			return sha.ComputeHash(pData, 0, pData.Length);
		}
	}
	private static TimeSpan UnixTimeSpan()
	{
		DateTime CetTime = DateTime.Now.ToUniversalTime();
		DateTime CetTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
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
				for(int q = 0; q < pPrimes.Count; q++)
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
		return null;
	}
	public static bool IsPrime(Math.BigInteger pPotentialPrime)
	{
		for(Math.BigInteger x = pPotentialPrime / 2; x > 1; x--)
		{
			if(pPotentialPrime % x == 0)
			{
				return false;
			}
		}
		return true;
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
	public static byte[] Aes256IgeEncrypt(byte[] data, byte[] key, byte[] iv)
	{
		var iv1 = new byte[iv.Length/2];
		var iv2 = new byte[iv.Length/2];
		Array.Copy(iv, 0, iv1, 0, iv1.Length);
		Array.Copy(iv, iv.Length/2, iv2, 0, iv2.Length);
		/*
		using (var aes = new AesCryptoServiceProvider())
		{
			aes.Mode = CipherMode.ECB;
			aes.KeySize = key.Length*8;
			aes.Padding = PaddingMode.None;
			aes.IV = iv1;
			aes.Key = key;
			
			int blockSize = aes.BlockSize/8;
			
			var xPrev = new byte[blockSize];
			Buffer.BlockCopy(iv2, 0, xPrev, 0, blockSize);
			var yPrev = new byte[blockSize];
			Buffer.BlockCopy(iv1, 0, yPrev, 0, blockSize);
			
			using (var encrypted = new MemoryStream())
			{
				using (var bw = new BinaryWriter(encrypted))
				{
					var x = new byte[blockSize];
					
					ICryptoTransform encryptor = aes.CreateEncryptor();
					
					for (int i = 0; i < data.Length; i += blockSize)
					{
						Buffer.BlockCopy(data, i, x, 0, blockSize);
						byte[] y = Xor(encryptor.TransformFinalBlock(Xor(x, yPrev), 0, blockSize), xPrev);
						
						Buffer.BlockCopy(x, 0, xPrev, 0, blockSize);
						Buffer.BlockCopy(y, 0, yPrev, 0, blockSize);
						
						bw.Write(y);
					}
				}
				return encrypted.ToArray();
			}
		}*/
		return null;
	}

	public static byte[] Xor(byte[] pData0, byte[] pData1)
	{
		for (int x = 0; x < pData0.Length; x++) 
		{
			pData0[x] ^= pData1[x];
		}
		return pData0;
	}

	public static byte[] Aes256IgeDecrypt(byte[] encryptedData, byte[] key, byte[] iv)
	{
		var iv1 = new byte[iv.Length/2];
		var iv2 = new byte[iv.Length/2];
		Array.Copy(iv, 0, iv1, 0, iv1.Length);
		Array.Copy(iv, iv.Length/2, iv2, 0, iv2.Length);
		/*
		using (var aes = new AesCryptoServiceProvider())
		{
			aes.Mode = CipherMode.ECB;
			aes.KeySize = key.Length*8;
			aes.Padding = PaddingMode.None;
			aes.IV = iv1;
			aes.Key = key;
			
			int blockSize = aes.BlockSize/8;
			
			var xPrev = new byte[blockSize];
			Buffer.BlockCopy(iv1, 0, xPrev, 0, blockSize);
			var yPrev = new byte[blockSize];
			Buffer.BlockCopy(iv2, 0, yPrev, 0, blockSize);
			
			using (var decrypted = new MemoryStream())
			{
				using (var bw = new BinaryWriter(decrypted))
				{
					var x = new byte[blockSize];
					ICryptoTransform decryptor = aes.CreateDecryptor();
					
					for (int i = 0; i < encryptedData.Length; i += blockSize)
					{
						Buffer.BlockCopy(encryptedData, i, x, 0, blockSize);
						byte[] y = Xor(decryptor.TransformFinalBlock(Xor(x, yPrev), 0, blockSize), xPrev);
						
						Buffer.BlockCopy(x, 0, xPrev, 0, blockSize);
						Buffer.BlockCopy(y, 0, yPrev, 0, blockSize);
						
						bw.Write(y);
					}
				}
				return decrypted.ToArray();
			}
		}*/
		return null;
	}
}
