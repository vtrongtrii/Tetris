using UnityEngine;

public static class Challenge
{
    private static float timer = 0f;
    private static float interval = 1.5f;

    public static void RandomTetrominoEveryFewSecondsUpdate(System.Action callback)
    {
        if (ChallengeRuleManager.Instance.currentRule != ChallengeRule.RandomTetrominoEveryFewSeconds)
            return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            callback?.Invoke();
            timer = 0f;
        }
    }

    public static bool BlockRotationIfNeeded()
    {
        return ChallengeRuleManager.Instance.currentRule == ChallengeRule.NoRotation;
    }

    public static bool ShouldHidePreview()
    {
        return ChallengeRuleManager.Instance.currentRule == ChallengeRule.HiddenPreview;
    }

}
