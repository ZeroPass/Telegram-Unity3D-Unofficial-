using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System;
using System.Threading;

public class TTCPClient {
    private Thread pListeningThread = null;
    private TcpListener pListener = null;
	private TcpClient pClient;
    private Socket pSendSocket = null;
	private IPEndPoint pEndPoint = null;
	public RSACryptoServiceProvider PublicServerRsaKey = null;
	private int pPackageSeqNum = 0;
    private bool pListen = true;
	public TTCPClient(IPEndPoint pArgEndPoint, RSACryptoServiceProvider pPublicServerKey)
	{
		this.PublicServerRsaKey = pPublicServerKey;
		this.pEndPoint = pArgEndPoint;
        this.StartListening();
		pClient = new TcpClient ();
		pClient.ReceiveTimeout = 3000;
        pSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		this.ConnectNow ();
	}
    private void StartListening()
    {
        pListeningThread = new Thread(new ThreadStart(ListeningWorker));
        pListeningThread.Start();
    }
    private void ListeningWorker()
    {
      //  this.pListener = new TcpListener(IPAddress.Any, 443);
      //  pListener.Start();
        while(pListen)
        {
            if(pSendSocket.Available > 0)
            {
                byte[] pData = new byte[2048];
                pSendSocket.Receive(pData, pData.Length, SocketFlags.None);
            }
            else
            {
                Thread.Sleep(10);
            }
           /* Socket pConSocker = pListener.AcceptSocket();            
            pConSocker.Receive(pData, pData.Length, SocketFlags.None);*/
        }
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
                int pSendBytes = pSendSocket.Send(pData, pData.Length, SocketFlags.None);

                //NetworkStream pStream = pClient.GetStream();
                //pStream.Write(pData, 0, pData.Length);
				//int pSendBytes = pClient.Client.Send(pData);
				System.Threading.Thread.Sleep(5000);
				
                byte[] pReceivedData = new byte[2048];
                /*
                for (short x = 0; pSendSocket.Available == 0 && x < 100; x++)
                {                   
                     Thread.Sleep(10);
                }
                 */
                pSendSocket.Receive(pReceivedData, pReceivedData.Length, SocketFlags.None);//.Read(pData, 0, pData.Length);
				//pClient.ReceiveBufferSize = pReceivedData.Length;
				//int pReceivedBytes = pClient.Client.Receive(pReceivedData, pReceivedData.Length, SocketFlags.None);
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
