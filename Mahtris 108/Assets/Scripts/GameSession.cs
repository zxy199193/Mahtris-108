// FileName: GameSession.cs
using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int CurrentGold { get; private set; }

    // ��ѡ�����������˵�ʵʱ���½�ҵ��¼�
    public static event System.Action<int> OnGoldChanged;

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
        OnGoldChanged?.Invoke(CurrentGold);
        Debug.Log($"����� {amount} ���, ��ǰ�ܽ��: {CurrentGold}");
    }
}