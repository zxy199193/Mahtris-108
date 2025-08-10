using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public Text scoreText; // ����� TMP: public TextMeshProUGUI scoreText;

    private int score = 0;

    public Text remainingBlocksText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // ��Ϸһ��ʼ����ʾ 0 ��
        UpdateScoreUI();
    }

    public void AddScore(int linesCleared)
    {
        int points = 0;
        switch (linesCleared)
        {
            case 1: points = 100; break;
            case 2: points = 300; break;
            case 3: points = 500; break;
            case 4: points = 800; break;
        }
        score += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }


    void Update()
    {
        remainingBlocksText.text = "Blocks Left: " + BlockPool.Instance.GetRemainingBlocks();
    }
}