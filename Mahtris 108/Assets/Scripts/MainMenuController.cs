// FileName: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // ȷ��������Ϸ������ Build Settings �У����ҳ���������˴����ַ���ƥ��
    private string gameSceneName = "GameScene";

    // ���������Ҫ����Unity�༭���ġ���ʼ��Ϸ����ť��OnClick�¼��б��н��й���
    public void StartGame()
    {
        // ȷ��AudioManager���ڣ������ŵ����Ч
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // ����������Թ��������˳���Ϸ����ť��
    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Application.Quit();

        // ���д������Unity�༭��������ʱ��Ч�����ڷ������
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}