// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI����")]
    [SerializeField] private Text goldText;

    private string gameSceneName = "GameScene";

    void Start()
    {
        UpdateGoldText();
        GameSession.OnGoldChanged += UpdateGoldText;
    }

    void OnDestroy()
    {
        GameSession.OnGoldChanged -= UpdateGoldText;
    }

    private void UpdateGoldText(int newGoldAmount)
    {
        if (goldText != null)
        {
            goldText.text = $"���: {newGoldAmount}";
        }
    }

    private void UpdateGoldText()
    {
        if (GameSession.Instance != null && goldText != null)
        {
            goldText.text = $"���: {GameSession.Instance.CurrentGold}";
        }
        else if (goldText != null)
        {
            goldText.text = "���: 0";
        }
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}