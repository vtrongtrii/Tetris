using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStageManager : MonoBehaviour
{
    public static LevelStageManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public enum StageType
    {
        Normal,
        Challenge
    }

    // Biến stage hiện tại
    private StageType currentStage = StageType.Normal;

    // Event khi thay đổi màn chơi
    public static event Action<StageType, int> OnStageChanged;

    private void Start()
    {
        OnStageChanged += HandleStageChanged;

        // Ví dụ kích hoạt lần đầu:
        TriggerStageChanged(StageType.Normal, 1);
    }

    // Gọi sự kiện StageChanged
    private void TriggerStageChanged(StageType type, int stage)
    {
        currentStage = type;
        Debug.Log("Stage changed to: " + currentStage);

        OnStageChanged?.Invoke(type, stage);
    }

    // Hàm xử lý khi stage thay đổi
    private void HandleStageChanged(StageType type, int stage)
    {
        if (type == StageType.Challenge)
        {
            ChallengeRuleManager.Instance.ApplyRandomRule();
        }
        else
        {
            ChallengeRuleManager.Instance.ClearRule();
        }
    }

    // Ví dụ hàm chuyển stage:
    public void GoToNextStage(int level)
    {
        if (level % 3 == 0)
        {
            TriggerStageChanged(StageType.Challenge, level / 3);
        }
        else
        {
            TriggerStageChanged(StageType.Normal, level);
        }
    }
}
