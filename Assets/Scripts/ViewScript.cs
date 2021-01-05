using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewScript : MonoBehaviour
{
    public GameObject Bullet_Prefab;
    public float mouseSens;
    public float upDownRange;
    public float altRotUD = 0;
    public float bulletImpulse = 100f;

    float rotUD = 0;

    CharacterController Character;
    GameObject player;
    GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        Character = player.GetComponent<CharacterController>();
        cam = GameObject.Find("Camera");
        
    }

    // Update is called once per frame
    void Update()
    {
        Rotation();
        if (Input.GetMouseButtonDown(0))
        {
            GameObject The_Bullet = (GameObject)Instantiate(Bullet_Prefab, cam.transform.position + cam.transform.forward, cam.transform.rotation);


            The_Bullet.GetComponent<Rigidbody>().AddForce(cam.transform.forward * bulletImpulse, ForceMode.Impulse);
        }
    }
    void Rotation()
    {

        // rotation

        float rotLR = Input.GetAxis("Mouse X") * mouseSens*100;
        //rotates, but is clamped based on how far up or down a head could turn.
        Vector3 rotate = new Vector3(altRotUD, rotLR, 0) * Time.deltaTime;
        Character.transform.Rotate(rotate);



        rotUD -= Input.GetAxis("Mouse Y") * mouseSens;
        rotUD = Mathf.Clamp(rotUD, -upDownRange, upDownRange);
        cam.transform.localRotation = Quaternion.Euler(rotUD, 0, 0);

    }
}
