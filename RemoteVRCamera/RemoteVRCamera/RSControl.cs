using System;
using Intel.RealSense;
using System.Numerics;

namespace RemoteVRCamera
{
    class RSControl
    {
        TCPL tcp_control;
        TCPL tcp_depth;
        TCPL tcp_color;
        ushort[] uDepth;
        byte[] bDepth;
        byte[] bitBuffer;
        byte[] rgb;
        static public int width = 640;
        static public int height = 480;

        Pipeline pipe;
        Config cfg;
        PipelineProfile pp;
        StreamProfile depth_stream;
        StreamProfile color_stream;
        byte[] bEx;
        Sever sever;
        Vector3[] vec;
        PointCloud pc;
        public RSControl(int depthPort,int colorPort, bool whill)
        {
            tcp_control = new TCPL(2000);
            tcp_depth = new TCPL(depthPort);
            tcp_color = new TCPL(colorPort);
            uDepth = new ushort[width * height];
            bDepth = new byte[width * height * 2];
            bitBuffer = new byte[4];
            rgb = new byte[width * height * 3];
            bEx = new byte[48];
            if (whill)
            {
                sever = new Sever("COM5", 20000);
                vec = new Vector3[width * height];
                pc = new PointCloud();
            }
        }
        public void Initialize()
        {
            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            cfg.EnableStream(Stream.Color, width, height);
            pp = pipe.Start(cfg);   Console.WriteLine("Pipe Start");
            depth_stream = pp.GetStream(Stream.Depth);
            color_stream = pp.GetStream(Stream.Color);
            ExtrinsicsToByte(depth_stream.GetExtrinsicsTo(color_stream));
            //SystemControl();
            if(sever != null) sever.Start();
        }
        private void ExtrinsicsToByte(Extrinsics e)
        {
            //Console.WriteLine("-----Rotetion-----");
            for (int n = 0; n < 9; n++)
            {
                bitBuffer = BitConverter.GetBytes(e.rotation[n]);
                //Console.WriteLine("{0}: {1} is {2} bits", n, e.rotation[n], BitConverter.GetBytes(e.rotation[n]).Length);
                Array.Copy(bitBuffer, 0, bEx, bitBuffer.Length * n, bitBuffer.Length);
            }
            //Console.WriteLine("-----Translation-----");
            for (int n = 0; n < 3; n++)
            {
                bitBuffer = BitConverter.GetBytes(e.translation[n]);
                //Console.WriteLine("{0}: {1} is {2} bits", 9+n, e.translation[n], BitConverter.GetBytes(e.translation[n]).Length);
                Array.Copy(bitBuffer, 0, bEx, bitBuffer.Length * (9+n), bitBuffer.Length);
            }
            //Console.WriteLine(bEx.Length);
            //Console.WriteLine(BitConverter.ToString(bEx));
            tcp_control.SendEx(bEx);
        }
        public void SystemControl()
        {
            //tcp_control.SendEx(bEx);
        }
        public void GetAndSendDepth()
        {
            while (true)
            {
                using (var frames = pipe.WaitForFrames())
                using (var depth = frames.DepthFrame)
                {
                    if(sever != null)
                    {
                        using (var points = pc.Process(depth))
                        using (var point = points.As<Points>())
                        {
                            point.CopyVertices(sever.depthData);
                        }
                    }
                    //Console.WriteLine(vec[width * height / 4]);
                    depth.CopyTo<ushort>(uDepth);
                    for (int n = 0; n < width * height; n++)
                    {
                        bitBuffer = BitConverter.GetBytes(uDepth[n]);
                        Array.Copy(bitBuffer, 0, bDepth, 2 * n, bitBuffer.Length);
                    }
                    tcp_depth.Send(bDepth);
                }
            }
        }
        public void GetAndSendColor()
        {
            while (true)
            {
                using (var frames = pipe.WaitForFrames())
                using (var color = frames.ColorFrame)
                {
                    color.CopyTo(rgb);
                    tcp_color.Send(rgb);
                }
            }
        }
        public void Exit()
        {
            tcp_depth.Exit();
            tcp_color.Exit();
            Console.WriteLine("TCP Exit");
        }
    }
}
