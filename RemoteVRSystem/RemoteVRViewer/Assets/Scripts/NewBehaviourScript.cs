using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputTracking.Recenter();
        //XRInputSubsystem.TryRecenter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
