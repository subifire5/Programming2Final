using System.Collections;
using System.Collections.Generic;
using System;
using Unity;
using UnityEngine;
using System.Diagnostics;

public class BasicAIScript : MonoBehaviour
{
    public bool Aware = false;
    public float lookDist;
    public double FOV;
    public float speed;
    public float damping;
    public float damage;
    public float enemyHealth;
    public float bulletImpulse = 100f;
    public int timeBetweenAttacks;
    GameObject cam;
    public GameObject Bullet_Prefab;
    GameObject player;
    GameObject enemyShooting;
    GameObject firstRoom;
    GameObject home;
    GameObject HiveControl;
    Transform playerTra;
    PlayerMovement playerScript;
    float playerDist;
    public Rigidbody theRigidbody;
    public Renderer myRender;
    public Light myLight;
    GameObject Map;
    Transform MapTransform;
    Stopwatch timer = new Stopwatch();
    Stopwatch shotTimer = new Stopwatch();
    Transform myTransform;
    LineRenderer[] viewLines = new LineRenderer[5];
    Vector3 playerSpot;
    bool semiAware;
    String homeSpot;
    AIScript AIScript;
    
    // Start is called before the first frame update
    void Start()
    {
        // these lines get the local components
        // the renderer of this specific enemy, the light, etc.
        myRender = GetComponent<Renderer>();
        theRigidbody = GetComponent<Rigidbody>();
        myLight = GetComponentInChildren<Light>();
        myTransform = GetComponent<Transform>();
        // then it finds some objects in the world, and gets what it needs from them.
        player = GameObject.Find("Player");
        cam = GameObject.Find("Camera");
        playerTra = player.GetComponent<Transform>();
        playerScript = player.GetComponent<PlayerMovement>();
        viewLines = GetComponentsInChildren<LineRenderer>();
        Map = GameObject.Find("TestDemo3");
        MapTransform = Map.GetComponent<Transform>();
        HiveControl = GameObject.Find("HiveControl");
        AIScript = HiveControl.GetComponent<AIScript>();
    }

    // Update is called once per frame
    void Update()
    {
        playerSpot = playerTra.position;
        playerSpot.y = -2;

        playerDist = Vector3.Distance(playerTra.position, transform.position);

        Aware = viewCheck();



        if (Aware)
        {
            // this is when the enemy has seen the player
            // but doesn't know where they are.
            semiAware = true;
        }


        if (playerDist <= lookDist)
        {

            // turns enemy to look at player, makes it red, attacks
            if (Aware)
            {
                attack();
            }
            else if (semiAware)
            {
                // spins around looking for the player
                look();
            }
        }

    }
    void attack()
    {
        // changes the color, makes a bullet appear with a position, rotation, and force behind it

        myRender.material.color = Color.red;
        myLight.color = Color.red;
        transform.LookAt(playerTra);
        theRigidbody.velocity += transform.forward * Time.deltaTime * speed;
        if (shotTimer.ElapsedMilliseconds >= 1000)
        {
            GameObject The_Bullet = (GameObject)Instantiate(Bullet_Prefab, transform.position + transform.forward, transform.rotation);
            The_Bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bulletImpulse, ForceMode.Impulse);
        }
    }
    void look()
    {
        myRender.material.color = Color.yellow;
        myLight.color = Color.yellow;
        // this rotates the enemy by increments of 20 degrees
        transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime);
    }
    bool viewCheck()
    {
        // this checks if the enemy can see the player
        // this involves some vector math so buckle up
        // first we find the vector between the player and us
        // to do that we subtract our position from the player's position
        Vector3 heading = playerTra.position - transform.position;
        // next we use Unity to get the vector of us if we were to move forward
        // Then we get the Dot product of the heading and our forward to find the angle
        // we turn that dot product into an angle using inverse cosine
        // if we used sine we would get the axis, not the angle
        // for more reading: http://www.euclideanspace.com/maths/algebra/vectors/vecAlgebra/dot/index.htm
        //http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/index.htm
        //https://docs.unity3d.com/ScriptReference/Vector3.Dot.html
        double angleV = Math.Acos(Vector3.Dot(heading, transform.forward));
        // or alternatively i could've just used unity's vector angle function
        //angleV = Vector3.Angle(heading, transform.forward);
        // check if the angle is smaller than the Field of View 
        bool See = false;
        if (angleV > FOV)
        {
            return See;
        }
        // this creates a point relative to the enemy, then converts that
        // into a point in the world, and makes that the spot where I will begin raycasting.
        Vector3 rayOrigin = myTransform.TransformPoint(new Vector3(0, 0.4f, 0.54f));
        // make an array of vectors pointing towards the player
        // make these vectors by subtracting the origin from points near the player
        List<Vector3> rayDirs = new List<Vector3>();
        for (float j = 0; j <= 0.4f; j += 0.001f)
        {
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, j, j)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, j, 0)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, j, -j)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, -j, -j)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, -j, j)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, -j, 0)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, 0, -j)) - rayOrigin);
            rayDirs.Add(playerTra.TransformPoint(new Vector3(0, 0, j)) - rayOrigin);




        }

        RaycastHit hit = new RaycastHit();
        int i = 0;
        foreach (Vector3 rayDir in rayDirs)
        {
            //casts the ray, and if it collided with the player, it'll say so.
            if (Physics.Raycast(rayOrigin, rayDir, out hit, lookDist))
            {
                // this code caused lasers to appear for the 5 points when sighting the player
                //viewLines[i].SetPosition(0, rayOrigin);
                //viewLines[i].SetPosition(1, hit.point);
                //viewLines[i].enabled = true;
                if (hit.transform == MapTransform)
                {

                }
                else if (hit.transform == playerTra)
                {
                    timer.Start();
                    UnityEngine.Debug.Log("player seen");
                    See = true;
                    break;
                }
            }
            i++;

        }
        return See;
    }
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.name == "Bullet_Prefab(Clone)")
        {
            myRender.material.color = Color.black;
            myLight.color = Color.black;
            enemyHealth -= 10;
            if (enemyHealth <= 0)
            {
                myRender.material.color = Color.cyan;
                myLight.color = Color.cyan;
                Destroy(gameObject);

            }
            Aware = true;
        }
        else if (collision.gameObject.name == "Player")
        {
            playerScript.TakeDamage(damage);
            UnityEngine.Debug.Log("attack successful");
            if (firstRoom == null)
            {
                firstRoom = collision.gameObject;
                homeSpot = firstRoom.name[0] + firstRoom.name[5] + "S1";
                home = AIScript.PatrolNodes[homeSpot];

            }
        }

    }

}

