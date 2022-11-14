using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rigid;
    GameObject canvas;
    [SerializeField]
    GameObject text = null;
    [SerializeField]
    int hp = 10;
    [SerializeField]
    float speed = 1f;
    [SerializeField, Range(-10,0)]
    int power = 0;
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        rigid = GetComponent<Rigidbody2D>();
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //rigid.AddForce(transform.right * -1 * speed);
        pos.x = pos.x - speed * Mathf.Pow(10f, power);
        transform.position = pos;
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            hp = hp - 3;
            
            Instantiate(text, transform.position, text.transform.rotation).transform.SetParent(canvas.transform,false);
        }
    }
}
