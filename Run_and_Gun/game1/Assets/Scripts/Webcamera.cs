using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcamera : MonoBehaviour
{
    GameObject image_obj;
    RawImage img;
    WebCamTexture webcam;
    WebCamDevice device;
    Color32[] color;
    Color32[] output;
    byte average;
    Texture2D texture;

    bool shotflg = false;
    GameObject player;
    [SerializeField]
    GameObject bullet = null;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        image_obj = GameObject.Find("Wipe");
        img = image_obj.GetComponent<RawImage>();
        if(WebCamTexture.devices.Length != 0)
        {
            device = WebCamTexture.devices[0];
            webcam = new WebCamTexture(device.name, 160, 120);
            //img.texture = webcam;
            webcam.Play();
        }
        color = new Color32[webcam.width * webcam.height];
        output = new Color32[webcam.width * webcam.height];
        texture = new Texture2D(webcam.width, webcam.height);
        img.texture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        webcam.GetPixels32(color);
        for (int n = 0; n < color.Length; n++)
        {
            average = (byte)((color[n].r + color[n].g + color[n].b) / 3);
            output[n] = new Color32(average, average, average, 255);
        }
        texture.SetPixels32(output);
        texture.Apply(false);
        ShotCheck(output);
    }
    void ShotCheck(Color32[] cols)
    {
        int csum = 0;
        for(int n = 0; n < cols.Length; n++)
        {
            csum += cols[n].r;
        }
        int cave = csum / cols.Length;
        int count = 0;
        for (int n = 0; n < cols.Length; n++)
        {
            if(cols[n].r < cave + 5 && cols[n].r > cave - 5)
            {
                count++;
            }
        }
        if (count == cols.Length) return;
        //Debug.Log(count);
        //Debug.Log((float)count / (float)cols.Length);
        if((float)count / cols.Length > 0.15 && shotflg == false)
        {
            Instantiate(bullet, player.transform.position, new Quaternion(90, 90, 0, 1));//’e”­ŽË
            shotflg = true;
        }else if((float)count / cols.Length < 0.1 && shotflg == true)
        {
            shotflg = false;
        }
    }
}
