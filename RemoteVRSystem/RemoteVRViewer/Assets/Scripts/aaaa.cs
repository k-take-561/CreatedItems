using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Linq;
using System;

public class aaaa : MonoBehaviour
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

    Vector3[] vec;
    int[] indecies;
    Color[] colors;
    Mesh mesh;
    PointCloud pc;

    public bool aligning = false;

    // Start is called before the first frame update
    void Start()
    {
        width = 640;
        height = 480;
        iro = new byte[width * height * 3];
        color = new Color32(0, 0, 0, 0);
        colorArray = new Color32[height * width];
        texture = new Texture2D(width, height);
        pc = new PointCloud();
        
        //align = new Align(Stream.Depth);
        //pipe = new Pipeline();
        //cfg = new Config();
        //cfg.EnableStream(Stream.Depth, width, height);
        //cfg.EnableStream(Stream.Color, width, height);
        //pipe.Start(cfg);
    }

    // Update is called once per frame
    void Update()
    {
        //using (var frame = pipe.WaitForFrames())
        //{
        //    /*if(aligning)*/
        //    using (var frameA = align.Process(frame))
        //    using(var frameB = frameA.AsFrameSet())
        //    {
        //        using (var colorframe = frameB.ColorFrame)
        //        {
        //            colorframe.CopyTo(iro);
        //        }
        //    }
        //}
        for (int n = 0; n < height * width; n++)
        {
            color.r = iro[3 * n + 0];
            color.g = iro[3 * n + 1];
            color.b = iro[3 * n + 2];
            colorArray[n] = color;
        }
        texture.SetPixels32(colorArray);
        GetComponent<Renderer>().material.mainTexture = texture;
        texture.Apply();
    }
}
