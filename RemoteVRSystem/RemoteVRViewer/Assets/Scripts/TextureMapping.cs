using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Threading;
using System;
using System.Linq;

public class TextureMapping : MonoBehaviour
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
    static int width = 640;
    static int height = 480;

    Vector3[] vec;
    int[] indecies;
    Color[] colors;
    Mesh mesh;
    Vector3[] o;

    Pipeline pipe;
    Config cfg;
    static DepthFrame depthframe;
    PointCloud pc;
    static bool flg = false;

    Vector2[] tx;
    Color32 color;
    Color32[] colorArray;
    Texture2D texture;

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

        tx = new Vector2[width * height];
        color = new Color32(0, 0, 0, 0);
        colorArray = new Color32[width * height];
        texture = new Texture2D(width, height);
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
        //GetComponent<MeshRenderer>().material.mainTexture = texture;
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
        for (int n = 0; n < ex.Length; n++)
        {
            ex[n] = BitConverter.ToSingle(extrinsicsBuffer, 4 * n);
            //Debug.Log(n + ":" + ex[n]);
        }
    }
    private static void PointCloudReceive()
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
                    point.CopyTextureCoords(tx);
                    RGBMaker();
                    
                    //mesh.SetVertices(DepthToColor(vec));
                    mesh.SetVertices(vec);
                    //mesh.SetColors(colors);
                    //mesh.SetUVs(0, tx);
                    //mesh.uv = tx;
                    //TexruteMaker();
                    mesh.UploadMeshData(false);
                    
                }
            }
        }
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
        //Debug.Log("x:" + i[0].x + " to " + o[0].x);
        //Debug.Log("y:" + i[0].y + " to " + o[0].y);
        //Debug.Log("z:" + i[0].z + " to " + o[0].z);
        return o;
    }
    Vector3[] RayTest(Vector3[] v)
    {
        RaycastHit hit;
        
        //for(int n = 0; n < v.Length; n++)
        //{
        //    if (Physics.Raycast(Vector3.zero, v[n], out hit))
        //    {
        //        v[n] = hit.collider.gameObject.GetComponent<Mesh>().vertices[n];
        //    }
        //    else
        //    {
        //        Debug.Log(n + " cannot get vertex");
        //    }
        //}



        return v;
    }
    Vector3 VertexSeacher(Vector3[] vv)
    {
        //Align a = new Align()
        return vv[0];
    }
    void TexruteMaker()
    {
        //Color32 color = new Color32(0, 0, 0, 0);
        //Color32[] colorArray = new Color32[width * height];
        //Texture2D texture = new Texture2D(width, height);

        for (int n = 0; n < height * width; n++)
        {
            color.r = colorBuffer[3 * n + 0];
            color.g = colorBuffer[3 * n + 1];
            color.b = colorBuffer[3 * n + 2];
            colorArray[n] = color;
        }
        texture.SetPixels32(colorArray);
        GetComponent<Renderer>().material.mainTexture = texture;
        
        texture.Apply();
    }
}
