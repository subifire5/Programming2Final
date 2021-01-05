using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlane : MonoBehaviour
{
    public Renderer myRender;
    public Light myLight;
    public Transform myTransform;
    GameObject[] quietPlanes;
    GameObject closestPlane;
    GameObject HiveControl;
    float distance;
    AIScript AIScript;
    bool Noise;
    // Start is called before the first frame update
    void Start()
    {
        myRender = GetComponent<Renderer>();
        myLight = GetComponentInChildren<Light>();
        HiveControl = GameObject.Find("HiveControl");
        AIScript = HiveControl.GetComponent<AIScript>();
        myTransform = GetComponent<Transform>();
        quietPlanes = GameObject.FindGameObjectsWithTag("QuietPlane");
        distance = Vector3.Distance(quietPlanes[0].transform.position, this.transform.position);
        foreach(GameObject plane in quietPlanes)
        {
            if(Vector3.Distance(plane.transform.position, this.transform.position) < distance)
            {
                distance = Vector3.Distance(plane.transform.position, this.transform.position);
                closestPlane = plane;
            }

        }
    }

    // Update is called once per frame
    void Update()
    { }
    public void makeNoise()
    {
        UnityEngine.Debug.Log(closestPlane);
        myRender.material.color = Color.black;
        myLight.color = Color.black;
        AIScript.SetHiveSpotted(closestPlane);


    }
}
