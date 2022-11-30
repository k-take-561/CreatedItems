using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Net.Sockets;
using System.Net;

public class ColorRcv : MonoBehaviour
{
    static int remotePote = 2004;
    static byte[] rcvByte;
    static int rcvTimes;
    public static byte[] buffer;

    static TcpListener listener;
    static int dataSize;
    static int canSize;
    static bool s;

    static int width = 640;
    static int height = 480;

    Thread thread;

    // Start is called before the first frame update
    void Start()
    {
        rcvByte = new byte[61440];
        rcvTimes = width * height * 3 / rcvByte.Length;
        buffer = new byte[width * height * 3];

        listener = new TcpListener(IPAddress.Any, remotePote);
        listener.Start();

        thread = new Thread(new ThreadStart(RGBReceive));
        thread.Start();
    }

    void OnApplicationQuit()
    {
        thread.Abort();
        listener.Stop();
    }

    private static void RGBReceive()
    {
        using (var client = listener.AcceptTcpClient())
        using (var ns = client.GetStream())
        {
            while (true)
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

            }
        }

    }
}
