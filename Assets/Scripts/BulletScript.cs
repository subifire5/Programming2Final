using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    Transform BT;
    Stopwatch timer = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        BT = GetComponent<Transform>();
        timer.Start();
        


    }

    // Update is called once per frame
    void Update()
    {
        if (timer.ElapsedMilliseconds >= 250) {
            Destroy(gameObject);
        }
        
    }
}
