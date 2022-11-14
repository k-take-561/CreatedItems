using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    //[SerializeField]
    Rigidbody2D rigid;
    [SerializeField]
    float speed = 10f;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = transform.up * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
