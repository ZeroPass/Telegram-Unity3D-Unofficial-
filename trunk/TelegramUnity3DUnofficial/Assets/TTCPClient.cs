using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;
using System.Threading;
using System.Diagnostics;

public class TTCPClient {
    private Thread pListeningThread = null;
    private Socket pSendSocket = null;
	private IPEndPoint pEndPoint = null;
	public RSACryptoServiceProvider PublicServerRsaKey = null;
	private int pPackageSeqNum = 1;
    private bool pListen = true;
	public TTCPClient(IPEndPoint pArgEndPoint, RSACryptoServiceProvider pPublicServerKey)
	{
		this.PublicServerRsaKey = pPublicServerKey;
		this.pEndPoint = pArgEndPoint;        
        
        pSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        pSendSocket.ReceiveTimeout = 1000;
        pSendSocket.SendTimeout = 1000;
		this.ConnectNow ();
        this.StartListening();
	}
    private void StartListening()
    {
        pListeningThread = new Thread(new ThreadStart(ListeningWorker));
        pListeningThread.Start();
    }
    private void ListeningWorker()
    {
        while(pListen)
        {
            if (pSendSocket.Available > 0) // if any data available to receive
            {
                Debugger.Break(); // pauses execution when something is received
                byte[] pData = new byte[2048];
                pSendSocket.Receive(pData, pData.Length, SocketFlags.None);
            }
            else
            {
                Thread.Sleep(10);
            }
            if(!pSendSocket.Connected)
            {
                throw new Exception("Connection closed");
            }
        }
    }
	public void Send(byte[] pData)
	{
		if(ConnectNow ()) 
		{
			pSendSocket.Send(pData);
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
                int pSendBytes = pSendSocket.Send(pData, pData.Length, SocketFlags.None);
                byte[] pReceivedData = new byte[2048]; // dummy return, from sync receive
                //pSendSocket.Receive(pReceivedData, SocketFlags.None);
                Thread.Sleep(2000); // wait for async to receive, ListeningWorker() breaks program execution when received
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
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pData.Length + 12), 0); // length
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pPackageSeqNum++), 4); // sequence num. starting at 1
		Helper.SetData (ref pReadyPackage, pData, 8); // actual data
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (Crc32.Compute (Helper.GetData(pReadyPackage, 0 , pReadyPackage.Length - 4))), pData.Length + 8); // crc32 of the whole package, with len & seq. num.
		pData = pReadyPackage;
	}
	public bool ConnectNow()
	{
        if (pSendSocket != null)
		{
            if (!pSendSocket.Connected)
			{
				try
				{
                    pSendSocket.Connect(this.pEndPoint);
				}
				catch (WebException e)
				{
					//Debug.LogException(e);
				}
			}
            return pSendSocket.Connected;
		}
		else
		{
			return false;
		}
	}
}
