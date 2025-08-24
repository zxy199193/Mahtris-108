// FileName: GameSession.cs
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int CurrentGold { get; private set; }

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

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        Debug.Log($"����� {amount} ���, ��ǰ�ܽ��: {CurrentGold}");
    }
}