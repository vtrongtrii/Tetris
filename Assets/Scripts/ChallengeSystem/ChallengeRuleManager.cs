using UnityEngine;
using System;

public class ChallengeRuleManager : MonoBehaviour
{
    public static ChallengeRuleManager Instance { get; private set; }

    public ChallengeRule currentRule { get; private set; } = ChallengeRule.None;

    public event Action<ChallengeRule> OnChallengeRuleActivated;
    public event Action OnChallengeRuleCleared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void ActivateRandomRule()
    {
        Array values = Enum.GetValues(typeof(ChallengeRule));
        ChallengeRule randomRule;

        do
        {
            randomRule = (ChallengeRule)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        } while (randomRule == ChallengeRule.None);

        currentRule = randomRule;
        OnChallengeRuleActivated?.Invoke(currentRule);
        Debug.Log($"🔸 Luật thử thách: {currentRule}");
    }

    public void ClearRule()
    {
        currentRule = ChallengeRule.None;
        OnChallengeRuleCleared?.Invoke();
        Debug.Log("✅ Đã gỡ luật thử thách.");
    }
    public void ApplyRandomRule()
    {
        currentRule = (ChallengeRule)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ChallengeRule)).Length);
        Debug.Log("[ChallengeRuleManager] Đã áp dụng rule: " + currentRule);
    }

}
