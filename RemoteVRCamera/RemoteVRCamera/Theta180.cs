using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace RemoteVRCamera
{
    class Theta180
    {
        #region define
        static readonly string theta_v_FullHD = "RICOH THETA V FullHD";
        static readonly string theta_v_4K = "RICOH THETA V 4K";

        static readonly string[] thetaCameraModeList =
        {
            theta_v_FullHD,
            theta_v_4K,
        };
        public enum THETA_V_CAMERA_MODE
        {
            THETA_V_FullHD,
            THETA_V_4K,
        }
        #endregion

        public THETA_V_CAMERA_MODE cameraMode = THETA_V_CAMERA_MODE.THETA_V_FullHD;
        Mat frame;
        VideoCapture capture;
        byte[] bFrame;
        int height;
        int width;

        TCPL tcp;

        public Theta180(int deviceIndex, int port)
        {
            height = 960;
            width = 1920;
            frame = new Mat(height, width, MatType.CV_8UC3);
            capture = new VideoCapture();
            bFrame = new byte[height * width * 3 / 2];  //61440*45

            tcp = new TCPL(port);

            capture.Open(deviceIndex);
            if (!capture.IsOpened())
            {
                Console.WriteLine("camera was not found");
                return;
            }
        }
        public void GetAndSend()
        {
            while (true)
            {
                capture.Read(frame);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width / 4; x++)
                    {
                        bFrame[3 * (width / 2 * y + x)] = frame.At<Vec3b>(y, x + width * 3 / 4).Item2;
                        bFrame[3 * (width / 2 * y + x) + 1] = frame.At<Vec3b>(y, x + width * 3 / 4).Item1;
                        bFrame[3 * (width / 2 * y + x) + 2] = frame.At<Vec3b>(y, x + width * 3 / 4).Item0;
                    }
                    for (int x = 0; x < width / 4; x++)
                    {
                        bFrame[3 * (width / 2 * y + x + width / 4)] = frame.At<Vec3b>(y, x).Item2;
                        bFrame[3 * (width / 2 * y + x + width / 4) + 1] = frame.At<Vec3b>(y, x).Item1;
                        bFrame[3 * (width / 2 * y + x + width / 4) + 2] = frame.At<Vec3b>(y, x).Item0;
                    }
                }
                tcp.Send(bFrame);
            }
        }
        public void Exit()
        {
            tcp.Exit();
        }
    }
}
