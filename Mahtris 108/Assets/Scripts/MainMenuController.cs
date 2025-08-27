// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Text goldText;

    private string gameSceneName = "GameScene";

    void Start()
    {
        // 游戏开始时，尝试找到GameSession并更新金币显示
        if (GameSession.Instance != null && goldText != null)
        {
            goldText.text = $"金币: {GameSession.Instance.CurrentGold}";
        }
        else if (goldText != null)
        {
            goldText.text = "金币: 0";
        }
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}