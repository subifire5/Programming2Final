using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Bullet_ThermalDetonator : MonoBehaviour {
    public GameObject fireEffect;
    public double lifespan = 1.0f;
    Stopwatch timer = new Stopwatch();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timer.Start();
        lifespan -= Time.deltaTime;
        if (lifespan <= 0) {
            Explode();
        }
        timer.Stop();
        UnityEngine.Debug.Log("thermal Detonator update took: "+ timer.ElapsedMilliseconds);
        timer.Reset();
	}
    
    void Explode() {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy") {
            collision.gameObject.tag = "Untagged";
            Instantiate(fireEffect, collision.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }
}
