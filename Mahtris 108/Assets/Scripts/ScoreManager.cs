using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int score = 0;
    public Text scoreText; // inspector °ó¶¨¿ÉÑ¡

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void AddScore(int delta)
    {
        score += delta;
        Debug.Log($"[ScoreManager] +{delta}, total={score}");
        if (scoreText != null) scoreText.text = "Score: " + score;
    }
}
