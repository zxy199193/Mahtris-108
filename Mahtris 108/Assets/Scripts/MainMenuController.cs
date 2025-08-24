// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // 确保您的游戏场景在 Build Settings 中，并且场景名称与此处的字符串匹配
    private string gameSceneName = "GameScene";

    // 这个方法需要您在Unity编辑器的“开始游戏”按钮的OnClick事件列表中进行关联
    public void StartGame()
    {
        // 确保AudioManager存在，并播放点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // 这个方法可以关联到“退出游戏”按钮上
    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Application.Quit();

        // 这行代码仅在Unity编辑器中运行时有效，用于方便测试
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}