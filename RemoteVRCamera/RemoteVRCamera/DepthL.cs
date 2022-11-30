using System;
using System.Collections.Generic;
using System.Text;
using Intel.RealSense;

namespace RemoteVRCamera
{
    class DepthL
    {
        TCPL tcp;
        Pipeline pipe;
        Config cfg;
        ushort[] uDepth;
        byte[] bDepth;
        byte[] buffer;
        int width = 640;
        int height = 480;

        public DepthL(int port)
        {
            tcp = new TCPL(port);

            uDepth = new ushort[width * height];
            bDepth = new byte[width * height * 2];
            buffer = new byte[2];

            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            pipe.Start(cfg);
            Console.WriteLine("DepthPipe Start");
        }

        public void GetAndSend()
        {
            while (true)
            {
                using (var frames = pipe.WaitForFrames())
                using (var depth = frames.DepthFrame)
                {
                    depth.CopyTo<ushort>(uDepth);
                    for (int n = 0; n < width * height; n++)
                    {
                        buffer = BitConverter.GetBytes(uDepth[n]);
                        Array.Copy(buffer, 0, bDepth, 2 * n, buffer.Length);
                    }
                    tcp.Send(bDepth);
                }
            }
        }

        public void Exit()
        {
            tcp.Exit();
            Console.WriteLine("TCP Exit");
        }
    }
}
