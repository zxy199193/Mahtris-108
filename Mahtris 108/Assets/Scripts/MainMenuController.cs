// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI����")]
    [SerializeField] private Text goldText;
    [SerializeField] private Text highScoreText; // ����

    private string gameSceneName = "GameScene";

    void Start()
    {
        // ��Ϸ��ʼʱ�������ҵ�GameSession�����½����ʾ
        UpdateGoldText();
        // ͬʱ��ȡ����ʾ��߷�
        UpdateHighScoreText();

        // ���Ľ�ұ仯�¼����Ա�ʵʱ���£�������̵귵��ʱ��
        GameSession.OnGoldChanged += UpdateGoldText;
    }

    void OnDestroy()
    {
        // ��������ʱȡ�����ģ���ֹ�ڴ�й©
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
            // ���µĴ浵��������ȡ��߷�
            int highScore = SaveManager.LoadHighScore();
            highScoreText.text = $"{highScore}";
        }
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
        SceneManager.LoadScene(gameSceneName);
    }

    // ������������
    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        Application.Quit();

        // ���д������Unity�༭��������ʱ��Ч�����ڷ������
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}