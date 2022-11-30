using System;
using Intel.RealSense;

namespace RemoteVRCamera
{
    class ColorL
    {
        TCPL tcp;
        Pipeline pipe;
        Config cfg;
        byte[] rgb;
        int width = 640;
        int height = 480;

        public ColorL(int port)
        {
            tcp = new TCPL(port);
            rgb = new byte[width * height * 3];

            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Color, width, height);
            pipe.Start(cfg);
            Console.WriteLine("ColorPipe Start");
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
