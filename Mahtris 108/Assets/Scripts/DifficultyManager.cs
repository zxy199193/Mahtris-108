using UnityEngine;

public enum Difficulty { Easy, Normal, Hard }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Easy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        CurrentDifficulty = newDifficulty;
    }
}