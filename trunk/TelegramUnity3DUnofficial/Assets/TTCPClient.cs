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
    private object pReceivingLock = new object();
    private List<SendData> pDataToSend = new List<SendData>();
    private List<ReceivedData> pDataReceived = new List<ReceivedData>();
    private TcpClient pClient = null;
    private NetworkStream pClientStream = null;
	private IPEndPoint pEndPoint = null;
	public RSACryptoServiceProvider PublicServerRsaKey = null;
	private int pPackageSeqNum = 0;
    private uint ReceiveTimout = 1000;
    private bool pListen = true;
    private bool pSendingEnabled = true;
    public class ReceivedData
    {
        public int Constructor;
        public int MessageId;
        public int Lenght;
        public int SequenceNum = -1;
        public byte[] DataContent;
        private uint pCrc32;
        private bool pDataValid = false;
        public bool DataValid
        {
            get
            {
                return pDataValid;
            }
        }
        public ReceivedData(byte[] pData)
        {
            this.pCrc32 = BitConverter.ToUInt32(pData, pData.Length - 4);
            this.pDataValid = Crc32.IsValid(Helper.GetData(pData, 0, pData.Length - 4), this.pCrc32);
            if (this.DataValid)
            {
                this.Lenght = BitConverter.ToInt32(pData, 0);
                this.DataContent = Helper.GetData(pData, 8, pData.Length - 12);
                this.SequenceNum = BitConverter.ToInt32(pData, 4);
            }
        }
    }
    public class SendData
    {
        public int SeqNum = 0;
        public byte[] Data = null;
        public SendData(byte[] pData, int pPackageSeq)
        {
            this.Data = pData;
            this.SeqNum = pPackageSeq;
        }
    }
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
                    SendReceive(pDataToSend[x].Data);
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
            if (pClientStream != null && pClientStream.DataAvailable)
            {
                int pLen = pClientStream.Read(pData, 0, pData.Length);
                lock(pReceivingLock)
                {
                    pDataReceived.Add(new ReceivedData(Helper.GetData(pData, 0, pLen)));
                }                
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
        SendData pSendData = null;
		lock(pSendingLock)
        {
            pSendData = PreparePackage(ref pData);
            pDataToSend.Add(pSendData);
            Monitor.PulseAll(pSendingLock);
        }
        for (uint x = (ReceiveTimout / 10); x > 0; x--)
        {
            if(pDataReceived.Count > 0)
            {
                lock (pReceivingLock)
                {
                    for (ushort y = 0; y < pDataReceived.Count; y++)
                    {
                        if (pDataReceived[y].SequenceNum == pSendData.SeqNum)
                        {
                            byte[] pReturn = pDataReceived[y].DataContent;
                            pDataReceived.RemoveAt(y);
                            return pReturn;
                        }
                    }
                }
            }
            Thread.Sleep(10);
        }
        throw new Exception("No data received, timeout.");
        return null;
	}
	public void SendReceive(byte[] pData)
	{
		if(ConnectNow ()) 
		{
			try
			{				
                pClientStream.Write(pData, 0, pData.Length);
			}
			catch(WebException e)
			{
				//Debug.LogException(e);
			}
		}
	}
	private SendData PreparePackage(ref byte[] pData)
	{
        if (PublicServerRsaKey != null)
        {
            pData = PublicServerRsaKey.Encrypt(pData, true);
        }        
        int pPackageSeq = pPackageSeqNum++;
		byte[] pReadyPackage = new byte[pData.Length + 12];
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (pData.Length + 12), 0); // length
        Helper.SetData(ref pReadyPackage, BitConverter.GetBytes(pPackageSeq), 4); // sequence num. starting at 0
		Helper.SetData (ref pReadyPackage, pData, 8); // actual data
		Helper.SetData (ref pReadyPackage, BitConverter.GetBytes (Crc32.Compute (Helper.GetData(pReadyPackage, 0 , pReadyPackage.Length - 4))), pData.Length + 8); // crc32 of the whole package, with len & seq. num.
		pData = pReadyPackage;
        return new SendData(pData, pPackageSeq);
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
