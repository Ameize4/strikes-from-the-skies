using System;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Interfaces;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Telegraph : MonoBehaviour, IInteractive
{
    [Serializable]
    public class Rule
    {
        public float Threshold;
        public Action Action;
        public RuleType Type;

        public Rule(float threshold, RuleType type, Action action)
        {
            Threshold = threshold;
            Type = type;
            Action = action;
        }
    }
    
    public enum RuleType { Press, Idle }
    private List<Rule> rules = new List<Rule>();
    private HashSet<Rule> triggered = new HashSet<Rule>();
    
    public float MaxPressDuration = 1f;
    public Action OverholdRule;
    
    public Vector3 positionTarget;
    public Vector3 rotationTarget;

    private Vector3 positionInit;
    private Vector3 rotationInit;

    public AudioClip toneClip;
    private AudioSource audioSource;

    public float volume = 0.5f;
    public float frequency = 700f;

    public float duration;
    public bool isDebug;

    private Sequence seq;

    public float dotLen = 0.3f;
    public float letterPause = 0.3f;
    private List<string> morseInput;

    private float mouseInputDownTime = 0f;
    private float timeOfInputRelease = 0f;

    public TMP_Text label;
    private string translatedText = "";
    
    private bool isInteractiveModeEnabled = false;

    private bool isPressed;
    private float pressStart, idleStart;


    void Start()
    {
        rules.Add(new Rule(0, RuleType.Press, () => { morseInput.Add("."); }));
        rules.Add(new Rule(dotLen, RuleType.Press, () => { morseInput.Add("-"); }));
        rules.Add(new Rule(0.01f, RuleType.Idle, AddMorseDotsToLabel));
        rules.Add(new Rule(letterPause, RuleType.Idle, TranslateMorseToLetter));
        rules.Add(new Rule(letterPause*2, RuleType.Idle, SendMessageAndClear));

        OverholdRule = RemoveOneLetter;

        positionInit = transform.localPosition;
        rotationInit = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        
        audioSource = GetComponent<AudioSource>();
        
        toneClip = GenerateTone(frequency, 1);
        audioSource.clip = toneClip;

        seq = DOTween.Sequence();
        morseInput = new List<string>();
        // audioSource.clip = AudioClip.Create("Gha", 1, 1, 44100 * 2, false);
        // var numSamples = audioSource.clip.samples * audioSource.clip.channels;
        // var samples = new NativeArray<float>(numSamples, Allocator.Temp);
        // audioSource.clip.GetData(samples, 0);
        //
        // for (int i = 0; i < samples.Length; ++i)
        // {
        //     samples[i] = samples[i] * 0.5f;
        // }
        //
        // audioSource.clip.SetData(samples, 0);
    }

    private void AddMorseDotsToLabel()
    {
        var res = string.Join("", morseInput);
        label.text = translatedText + res;
        idleStart = Time.time;
    }

    private void TranslateMorseToLetter()
    {
        translatedText += MorseCodeTranscription.GetStringFromMorseOrEmpty(
            string.Join("", morseInput));
        label.text = translatedText;
        morseInput.Clear();
    }

    private void SendMessageAndClear()
    {
        GameManager.Instance.SendMorseCoordinates(translatedText);
        label.color = Color.green;
        DOTween.To(() => label.color, x => label.color = x, Color.white, 0.6f)
            .OnComplete(() =>
            {
                label.text = "";
                translatedText = "";
            });
    }

    private void RemoveOneLetter()
    {
        if (translatedText.Length <= 0)
        {
            return;
        }
        
        translatedText = translatedText.Remove(translatedText.Length - 1, 1);
        label.text = translatedText;
    }

    private AudioClip GenerateTone(float freq, float lengthSec)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int sampleLength = Mathf.CeilToInt(sampleRate * lengthSec);
        float[] samples = new float[sampleLength];

        for (int i = 0; i < sampleLength; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * volume;
        }

        AudioClip clip = AudioClip.Create($"Tone_{freq}Hz", sampleLength, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void Update()
    {
        if (isInteractiveModeEnabled == false) return;
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isPressed = true;
            pressStart = Time.time;
            triggered.Clear();
            AnimateClickOn();
        }

        if (isPressed)
        {
            float held = Time.time - pressStart;

            if (held >= MaxPressDuration)
            {
                Debug.Log("OverHold detected, resetting...");
                isPressed = false;
                OverholdRule?.Invoke();
                idleStart = Time.time;
                triggered.Clear();
                AnimateClickOff();
                return;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                isPressed = false;
                TriggerPressRule(held);
                idleStart = Time.time;
                triggered.Clear();
                AnimateClickOff();
            }
        }
        else
        {
            float idle = Time.time - idleStart;
            TriggerIdleRule(idle);
        }
    }

    private void AnimateClickOn()
    {
        audioSource.Play();
        seq.Kill();
        seq = DOTween.Sequence();
        seq.Append(DOTween.To(
                () => transform.rotation, 
                x => transform.rotation = x, 
                rotationTarget, 
                duration).SetEase(Ease.OutQuad))
            .Join(DOTween.To(
                () => transform.localPosition, 
                x => transform.localPosition = x, 
                positionTarget, 
                duration).SetEase(Ease.OutQuad));
    }

    private void AnimateClickOff()
    {
        audioSource.Stop();
        seq.Kill();
        seq = DOTween.Sequence();
        seq.Append(DOTween.To(
                () => transform.rotation,
                x => transform.rotation = x,
                rotationInit,
                duration).SetEase(Ease.InQuad))
            .Join(DOTween.To(
                () => transform.localPosition,
                x => transform.localPosition = x,
                positionInit,
                duration).SetEase(Ease.InQuad));
    }
    
    public void SetInteraction(bool value)
    {
        isInteractiveModeEnabled = value;
        if (value)
        { }
        else
        {
            label.text = "";
            translatedText = "";
            morseInput?.Clear();
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
    }}
