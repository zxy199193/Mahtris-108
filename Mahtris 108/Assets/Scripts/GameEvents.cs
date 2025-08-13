// FileName: GameEvents.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    // 当一个或多个行被填满并准备好被处理时触发
    // 参数: List<int> - 被消除的行的Y坐标索引列表
    public static event Action<List<int>> OnRowsCleared;
    public static void TriggerRowsCleared(List<int> clearedRowIndices) => OnRowsCleared?.Invoke(clearedRowIndices);

    // 当满足胡牌条件时触发
    // 参数: List<List<int>> - 用于展示的所有面子（包括将牌）
    public static event Action<List<List<int>>> OnHuDeclared;
    public static void TriggerHuDeclared(List<List<int>> huHand) => OnHuDeclared?.Invoke(huHand);

    // 当游戏结束时触发
    public static event Action OnGameOver;
    public static void TriggerGameOver() => OnGameOver?.Invoke();

    // 当下一个方块准备好时触发，用于UI预览
    public static event Action<GameObject, List<int>> OnNextBlockReady;
    public static void TriggerNextBlockReady(GameObject prefab, List<int> ids) => OnNextBlockReady?.Invoke(prefab, ids);

    // 当牌库数量变化时触发
    public static event Action<int> OnPoolCountChanged;
    public static void TriggerPoolCountChanged(int count) => OnPoolCountChanged?.Invoke(count);

    // 当分数变化时触发
    public static event Action<int> OnScoreChanged;
    public static void TriggerScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
}
