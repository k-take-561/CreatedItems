using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    [SerializeField]
    GameObject ene = null;
    float next = 0.0f;
    [SerializeField]
    float aida = 5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.deltaTime);
        if(Time.time > next && Time.time <= next + Time.deltaTime)
        {
            next = Time.time + aida;
            Instantiate(ene,ene.transform.position,ene.transform.rotation);
            Debug.Log(Time.time);
        }
    }
}
