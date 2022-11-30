using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel.RealSense;

namespace RemoteVRCamera
{
    class DepthSensor
    {
        TCP tcp;
        Pipeline pipe;
        Config cfg;
        ushort[] uDepth;
        byte[] bDepth;
        byte[] buffer;
        int width = 640;
        int height = 480;

        public DepthSensor(string ip , int port)
        {
            tcp = new TCP(ip,port);

            uDepth = new ushort[width * height];
            bDepth = new byte[width * height * 2];
            buffer = new byte[2];

            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            pipe.Start(cfg);
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
        }
    }
}
