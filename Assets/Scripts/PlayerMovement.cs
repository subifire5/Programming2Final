using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // all public items are change-able in the editor and by other scripts
    public float StartingLife = 100;
    public float moveSpeed = 500;
    public float MSensitivity = 5.0f;
    public float JumpSpeed = 2;
    public float amount;
    public float health;
    public float vertSpeed = 2;
    public Slider HealthSlider;
    
    float verticalVelocity = 0;
    Vector3 movement;
    CharacterController Character;
    GameObject player;
    bool DoubleJum;
    public GameObject[] planes;

    void Start()
    {
        //sets the health to the Starting Life
        health = StartingLife;
        player = GameObject.Find("Player");
        Character = player.GetComponent<CharacterController>();
        planes = GameObject.FindGameObjectsWithTag("NoisyPlane");
    }
    void Update()
    {
        //Movement and Rotation in Unity is stored in Vector3
        //you put in the X,Y,Z and then multiply the entire vector by Time.deltaTime
        //that affects it by time; I don't know how THAT works, but it makes it move properly

        Movement(verticalVelocity, Character);



    }
    public void Movement(float vertVel, CharacterController Character)
    {
        // The up-down velocity is one of the things I put into Vector 3
        // Here it's being affected by gravity and Time.
        vertVel += Physics.gravity.y * Time.deltaTime;
        // this Checks if the player is on the ground, and then if they pressed jump
        if (Character.isGrounded && Input.GetButtonDown("Jump"))
        {


            vertVel += JumpSpeed;
        }
        // the lateral movement is unrealistic in the air, but that's ok.
        float forwardSpeed = Input.GetAxis("Vertical");
        float sideSpeed = Input.GetAxis("Horizontal");
        Vector3 Speed = new Vector3(sideSpeed, vertVel*vertSpeed, forwardSpeed) * moveSpeed;
        Speed = transform.rotation * Speed;

        Character.Move(Speed * Time.deltaTime*5);

    }
    void OnCollisionEnter(Collision col)
    {
        //sees what is colliding with it
        if (col.gameObject.tag == "Bullet")
        {
            TakeDamage(20);
        }else if(col.gameObject.tag == "BaseEnemy" || col.gameObject.tag == "HiveMember")
        {
            TakeDamage(20);
        }


    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // find which plane the player has hit
        if(hit.gameObject.tag == "NoisyPlane")
        {
            foreach (GameObject plane in planes)
            {
                if(hit.gameObject == plane)
                {
                    // runs the "make noise" script for that specific plane
                    plane.GetComponent<SoundPlane>().makeNoise();
                }
            }
        }
    }

    public void TakeDamage(float Damage)
    {
        // takes away health, and if health drops below 0, kills player
        health = health - Damage;
        if (health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update

    // Update is called once per frame
    
    }
    

