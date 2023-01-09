using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Cinemachine;
public class CinemachineShake : MonoBehaviour
{ 
    public CinemachineVirtualCamera cineVirCam;
    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTime = 0;

    public static CinemachineShake Instance { get; private set; }
    [SerializeField] private float addedShakes = 0;

    [SerializeField] private float[] shakes;

    private void Awake()
    {
        shakes = new float[50];
        Instance = this;
        if (cineVirCam == null)
        {
            cineVirCam = GetComponent<CinemachineVirtualCamera>();
        }
        if (perlin == null)
        {
            perlin = cineVirCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        } 
    }
    private void Start()
    {
        InvokeRepeating("AddUpShakes", 0, 1);
    }
    public void ShakeCamera(float intensity, float duration, Vector3 sourcePos, float endRadius, int id)
    {
        float distance = Vector3.Distance(sourcePos, transform.position); 
        float distanceRelativeToEndRadius = Mathf.Clamp(endRadius - distance, 0, endRadius); // at dist 0, 30 ; at dist 30, 0
        float attenuate = distanceRelativeToEndRadius / endRadius;
        float amplitude = intensity * attenuate;
        shakes[id] = amplitude;
    }
    private void AddUpShakes()
    {
        float total = 0;
        for (int i = 0; i < shakes.Length; i++)
        {
            total += shakes[i];
        }
        if (cineVirCam != null)
        { 
            perlin.m_AmplitudeGain = total;
            shakeTime = 1;
        }
    } 

    private void UpdateShakeTime()
    { 
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            { 
                perlin.m_AmplitudeGain = 0;
            }
        }
    }
}
