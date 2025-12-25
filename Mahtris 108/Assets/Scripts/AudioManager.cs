using System.Collections.Generic;
using DG.Tweening; // 【新增】引入 DOTween 用于音乐渐变
using UnityEngine;
using System.Collections;

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

    [Header("游戏结果")]
    public AudioClip gameWin;
    public AudioClip gameOver;
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
    [Header("背景音乐配置 (BGM)")]
    [SerializeField] private AudioClip mainMenuBgm;   // 主菜单音乐
    [SerializeField] private AudioClip easyGameBgm;   // 游戏音乐 - 新手
    [SerializeField] private AudioClip normalGameBgm; // 游戏音乐 - 专家
    [SerializeField] private AudioClip hardGameBgm;   // 游戏音乐 - 大师
    [SerializeField] private float bgmRestDuration = 20f;

    [Header("音效库")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip countdownClip;

    public SoundLibrary SoundLibrary => soundLibrary;
    private Coroutine bgmCoroutine;

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuBGM();
        }

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

    // 1. 播放主菜单音乐
    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBgm);
    }

    // 2. 根据难度播放游戏内音乐
    public void PlayGameBGM(Difficulty difficulty)
    {
        AudioClip clipToPlay = null;

        switch (difficulty)
        {
            case Difficulty.Easy:
                clipToPlay = easyGameBgm;
                break;
            case Difficulty.Normal:
                clipToPlay = normalGameBgm;
                break;
            case Difficulty.Hard:
                clipToPlay = hardGameBgm;
                break;
            default:
                clipToPlay = normalGameBgm; // 默认
                break;
        }

        PlayBGM(clipToPlay);
    }

    // =========================================================
    // 核心 BGM 播放逻辑 (保持你现有的淡入淡出逻辑)
    // =========================================================
    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        // 特殊情况：如果请求播放的是当前这首，且正在播放中，则忽略
        // (如果当前是在休息期，isPlaying为false，则会继续执行下面逻辑，重新唤醒音乐，符合预期)
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        // 1. 停止上一次的循环协程 (防止多重循环)
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        // 2. 关闭 AudioSource 自带的循环 (我们要手动控制)
        bgmSource.loop = false;

        // 3. 杀掉旧的淡入淡出动画
        bgmSource.DOKill();

        // 定义播放开始的逻辑 (封装成 Action 方便复用)
        System.Action onPlayStart = () =>
        {
            bgmSource.clip = clip;
            bgmSource.Play();

            // 启动新的循环管理协程
            bgmCoroutine = StartCoroutine(BgmLoopRoutine(clip));
        };

        // 4. 执行切换逻辑 (淡出旧的 -> 播放新的)
        if (bgmSource.isPlaying)
        {
            bgmSource.DOFade(0, 0.5f).OnComplete(() => {
                onPlayStart();
                if (IsBgmOn) bgmSource.DOFade(_bgmVolume, 0.5f);
            });
        }
        else
        {
            // 如果本来就是静音或没在播，直接开始
            bgmSource.volume = 0;
            onPlayStart();
            if (IsBgmOn) bgmSource.DOFade(_bgmVolume, 0.8f);
        }
    }
    private IEnumerator BgmLoopRoutine(AudioClip clip)
    {
        // 1. 等待第一遍播放结束
        // 使用 WaitForSecondsRealtime 确保不受 Time.timeScale 影响 (暂停时音乐时间依然在流逝)
        // 如果你希望游戏暂停时音乐倒计时也暂停，请改用 WaitForSeconds
        yield return new WaitForSecondsRealtime(clip.length);

        while (true)
        {
            // 2. 进入休息期 (Rest)
            // 在这段时间内，bgmSource.isPlaying 会自然变为 false
            yield return new WaitForSecondsRealtime(bgmRestDuration);

            // 3. 检查开关 (防止休息期间玩家关了音乐，结果突然响了)
            if (IsBgmOn)
            {
                bgmSource.Play();
                // 确保音量正确 (防止意外变动)
                bgmSource.volume = _bgmVolume;
            }

            // 4. 等待这一遍播放结束
            yield return new WaitForSecondsRealtime(clip.length);
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
        SaveManager.SaveBgmState(isOn);

        if (isOn)
        {
            // 开启：取消静音，并淡入
            bgmSource.mute = false;
            bgmSource.DOFade(_bgmVolume, 0.3f);

            // 如果当前处于“休息期”(没在播)，且协程还在运行，我们不需要强制 Play，
            // 让它等到休息结束自然播放即可。
            // 但如果协程没了(比如刚启动)，可能需要重启 PlayBGM。
            // 这里通常不需要大改，因为 PlayBGM 里的协程在一直在跑。

            // 唯一的边缘情况：如果玩家在休息期关了BGM又开了BGM，
            // 协程会继续跑，等时间到了自然会响，符合预期。
        }
        else
        {
            // 关闭：静音 (不停止协程，保持节奏，只是听不见了)
            bgmSource.mute = true;
        }
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
    public void StopBGM()
    {
        // 1. 停止负责循环的协程，防止它自动切歌或重启
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        if (bgmSource != null)
        {
            // 2. 杀掉旧的动画 (防止正在淡入时被打断)
            bgmSource.DOKill();

            // 3. 快速淡出 (0.2秒)，然后停止
            // 使用 SetUpdate(true) 确保即使 Time.timeScale = 0 也能执行淡出
            bgmSource.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
            {
                bgmSource.Stop();
            });
        }
    }
}