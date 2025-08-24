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

    [Header("��Դ")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [Header("��������")]
    [SerializeField] private AudioClip backgroundMusic;
    [Header("��Ч��")]
    [SerializeField] private SoundLibrary soundLibrary;

    // --- ����Ҫ���������ԣ�ȷ���ⲿ���԰�ȫ���� ---
    public SoundLibrary SoundLib => soundLibrary;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        PlayBGM();
        GameEvents.OnRowsCleared += (rows) => PlaySFX(soundLibrary.clearRow);
        GameEvents.OnHuDeclared += (hand) => PlaySFX(soundLibrary.huSuccess);
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