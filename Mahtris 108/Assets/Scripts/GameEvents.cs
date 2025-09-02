// FileName: GameEvents.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event Action<List<int>> OnRowsCleared;
    public static void TriggerRowsCleared(List<int> clearedRowIndices) => OnRowsCleared?.Invoke(clearedRowIndices);

    public static event Action<List<List<int>>> OnHuDeclared;
    public static void TriggerHuDeclared(List<List<int>> huHand) => OnHuDeclared?.Invoke(huHand);

    public static event Action OnGameOver;
    public static void TriggerGameOver() => OnGameOver?.Invoke();

    public static event Action<GameObject, List<int>> OnNextBlockReady;
    public static void TriggerNextBlockReady(GameObject prefab, List<int> ids) => OnNextBlockReady?.Invoke(prefab, ids);

    public static event Action<int> OnPoolCountChanged;
    public static void TriggerPoolCountChanged(int count) => OnPoolCountChanged?.Invoke(count);

    public static event Action<int> OnScoreChanged;
    public static void TriggerScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
}