using UnityEngine;
using System.Collections;
using System.Net;

public class MTProto
{
	public class ServerInfo
	{
		string pPublicKey = @"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEAwVACPi9w23mF3tBkdZz+zwrzKOaaQdr01vAbU4E1pvkfj4sqDsm6
lyDONS789sVoD/xCS9Y0hkkC3gtL1tSfTlgCMOOul9lcixlEKzwKENj1Yz/s7daS
an9tqw3bfUV/nqgbhGX81v/+7RFAEd+RwFnK7a+XYl9sluzHRyVVaTTveB2GazTw
Efzk2DWgkBluml8OREmvfraX3bkHZJTKX4EQSjBbbdJ2ZXIsRrYOXfaA+xayEGB+
8hdlLmAjbCVfaigxX0CDqWeR1yFL9kwd9P0NsZRPsmoqVwMbMu7mStFai6aIhc3n
Slv8kg9qv1m6XHVQY3PnEw+QQtqSIXklHwIDAQAB
-----END RSA PUBLIC KEY-----";

		private IPAddress pServerIp = null;
		public IPAddress ServerIp
		{
			get
			{
				if(pServerIp == null)
				{
					// 149.154.167.50:443 - production
					// 149.154.167.40:443 - test
					IPAddress.TryParse("149.154.167.40", pServerIp);
				}
				return pServerIp;
			}
		}
		public int ServerPort
		{
			get
			{
				return 443;
			}
		}
	}
	public void Connect()
	{

	}
	public void Disconnect()
	{
	}
}
