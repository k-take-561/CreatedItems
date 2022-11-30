using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using degPoint;

public class PixelMatchingTest : MonoBehaviour
{
    public GameObject rs;
    public GameObject theta;
    string rs_line = "";
    string theta_line = "";

    // Start is called before the first frame update
    //void Start()
    //{
        
    //}
    //// Update is called once per frame
    //void Update()
    //{
    //    if(flg)  Initialize();
    //}
    public void OnClick()
    {
        Initialize();
    }
    void Initialize()
    {
        rs = GameObject.Find("PointCloud");
        theta = GameObject.Find("ThetaView");
        //ThetaColorStr();
        //PCColorString();
        //VertexMatch();
        //RayTest();
        BinarizedColorMatch();
    }
    void ThetaColorStr()
    {
        ThetaPC theta_scr = theta.GetComponent<ThetaPC>();
        int w = theta_scr.width;
        int h = theta_scr.height;
        Color[] array = theta.GetComponent<MeshFilter>().sharedMesh.colors;
        for(int m = 1; m < w; m++)
        {
            //theta_line += Math.Round(array[m + w * h / 2].r, 1, MidpointRounding.AwayFromZero).ToString();
            //theta_line += Math.Round(array[m + w * h / 2].g, 1, MidpointRounding.AwayFromZero).ToString();
            //theta_line += Math.Round(array[m + w * h / 2].b, 1, MidpointRounding.AwayFromZero).ToString();
            theta_line += Math.Round(array[m + w * h / 2].r - array[m - 1 + w * h / 2].r, 2, MidpointRounding.AwayFromZero).ToString();
            theta_line += Math.Round(array[m + w * h / 2].g - array[m - 1 + w * h / 2].g, 2, MidpointRounding.AwayFromZero).ToString();
            theta_line += Math.Round(array[m + w * h / 2].b - array[m - 1 + w * h / 2].b, 2, MidpointRounding.AwayFromZero).ToString();
            
        }
        Debug.Log("theta_line:" + theta_line);
    }
    void PCColorString()
    {
        SphericalTrans rs_scr = rs.GetComponent<SphericalTrans>();
        int w = rs_scr.width;
        int h = rs_scr.height;
        Color[] array = rs.GetComponent<MeshFilter>().sharedMesh.colors;
        int match = 0;
        for (int n = 0; n < h; n++)
        {
            rs_line = "";
            for (int m = 1; m < w; m++)
            {
                //rs_line += Math.Round(array[m + w * n].r, 1, MidpointRounding.AwayFromZero).ToString();
                //rs_line += Math.Round(array[m + w * n].g, 1, MidpointRounding.AwayFromZero).ToString();
                //rs_line += Math.Round(array[m + w * n].b, 1, MidpointRounding.AwayFromZero).ToString();
                rs_line += Math.Round(array[m + w * n].r - array[m - 1 + w * n].r, 2, MidpointRounding.AwayFromZero).ToString();
                rs_line += Math.Round(array[m + w * n].g - array[m - 1 + w * n].g, 2, MidpointRounding.AwayFromZero).ToString();
                rs_line += Math.Round(array[m + w * n].b - array[m - 1 + w * n].b, 2, MidpointRounding.AwayFromZero).ToString();

                if ((m+1) % (w/8) == 0)
                {
                    match = theta_line.IndexOf(rs_line, 0, theta_line.Length);
                    if (match != -1) { Debug.Log(n + " : " + match); Debug.Log("rs_line:" + rs_line); }
                    rs_line = "";
                }
            }
        }
    }
    void VertexMatch()
    {
        SphericalTrans rs_scr = rs.GetComponent<SphericalTrans>();
        Vector3[] rs_vec = rs.GetComponent<MeshFilter>().sharedMesh.vertices;
        Vector3[] theta_vec = theta.GetComponent<MeshFilter>().sharedMesh.vertices;
        int rs_width = rs_scr.width;
        int rs_height = rs_scr.height;
        int startNum = 0;
        int match = 0;
        while (Double.IsNaN(rs_vec[startNum].x))
        {
            startNum++;
        }
        Debug.Log(startNum + ":" + rs_vec[startNum]);
        for (int l = 0; l < 1920 * 960; l++)
        {
            if (Math.Round(theta_vec[l].x - rs_vec[startNum].x, 1, MidpointRounding.AwayFromZero) == 0 && Math.Round(theta_vec[l].y - rs_vec[startNum].y, 1, MidpointRounding.AwayFromZero) == 0 && Math.Round(theta_vec[l].z - rs_vec[startNum].z, 1, MidpointRounding.AwayFromZero) == 0)
            {
                //Debug.Log(l + ":" + theta_vec[l]);
                match = l;
                Debug.Log(match + ":" + theta_vec[l]);
                break;
            }
        }
        for(int n = 0; n < rs_height; n++)
        {
            for(int m = 0; m < rs_width; m++)
            {
                if(match + m + 1920 * n <= 1920*960) theta_vec[match + m + 1920 * n] = rs_scr.vec[m + n * rs_width];
            }
        }
        
    }
    void RayTest()
    {
        Vector3[] theta_vec = theta.GetComponent<MeshFilter>().sharedMesh.vertices;

        // Debug.DrawRay(Vector3.zero, Vector3.up * 30, Color.red, 5.0f);
        Ray ray = new Ray(Vector3.zero, Vector3.up);
        RaycastHit hit;
        int flg = 0;
        for(int r = 0; r < theta_vec.Length; r++)
        {
            ray.direction = theta_vec[r];
            Debug.DrawRay(ray.origin, ray.direction * 2, Color.red);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(r + ":" + hit.collider.gameObject.name);
                flg++;
            }
            if (flg > 10) break;
        }
    }
    void BinarizedColorMatch()
    {
        ThetaPC theta_scr = theta.GetComponent<ThetaPC>();
        int w_theta = theta_scr.width;
        int h_theta = theta_scr.height;
        Color[] array_theta = theta.GetComponent<MeshFilter>().sharedMesh.colors;
        SphericalTrans rs_scr = rs.GetComponent<SphericalTrans>();
        int w_rs = rs_scr.width;
        int h_rs = rs_scr.height;
        Color[] array_rs = rs.GetComponent<MeshFilter>().sharedMesh.colors;

        int width_sample = 400;
        int height_sample = 309;
        Vector3[] match = new Vector3[(h_theta - height_sample) * (w_theta - width_sample)];

        for(int n =0; n<h_theta - height_sample; n++)
        {
            for (int m = 0; m < w_theta - width_sample; m++)
            {
                match[m + n * (w_theta - width_sample)] = CalMatch(m + n * (w_theta - width_sample), width_sample * height_sample, array_theta, match[m + n * (w_theta - width_sample)]); 
            }
        }
        Debug.Log(match[10000]);
    }
    Vector3 CalMatch(int offset, int length, Color[] c, Vector3 v)
    {
        for(int l = 0; l < length; l++)
        {
            v.x += c[offset + l].r;
            v.y += c[offset + l].g;
            v.z += c[offset + l].b;
        }
        v.x = v.x / length;
        v.y = v.y / length;
        v.z = v.z / length;
        return v;
    }
}
