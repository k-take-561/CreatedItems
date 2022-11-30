using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Linq;
using System;

public class AlignTest : MonoBehaviour
{
    Vector3[] vec;
    int[] indecies;
    Color[] colors;
    Mesh mesh;

    Pipeline pipe;
    Config cfg;
    PointCloud pc;
    int width;
    int height;
    byte[] rgb;

    Align align;

    public bool fix;
    [SerializeField, Range(0, 639)]
    public int offset;
    public bool aligning = true;
    FrameSet f;

    public enum WhatColor { none, colorize, rgb }
    [SerializeField] WhatColor whatcolor = WhatColor.colorize;
    // Start is called before the first frame update
    void Start()
    {
        width = 640;
        height = 480;
        vec = new Vector3[width * height];
        rgb = new byte[width * height * 3];
        pc = new PointCloud();
        offset = 25;
        fix = true;
        align = new Align(Stream.Depth);

        pipe = new Pipeline();
        cfg = new Config();
        cfg.EnableStream(Stream.Depth, width, height);
        cfg.EnableStream(Stream.Color, width, height);
        pipe.Start(cfg);

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

    // Update is called once per frame
    void Update()
    {
        using (var frame = pipe.WaitForFrames())
        using (var alignedFrame = align.Process(frame))
        using (var frameset = alignedFrame.AsFrameSet())
        {
            if (aligning) { f = frameset; } else { f = frame; }
            using (var colorframe = f.ColorFrame)
            {
                colorframe.CopyTo(rgb);
                if (whatcolor == WhatColor.rgb) { if (fix) RGBFixer(offset); else RGBMaker(); }
            }

            using (var depthframe = f.DepthFrame)
            using (var points = pc.Process(depthframe))
            using (var point = points.As<Points>())
            {
                if (point.Count == mesh.vertexCount)
                {
                    point.CopyVertices(vec);
                    if (whatcolor == WhatColor.none)
                    {
                        for (int n = 0; n < width * height; n++)
                        {
                            colors[n].r = 1f;
                            colors[n].g = 1f;
                            colors[n].b = 1f;
                        }
                    }
                    if (whatcolor == WhatColor.colorize) Colorizer(vec);
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
    void RGBFixer(int o)
    {
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                if (w >= o)
                {
                    colors[w + h * width].r = rgb[(w - o + h * width) * 3] / 255f;
                    colors[w + h * width].g = rgb[(w - o + h * width) * 3 + 1] / 255f;
                    colors[w + h * width].b = rgb[(w - o + h * width) * 3 + 2] / 255f;
                }
                else
                {
                    if (colors[w + h * width].r != 0 || colors[w + h * width].g != 0 || colors[w + h * width].b != 0)
                    {
                        colors[w + h * width].r = 1f;
                        colors[w + h * width].g = 1f;
                        colors[w + h * width].b = 1f;
                    }
                }
            }
        }
    }
    void Colorizer(Vector3[] v)
    {
        for (int n = 0; n < width * height; n++)
        {
            //colors[n].r = 1 - v[n].z / 6f;
            //colors[n].g = v[n].z / 6f;
            //colors[n].b = v[n].z / 6f;

            colors[n].r = 1 - (float)Math.Pow(2, v[n].z) / 6f;
            colors[n].g = (float)Math.Pow(2, v[n].z) / 6f;
            colors[n].b = (float)Math.Pow(2, v[n].z) / 6f;
        }
    }
}
