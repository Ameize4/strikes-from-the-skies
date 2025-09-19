using System;
using System.Collections.Generic;
using UnityEngine;

public class InputDurationHandler
{
    [Serializable]
    public class Rule
    {
        public float Threshold;
        public Action Action;
        public RuleType Type;
    }

    public enum RuleType { Press, Idle }

    public event Action Press, Release;

    private float MaxPressDuration;
    private Action OverholdRule;

    private List<Rule> rules = new List<Rule>();
    private float pressStart;
    private float idleStart;
    private bool isPressed;

    private HashSet<Rule> triggered = new HashSet<Rule>();

    private KeyCode key;

    public InputDurationHandler(KeyCode key)
    {
        // Example rules:
        // rules.Add(new Rule { Threshold = 0f, Type = RuleType.Press, Action = () => Debug.Log("Press ≥0s") });
        // rules.Add(new Rule { Threshold = 1f, Type = RuleType.Press, Action = () => Debug.Log("Press ≥1s") });
        // rules.Add(new Rule { Threshold = 3f, Type = RuleType.Press, Action = () => Debug.Log("Press ≥3s") });
        //
        // rules.Add(new Rule { Threshold = 1f, Type = RuleType.Idle, Action = () => Debug.Log("Idle 1s") });
        // rules.Add(new Rule { Threshold = 3f, Type = RuleType.Idle, Action = () => Debug.Log("Idle 3s") });
        //
        // OnOverHold = () => Debug.Log("OverHold! Press too long, reset.");

        this.key = key;
        idleStart = Time.time;
    }

    public void AddPressRule(float threshold, Action action)
    {
        var rule = new Rule();
        rule.Type = RuleType.Press;
        rule.Threshold = threshold;
        rule.Action = action;
        
        rules.Add(rule);
    }

    public void AddIdleRule(float threshold, Action action)
    {
        var rule = new Rule();
        rule.Type = RuleType.Idle;
        rule.Threshold = threshold;
        rule.Action = action;
        
        rules.Add(rule);
    }

    public void AddOverHoldRule(float MaxPressDuration, Action action)
    {
        this.MaxPressDuration = MaxPressDuration;
        OverholdRule = action;
    }

    public void Process()
    {
        if (Input.GetKeyDown(key))
        {
            isPressed = true;
            pressStart = Time.time;
            triggered.Clear();
            Press?.Invoke();
        }

        if (isPressed)
        {
            float held = Time.time - pressStart;

            // Проверяем превышение лимита
            if (MaxPressDuration != 0 && held >= MaxPressDuration)
            {
                Debug.Log("OverHold detected, resetting...");
                isPressed = false;
                OverholdRule?.Invoke();
                idleStart = Time.time;
                triggered.Clear();
                Release?.Invoke();
                return;
            }

            if (Input.GetKeyUp(key))
            {
                isPressed = false;
                TriggerPressRule(held);
                idleStart = Time.time;
                triggered.Clear();
                Release?.Invoke();
            }
        }
        else
        {
            float idle = Time.time - idleStart;
            TriggerIdleRule(idle);
        }
    }

    private void TriggerPressRule(float duration)
    {
        Rule bestRule = null;
        
        foreach (var rule in rules)
        {
            if (rule.Type == RuleType.Press && rule.Threshold <= duration)
            {
                if (bestRule == null || rule.Threshold > bestRule.Threshold)
                {
                    bestRule = rule;
                }
            }
        }
        bestRule?.Action?.Invoke();
    }
    private void TriggerIdleRule(float duration)
    {
        foreach (var rule in rules)
        {
            if (rule.Type == RuleType.Idle && duration >= rule.Threshold)
            {
                if (!triggered.Contains(rule))
                {
                    rule.Action?.Invoke();
                    triggered.Add(rule);
                }
            }
        }
    }
}
