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
    private int mediumBattleSize = 9; //8 or less small 4-4, 7-7 14 medium
    private int largeBattleSize = 15;
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
        AudioClip song;
        if (OverworldManager.Instance.enemyBattleGroup != null)
        { 
            source.Stop();
            int battleSize = OverworldManager.Instance.enemyBattleGroup.listOfUnitsInThisArmy.Count + OverworldManager.Instance.playerBattleGroup.listOfUnitsInThisArmy.Count;

            if (battleSize >= largeBattleSize)
            {
                song = largeTracks[Random.Range(0, largeTracks.Count - 1)];
                source.PlayOneShot(song);
                Debug.Log("Now playing song: " + song.name);
            }
            else if (battleSize >= mediumBattleSize)
            {
                song = mediumTracks[Random.Range(0, mediumTracks.Count - 1)];
                source.PlayOneShot(song);
                Debug.Log("Now playing song: " + song.name);
            }
            else
            {
                song = skirmishTracks[Random.Range(0, skirmishTracks.Count - 1)];
                source.PlayOneShot(song);
                Debug.Log("Now playing song: " + song.name);
            }
            musicPaused = false;
        }
    }
}
