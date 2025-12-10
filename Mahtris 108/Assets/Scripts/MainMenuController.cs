// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Text goldText;
    [SerializeField] private Text highScoreText; // 新增
    [SerializeField] private GameObject difficultyPopupPanel; // 【新增】拖入你的难度选择弹窗面板
    [SerializeField] private Text currentDifficultyText;

    private string gameSceneName = "GameScene";

    public IntroPanelController introPanel;
    public Button openIntroButton;

    void Start()
    {
        // 游戏开始时，尝试找到GameSession并更新金币显示
        UpdateGoldText();
        // 同时读取并显示最高分
        UpdateHighScoreText();
        bool savedFullscreenState = SaveManager.LoadFullscreenState();
        if (Screen.fullScreen != savedFullscreenState)
        {
            Screen.fullScreen = savedFullscreenState;
        }
        // 订阅金币变化事件，以便实时更新（例如从商店返回时）
        GameSession.OnGoldChanged += UpdateGoldText;
        UpdateDifficultyText(DifficultyManager.Instance.CurrentDifficulty);

        openIntroButton.onClick.AddListener(() =>
        {
            introPanel.Open();
        });
    }

    void OnDestroy()
    {
        // 场景销毁时取消订阅，防止内存泄漏
        GameSession.OnGoldChanged -= UpdateGoldText;
    }

    private void UpdateGoldText(int newGoldAmount)
    {
        if (goldText != null)
        {
            goldText.text = $"金币: {newGoldAmount}";
        }
    }

    private void UpdateGoldText()
    {
        if (GameSession.Instance != null && goldText != null)
        {
            goldText.text = $"{GameSession.Instance.CurrentGold}";
        }
        else if (goldText != null)
        {
            goldText.text = "0";
        }
    }

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            // 从新的存档管理器读取最高分
            int highScore = SaveManager.LoadHighScore();
            highScoreText.text = $"{highScore}";
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // 【新增方法】
    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        Application.Quit();

        // 这行代码仅在Unity编辑器中运行时有效，用于方便测试
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void SelectEasy()
    {
        DifficultyManager.Instance.SetDifficulty(Difficulty.Easy);
        UpdateDifficultyText(Difficulty.Easy); // 【新增】
    }
    public void SelectNormal()
    {
        DifficultyManager.Instance.SetDifficulty(Difficulty.Normal);
        UpdateDifficultyText(Difficulty.Normal); // 【新增】
    }
    public void SelectHard()
    {
        DifficultyManager.Instance.SetDifficulty(Difficulty.Hard);
        UpdateDifficultyText(Difficulty.Hard); // 【新增】
    }
    // 【新增】用于打开弹窗
    public void OpenDifficultyPopup()
    {
        if (difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(true);
        }
    }

    // 【新增】用于关闭弹窗
    public void CloseDifficultyPopup()
    {
        if (difficultyPopupPanel != null)
        {
            difficultyPopupPanel.SetActive(false);
        }
    }
    private void UpdateDifficultyText(Difficulty difficulty)
    {
        if (currentDifficultyText != null)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    currentDifficultyText.text = "新手";
                    break;
                case Difficulty.Hard:
                    currentDifficultyText.text = "专家";
                    break;
                case Difficulty.Normal:
                default:
                    currentDifficultyText.text = "大师";
                    break;
            }
        }
    }
}