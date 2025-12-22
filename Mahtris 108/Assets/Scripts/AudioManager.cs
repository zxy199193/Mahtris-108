using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // 【新增】引入 DOTween 用于音乐渐变

// 【保留】配置容器
[System.Serializable]
public class SoundLibrary
{
    public AudioClip buttonClick;
    public AudioClip tetrominoRotate;
    public AudioClip clearRow;
    public AudioClip addSetToHuArea;
    public AudioClip huSuccess;
    public AudioClip targetReached;

    [Header("通用音效")]
    public AudioClip defaultItemUse;
    [Header("商店音效")]
    public AudioClip buySuccess;
    public AudioClip buyFail;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // 状态
    public bool IsBgmOn { get; private set; } = true;
    public bool IsSfxOn { get; private set; } = true;

    [Header("核心音源")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource loopSfxSource; // 用于倒计时等循环音效

    [Header("音效对象池设置")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private GameObject sfxSourcePrefab; // 可选：如果没有预制体，代码会自动生成

    // 【新增】音效池：解决 PlayOneShot 共用 Pitch 导致的变调冲突
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private GameObject poolRoot;

    [Header("音量控制")]
    [Range(0f, 1f)][SerializeField] private float _bgmVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1.0f;

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("音效库")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip countdownClip;

    public SoundLibrary SoundLibrary => soundLibrary;

    // 属性访问器
    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            if (bgmSource) bgmSource.volume = IsBgmOn ? _bgmVolume : 0;
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            // 更新池中所有空闲或正在播放的音源音量
            foreach (var source in sfxPool)
            {
                source.volume = _sfxVolume;
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool(); // 初始化对象池
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化设置
        InitAudioSettings();

        PlayBGM();

        // 注册事件
        GameEvents.OnRowsCleared += OnRowsClearedHandler;
        GameEvents.OnHuDeclared += OnHuDeclaredHandler;
    }

    void OnDestroy()
    {
        // 【优化】注销事件，防止内存泄漏或报错
        GameEvents.OnRowsCleared -= OnRowsClearedHandler;
        GameEvents.OnHuDeclared -= OnHuDeclaredHandler;
    }

    // --- 事件处理 ---

    private void OnRowsClearedHandler(List<int> rows)
    {
        // 动态音调：每多消一行增加 0.05
        float pitch = Mathf.Clamp(1.0f + (rows.Count - 1) * 0.05f, 1.0f, 1.2f);
        PlaySFX(soundLibrary.clearRow, pitch);
    }

    private void OnHuDeclaredHandler(List<List<int>> hand)
    {
        PlaySFX(soundLibrary.huSuccess);
    }

    // --- 对象池系统 (核心优化) ---

    private void InitializePool()
    {
        poolRoot = new GameObject("SFX_Pool");
        poolRoot.transform.SetParent(this.transform);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSource();
        }
    }

    private AudioSource CreateNewSource()
    {
        GameObject go = new GameObject($"SFX_Source_{sfxPool.Count}");
        go.transform.SetParent(poolRoot.transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D 声音
        sfxPool.Add(source);
        return source;
    }

    private AudioSource GetAvailableSource()
    {
        // 1. 查找空闲的 Source
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }

        // 2. 如果没有空闲的，创建一个新的 (动态扩容)
        return CreateNewSource();
    }

    // --- 播放控制 ---

    // 【核心修改】支持独立 Pitch 的播放方法
    public void PlaySFX(AudioClip clip, float pitch = 1.0f, float volumeScale = 1.0f)
    {
        if (clip == null || !IsSfxOn) return;

        AudioSource source = GetAvailableSource();

        source.clip = clip;
        source.pitch = pitch;
        source.volume = _sfxVolume * volumeScale; // 使用当前全局音效音量 * 缩放
        source.mute = !IsSfxOn;
        source.loop = false;

        source.Play();
    }

    public void PlayBGM()
    {
        if (bgmSource && backgroundMusic)
        {
            if (bgmSource.clip == backgroundMusic && bgmSource.isPlaying) return;

            // 【优化】使用 DOTween 做淡入淡出
            if (bgmSource.isPlaying)
            {
                // 先淡出旧音乐
                bgmSource.DOFade(0, 0.5f).OnComplete(() => {
                    bgmSource.clip = backgroundMusic;
                    bgmSource.loop = true;
                    bgmSource.Play();
                    bgmSource.DOFade(_bgmVolume, 0.5f);
                });
            }
            else
            {
                // 直接播放并淡入
                bgmSource.clip = backgroundMusic;
                bgmSource.loop = true;
                bgmSource.volume = 0;
                bgmSource.Play();
                bgmSource.DOFade(_bgmVolume, 0.8f);
            }
        }
    }

    // --- 设置与状态 ---

    private void InitAudioSettings()
    {
        IsBgmOn = SaveManager.LoadBgmState();
        IsSfxOn = SaveManager.LoadSfxState();

        // 应用初始状态
        if (bgmSource) bgmSource.mute = !IsBgmOn;
        // 池子里的音源会在播放时检查 IsSfxOn，不需要在这里逐个设置
    }

    public void SetBgmOn(bool isOn)
    {
        IsBgmOn = isOn;
        if (bgmSource)
        {
            bgmSource.mute = !isOn;
            // 如果开启，确保音量正确
            if (isOn) bgmSource.DOFade(_bgmVolume, 0.3f);
        }
        SaveManager.SaveBgmState(isOn);
    }

    public void SetSfxOn(bool isOn)
    {
        IsSfxOn = isOn;
        // 这里不需要遍历池子 mute，因为 PlaySFX 时会检查这个布尔值
        // 如果想立即停止所有正在播放的音效：
        if (!isOn)
        {
            foreach (var source in sfxPool) source.Stop();
        }

        if (loopSfxSource) loopSfxSource.mute = !isOn;

        SaveManager.SaveSfxState(isOn);
    }

    // --- 便捷方法 (保持原有接口不变，方便兼容) ---

    public void PlayButtonClickSound() => PlaySFX(soundLibrary.buttonClick);
    public void PlayBuySuccessSound() => PlaySFX(soundLibrary.buySuccess);
    public void PlayBuyFailSound() => PlaySFX(soundLibrary.buyFail);

    public void PlayRotateSound()
    {
        // 旋转音效带一点随机 Pitch，增加动感
        float randomPitch = Random.Range(0.92f, 1.08f);
        PlaySFX(soundLibrary.tetrominoRotate, randomPitch);
    }

    public void PlayItemUseSound(AudioClip specificClip)
    {
        if (specificClip != null)
        {
            PlaySFX(specificClip);
        }
        else if (soundLibrary.defaultItemUse != null)
        {
            float randomPitch = Random.Range(0.95f, 1.05f);
            PlaySFX(soundLibrary.defaultItemUse, randomPitch);
        }
    }

    // --- 倒计时循环音效控制 ---

    public void PlayCountdownSound()
    {
        if (!IsSfxOn) return; // 检查开关

        if (loopSfxSource != null && countdownClip != null)
        {
            if (loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip) return;

            loopSfxSource.clip = countdownClip;
            loopSfxSource.loop = true;
            loopSfxSource.volume = _sfxVolume;
            loopSfxSource.Play();

            // 淡入效果
            loopSfxSource.volume = 0;
            loopSfxSource.DOFade(_sfxVolume, 0.5f);
        }
    }

    public void StopCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip)
        {
            // 淡出后停止
            loopSfxSource.DOFade(0, 0.3f).OnComplete(() => {
                loopSfxSource.Stop();
                loopSfxSource.clip = null;
            });
        }
    }

    public void PauseCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying)
        {
            loopSfxSource.Pause();
        }
    }

    public void ResumeCountdownSound()
    {
        if (loopSfxSource != null && !loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip)
        {
            loopSfxSource.UnPause();
        }
    }
}