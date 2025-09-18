// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Text goldText;
    [SerializeField] private Text highScoreText; // 新增

    private string gameSceneName = "GameScene";

    void Start()
    {
        // 游戏开始时，尝试找到GameSession并更新金币显示
        UpdateGoldText();
        // 同时读取并显示最高分
        UpdateHighScoreText();

        // 订阅金币变化事件，以便实时更新（例如从商店返回时）
        GameSession.OnGoldChanged += UpdateGoldText;
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
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
}