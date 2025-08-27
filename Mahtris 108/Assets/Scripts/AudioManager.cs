// FileName: AudioManager.cs
using UnityEngine;

[System.Serializable]
public class SoundLibrary
{
    public AudioClip buttonClick;
    public AudioClip tetrominoRotate;
    public AudioClip clearRow;
    public AudioClip addSetToHuArea;
    public AudioClip huSuccess;
    public AudioClip targetReached;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音源")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("音量控制")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 1.0f;

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("音效库")]
    [SerializeField] private SoundLibrary soundLibrary;

    // --- 【重大修正】---
    // 统一并修正了公开访问音效库的属性名
    public SoundLibrary SoundLibraryProperty => soundLibrary;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ApplyVolume();
        PlayBGM();
        GameEvents.OnRowsCleared += (rows) => PlaySFX(soundLibrary.clearRow);
        GameEvents.OnHuDeclared += (hand) => PlaySFX(soundLibrary.huSuccess);
    }

    void Update()
    {
        if (bgmSource != null && bgmSource.volume != bgmVolume) bgmSource.volume = bgmVolume;
        if (sfxSource != null && sfxSource.volume != sfxVolume) sfxSource.volume = sfxVolume;
    }

    public void ApplyVolume()
    {
        if (bgmSource) bgmSource.volume = bgmVolume;
        if (sfxSource) sfxSource.volume = sfxVolume;
    }

    public void PlayBGM()
    {
        if (bgmSource && backgroundMusic)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClickSound() => PlaySFX(soundLibrary.buttonClick);
    public void PlayRotateSound() => PlaySFX(soundLibrary.tetrominoRotate);
}