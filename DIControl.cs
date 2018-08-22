using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DIControl : MonoBehaviour {

    public Transform player;
    Animator anim;

    float rotationSpeed = 2.0f;
    float speed = 2.0f;
    float visDist = 8.0f;
    float visAngle = 30.0f;
    float hiDist = 5.0f;
    string state = "IDLE";

    // Use this for initialization
    void Start () {

        anim = this.GetComponent<Animator>();
}
	
	// Update is called once per frame
	void Update () {
        Vector3 direction = player.position - this.transform.position;
        float angle = Vector3.Angle(direction, this.transform.forward);

        if (direction.magnitude < visDist && angle < visAngle)
        {

            direction.y = 0;

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                        Quaternion.LookRotation(direction),
                                        Time.deltaTime * rotationSpeed);

            if (direction.magnitude >= hiDist)
            {
                if (state != "WALKING")
                {
                    state = "WALKING";
                    anim.SetTrigger("isWalking");
                }
            }
            else
            {
                if (state != "TALKING")
                {
                    state = "TALKING";
                    anim.SetTrigger("isTalking");
                }
            }

        }
        else
        {
            if (state != "IDLE")
            {
                state = "IDLE";
                anim.SetTrigger("isIdle");
            }
        }

        if (state == "WALKING")
            this.transform.Translate(0, 0, Time.deltaTime * speed);

    }
}

