using UnityEngine;

// 【保留】这个类必须存在，它是用来在 Inspector 中配置音效的容器
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
    public bool IsBgmOn { get; private set; } = true;
    public bool IsSfxOn { get; private set; } = true;

    [Header("音源")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    [Header("音量控制")]
    [Range(0f, 1f)][SerializeField] private float _bgmVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1.0f;

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("音效库")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip countdownClip;

    // 公开访问器
    public SoundLibrary SoundLibrary => soundLibrary;

    // 【优化】使用属性来控制音量，只在数值变化时修改 AudioSource，替代 Update 轮询
    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            if (bgmSource) bgmSource.volume = _bgmVolume;
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            if (sfxSource) sfxSource.volume = _sfxVolume;
        }
    }

    void Awake()
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
    }

    void Start()
    {
        // 初始化音量
        if (bgmSource) bgmSource.volume = _bgmVolume;
        if (sfxSource) sfxSource.volume = _sfxVolume;

        PlayBGM();

        // 【优化】订阅事件，并添加动态音效逻辑
        // 消除行越多，音调稍微高一点点，给予更强的反馈
        GameEvents.OnRowsCleared += (rows) =>
        {
            // 基础音调 1.0，每多消一行增加 0.05，最高不超过 1.2
            float pitch = Mathf.Clamp(1.0f + (rows.Count - 1) * 0.05f, 1.0f, 1.2f);
            PlaySFX(soundLibrary.clearRow, pitch);
        };

        GameEvents.OnHuDeclared += (hand) => PlaySFX(soundLibrary.huSuccess);
        InitAudioSettings();
    }

    // 已移除 Update() 方法，节省性能

    public void ApplyVolume()
    {
        if (bgmSource) bgmSource.volume = _bgmVolume;
        if (sfxSource) sfxSource.volume = _sfxVolume;
    }

    public void PlayBGM()
    {
        if (bgmSource && backgroundMusic)
        {
            // 如果已经在播放同一首音乐，则不打断
            if (bgmSource.clip == backgroundMusic && bgmSource.isPlaying) return;

            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // 【新增】核心通用方法：支持指定音调 (pitch) 和音量缩放
    public void PlaySFX(AudioClip clip, float pitch = 1.0f, float volumeScale = 1.0f)
    {
        if (sfxSource && clip)
        {
            // 临时改变音源的音调
            sfxSource.pitch = pitch;
            // 播放一次
            sfxSource.PlayOneShot(clip, volumeScale);
            // 注意：对于 PlayOneShot，音调改变会立即生效。
            // 由于 sfxSource 是单通道共享的，为了不影响后续音效，
            // 理想情况下最好在下一帧重置 pitch，或者我们默认所有 PlaySFX 调用者都会设置 pitch。
            // 简单处理：如果你大部分音效都需要随机感，保持 pitch 变化其实问题不大。
            // 为了安全起见，你也可以在这里开启一个协程在 0.1秒后把 pitch 重置回 1.0，但对该类游戏通常不需要这么严格。
        }
    }

    // --- 便捷方法 ---

    public void PlayButtonClickSound() => PlaySFX(soundLibrary.buttonClick);

    // 【优化】旋转音效增加随机音调，防止听觉疲劳
    public void PlayRotateSound()
    {
        // 音调在 0.9 到 1.1 之间浮动
        float randomPitch = Random.Range(0.9f, 1.1f);
        PlaySFX(soundLibrary.tetrominoRotate, randomPitch);
    }

    // 【新增】供道具系统调用的接口
    public void PlayItemUseSound(AudioClip specificClip)
    {
        // 1. 优先检查是否有特定的音效 (来自 ItemData)
        if (specificClip != null)
        {
            PlaySFX(specificClip);
        }
        // 2. 如果特定音效为空，则检查 SoundLibrary 里有没有配置通用音效
        else if (soundLibrary.defaultItemUse != null)
        {
            // 为了防止通用音效听起来太单调，我们可以加一点点音调随机 (0.95 - 1.05)
            float randomPitch = Random.Range(0.95f, 1.05f);
            PlaySFX(soundLibrary.defaultItemUse, randomPitch);
        }
        // 3. 都没有配置，则不播放，或者你可以选择播放 buttonClick 作为最后的兜底
        else
        {
            // PlayButtonClickSound(); // 可选
        }
    }
    private void InitAudioSettings()
    {
        IsBgmOn = SaveManager.LoadBgmState();
        IsSfxOn = SaveManager.LoadSfxState();

        ApplyMuteState();
    }
    private void ApplyMuteState()
    {
        if (bgmSource) bgmSource.mute = !IsBgmOn; // 如果开启，则 mute = false
        if (sfxSource) sfxSource.mute = !IsSfxOn;
    }
    public void SetBgmOn(bool isOn)
    {
        IsBgmOn = isOn;
        if (bgmSource) bgmSource.mute = !isOn;
        SaveManager.SaveBgmState(isOn);
    }

    public void SetSfxOn(bool isOn)
    {
        IsSfxOn = isOn;
        if (sfxSource) sfxSource.mute = !isOn;
        SaveManager.SaveSfxState(isOn);
    }
    public void PlayBuySuccessSound() => PlaySFX(soundLibrary.buySuccess);
    public void PlayBuyFailSound() => PlaySFX(soundLibrary.buyFail);

    public void PlayCountdownSound()
    {
        if (loopSfxSource != null && countdownClip != null)
        {
            // 如果已经在播放了，就不要重新开始 (防止鬼畜)
            if (loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip)
                return;

            loopSfxSource.clip = countdownClip;
            loopSfxSource.loop = true; // 设置为循环
            loopSfxSource.Play();
        }
    }

    // 【新增】停止倒计时音效
    public void StopCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying)
        {
            // 只有当前播放的是倒计时音效才停止 (防止误停其他循环音效)
            if (loopSfxSource.clip == countdownClip)
            {
                loopSfxSource.Stop();
                loopSfxSource.clip = null; // 清空引用
            }
        }
    }
    // 【新增】暂停倒计时音效
    public void PauseCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip)
        {
            loopSfxSource.Pause();
        }
    }

    // 【新增】恢复倒计时音效
    public void ResumeCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.clip == countdownClip)
        {
            loopSfxSource.UnPause();
        }
    }
}