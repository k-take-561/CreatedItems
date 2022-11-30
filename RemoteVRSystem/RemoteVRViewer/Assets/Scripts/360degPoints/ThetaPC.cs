using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Threading;
using System;
using System.Linq;

namespace degPoint
{
    public class ThetaPC : MonoBehaviour
    {
        public float radias = 1f;
        public string remoteIPAddress = "192.168.1.200";
        public int thetaPort = 2003;
        Thread thetaThr;
        static TcpC thetaStream;
        static byte[] thetaBuffer;
        public int width = 1920;
        public int height = 960;
        static Color[] colorArray;
        public String theta_line = "";

        Vector3[] vec;
        int[] indecies;
        Color[] colors;
        Mesh mesh;
        // Start is called before the first frame update
        void Start()
        {
            thetaBuffer = new byte[width * height * 3];
            colorArray = new Color[width * height];
            thetaStream = new TcpC(remoteIPAddress, thetaPort, thetaBuffer.Length);
            thetaThr = new Thread(new ThreadStart(ThetaReceive));
            thetaThr.Start();
            vec = new Vector3[width * height];

            CreateMesh();
            //mesh.SetVertices(PointsToSphere(radias,vec));
            mesh.SetVertices(Polar(radias, vec));
        }
        void CreateMesh()
        {
            indecies = Enumerable.Range(0, width * height).ToArray();
            colors = new Color[width * height];
            colors = colors.Select(x => x = new Color(1.0f, 1.0f, 1.0f)).ToArray();
            if (mesh != null)
                mesh.Clear();
            else
                mesh = new Mesh()
                {
                    indexFormat = IndexFormat.UInt32,
                };
            mesh.vertices = vec;
            mesh.SetIndices(indecies, MeshTopology.Points, 0, false);
            mesh.colors = colors;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        void OnApplicationQuit()
        {
            thetaThr.Abort();
            thetaStream.Exit();
        }


        // Update is called once per frame
        void Update()
        {
            mesh.SetColors(Binarization(colors));
            mesh.UploadMeshData(false);
        }
        private void ThetaReceive()
        {
            while (true)
            {
                thetaBuffer = thetaStream.Receive();
                Color color = new Color(0, 0, 0);
                for (int n = 0; n < width * height; n++)
                {
                    color.r = thetaBuffer[3 * n + 0] / 255f;
                    color.g = thetaBuffer[3 * n + 1] / 255f;
                    color.b = thetaBuffer[3 * n + 2] / 255f;
                    colorArray[n] = color;
                }
                colors = colorArray;
                //theta_line = "";
                //for(int m = 0; m < width; m++)
                //{
                //    theta_line += colorArray[m + width * height / 2].r.ToString();
                //    theta_line += colorArray[m + width * height / 2].g.ToString();
                //    theta_line += colorArray[m + width * height / 2].b.ToString();
                //}
                //Debug.Log("theta_line:" + theta_line);
            }
        }
        private Vector3[] PointsToSphere(float r, Vector3[] v)
        {
            float x;
            float y;
            float z = 0f;
            float ry;
            for (int n = 0; n < height; n++)
            {
                y = (float)r * (1 - n / 480f);
                ry = (float)Math.Sqrt(Math.Pow(r, 2) - Math.Pow(y, 2));
                for (int m = 0; m < width; m++)
                {
                    x = (float)ry * (-1 + Math.Abs(960 - m) / 480f);
                    if (m < 960) { z = (float)Math.Sqrt(Math.Pow(ry, 2) - Math.Pow(x, 2)); }
                    else if (m >= 960) { z = -1f * (float)Math.Sqrt(Math.Pow(ry, 2) - Math.Pow(x, 2)); }
                    v[m + width * n] = new Vector3(x, y, z);
                }
            }
            return v;
        }
        private Vector3[] Polar(float r, Vector3[] v)
        {
            float polar;
            float angle;
            float x;
            float y;
            float z;
            for (int n = 0; n < height; n++)
            {
                polar = ((float)Math.PI / 2f) * (480f - n) / 480f;
                y = r * (float)Math.Sin(polar);
                for (int m = 0; m < width; m++)
                {
                    angle = (2 * (float)Math.PI) * m / 1920f;
                    x = r * (float)Math.Cos(polar) * (float)Math.Cos(angle);
                    z = r * (float)Math.Cos(polar) * (float)Math.Sin(angle);
                    v[m + width * n].Set(x, y, z);
                    //v[m + width * n] = new Vector3(x, y, z);
                }
            }
            return v;
        }
        Color[] Binarization(Color[] array)
        {
            for (int n = 0; n < array.Length; n++)
            {
                if (array[n].r > 0.5f) array[n].r = 1;
                else array[n].r = 0;
                if (array[n].g > 0.5f) array[n].g = 1;
                else array[n].g = 0;
                if (array[n].b > 0.5f) array[n].b = 1;
                else array[n].b = 0;
            }
            return array;
        }

    }
}

