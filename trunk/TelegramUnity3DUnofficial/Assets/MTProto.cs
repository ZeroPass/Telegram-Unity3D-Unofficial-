using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Collections.Generic;
public class MTProto
{
	private TTCPClient pTcpClient = null;
	private Authorization pAuthorization = null;
	public static class ServerInfo
	{
        public static StoredPublicKeyList PublicKeys
        {
            get
            {
                if(pPublicKeys == null)
                {
                    pPublicKeys = new StoredPublicKeyList();
                }
                return pPublicKeys;
            }
        }
        private static StoredPublicKeyList pPublicKeys = null;
        public class StoredPublicKeyList : List<PublicRSAKey>
        {
            public StoredPublicKeyList()
            {
                Math.BigInteger pModulus0 = new Math.BigInteger("c150023e2f70db7985ded064759cfecf0af328e69a41daf4d6f01b538135a6f91f8f8b2a0ec9ba9720ce352efcf6c5680ffc424bd634864902de0b4bd6d49f4e580230e3ae97d95c8b19442b3c0a10d8f5633fecedd6926a7f6dab0ddb7d457f9ea81b8465fcd6fffeed114011df91c059caedaf97625f6c96ecc74725556934ef781d866b34f011fce4d835a090196e9a5f0e4449af7eb697ddb9076494ca5f81104a305b6dd27665722c46b60e5df680fb16b210607ef217652e60236c255f6a28315f4083a96791d7214bf64c1df4fd0db1944fb26a2a57031b32eee64ad15a8ba68885cde74a5bfc920f6abf59ba5c75506373e7130f9042da922179251f", 16);
                Math.BigInteger pExponent0 = new Math.BigInteger(10001);
                PublicRSAKey pKey0 = new PublicRSAKey(pModulus0, pExponent0, 0xc3b42b026ce86b21UL);
                this.Add(pKey0);
            }
            public PublicRSAKey GetKeyByFingerprint(ulong pFingerprint)
            {
                for (ushort x = 0; x < this.Count; x++)
                {
                    if(this[x].Fingerprint == pFingerprint && pFingerprint != 0)
                    {
                        return this[x];
                    }
                }
                return null;
            }
        }
        public class PublicRSAKey
        {
            private RSACryptoServiceProvider pServerPublicKey = null;
            private Math.BigInteger pModulus = -1;
            private Math.BigInteger pExponent = -1;
            public ulong Fingerprint = 0;
            public PublicRSAKey(Math.BigInteger pArgModulus, Math.BigInteger pArgExponent, ulong pFingerprint = 0)
            {
                this.pModulus = pArgModulus;
                this.pExponent = pArgExponent;                
                pServerPublicKey = new RSACryptoServiceProvider();
                RSAParameters pParams = new RSAParameters();
                pParams.Exponent = this.pExponent.getBytes();
                pParams.Modulus = pModulus.getBytes();
                pServerPublicKey.ImportParameters(pParams);
                if(pFingerprint != 0)
                {
                    this.Fingerprint = pFingerprint;
                }
                else
                {
                    this.CalculateFingerprint();
                }
            }
            private void CalculateFingerprint()
            {

            }
            public byte[] Encrypt(byte[] pData)
            {
                return pServerPublicKey.Encrypt(pData, false);
            }
        }
		private static string pPublicKey = @"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEAwVACPi9w23mF3tBkdZz+zwrzKOaaQdr01vAbU4E1pvkfj4sqDsm6
lyDONS789sVoD/xCS9Y0hkkC3gtL1tSfTlgCMOOul9lcixlEKzwKENj1Yz/s7daS
an9tqw3bfUV/nqgbhGX81v/+7RFAEd+RwFnK7a+XYl9sluzHRyVVaTTveB2GazTw
Efzk2DWgkBluml8OREmvfraX3bkHZJTKX4EQSjBbbdJ2ZXIsRrYOXfaA+xayEGB+
8hdlLmAjbCVfaigxX0CDqWeR1yFL9kwd9P0NsZRPsmoqVwMbMu7mStFai6aIhc3n
Slv8kg9qv1m6XHVQY3PnEw+QQtqSIXklHwIDAQAB
-----END RSA PUBLIC KEY-----";
		
		private static IPAddress pServerIp = null;
		public static IPAddress ServerIp
		{
			get
			{
				if(pServerIp == null)
				{
					// 149.154.167.50:443 - production
					// 149.154.167.40:443 - test
					IPAddress.TryParse("149.154.167.50", out pServerIp);
				}
				return pServerIp;
			}
		}
		public static int ServerPort
		{
			get
			{
				return 443;
			}
		}
	}
	public MTProto()
	{
        //RSACryptoServiceProvider pServerPublicKey = ServerInfo.ServerPublicKey;
		IPEndPoint pEndPoint = new IPEndPoint (ServerInfo.ServerIp, ServerInfo.ServerPort);
		pTcpClient = new TTCPClient (pEndPoint, null);
		pAuthorization = new Authorization (pTcpClient);       
	}
	public void Connect()
	{

	}
	public void Disconnect()
	{
	}
}
