using UnityEngine;
using System.Collections;
using Pathfinding;

[HelpURL("http://arongranberg.com/astar/docs/class_blocker_test.php")]
public class BlockerTest : MonoBehaviour
{
    public void Start()
    {
        var blocker = GetComponent<SingleNodeBlocker>();

        blocker.BlockAtCurrentPosition();
    }
}