using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AnimatedMesh : MonoBehaviour
{
    [SerializeField]
    private AnimatedMeshScriptableObject AnimationSO;
    private MeshFilter Filter;

    [Header("Debug")]
    [SerializeField]
    private int Tick = 1;
    [SerializeField]
    private int AnimationIndex;
    [SerializeField]
    private string AnimationName;
    private List<Mesh> AnimationMeshes;

    public delegate void AnimationEndEvent(string Name);
    public event AnimationEndEvent OnAnimationEnd;

    private float LastTickTime;

    public AnimatedMesh linkedMesh; //optional
    public MeshRenderer meshRenderer;
    public GameObject optionalSelectionCircle;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Filter = GetComponent<MeshFilter>();
        if (linkedMesh != null)
        {
            linkedMesh.AnimationSO = AnimationSO; 
        }
    }
    public void Play(string AnimationName, bool loop = true)
    {
        if (AnimationName != this.AnimationName)
        {
            animationFinished = false;
            loopAnimation = loop;
            this.AnimationName = AnimationName;
            Tick = 1;
            AnimationIndex = 0;
            AnimatedMeshScriptableObject.Animation animation = AnimationSO.Animations.Find((item) => item.Name.Equals(AnimationName));
            AnimationMeshes = animation.Meshes;
            if (string.IsNullOrEmpty(animation.Name))
            {
                Debug.LogError($"Animated model {name} does not have an animation baked for {AnimationName}!");
            }
        }
        if (linkedMesh != null)
        {
            linkedMesh.Play(AnimationName, loop);
        }
    }
    public bool animationFinished = false;
    public bool loopAnimation = true;
    public void ManualUpdate()
    {
        if (AnimationMeshes != null)
        {
            if (!animationFinished)
            {
                if (Time.time >= LastTickTime + (1f / AnimationSO.AnimationFPS))
                {
                    Filter.mesh = AnimationMeshes[AnimationIndex];

                    AnimationIndex++;
                    if (AnimationIndex >= AnimationMeshes.Count)
                    {
                        OnAnimationEnd?.Invoke(AnimationName);
                        if (loopAnimation)
                        {
                            AnimationIndex = 0;
                            animationFinished = false;
                        }
                        else
                        {
                            animationFinished = true;
                        }
                    }
                    LastTickTime = Time.time;
                }
                Tick++;
            } 
        }

        if (linkedMesh != null)
        {
            linkedMesh.ManualUpdate();
        }
    }
    /*private void Update() //optimize
    {
        if (AnimationMeshes != null)
        {
            if (Time.time >= LastTickTime + (1f / AnimationSO.AnimationFPS))
            {
                Filter.mesh = AnimationMeshes[AnimationIndex];

                AnimationIndex++;
                if (AnimationIndex >= AnimationMeshes.Count)
                {
                    OnAnimationEnd?.Invoke(AnimationName);
                    AnimationIndex = 0;
                }
                LastTickTime = Time.time;
            }
            Tick++;
        }
    }*/
}
