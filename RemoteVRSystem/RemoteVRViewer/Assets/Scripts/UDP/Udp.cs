using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Udp : MonoBehaviour
{
    IPEndPoint localEp;
    IPEndPoint remoteEp;
    UdpClient client;

    public Udp(string localIP, string remoteIP,int port)
    {
        localEp = new IPEndPoint(IPAddress.Parse(localIP), port);
        remoteEp = new IPEndPoint(IPAddress.Parse(remoteIP), port);
        client = new UdpClient(localEp);
    }
    public byte[] Receive()
    {
        return client.Receive(ref remoteEp);
    }
    public void Exit()
    {
        client.Close();
    }
}
