using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    public bool flg;
    GameObject message;
    Text mes;
    void Start()
    {
        flg = false;
        message = GameObject.Find("Message");
        mes = message.GetComponent<Text>();
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            mes.text = "ゴール！\nEscapeキーで終了";
            flg = true;
        }
    }
}
