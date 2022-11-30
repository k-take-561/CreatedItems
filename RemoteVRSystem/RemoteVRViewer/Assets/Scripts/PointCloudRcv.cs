using UnityEngine;
using UnityEngine.Rendering;
using System.Net;
using System.Net.Sockets;
using Intel.RealSense;
using System.Threading;
using System;
using System.Linq;

public class PointCloudRcv : MonoBehaviour
{
    static int remotePote = 2002;
    static byte[] rcvByte;
    static int rcvTimes;
    static byte[] buffer;

    static TcpListener listener;
    static int dataSize;
    static int canSize;
    static bool s;

    static ushort[] rUDepth;
    static int width = 640;
    static int height = 480;

    Vector3[] vec;
    int[] indecies;
    Color[] colors;
    Mesh mesh;

    Pipeline pipe;
    Config cfg;
    static DepthFrame depthframe;
    PointCloud pc;
    static bool flg = false;
    byte[] rgb;

    Thread thread;

    // Start is called before the first frame update
    void Start()
    {
        rcvByte = new byte[61440];
        rcvTimes = width * height * 2 / rcvByte.Length;
        rUDepth = new ushort[width * height];
        buffer = new byte[width * height * 2];

        listener = new TcpListener(IPAddress.Any, remotePote);
        listener.Start();

        vec = new Vector3[width * height];
        pc = new PointCloud();

        thread = new Thread(new ThreadStart(PointCloudReceive));
        thread.Start();

        pipe = new Pipeline();
        cfg = new Config();
        cfg.EnableStream(Stream.Depth, width, height);
        cfg.EnableStream(Stream.Color, width, height);

        pipe.Start(cfg);
        depthframe = pipe.WaitForFrames().DepthFrame;

        rgb = new byte[width * height * 3];
        rgb = ColorRcv.buffer;

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
        thread.Abort();
        listener.Stop();
        pipe.Stop();
    }
    private static void PointCloudReceive()
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
                for (int n = 0; n < width * height; n++)
                {
                    rUDepth[n] = BitConverter.ToUInt16(buffer, 2 * n);
                }
                depthframe.CopyFrom<ushort>(rUDepth);
                if (flg == false) { flg = true; }
            }
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
                    RGBMaker();
                    //mesh.SetVertices(DepthToColor(vec));
                    mesh.SetVertices(vec);
                    mesh.SetColors(colors);
                    mesh.UploadMeshData(false);
                }
            }
        }
    }
    void RGBMaker()
    {
        for (int n = 0; n < width * height; n++)
        {
            colors[n].r = rgb[n * 3] / 255f;
            colors[n].g = rgb[n * 3 + 1] / 255f;
            colors[n].b = rgb[n * 3 + 2] / 255f;
        }
    }
}
