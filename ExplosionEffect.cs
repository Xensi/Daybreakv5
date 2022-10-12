using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particlesList;
    [SerializeField] private float timeToFizzle = 5;
    [SerializeField] private float fadeOutTime = 1;
    [SerializeField] private float endTime = 5;
    [SerializeField] private float FXFadeOutTime = 1;
    [SerializeField] private AudioSource audioSource; 
    private float setter = 0;
    [SerializeField] private bool ShouldFadeOut = true;
    [SerializeField] private DamageZone damageZone;
    // Start is called before the first frame update
    void Start()
    {
        if (ShouldFadeOut)
        { 
            Invoke("FizzleOut", timeToFizzle);
        }
    }

    private void End()
    {
        damageZone.enabled = false;
    }
    private void FizzleOut()
    {
        foreach (ParticleSystem sys in particlesList)
        {
            var emission = sys.emission;
            //sys.Stop();
            StartCoroutine(FadeOutParticles(sys, emission, FXFadeOutTime));
        }
        StartCoroutine(FadeOutAudio.FadeOut(audioSource, fadeOutTime));
    }
    public IEnumerator FadeOutParticles(ParticleSystem particles, ParticleSystem.EmissionModule mod, float FadeTime)
    {

        float startingEm = mod.rateOverTime.constant; 

        while (mod.rateOverTime.constant > 0)
        {
            setter += startingEm * Time.deltaTime / FadeTime;
            mod.rateOverTime = startingEm - setter; 

            yield return null;
        } 
        particles.Stop();
        Invoke("End", endTime);
    }
}
