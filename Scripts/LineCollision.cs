using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class LineCollision : MonoBehaviour
{
    public Piece parentPiece;
    public LineRenderer line;
    public GameObject prefab;
    public GameObject aestheticPrefab;
    public GameObject redPrefab;
    public GameObject yellowPrefab;
    public int segments = 10;
    public int originalSegments = 10;
    public GameInitializer gameInit;
    public Material yellow;
    public Material red;
    public Material green;
    public Material white;

    //public List<GameObject> instantiatedCylinders;
    private void Start()
    {

        var game = GameObject.Find("GameInitializer");
        gameInit = game.GetComponent(typeof(GameInitializer)) as GameInitializer;
        originalSegments = segments;
    }

    public void ApproximateCollision()
    {
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);

        List<Vector3> originalPositions = new List<Vector3>(positions);
        List<Vector3> positionList = new List<Vector3>();
        List<GameObject> instantiatedCylinders = new List<GameObject>();

        foreach (var item in originalPositions)
        {
            Debug.Log("pos" + item);
        }
        float distance = Mathf.Sqrt(Mathf.Pow((originalPositions[1].x - originalPositions[0].x), 2) + Mathf.Pow((originalPositions[1].z - originalPositions[0].z), 2));

        Debug.Log("Distance" + distance);

        segments = Mathf.RoundToInt(segments * distance);
        float deltaX = (originalPositions[1].x - originalPositions[0].x) / segments;
        float deltaY = (originalPositions[1].z - originalPositions[0].z) / segments;

        for (int i = 0; i < segments; i++)
        {
            positionList.Add(new Vector3(originalPositions[0].x + deltaX * i, originalPositions[0].y, originalPositions[0].z + deltaY * i));
        }

        foreach (var item in positionList)
        {
            var obj = Instantiate(prefab, item, Quaternion.identity);

            var script = obj.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;

            if (parentPiece != null)
            {

                script.parentPiece = parentPiece; //share the parent 
                                                  //instantiatedCylinders.Add(obj);
                parentPiece.instantiatedCylinders.Add(obj);
            }
        }

        //Debug.Log("completed approximation");
    }

    public void PlaceAestheticCylinders()
    {
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);

        List<Vector3> originalPositions = new List<Vector3>(positions);
        List<Vector3> positionList = new List<Vector3>();

        foreach (var item in originalPositions)
        {
            Debug.Log("pos" + item);
        }
        float distance = Mathf.Sqrt(Mathf.Pow((originalPositions[1].x - originalPositions[0].x), 2) + Mathf.Pow((originalPositions[1].z - originalPositions[0].z), 2));

        Debug.Log("Distance" + distance);
        Debug.Log("Segments" + originalSegments);
        originalSegments = Mathf.RoundToInt(originalSegments * distance);
        float deltaX = (originalPositions[1].x - originalPositions[0].x) / segments;
        float deltaY = (originalPositions[1].z - originalPositions[0].z) / segments;


        for (int i = 0; i < originalSegments; i++)
        {
            positionList.Add(new Vector3(originalPositions[0].x + deltaX * i, originalPositions[0].y, originalPositions[0].z + deltaY * i));
        }

        foreach (var item in positionList)
        {
            NavMeshHit closestHit;
            if (NavMesh.SamplePosition(item, out closestHit, 500, 1))
            {
                Vector3 newPos = new Vector3(closestHit.position.x, closestHit.position.y - 0.063f, closestHit.position.z);
                GameObject thing;
                if (parentPiece.attacking)
                {
                    thing = redPrefab;
                }
                else if (parentPiece.disengaging)
                {
                    thing = yellowPrefab;
                }
                else
                {
                    thing = aestheticPrefab;
                }

                GameObject obj = Instantiate(thing, newPos, Quaternion.identity);
                var script = obj.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;

                if (parentPiece != null)
                {

                    script.parentPiece = parentPiece; //share the parent 
                                                      //instantiatedCylinders.Add(obj);
                    parentPiece.aestheticCylinders.Add(obj);
                }


            }

        }
        //StartCoroutine(ChangeColor());
        //Debug.Log("completed approximation");
    }

}
