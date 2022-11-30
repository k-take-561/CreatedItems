using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Linq;
using System;

public class DepthToPlain : MonoBehaviour
{
    Pipeline pipe;
    Config cfg;
    Align align;
    int width;
    int height;
    static public byte[] iro;
    Color32 color;
    Color32[] colorArray;
    Texture2D texture;

    public bool aligning = false;
    ushort[] u;
    byte[] b;
    byte[] bb;

    // Start is called before the first frame update
    void Start()
    {
        width = 640;
        height = 480;
        iro = new byte[width * height * 3];
        color = new Color32(0, 0, 0, 0);
        colorArray = new Color32[height * width];
        texture = new Texture2D(width, height);
        u = new ushort[width * height];
        b = new byte[width * height * 2];
        bb = new byte[2];

        align = new Align(Stream.Depth);
        pipe = new Pipeline();
        cfg = new Config();
        cfg.EnableStream(Stream.Depth, width, height);
        cfg.EnableStream(Stream.Color, width, height);
        pipe.Start(cfg);
    }

    // Update is called once per frame
    void Update()
    {
        using (var frame = pipe.WaitForFrames())
        {
            /*if(aligning)*/
            using (var frameA = align.Process(frame))
            using (var frameB = frameA.AsFrameSet())
            {
                using (var colorframe = frameB.ColorFrame)
                {
                    colorframe.CopyTo(aaaa.iro);
                }
                using (var depthframe = frameB.DepthFrame)
                {
                    depthframe.CopyTo(u);
                    for(int n = 0; n < width * height; n++)
                    {
                        bb = BitConverter.GetBytes(u[n]);
                        Array.Copy(bb, 0, b, 2 * n, bb.Length);
                    }
                    for (int n = 0; n < height * width; n++)
                    {
                        color.r = b[2 * n + 0];
                        color.g = b[2 * n + 1];
                        color.b = 0;
                        colorArray[n] = color;
                    }
                    texture.SetPixels32(colorArray);
                    GetComponent<Renderer>().material.mainTexture = texture;
                    texture.Apply();
                }
            }
        }
    }
}
