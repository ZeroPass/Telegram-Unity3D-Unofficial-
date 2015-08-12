using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

public class TTCPClient {
    private Thread pListeningThread = null;
    private Thread pSendingThread = null;
    private object pSendingLock = new object();
    private List<byte[]> pDataToSend = new List<byte[]>();
    private TcpClient pClient = null;
    private NetworkStream pClientStream = null;
	private IPEndPoint pEndPoint = null;
	public RSACryptoServiceProvider PublicServerRsaKey = null;
	private int pPackageSeqNum = 1;
    private bool pListen = true;
    private bool pSendingEnabled = true;
	public TTCPClient(IPEndPoint pArgEndPoint, RSACryptoServiceProvider pPublicServerKey)
	{
		this.PublicServerRsaKey = pPublicServerKey;
		this.pEndPoint = pArgEndPoint;
        this.pClient = new TcpClient();        
		this.ConnectNow ();
        this.pClientStream = pClient.GetStream();
        this.StartListening();
        this.StartSendingThread();
	}
    private void StartListening()
    {
        pListeningThread = new Thread(new ThreadStart(ListeningWorker));
        pListeningThread.Start();
    }
    private void StartSendingThread()
    {
        pSendingThread = new Thread(new ThreadStart(SendingWorker));
        pSendingThread.Start();
    }
    private void SendingWorker()
    {
        while(pSendingEnabled)
        {
            lock(pSendingLock)
            {
                if(pDataToSend.Count == 0)
                {
                    Monitor.Wait(pSendingLock);
                }
                for (short x = 0; x < pDataToSend.Count; x++ )
                {
                    SendReceive(pDataToSend[x]);
                }
                pDataToSend.Clear();
            }
        }
    }
    private void ListeningWorker()
    {
        byte[] pData = new byte[2048];
        while(pListen)
        {
            if (pClientStream.DataAvailable)
            {
                pClientStream.Read(pData, 0, pData.Length);
                Debugger.Break(); // pauses execution when something is received
            }
            else
            {
                Thread.Sleep(10);
            }
            if (!pClient.Connected)
            {
                throw new Exception("Connection closed");
            }
        }
    }
	public byte[] Send(byte[] pData)
	{
		lock(pSendingLock)
        {
            pDataToSend.Add(pData);
            Monitor.PulseAll(pSendingLock);
        }
        return null;
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
                pClientStream.Write(pData, 0, pData.Length);
                byte[] pReceivedData = new byte[2048]; // dummy return, from sync receive
                //pSendSocket.Receive(pReceivedData, SocketFlags.None);
               // Thread.Sleep(2000); // wait for async to receive, ListeningWorker() breaks program execution when received
				return pReceivedData;
			}
			catch(WebException e)
			{
				//Debug.LogException(e);
			}
		}
		return null;
	}
	private int PreparePackage(ref byte[] pData)
	{
        int pPackageSeq = pPackageSeqNum++;
		byte[] pReadyPackage = new byte[pData.Length + 12];
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pData.Length + 8), 0); // length
        Helper.SetData(ref pReadyPackage, BitConverter.GetBytes(pPackageSeq), 4); // sequence num. starting at 1
		Helper.SetData (ref pReadyPackage, pData, 8); // actual data
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (Crc32.Compute (Helper.GetData(pReadyPackage, 0 , pReadyPackage.Length - 4))), pData.Length + 8); // crc32 of the whole package, with len & seq. num.
		pData = pReadyPackage;
        return pPackageSeq;
	}
	public bool ConnectNow()
	{
        if (pClient != null)
		{
            if (!pClient.Connected)
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
