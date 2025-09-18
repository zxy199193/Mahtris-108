// FileName: GameSession.cs
using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public int CurrentGold { get; private set; }

    // 可选：用于在主菜单实时更新金币的事件
    public static event System.Action<int> OnGoldChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 【新增】游戏启动时，从存档加载金币
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

        // 【新增】金币变化时，保存到存档
        SaveManager.SaveGold(CurrentGold);

        OnGoldChanged?.Invoke(CurrentGold);
        Debug.Log($"获得了 {amount} 金币, 当前总金币: {CurrentGold}");
    }
}
