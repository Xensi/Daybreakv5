using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    public string team = "Altgard";
    [SerializeField] private RichAI aiPath;
    [SerializeField] private float threshold = .5f;


    [SerializeField] private List<FormationPosition> listOfNearbyEnemies;

    private void Update()
    {
        if (aiPath.remainingDistance > threshold) // if there's still path to traverse
        { 
            aiPath.canMove = true;
        }
        if (aiPath.reachedDestination) //if we've reached destination
        { 
            aiPath.canMove = false; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        FormationPosition block = other.gameObject.GetComponent<FormationPosition>();
        if (block != null && block != this && team != block.team)
        {
            Debug.LogError("collided with enemy");
            listOfNearbyEnemies.Add(block);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        FormationPosition block = other.gameObject.GetComponent<FormationPosition>();
        if (block != null && block != this && team != block.team)
        {
            Debug.LogError("far away from enemy");
            listOfNearbyEnemies.Remove(block);
        }
    }
}
