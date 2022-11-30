using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class Theta_fast : MonoBehaviour
{
    [SerializeField]
    public string remoteIPAddress = "192.168.1.200";
    public int redPort = 2003;
    public int greenPort = 2004;
    public int bluePort = 2005;
    Thread thetaThr;
    static TcpC thetaStream;

    static int width = 1920;
    static int height = 960;
    static Color32[] colorArray;
    Texture2D texture;

    static byte[] bufferR;
    static byte[] bufferG;
    static byte[] bufferB;
    static TcpC redStream;
    static TcpC greenStream;
    static TcpC blueStream;
    Task<byte[]> taskR;
    Task<byte[]> taskG;
    Task<byte[]> taskB;

    // Start is called before the first frame update
    void Start()
    {
        colorArray = new Color32[width * height];
        texture = new Texture2D(width, height);
        bufferR = new byte[width * height];
        bufferG = new byte[width * height];
        bufferB = new byte[width * height];

        redStream = new TcpC(remoteIPAddress, redPort, bufferR.Length);
        greenStream = new TcpC(remoteIPAddress, greenPort, bufferG.Length);
        blueStream = new TcpC(remoteIPAddress, bluePort, bufferB.Length);
        thetaThr = new Thread(new ThreadStart(ThetaReceive));
        thetaThr.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (colorArray != null)
        {
            texture.SetPixels32(colorArray);
            GetComponent<Renderer>().material.mainTexture = texture;
            texture.Apply();
        }
        else Debug.Log("colorArray is null");
    }

    void OnApplicationQuit()
    {
        thetaThr.Abort();
        thetaStream.Exit();
    }

    private async void ThetaReceive()
    {
        while (true)
        {
            taskR = Task.Run(() => redStream.Receive());
            taskG = Task.Run(() => greenStream.Receive());
            taskB = Task.Run(() => blueStream.Receive());
            bufferR = taskR.Result;
            bufferG = taskG.Result;
            bufferB = taskB.Result;
            Color32 color = new Color32(0, 0, 0, 0);
            for (int n = 0; n < width * height; n++)
            {
                color.r = bufferR[n];
                color.g = bufferG[n];
                color.b = bufferB[n];
                colorArray[n] = color;
            }
        }
    }
}
