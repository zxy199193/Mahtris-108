// FileName: AudioManager.cs
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Linq; // 必须引入 Linq 以使用 FindAll/Where

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

    public bool IsBgmOn { get; private set; } = true;
    public bool IsSfxOn { get; private set; } = true;

    [Header("核心音源")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource loopSfxSource;

    [Header("音效对象池设置")]
    [SerializeField] private int initialPoolSize = 10;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private GameObject poolRoot;

    [Header("音量控制")]
    [Range(0f, 1f)][SerializeField] private float _bgmVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1.0f;

    [Header("背景音乐配置 (BGM)")]
    [SerializeField] private AudioClip mainMenuBgm;

    [Header("难度 BGM 列表")]
    [SerializeField] private List<AudioClip> easyGameBgmList;
    [SerializeField] private List<AudioClip> normalGameBgmList;
    [SerializeField] private List<AudioClip> hardGameBgmList;
    [SerializeField] private List<AudioClip> unmatchedGameBgmList;

    [SerializeField] private float bgmRestDuration = 20f;

    [Header("音效库")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip countdownClip;

    public SoundLibrary SoundLibrary => soundLibrary;

    private Coroutine bgmCoroutine;

    private Difficulty _currentBgmDifficulty;
    private bool _isGameBgmActive = false;

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
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitAudioSettings();
    }

    void OnDestroy()
    {
        GameEvents.OnRowsCleared -= OnRowsClearedHandler;
        GameEvents.OnHuDeclared -= OnHuDeclaredHandler;
    }

    private void OnRowsClearedHandler(List<int> rows)
    {
        float pitch = Mathf.Clamp(1.0f + (rows.Count - 1) * 0.05f, 1.0f, 1.2f);
        PlaySFX(soundLibrary.clearRow, pitch);
    }

    private void OnHuDeclaredHandler(List<List<int>> hand)
    {
        PlaySFX(soundLibrary.huSuccess);
    }

    private void InitializePool()
    {
        poolRoot = new GameObject("SFX_Pool");
        poolRoot.transform.SetParent(this.transform);
        for (int i = 0; i < initialPoolSize; i++) CreateNewSource();

        GameEvents.OnRowsCleared += OnRowsClearedHandler;
        GameEvents.OnHuDeclared += OnHuDeclaredHandler;
    }

    private AudioSource CreateNewSource()
    {
        GameObject go = new GameObject($"SFX_Source_{sfxPool.Count}");
        go.transform.SetParent(poolRoot.transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        sfxPool.Add(source);
        return source;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in sfxPool) if (!source.isPlaying) return source;
        return CreateNewSource();
    }

    public void PlaySFX(AudioClip clip, float pitch = 1.0f, float volumeScale = 1.0f)
    {
        if (clip == null || !IsSfxOn) return;
        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = _sfxVolume * volumeScale;
        source.mute = !IsSfxOn;
        source.loop = false;
        source.Play();
    }

    public void PlayMainMenuBGM()
    {
        _isGameBgmActive = false;
        PlayBGM(mainMenuBgm);
    }

    public void PlayGameBGM(Difficulty difficulty)
    {
        _currentBgmDifficulty = difficulty;
        _isGameBgmActive = true;

        // 首次播放不需要排除任何曲子
        AudioClip clipToPlay = GetRandomClipForDifficulty(difficulty, null);
        PlayBGM(clipToPlay);
    }

    private AudioClip GetRandomClipForDifficulty(Difficulty difficulty, AudioClip excludeClip)
    {
        List<AudioClip> targetList = null;

        switch (difficulty)
        {
            case Difficulty.Easy: targetList = easyGameBgmList; break;
            case Difficulty.Normal: targetList = normalGameBgmList; break;
            case Difficulty.Hard: targetList = hardGameBgmList; break;
            case Difficulty.Unmatched: targetList = unmatchedGameBgmList; break;
        }

        if (targetList != null && targetList.Count > 0)
        {
            // 1. 如果只有一首，没得选，直接返回 (无论是否排除)
            if (targetList.Count == 1) return targetList[0];

            // 2. 如果有多首，尝试建立一个“候选列表”，剔除掉 excludeClip
            List<AudioClip> candidates = targetList;

            if (excludeClip != null)
            {
                // 使用 FindAll 找出所有不等于 excludeClip 的曲子
                var filtered = targetList.FindAll(c => c != excludeClip);

                // 只有当剔除后还有剩余曲子时，才使用剔除后的列表
                // (理论上 Count > 1 时肯定有剩余，但为了健壮性加个判断)
                if (filtered.Count > 0)
                {
                    candidates = filtered;
                }
            }

            // 3. 从候选列表中随机选一首
            return candidates[Random.Range(0, candidates.Count)];
        }

        return null;
    }

    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);

        bgmSource.loop = false;
        bgmSource.DOKill();

        System.Action onPlayStart = () =>
        {
            bgmSource.clip = clip;
            bgmSource.Play();
            bgmCoroutine = StartCoroutine(BgmLoopRoutine(clip));
        };

        if (bgmSource.isPlaying)
        {
            bgmSource.DOFade(0, 0.5f).OnComplete(() => {
                onPlayStart();
                if (IsBgmOn) bgmSource.DOFade(_bgmVolume, 0.5f);
            });
        }
        else
        {
            bgmSource.volume = 0;
            onPlayStart();
            if (IsBgmOn) bgmSource.DOFade(_bgmVolume, 0.8f);
        }
    }

    private IEnumerator BgmLoopRoutine(AudioClip currentClip)
    {
        // 记录当前播放的这首，作为下一轮的排除项
        AudioClip lastPlayedClip = currentClip;

        // 1. 等待当前这首歌播完
        if (currentClip != null)
        {
            yield return new WaitForSecondsRealtime(currentClip.length);
        }

        while (true)
        {
            // 2. 休息时间
            yield return new WaitForSecondsRealtime(bgmRestDuration);

            // 3. 决定下一首
            AudioClip nextClip = null;

            if (_isGameBgmActive)
            {
                // 【修改】传入 lastPlayedClip，要求随机逻辑避开这一首
                nextClip = GetRandomClipForDifficulty(_currentBgmDifficulty, lastPlayedClip);
            }
            else
            {
                nextClip = mainMenuBgm;
            }

            // 4. 播放下一首
            if (nextClip != null && IsBgmOn)
            {
                bgmSource.clip = nextClip;
                bgmSource.Play();
                bgmSource.volume = _bgmVolume;

                // 更新记录，这样下下首就不会随机到这一首了
                lastPlayedClip = nextClip;

                // 等待这首播完
                yield return new WaitForSecondsRealtime(nextClip.length);
            }
            else
            {
                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }

    private void InitAudioSettings()
    {
        IsBgmOn = SaveManager.LoadBgmState();
        IsSfxOn = SaveManager.LoadSfxState();
        if (bgmSource) bgmSource.mute = !IsBgmOn;
    }

    public void SetBgmOn(bool isOn)
    {
        IsBgmOn = isOn;
        SaveManager.SaveBgmState(isOn);

        if (isOn)
        {
            bgmSource.mute = false;
            bgmSource.DOFade(_bgmVolume, 0.3f);
        }
        else
        {
            bgmSource.mute = true;
        }
    }

    public void SetSfxOn(bool isOn)
    {
        IsSfxOn = isOn;
        if (!isOn) foreach (var source in sfxPool) source.Stop();
        if (loopSfxSource) loopSfxSource.mute = !isOn;
        SaveManager.SaveSfxState(isOn);
    }

    public void PlayButtonClickSound() => PlaySFX(soundLibrary.buttonClick);
    public void PlayBuySuccessSound() => PlaySFX(soundLibrary.buySuccess);
    public void PlayBuyFailSound() => PlaySFX(soundLibrary.buyFail);

    public void PlayRotateSound()
    {
        float randomPitch = Random.Range(0.92f, 1.08f);
        PlaySFX(soundLibrary.tetrominoRotate, randomPitch);
    }

    public void PlayItemUseSound(AudioClip specificClip)
    {
        if (specificClip != null) PlaySFX(specificClip);
        else if (soundLibrary.defaultItemUse != null)
        {
            float randomPitch = Random.Range(0.95f, 1.05f);
            PlaySFX(soundLibrary.defaultItemUse, randomPitch);
        }
    }

    public void PlayCountdownSound()
    {
        if (!IsSfxOn) return;
        if (loopSfxSource != null && countdownClip != null)
        {
            if (loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip) return;
            loopSfxSource.clip = countdownClip;
            loopSfxSource.loop = true;
            loopSfxSource.volume = _sfxVolume;
            loopSfxSource.Play();
            loopSfxSource.volume = 0;
            loopSfxSource.DOFade(_sfxVolume, 0.5f);
        }
    }

    public void StopCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip)
        {
            loopSfxSource.DOFade(0, 0.3f).OnComplete(() => {
                loopSfxSource.Stop();
                loopSfxSource.clip = null;
            });
        }
    }

    public void PauseCountdownSound()
    {
        if (loopSfxSource != null && loopSfxSource.isPlaying) loopSfxSource.Pause();
    }

    public void ResumeCountdownSound()
    {
        if (loopSfxSource != null && !loopSfxSource.isPlaying && loopSfxSource.clip == countdownClip) loopSfxSource.UnPause();
    }

    public void StopBGM()
    {
        if (bgmCoroutine != null) StopCoroutine(bgmCoroutine);
        if (bgmSource != null)
        {
            bgmSource.DOKill();
            bgmSource.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => bgmSource.Stop());
        }
    }
}