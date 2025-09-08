using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    [Serializable]
    public class Rule
    {
        public float Threshold;
        public Action Action;
        public RuleType Type;
    }

    public enum RuleType { Press, Idle }

    private List<Rule> rules = new List<Rule>();

    private float pressStart;
    private float idleStart;
    private bool isPressed;

    void Start()
    {
        // Пример конфигурации:
        rules.Add(new Rule { Threshold = 0f, Type = RuleType.Press, Action = () => Debug.Log("Press 0s") });
        rules.Add(new Rule { Threshold = 1f, Type = RuleType.Press, Action = () => Debug.Log("Press 1s") });
        rules.Add(new Rule { Threshold = 3f, Type = RuleType.Press, Action = () => Debug.Log("Press 3s") });
        rules.Add(new Rule { Threshold = 5f, Type = RuleType.Press, Action = () => Debug.Log("Press 5s") });

        rules.Add(new Rule { Threshold = 1f, Type = RuleType.Idle, Action = () => Debug.Log("Idle 1s") });
        rules.Add(new Rule { Threshold = 3f, Type = RuleType.Idle, Action = () => Debug.Log("Idle 3s") });
        rules.Add(new Rule { Threshold = 5f, Type = RuleType.Idle, Action = () => Debug.Log("Idle 5s") });

        idleStart = Time.time;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPressed = true;
            pressStart = Time.time;
            triggered.Clear();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isPressed = false;
            float held = Time.time - pressStart;
            TriggerPressRule(held);
            idleStart = Time.time;
            triggered.Clear();
        }

        if (!isPressed)
        {
            float idle = Time.time - idleStart;
            TriggerIdleRule(idle);
        }
    }

    private HashSet<Rule> triggered = new HashSet<Rule>();

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
