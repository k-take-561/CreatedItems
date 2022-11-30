using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Intel.RealSense;
using System.Linq;
using System;

public class PcOff : MonoBehaviour
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
        {
            using (var frameA = align.Process(frame))
            using (var frameB = frameA.AsFrameSet())
            {
                using (var colorframe = frameB.ColorFrame)
                {
                    colorframe.CopyTo(aaaa.iro);
                }
                using (var depthframe = frameB.DepthFrame)
                using (var points = pc.Process(depthframe))
                using (var point = points.As<Points>())
                {
                    if (point.Count == mesh.vertexCount)
                    {
                        point.CopyVertices(vec);
                        mesh.SetVertices(vec);
                        //mesh.SetColors(colors);
                        mesh.UploadMeshData(false);
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
    Vector3[] Planize(Vector3[] v)
    {
        for(int n = 0; n < v.Length; n++)
        {
            v[n].z = 0;
        }
        return v;
    }
}
