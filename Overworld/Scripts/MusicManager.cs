using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] private AudioSource source;

    [SerializeField] private List<AudioClip> overworldTracks;
    [SerializeField] private List<AudioClip> skirmishTracks; //battle size
    [SerializeField] private List<AudioClip> mediumTracks;
    [SerializeField] private List<AudioClip> largeTracks;
    [SerializeField] private Slider volumeSlider;
    private int mediumBattleSize = 11; //10 or less small 5-5, 8-8 16 medium
    private int largeBattleSize = 17;
    public float volume = 1;
    private void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>(); 
    }
    public bool musicPaused = false;
    public void PauseMusic()
    {
        source.Pause();
        musicPaused = true;
    }
    public void UnpauseMusic()
    {
        source.UnPause();
        musicPaused = false;
    }
    private void Start()
    {
        PlayOverworldMusic();
        source.volume = volume;
        volumeSlider.value = volume;
        InvokeRepeating("CheckIfMusicStopped", 1, 1);
    }
    public void AdjustVolume()
    {
        volume = volumeSlider.value;
        source.volume = volume;
    }
    public void PlayOverworldMusic()
    {
        source.Stop();
        AudioClip clip = overworldTracks[Random.Range(0, overworldTracks.Count - 1)];
        source.PlayOneShot(clip);
        source.clip = clip;
        musicPaused = false;
    }
    private void CheckIfMusicStopped()
    { 
        if (!source.isPlaying && !musicPaused)
        {
            musicPaused = true;
            if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.Overworld)
            {
                PlayOverworldMusic();
            }
            else if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.FieldBattle)
            {
                PlayCombatMusicBasedOnBattleSize();
            }
        }
    } 
    public void PlayCombatMusicBasedOnBattleSize()
    {
        if (OverworldManager.Instance.enemyBattleGroup != null)
        { 
            source.Stop();
            int battleSize = OverworldManager.Instance.enemyBattleGroup.listOfUnitsInThisArmy.Count + OverworldManager.Instance.playerBattleGroup.listOfUnitsInThisArmy.Count;

            if (battleSize >= largeBattleSize)
            {
                source.PlayOneShot(skirmishTracks[Random.Range(0, skirmishTracks.Count - 1)]);
            }
            else if (battleSize >= mediumBattleSize)
            {
                source.PlayOneShot(mediumTracks[Random.Range(0, mediumTracks.Count - 1)]);
            }
            else
            {

                source.PlayOneShot(mediumTracks[Random.Range(0, mediumTracks.Count - 1)]);
            }
            musicPaused = false;
        }
    }
}
