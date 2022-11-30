using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteVRCamera
{
    class TCP
    {
        //string remoteHost = "192.168.50.27"/*"192.168.50.27"*/;
        TcpClient tcp;
        NetworkStream ns;
        IPAddress remoteAddress;
        int sendTimes;
        byte[] sendByte;
        public TCP(string remoteHost , int remotePort)
        {
            remoteAddress = IPAddress.Parse(remoteHost);
            tcp = new TcpClient();
            tcp.Connect(remoteAddress, remotePort);
            ns = tcp.GetStream();
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

        public void Exit()
        {
            tcp.Close();
        }
    }
}
