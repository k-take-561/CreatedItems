using UnityEngine;
using System.Threading;

public class Theta180deg : MonoBehaviour
{
    public string remoteIPAddress = "192.168.1.200";
    public int thetaPort = 2003;
    Thread thetaThr;
    static TcpC thetaStream;

    static byte[] thetaBuffer;
    static int width = 960;
    static int height = 960;
    static Color32[] colorArray;
    Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
        thetaBuffer = new byte[width * height * 3];
        colorArray = new Color32[width * height];
        texture = new Texture2D(width, height);

        thetaStream = new TcpC(remoteIPAddress, thetaPort, thetaBuffer.Length);
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

    private void ThetaReceive()
    {
        while (true)
        {
            thetaBuffer = thetaStream.Receive();
            Color32 color = new Color32(0, 0, 0, 0);
            for (int n = 0; n < width * height; n++)
            {
                color.r = thetaBuffer[3 * n + 0];
                color.g = thetaBuffer[3 * n + 1];
                color.b = thetaBuffer[3 * n + 2];
                colorArray[n] = color;
            }
        }
    }
}