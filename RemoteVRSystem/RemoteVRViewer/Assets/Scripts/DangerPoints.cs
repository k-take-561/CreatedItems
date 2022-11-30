using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Threading;
using System;
using System.Linq;

namespace degPoint
{
    public class DangerPoints : MonoBehaviour
    {
        public string remoteIPAddress = "192.168.1.200";
        public int controlPort = 2000;
        public int pcPort = 2001;
        public int rgbPort = 2002;
        Thread controlThr;
        Thread pcThr;
        Thread rgbThr;
        static TcpC controlStream;
        static TcpC pcStream;
        static TcpC rgbStream;

        static byte[] extrinsicsBuffer;
        static byte[] depthBuffer;
        static byte[] colorBuffer;
        static float[] ex;
        static ushort[] rUDepth;
        public int width = 640;
        public int height = 480;

        public Vector3[] vec;
        int[] indecies;
        Color[] colors;
        Mesh mesh;
        Vector3[] o;

        Pipeline pipe;
        Config cfg;
        static DepthFrame depthframe;
        PointCloud pc;
        static bool flg = false;

        float uh;
        float uhMax;
        [SerializeField] float K = 0.5f;
        [SerializeField] float C = 0.0f;
        [SerializeField] float D = 0.3f;
        [SerializeField] float MaxDepth = 15.0f;
        [SerializeField, Range(1, 7)] int ususa = 3;
        [SerializeField, Range(0, 1)] float iro = 0.3f;
        [SerializeField, Range(-1f, 1f)] float cut_height = 0f; //研究室の机上：-0.808

        // Start is called before the first frame update
        void Start()
        {
            rUDepth = new ushort[width * height];
            depthBuffer = new byte[width * height * 2];
            colorBuffer = new byte[width * height * 3];
            extrinsicsBuffer = new byte[48];
            ex = new float[12];
            o = new Vector3[width * height];

            vec = new Vector3[width * height];
            pc = new PointCloud();

            controlStream = new TcpC(remoteIPAddress, controlPort, extrinsicsBuffer.Length);
            controlThr = new Thread(new ThreadStart(Controller));
            controlThr.Start();

            pcStream = new TcpC(remoteIPAddress, pcPort, depthBuffer.Length);
            pcThr = new Thread(new ThreadStart(PointCloudReceive));
            pcThr.Start();

            rgbStream = new TcpC(remoteIPAddress, rgbPort, colorBuffer.Length);
            rgbThr = new Thread(new ThreadStart(RGBReceive));
            rgbThr.Start();

            pipe = new Pipeline();
            cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            cfg.EnableStream(Stream.Color, width, height);
            pipe.Start(cfg);
            depthframe = pipe.WaitForFrames().DepthFrame;

            CreateMesh();
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
            pcThr.Abort();
            rgbThr.Abort();
            controlThr.Abort();
            pcStream.Exit();
            rgbStream.Exit();
            controlStream.Exit();
            pipe.Stop();
        }
        private static void Controller()
        {
            while (BitConverter.ToSingle(extrinsicsBuffer, 0) == 0)
            {
                extrinsicsBuffer = controlStream.ReceiveEx();
            }
            //Debug.Log(BitConverter.ToString(extrinsicsBuffer));
            for (int n = 0; n < ex.Length; n++)
            {
                ex[n] = BitConverter.ToSingle(extrinsicsBuffer, 4 * n);
                //Debug.Log(n + ":" + ex[n]);
            }
        }
        void PointCloudReceive()
        {
            while (true)
            {
                depthBuffer = pcStream.Receive();
                for (int n = 0; n < width * height; n++)
                {
                    rUDepth[n] = BitConverter.ToUInt16(depthBuffer, 2 * n);
                }
                if (depthframe != null) depthframe.CopyFrom<ushort>(rUDepth);// else Debug.Log("depthframe is null");
                if (flg == false) { flg = true; /*Debug.Log("flg is true");*/ }
            }
        }
        private static void RGBReceive()
        {
            while (true)
            {
                colorBuffer = rgbStream.Receive();
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (flg == true)
            {
                using (var points = pc.Process(depthframe)) //depthframeが空の状態で呼び出されるとクラッシュ
                using (var point = points.As<Points>())
                {
                    if (point.Count == mesh.vertexCount)
                    {
                        point.CopyVertices(vec);
                        ColorizerCBF(vec);
                        mesh.SetVertices(Correct(vec));
                        //mesh.SetColors(colors);
                        mesh.UploadMeshData(false);
                        //MatchingTest();
                    }
                }
            }
        }
        Vector3[] Correct(Vector3[] i)
        {
            return Usui(FloorCut(i));
        }

        void RGBMaker()
        {
            for (int n = 0; n < width * height; n++)
            {
                colors[n].r = colorBuffer[n * 3] / 255f;
                colors[n].g = colorBuffer[n * 3 + 1] / 255f;
                colors[n].b = colorBuffer[n * 3 + 2] / 255f;
            }
        }
        Vector3[] DepthToColor(Vector3[] i)
        {
            for (int n = 0; n < i.Length; n++)
            {
                o[n].x = ex[0] * i[n].x + ex[3] * i[n].y + ex[6] * i[n].z + ex[9];
                o[n].y = ex[1] * i[n].x + ex[4] * i[n].y + ex[7] * i[n].z + ex[10];
                o[n].z = ex[2] * i[n].x + ex[5] * i[n].y + ex[8] * i[n].z + ex[11];
            }
            Debug.Log("x:" + i[0].x + " to " + o[0].x);
            Debug.Log("y:" + i[0].y + " to " + o[0].y);
            Debug.Log("z:" + i[0].z + " to " + o[0].z);
            return o;
        }
        Vector3[] Transform(Vector3[] p)
        {
            float alpha;
            float depth;
            for (int n = 0; n < p.Length; n++)
            {
                depth = p[n].z;
                alpha = (float)(1 / Math.Sqrt(Mathf.Pow(p[n].x, 2f) + Mathf.Pow(p[n].y, 2) + Mathf.Pow(p[n].z, 2)));
                p[n].x *= alpha;
                p[n].y *= alpha;
                p[n].z *= alpha;
                //p[n] = ThreeDizeFirst(p[n], depth);
            }
            return p;
        }
        Vector3 ThreeDizeFirst(Vector3 p, float d) // |p'| = d, r = 1
        {
            float alpha;
            float r = 1;

            alpha = (float)d / r;
            p.x *= alpha;
            p.y *= alpha;
            p.z *= alpha;
            return p;
        }

        void MatchingTest()
        {
            String rs_line = "";
            for (int n = 0; n < height; n++)
            {
                rs_line = "";
                for (int m = 0; m < width; m++)
                {
                    rs_line += colors[m].r.ToString();
                    rs_line += colors[m].g.ToString();
                    rs_line += colors[m].b.ToString();
                    //rs_line[3 * m] = colors[m].r;
                    //rs_line[3 * m + 1] = colors[m].g;
                    //rs_line[3 * m + 2] = colors[m].b;
                }
                Debug.Log("rs_line:" + rs_line);
            }
        }
        void ColorizerCBF(Vector3[] v)
        {
            for (int n = 0; n < width * height; n++)    //点の数:n回ループ
            {
                //電動車いすをモデルとして使用した．今回入力100以上は危険であると判断している．
                //しかしHSVカラーリングの緑の値が150であるため1.5倍してカラーリングに合わしている．
                //今回uhMaxの値を360.0fにしているのはHSVカラーリングの値の最大値が360であり，このプログラムでは0以上1以下の値でカラーリングしているため
                //360の値で割っている．
                uh = 3 * (-80 * K * (D - v[n].z) + 80 * C * (D - v[n].z) * (D - v[n].z)) / 2;
                if (uh >= 150) uh = 150;
                uhMax = 360.0f;
                HSVtoRGB(uh / uhMax, 1.0f, 1.0f, 1.0f, out colors[n].r, out colors[n].g, out colors[n].b, out colors[n].a);
                //****************
                if (colors[n].r < iro && colors[n].b < iro) vec[n] = Vector3.zero;
                //****************
            }
            mesh.SetColors(colors);         //色配列をメッシュにセット
            Thread.Sleep(10);
        }

        void HSVtoRGB(float h, float s, float v, float _a, out float r, out float g, out float b, out float a)
        {
            if (_a > 1.0) _a = 1.0f;
            r = v;
            g = v;
            b = v;
            a = _a;
            if (0.0f < s)
            {
                h *= 6.0f;
                int i = (int)h;
                float f = h - (float)i;
                switch (i)
                {
                    default:
                    case 0:
                        g *= 1 - s * (1 - f);
                        b *= 1 - s;
                        break;
                    case 1:
                        r *= 1 - s * f;
                        b *= 1 - s;
                        break;
                    case 2:
                        r *= 1 - s;
                        b *= 1 - s * (1 - f);
                        break;
                    case 3:
                        r *= 1 - s;
                        g *= 1 - s * f;
                        break;
                    case 4:
                        r *= 1 - s * (1 - f);
                        g *= 1 - s;
                        break;
                    case 5:
                        g *= 1 - s;
                        b *= 1 - s * f;
                        break;
                }
            }
        }
        Vector3[] Usui(Vector3[] i)
        {
            Vector3[] o = new Vector3[i.Length];
            for (int n = 0; n < i.Length; n++)
            {
                if (n % ususa == 0) o[n] = i[n];
                else o[n] = Vector3.zero;
            }
            return o;
        }

        Vector3[] FloorCut(Vector3[] i)
        {
            for (int n = 0; n < i.Length; n++)
            {
                if (-i[n].y < cut_height) i[n] = Vector3.zero;
            }
            return i;
        }
    }
}

