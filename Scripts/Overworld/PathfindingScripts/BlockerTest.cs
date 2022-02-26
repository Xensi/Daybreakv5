using UnityEngine;
using System.Collections;
using Pathfinding;

[HelpURL("http://arongranberg.com/astar/docs/class_blocker_test.php")]
public class BlockerTest : MonoBehaviour
{
    public SingleNodeBlocker blocker;
    private void Awake()
    {
        if (blocker == null)
        {
            blocker = GetComponent<SingleNodeBlocker>();
        }
        if (blocker.manager == null)
        {
            var blockManager = GameObject.FindWithTag("BlockManager");
            blocker.manager = blockManager.GetComponent<BlockManager>();
        }

    }

    private void Update()
    {
        if (blocker != null)
        {
            blocker.BlockAtCurrentPosition();
        }
    }
}