using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteVRCamera
{
    class TCPL
    {
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        int sendTimes;
        byte[] sendByte;
        public TCPL(int remotePort)
        {
            listener = new TcpListener(IPAddress.Any, remotePort);
            listener.Start();
            Console.WriteLine("{0} Listener Start",remotePort);
            Console.WriteLine("{0} Waiting...",remotePort);
            client = listener.AcceptTcpClient();
            ns = client.GetStream();
            Console.WriteLine("{0} Connected", remotePort);
            sendByte = new byte[61440];
        }
        public void Send(byte[] buffer)
        {
            sendTimes = buffer.Length / 61440;
            for (int t = 0; t < sendTimes; t++)
            {
                Array.Clear(sendByte, 0, sendByte.Length);
                Array.Copy(buffer, sendByte.Length * t, sendByte, 0, sendByte.Length);
                ns.Write(sendByte, 0, sendByte.Length);
            }
            Thread.Sleep(1);
        }
        public void SendEx(byte[] buffer)
        {
            ns.Write(buffer, 0, buffer.Length);
            Thread.Sleep(1);
        }
        public void Exit()
        {
            ns.Close();
            Console.WriteLine("NetworkStream Close");
            client.Close();
            Console.WriteLine("TCPClient Close");
            listener.Stop();
            Console.WriteLine("Listener Stop");
        }
    }
}
