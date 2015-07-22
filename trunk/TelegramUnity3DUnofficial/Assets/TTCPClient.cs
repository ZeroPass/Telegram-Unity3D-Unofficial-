using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;

public class TTCPClient {
	private TcpClient pClient;
	private IPEndPoint pEndPoint = null;
	public RSACryptoServiceProvider PublicServerRsaKey = null;
	private int pPackageSeqNum = 0;
	public TTCPClient(IPEndPoint pArgEndPoint, RSACryptoServiceProvider pPublicServerKey)
	{
		this.PublicServerRsaKey = pPublicServerKey;
		this.pEndPoint = pArgEndPoint;
		pClient = new TcpClient ();
		pClient.ReceiveTimeout = 3000;
		this.ConnectNow ();
	}
	public void Send(byte[] pData)
	{
		if(ConnectNow ()) 
		{
			pClient.Client.Send(pData);
		}
	}
	public byte[] SendReceive(byte[] pData)
	{
		if(ConnectNow ()) 
		{
			try
			{
				if(PublicServerRsaKey != null)
				{
					pData = PublicServerRsaKey.Encrypt(pData, true);
				}
				PreparePackage(ref pData);
				int pSendBytes = pClient.Client.Send(pData);
				//System.Threading.Thread.Sleep(500);
				byte[] pReceivedData = new byte[2048];
				pClient.ReceiveBufferSize = pReceivedData.Length;
				int pReceivedBytes = pClient.Client.Receive(pReceivedData, pReceivedData.Length, SocketFlags.None);
				return pReceivedData;
			}
			catch(WebException e)
			{
				//Debug.LogException(e);
			}
		}
		return null;
	}
	private void PreparePackage(ref byte[] pData)
	{
		byte[] pReadyPackage = new byte[pData.Length + 12];
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pData.Length), 0);
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pPackageSeqNum++), 4);
		Helper.SetData (ref pReadyPackage, pData, 8);
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (Crc32.Compute (pData)), pData.Length + 8);
		pData = pReadyPackage;
	}
	public bool ConnectNow()
	{
		if(pClient != null)
		{
			if(!pClient.Connected)
			{
				try
				{
					pClient.Connect(this.pEndPoint);
				}
				catch (WebException e)
				{
					//Debug.LogException(e);
				}
			}
			return pClient.Connected;
		}
		else
		{
			return false;
		}
	}
}
