using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

class MoveTowardsJobs : MonoBehaviour
{
    [BurstCompile(CompileSynchronously = true)]
    public struct VelocityJob : IJobParallelForTransform
    {
        // Jobs declare all data that will be accessed in the job
        // By declaring it as read only, multiple jobs are allowed to access the data in parallel        
        [ReadOnly]
        public NativeArray<Vector3> tar;

        // Delta time must be copied to the job since jobs generally don't have a concept of a frame.
        // The main thread waits for the job same frame or next frame, but the job should do work deterministically
        // independent on when the job happens to run on the worker threads.        
        public float deltaTime;

        public float speed;

        // The code actually running on the job
        public void Execute(int index, TransformAccess transform)
        {
            // Move the transforms based on delta time and velocity
            //var pos = transform.position;
            //pos += velocity[index] * deltaTime;
            //transform.position = pos;
            Vector3 target = tar[0];
            transform.position = Vector3.MoveTowards(transform.position, target, speed*deltaTime);

        }
    }

    // Assign transforms in the inspector to be acted on by the job
    [SerializeField] public Transform[] m_Transforms;
    TransformAccessArray m_AccessArray;
    public Transform target;
    public float speed = 10;

    void Awake()
    {
        // Store the transforms inside a TransformAccessArray instance,
        // so that the transforms can be accessed inside a job.

        MeshRenderer[] array = FindObjectsOfType<MeshRenderer>();
        m_Transforms = new Transform[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            m_Transforms[i] = array[i].transform;
        }
        m_AccessArray = new TransformAccessArray(m_Transforms);
    }

    void OnDestroy()
    {
        // TransformAccessArrays must be disposed manually. 
        m_AccessArray.Dispose();
    }

    public void Update()
    {
        var tar = new NativeArray<Vector3>(1, Allocator.Persistent);

        tar[0] = target.position;  

        // Initialize the job data
        var job = new VelocityJob()
        {
            speed = speed,
            deltaTime = Time.deltaTime, 
            tar = tar
        };

        // Schedule a parallel-for-transform job.
        // The method takes a TransformAccessArray which contains the Transforms that will be acted on in the job.
        JobHandle jobHandle = job.Schedule(m_AccessArray);

        // Ensure the job has completed.
        // It is not recommended to Complete a job immediately,
        // since that reduces the chance of having other jobs run in parallel with this one.
        // You optimally want to schedule a job early in a frame and then wait for it later in the frame.        
        jobHandle.Complete();

        //Debug.Log(m_Transforms[0].position);

        // Native arrays must be disposed manually.
        tar.Dispose();
    }
}