
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL || PLATFORM_WEBGL
using CrazyGames;
#endif

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private List<SoundUnit> units = new List<SoundUnit>();
    [SerializeField] private List<AudioSourceUnit> sources = new List<AudioSourceUnit>();

    private AudioSource audioSourceLoud;
    private AudioSource audioSourceMedium;
    private AudioSource audioSourceSilent;

    private Dictionary<SoundTypes, AudioClip> clipsMap = new();
    private Dictionary<VolumeLevels, AudioSource> audioSourcesMap = new();

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeClipsMap();
        InitializeAudioSources();
    }

    private void Start()
    {

    }

    private void InitializeClipsMap()
    {
        foreach (var unit in units)
        {
            clipsMap.Add(unit.Type, unit.Clip);
        }
    }

    private void InitializeAudioSources()
    {
        foreach (var unit in sources)
        {
            audioSourcesMap.Add(unit.VolumeLevel, unit.AudioSource);
        }
    }

    public void Play(SoundTypes soundType, VolumeLevels volume = VolumeLevels.Medium)
    {
        if (volume == VolumeLevels.Looped)
        {
            audioSourcesMap[volume].clip = clipsMap[soundType];
            audioSourcesMap[volume].Play();
        }
        else
        {
            if (clipsMap[soundType])
                audioSourcesMap[volume].PlayOneShot(clipsMap[soundType]);
        }
    }

    public void PlayAmbientMusic(SoundTypes ambientType)
    {
        audioSourcesMap[VolumeLevels.Looped].Stop();
        audioSourcesMap[VolumeLevels.Looped].clip =  clipsMap[ambientType];
        audioSourcesMap[VolumeLevels.Looped].Play();
    }

    public void ToggleSound(bool state)
    {
        foreach (var audioSource in sources)
            audioSource.AudioSource.mute = state;
    }

    public void ToggleMusic(bool state)
    {
        audioSourcesMap[VolumeLevels.Looped].mute = state;
    }


    [System.Serializable]
    public class SoundUnit
    {
        public SoundTypes Type;
        public AudioClip Clip;
    }

    [System.Serializable]
    public class AudioSourceUnit
    {
        public VolumeLevels VolumeLevel;
        public AudioSource AudioSource;
    }
}

public enum SoundTypes
{
    Button,
    Coin,
    Collect,
    Drop,
    Finish,
    FootstepTunnel_1,
    FootstepTunnel_2,
    FootstepVillage_1,
    FootstepVillage_2,
    StoneMine,
    WoodChop,
    QuestComplete,
    Scissors,
    Sicle,
    LevelUp,
    Sword,
    FallingStone,
    EnemyDeath,
    PopUp,
    BuildingSpawn,
    Village,
    Fight
}

public enum VolumeLevels
{
    Loud,
    Medium,
    Silent,
    Looped
}