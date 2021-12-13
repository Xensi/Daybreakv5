using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollisionBehavior : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rigid;
    private Quaternion finalRotation;
    private bool isFlying = true;
    private void Start()
    {

        rigid = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    private void Update()
    {
        if (isFlying)
        {

            transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
        }
    }

    //Detect collisions between the GameObjects with Colliders attached
    void OnCollisionEnter(Collision collision)
    {
        /*//Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "MyGameObjectName")
        {
            //If the GameObject's name matches the one you suggest, output this message in the console
            Debug.Log("Do something here");
        }*/

        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "Terrain")
        {
            //first, save the rotation
            finalRotation = transform.rotation;
            //stop updating rotation
            isFlying = false;

            //If the GameObject has the same tag as specified, output this message in the console
            //Debug.Log("Do something else here");
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            //final set for rotation?
            transform.rotation = finalRotation;
        }
    }
}
