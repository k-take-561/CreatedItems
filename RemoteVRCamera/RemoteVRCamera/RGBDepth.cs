using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel.RealSense;

namespace RemoteVRCamera
{
    class RGBDepth
    {
        TCP tcp;

        Pipeline pipe;
        Config cfg;
        byte[] rgb;
        int width = 640;
        int height = 480;

        public RGBDepth(string ip, int port)
        {
            tcp = new TCP(ip,port);
            rgb = new byte[width * height * 3];

            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Color, width, height);
            pipe.Start(cfg);
        }

        public void GetAndSend()
        {
            while (true)
            {
                using (var frames = pipe.WaitForFrames())
                using (var color = frames.ColorFrame)
                {
                    color.CopyTo(rgb);
                    tcp.Send(rgb);
                }

            }
        }

        public void Exit()
        {
            tcp.Exit();
        }
    }
}
