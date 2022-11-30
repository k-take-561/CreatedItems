using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TextureView_udp : MonoBehaviour
{
    static int width = 640;
    static int height = 480;
    static Color32[] colorArray;
    Texture2D texture;
    Udp udp_r;
    Udp udp_g;
    Udp udp_b;
    byte[] buffur_r;
    byte[] buffur_g;
    byte[] buffur_b;
    List<Task> tasks;
    Task receive_r;
    Task receive_g;
    Task receive_b;

    public string localIPAddress = "192.168.1.29";
    public string remoteIPAddress = "192.168.1.202";
    public int startPort = 2001;

    // Start is called before the first frame update
    async Task Start()
    {
        colorArray = new Color32[width * height];
        texture = new Texture2D(width, height);
        udp_r = new Udp(localIPAddress, remoteIPAddress, startPort);
        udp_g = new Udp(localIPAddress, remoteIPAddress, startPort + 1);
        udp_b = new Udp(localIPAddress, remoteIPAddress, startPort + 2);
        buffur_r = new byte[width * height];
        buffur_g = new byte[width * height];
        buffur_b = new byte[width * height];

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
    async Task TaskInitialize()
    {
        tasks = new List<Task>();
        receive_r = new Task(() =>
        {
            buffur_r = udp_r.Receive();
        });
        receive_g = new Task(() =>
        {
            buffur_g = udp_g.Receive();
        });
        receive_b = new Task(() =>
        {
            buffur_b = udp_b.Receive();
        });
        tasks.Add(receive_r);
        tasks.Add(receive_g);
        tasks.Add(receive_b);
        receive_r.Start();
        receive_g.Start();
        receive_b.Start();

    }
    private async Task ThetaReceive()
    {
        while (true)
        {
            buffur_r = udp_r.Receive();
            buffur_g = udp_g.Receive();
            buffur_b = udp_b.Receive();
            Color32 color = new Color32(0, 0, 0, 0);
            for (int n = 0; n < width * height; n++)
            {
                color.r = buffur_r[n];
                color.g = buffur_g[n];
                color.b = buffur_b[n];
                colorArray[n] = color;
            }
        }
    }

}
