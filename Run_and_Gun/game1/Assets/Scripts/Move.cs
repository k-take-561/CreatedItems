using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    Rigidbody2D rigid;
    Vector3 pos;
    Vector3 mouse;
    float cursorDist = 0f;
    [SerializeField]
    GameObject bullet = null;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        mouse = Input.mousePosition;
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        cursorDist = Vector3.Distance(mouse, Input.mousePosition);
        mouse = Input.mousePosition;
        rigid.AddForce(transform.right * cursorDist / 10f);
        KeyMove();
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    void KeyMove()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rigid.AddForce(transform.right * -3f);  //ç∂Ç…à⁄ìÆ
        }
        if (Input.GetKey(KeyCode.D))
        {
            rigid.AddForce(transform.right * 10f);    //âEÇ…à⁄ìÆ
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            rigid.AddForce(transform.up * 500f);    //ÉWÉÉÉìÉv
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Instantiate(bullet, transform.position, new Quaternion(90, 90, 0, 1));//íeî≠éÀ
        }

    }
}
