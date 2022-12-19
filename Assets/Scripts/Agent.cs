using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 1f;
    public float health = 1f;
    public float defense = 1f;

    public bool goalReached = false;

    Animator animator;
    GameObject body;
    GameObject armL;
    GameObject armR;
    GameObject legL;
    GameObject legR;

    void Start()
    {
        animator = GetComponent<Animator>();

        // find all the bones that are needed to scale the titans
        // and make the weirdly shaped
        Transform root = transform.Find("Armature").Find("Root_M");
        body = root.Find("Spine1_M").gameObject;
        legL = root.Find("Hip_L").gameObject;
        legR = root.Find("Hip_R").gameObject;
        armL = body.transform.Find("Spine2_M").Find("Chest_M").Find("Scapula_L").gameObject;
        armR = body.transform.Find("Spine2_M").Find("Chest_M").Find("Scapula_R").gameObject;

        // scale each body part based on the stats
        // body = health, legs = speed, ars = defense
        body.transform.localScale *= health;
        legL.transform.localScale *= (speed);
        legR.transform.localScale *= (speed);

        // scalar to manage size of the arms
        float scalar = 1f;
        if (defense > 1f)
        {
            scalar = 0.5f;
        }
        armL.transform.localScale *= (defense * scalar);
        armR.transform.localScale *= (defense * scalar);

        // Debug.Log("STATS:");
        // Debug.Log(health);
        // Debug.Log(speed);
        // Debug.Log(defense);
    }

    // Update is called once per frame
    void Update()
    {
        // if titan reached its goal and wasn't killed, have it attack the wall
        if (goalReached)
        {
            animator.Play("MeleeAttack_OneHanded");
        }
    }

    // 0 = arm, 1 = body, 2 = legs
    public bool TakeDamage(int part) {
        // can't attack the titan if it reached the wall
        // or if titan is already killed
        if (goalReached || health <= 0f) return false;

        else {
            // otherwise, animate getting hit
            animator.Play("GetHit");

            // and apply damage based on the body part hit
            switch(part)
            {
                case 0:
                    health -= (2f / defense);
                    break;
                case 1:
                    health -= (3f / defense);
                    break;
                // legs are not affected by defense and take most damage
                case 2:
                    health -= 5f;
                    break;
                default:
                    break;
            }
            // if it's health reaches 0, set speed to 0 and play stunned animation
            if (health <= 0f)
            {
                speed = 0f;
                animator.Play("StunnedLoop");
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // called whenever we need a new generation of titans
    public void UpdateAgent()
    {
        // reset goal and animatorr values
        goalReached = false;
        animator.Play("Sprint");


        // re-scale the titans based on the new health, speed, and defense values
        body.transform.localScale = Vector3.one * health;
        legL.transform.localScale = Vector3.one * (speed);
        legR.transform.localScale = Vector3.one * (speed);
        float scalar = 1f;
        if (defense > 1f)
        {
            scalar = 0.5f;
        }
        armL.transform.localScale = Vector3.one * (defense * scalar);
        armR.transform.localScale = Vector3.one * (defense * scalar);

        // Debug.Log("STATS:");
        // Debug.Log(health);
        // Debug.Log(speed); 
        // Debug.Log(defense);
    }

}
