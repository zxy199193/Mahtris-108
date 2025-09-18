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

            // ����������Ϸ����ʱ���Ӵ浵���ؽ��
            CurrentGold = SaveManager.LoadGold();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;

        // ����������ұ仯ʱ�����浽�浵
        SaveManager.SaveGold(CurrentGold);

        OnGoldChanged?.Invoke(CurrentGold);
        Debug.Log($"����� {amount} ���, ��ǰ�ܽ��: {CurrentGold}");
    }
}
