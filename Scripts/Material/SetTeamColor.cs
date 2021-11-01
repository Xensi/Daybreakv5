using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SetTeamColor : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinnedMeshRenderers;
    public MeshRenderer[] meshRenderers;

    public void SetTeamMaterial(Material material)
    {
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            skinnedMeshRenderers[i].material = material;
        }
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = material;
        }
    }

}
