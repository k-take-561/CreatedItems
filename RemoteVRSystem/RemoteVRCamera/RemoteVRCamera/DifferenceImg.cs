using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace RemoteVRCamera
{
    class DifferenceImg
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

        public DifferenceImg(int deviceIndex, int port)
        {
            height = 960;
            width = 1920;
            frame = new Mat(height, width, MatType.CV_8UC3, Scalar.Black);
            capture = new VideoCapture();
            bFrame = new byte[height * width * 3];  //61440*90
            Array.Fill(bFrame, frame.At<Vec3b>(0, 0).Item2);
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
                DefferenceImageCal();
                
                tcp.Send(bFrame);
            }
        }
        void DefferenceImageCal()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    bFrame[3 * (width * y + x)] -= frame.At<Vec3b>(y, x).Item2;
                    bFrame[3 * (width * y + x) + 1] -= frame.At<Vec3b>(y, x).Item1;
                    bFrame[3 * (width * y + x) + 2] -= frame.At<Vec3b>(y, x).Item0;
                }
                for (int x = 0; x < width / 2; x++)
                {
                    bFrame[3 * (width * y + width / 2 + x)] -= frame.At<Vec3b>(y, x + width / 2).Item2;
                    bFrame[3 * (width * y + width / 2 + x) + 1] -= frame.At<Vec3b>(y, x + width / 2).Item1;
                    bFrame[3 * (width * y + width / 2 + x) + 2] -= frame.At<Vec3b>(y, x + width / 2).Item0;
                }
            }
        }
        public void Exit()
        {
            tcp.Exit();
        }
    }
}
