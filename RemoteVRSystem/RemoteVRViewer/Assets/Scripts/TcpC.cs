using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TcpC : MonoBehaviour
{
    TcpClient tcp;
    NetworkStream ns;
    IPAddress remoteAddress;
    int rcvTimes;
    byte[] rcvByte;
    byte[] rcvEx;
    byte[] buffer;
    int dataSize;
    int canSize;
    bool s;
    public TcpC(string remoteHost, int remotePort, int bufferSize)
    {
        remoteAddress = IPAddress.Parse(remoteHost);
        tcp = new TcpClient();
        tcp.Connect(remoteAddress, remotePort);
        Debug.Log(remotePort + " Connected");
        ns = tcp.GetStream();
        rcvByte = new byte[61440];
        buffer = new byte[bufferSize];
        rcvTimes = bufferSize / 61440;
        rcvEx = new byte[48];
    }
    public byte[] ReceiveEx()
    {
        s = true;
        dataSize = 0;
        while (s)
        {
            canSize = ns.Read(rcvEx, dataSize, rcvEx.Length - dataSize);
            dataSize += canSize;
            if (dataSize >= rcvEx.Length) { s = false; }
        }
        return rcvEx;
    }
    public byte[] Receive()
    {
        for (int t = 0; t < rcvTimes; t++)
        {
            s = true;
            dataSize = 0;
            while (s)
            {
                canSize = ns.Read(rcvByte, dataSize, rcvByte.Length - dataSize);
                dataSize += canSize;
                if (dataSize >= rcvByte.Length) { s = false; }
            }
            Array.Copy(rcvByte, 0, buffer, rcvByte.Length * t, rcvByte.Length);
        }
        return buffer;
    }
    public void Exit()
    {
        tcp.Close();
    }
}
