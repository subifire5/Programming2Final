using System.Collections;
using System.Collections.Generic;
using System;
using Unity;
using UnityEngine.AI;
using UnityEngine;
using System.Diagnostics;

public class HiveScript : MonoBehaviour
{
    public bool Aware = false;
    public float lookDist;
    public double FOV;
    public float attackDist;
    public float speed;
    public float damping;
    public float damage;
    public float enemyHealth;
    public float bulletImpulse = 100f;
    public int timeBetweenAttacks;
    public GameObject Bullet_Prefab;
    bool alertMoving = false;
    GameObject[] HiveMem;
    GameObject HiveControl;
    GameObject cam;
    GameObject player;
    GameObject enemyShooting;
    GameObject Map;
    GameObject room;
    GameObject firstRoom;
    GameObject home;
    Transform MapTransform;
    Transform myTransform;
    Transform playerTra;
    String homeSpot;
    String currDest;
    public String liveRoom;
    int register;
    PlayerMovement playerScript;
    public NavMeshAgent myAgent;
    float playerDist;
    public Rigidbody theRigidbody;
    public Renderer myRender;
    public Light myLight;
    public String designation;
    public Queue<string> route;
    public bool looking = false;
    Stopwatch timer = new Stopwatch();
    Stopwatch shotTimer = new Stopwatch();
    public Stopwatch lookTimer = new Stopwatch();
    LineRenderer[] viewLines = new LineRenderer[5];
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
        myAgent = GetComponent<NavMeshAgent>();
        myAgent.speed = speed;
        // then it finds some objects in the world, and gets what it needs from them.
        player = GameObject.Find("Player");
        cam = GameObject.Find("Camera");
        playerTra = player.GetComponent<Transform>();
        playerScript = player.GetComponent<PlayerMovement>();
        viewLines = GetComponentsInChildren<LineRenderer>();
        HiveControl = GameObject.Find("HiveControl");
        AIScript = HiveControl.GetComponent<AIScript>();
        Map = GameObject.Find("TestDemo3");
        MapTransform = Map.GetComponent<Transform>();

        shotTimer.Start();
        register = AIScript.hiveMembers.Count;
        AIScript.hiveMembers.Add(register, this.gameObject);
        AIScript.enemyNum = register;
    }

    // Update is called once per frame
    void Update()
    {
        if (AIScript.hiveSpotted)
        {
            Aware = viewCheck();

            if (Aware)
            {
                AIScript.SetHiveSpotted(room);
                attack();
            }
            else
            {
                // this is a behavioral tree
                // if the hive was recently alerted to the presence of the player
                // then move to the room where that occured
                // this also auto updates, so  if the enemy spots the player, all other hives will move to
                // the room most recently spotted
                if (AIScript.alerted)
                {
                    if (liveRoom == AIScript.roomStr)
                    {
                        currDest = null;
                    }
                    else if (currDest != AIScript.roomStr)
                    {
                        // if not, it starts the agent on the path
                        currDest = AIScript.roomStr;
                        myAgent.destination = AIScript.PatrolNodes[currDest].GetComponent<Transform>().position;
                    }
                    else if (!myAgent.pathPending && myAgent.remainingDistance <= 0.5f)
                    {
                        // checks if the agent has finished the path

                        look();
                    }
                    // if alerted, but not aware, go where the player was last seen
                }
                else if (AIScript.searching)
                {
                    if (currDest != designation)
                    {
                        // if not, it starts the agent on the path
                        currDest = designation;
                        myAgent.destination = AIScript.PatrolNodes[currDest].GetComponent<Transform>().position;
                    }
                    else if (looking)
                    {
                        // checks if the agent has finished the path

                        look();
                    }
                    // if searching, sweep the area (this is the most complicated bit)
                    // the searching isn't controlled in this code
                    // rather, the HiveControl sets up a list of destinations for the enemy to go to
                    // then the enemy looks around
                    // and then moves to the next point
                    // if the enemy dies, they remove themselves from the list of enemies to be updated
                }
                else
                {
                    // if not doing any of those things, return to your position
                    if (currDest != homeSpot)
                    {
                        // if not, it starts the agent on the path
                        currDest = AIScript.roomStr;
                        myAgent.destination = AIScript.PatrolNodes[homeSpot].GetComponent<Transform>().position;
                    }
                    else if (!myAgent.pathPending && myAgent.remainingDistance <= 0.5f)
                    {
                        // checks if the agent has finished the path

                        look();
                    }
                }
                myRender.material.color = Color.blue;
            }
        }
        else
        {
            // checks if there is line of sight
            Aware = viewCheck();

            if (Aware)
            {
                // this is when the enemy has seen the player
                // but doesn't know where they are.
                AIScript.SetHiveSpotted(room);
                attack();
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
            shotTimer.Restart();
        }
    }
    void look()
    {
        myRender.material.color = Color.yellow;
        myLight.color = Color.yellow;
        // this rotates the enemy by increments of 45 degrees
        transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);

    }
    bool viewCheck()
    {// this checks if the enemy can see the player
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
                AIScript.hiveMembers.Remove(register);

                Destroy(gameObject);

            }
            Aware = true;
        }
        else if (collision.gameObject.name == "Player")
        {
            playerScript.TakeDamage(damage);
            UnityEngine.Debug.Log("attack successful");
        }
        else if (collision.gameObject.tag == "QuietPlane")
        {
            room = collision.gameObject;
            liveRoom = room.name[0] + "" + room.name[5] + "S1";
            if (Aware)
            {
                AIScript.currentRoom = collision.gameObject;
                UnityEngine.Debug.Log(collision.gameObject.name);
            }
            if (firstRoom == null)
            {

                firstRoom = collision.gameObject;
                char[] arr = firstRoom.name.ToCharArray();
                UnityEngine.Debug.Log(arr[0]);
                homeSpot = arr[0] + "" + arr[5] + "S1";
                UnityEngine.Debug.Log(homeSpot);
                home = AIScript.PatrolNodes[homeSpot];
            }

        }

    }
}
