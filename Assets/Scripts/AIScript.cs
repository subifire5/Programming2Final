using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AIScript : MonoBehaviour
{
    // Start is called before the first frame update
    public bool alerted;
    public bool hiveSpotted;
    public bool searching;
    public Dictionary<int, GameObject> hiveMembers = new Dictionary<int, GameObject>();
    public GameObject currentRoom;
    public string roomStr;
    Stopwatch timer = new Stopwatch();
    Stopwatch alertTimer = new Stopwatch();
    Stopwatch searchTimer = new Stopwatch();
    public Dictionary<string, GameObject> PatrolNodes;
    public Queue<string> patrolRoute;
    public int enemyNum;
    HiveScript HiveScript;
    void Start()
    {
        hiveSpotted = false;
        alerted = false;
        searching = false;
        PatrolNodes = new Dictionary<string, GameObject>();
        foreach (GameObject PN in GameObject.FindGameObjectsWithTag("PatrolNode"))
        {
            PatrolNodes.Add(PN.name, PN);
        }

    }
    public void SetHiveSpotted(GameObject room)
    {
        alertTimer.Reset();
        searchTimer.Reset();
        hiveSpotted = true;
        alerted = true;
        currentRoom = room;
        roomStr = currentRoom.name[0] + "" + currentRoom.name[5] + "S1";
        UnityEngine.Debug.Log(roomStr);
        alertTimer.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (alertTimer.ElapsedMilliseconds >= 15000 && roomStr!= "R5S1")
        {
            UnityEngine.Debug.Log("searching");
            searchTimer.Start();
            alertTimer.Reset();
            searching = true;
            alerted = false;
            for (int i = 0; i <= enemyNum; i++)
            {
                if (hiveMembers.TryGetValue(i, out GameObject enemy))
                {
                    HiveScript = enemy.GetComponent<HiveScript>();
                    HiveScript.route = GenDest(HiveScript.liveRoom);
                    HiveScript.designation = HiveScript.route.Dequeue();
                    HiveScript.route.Enqueue(HiveScript.designation);

                }
            }
        }
        else if(alertTimer.ElapsedMilliseconds >= 30000)
        {
            UnityEngine.Debug.Log("searching");
            searchTimer.Start();
            alertTimer.Reset();
            searching = true;
            alerted = false;
            for (int i = 0; i <= enemyNum; i++)
            {
                if (hiveMembers.TryGetValue(i, out GameObject enemy))
                {
                    HiveScript = enemy.GetComponent<HiveScript>();
                    HiveScript.route = GenDest(HiveScript.liveRoom);
                    HiveScript.designation = HiveScript.route.Dequeue();
                    HiveScript.route.Enqueue(HiveScript.designation);

                }
            }
        }
        else if (searchTimer.ElapsedMilliseconds == 40000)
        {
            searchTimer.Reset();
            searching = false;
           
        }
        if(searching == true)
        {
            for (int i = 0; i <= enemyNum; i++)
            {
                if (hiveMembers.TryGetValue(i, out GameObject enemy))
                {
                    HiveScript = enemy.GetComponent<HiveScript>();
                    if (HiveScript.lookTimer.ElapsedMilliseconds>= 2000) {
                        UnityEngine.Debug.Log("next spot");
                        HiveScript.looking = false;
                        HiveScript.lookTimer.Reset();

                        HiveScript.designation = HiveScript.route.Dequeue();
                        HiveScript.route.Enqueue(HiveScript.designation);
                    } else if (!HiveScript.myAgent.pathPending && HiveScript.myAgent.remainingDistance <= 0.5f && !HiveScript.looking)
                    {
                        HiveScript.looking = true;
                        HiveScript.lookTimer.Start();
                    }
                   
                }
            }
        }
    }

    public Queue<string> GenDest(string room)
    {
        //places the enemy has been
        Queue<string> destinations = new Queue<string>();
        
        // is this a room or a hall?
        if(room[0]+"" == "H")
        {
            // Hall:
            // determine best place to go next
            if(room[1]+"" == "1")
            {
                destinations.Enqueue("R1S1");
                destinations.Enqueue("R2S1");
                destinations.Enqueue("R3S1");
                destinations.Enqueue("R4S1");
            }else if (room[1] + "" == "2")
            {
                destinations.Enqueue("R3S1");
                destinations.Enqueue("R4S1");
                destinations.Enqueue("R2S1");
                destinations.Enqueue("R1S1");
            }
            else
            {
                destinations.Enqueue("R4S1");
                destinations.Enqueue("R3S1");
                destinations.Enqueue("R2S1");
                destinations.Enqueue("R1S1");
            }
        }
        else
        {
            if(room[1]+"" == "1")
            {
                destinations.Enqueue("R2S1");
                destinations.Enqueue("H2S1");
                destinations.Enqueue("R3S1");
                destinations.Enqueue("R4S1");
            }else if(room[1] + "" == "2")
            {
                destinations.Enqueue("R1S1");
                destinations.Enqueue("H2S2");
                destinations.Enqueue("R4S1");
                destinations.Enqueue("R3S1");
            }else if(room[1] + "" == "3")
            {
                destinations.Enqueue("H2S1");
                destinations.Enqueue("R2S1");
                destinations.Enqueue("R1S1");
                destinations.Enqueue("R4S1");
            }
            else
            {
                destinations.Enqueue("R3S1");
                destinations.Enqueue("R2S1");
                destinations.Enqueue("R1S1");
                destinations.Enqueue("H2S2");
            }

        }
        destinations.Enqueue(room);
        return destinations;
    }


}
