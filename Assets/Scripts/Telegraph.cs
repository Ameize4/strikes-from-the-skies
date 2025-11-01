using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Interfaces;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Telegraph : MonoBehaviour, IInteractive
{
    #region Inspector

    [Header("UI")]
    [SerializeField] private TMP_Text label;

    [Header("Timings in seconds")]
    [SerializeField] private float dotLen = 0.3f;
    [SerializeField] private float letterPause = 0.3f;
    [SerializeField] private float wordPause = 1f;
    [SerializeField] private float MaxPressDuration = 1f;

    [Header("DOTTween settings")]
    [SerializeField] private float tweenDuration;
    [SerializeField] private Vector3 positionTarget;
    [SerializeField] private Vector3 rotationTarget;

    private Vector3 positionInit;
    private Vector3 rotationInit;

    [Header("Audio settings")]
    [SerializeField] private AudioClip toneClip;
    private AudioSource audioSource;

    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float frequency = 700f;
    
    #endregion

    private InputDurationHandler inputDurationHandler;

    private Sequence seq;

    private List<string> morseInput;
    private string translatedText = "";
    
    private bool isInteractiveModeEnabled = false;


    void Start()
    {
        inputDurationHandler = new InputDurationHandler(KeyCode.Mouse0);
        
        inputDurationHandler.AddPressRule(0, () => { morseInput.Add("."); });
        inputDurationHandler.AddPressRule(dotLen, () => { morseInput.Add("-"); });
        inputDurationHandler.AddIdleRule(0.01f, AddMorseDotsToLabel);
        inputDurationHandler.AddIdleRule(letterPause, TranslateMorseToLetter);
        inputDurationHandler.AddIdleRule(wordPause, SendMessageAndClear);
        inputDurationHandler.AddOverHoldRule(MaxPressDuration, RemoveOneLetter);

        inputDurationHandler.Press += AnimateClickOn;
        inputDurationHandler.Release += AnimateClickOff;

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

    #region Input Reactions

    private void AddMorseDotsToLabel()
    {
        var res = string.Join("", morseInput);
        label.text = translatedText + res;
    }

    private void TranslateMorseToLetter()
    {
        translatedText += MorseCodeTranscription.GetStringFromMorseOrEmpty(
            string.Join("", morseInput));
        label.text = translatedText;
        morseInput.Clear();

        if (GameManager.Instance.twoLetterTelegraphLimitEnabled && translatedText.Length >= 2)
        {
            SendMessageAndClear();
        }
    }

    private void SendMessageAndClear()
    {
        if (translatedText == "" || translatedText.Length < 2) return;
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
        if (translatedText.Length <= 0) return;
        
        translatedText = translatedText.Remove(translatedText.Length - 1, 1);
        label.text = translatedText;
    }

    #endregion

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
        if (isInteractiveModeEnabled == false)
        {
            inputDurationHandler.ProcessIdle();
            return;
        }
        
        inputDurationHandler.Process();
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
                tweenDuration).SetEase(Ease.OutQuad))
            .Join(DOTween.To(
                () => transform.localPosition, 
                x => transform.localPosition = x, 
                positionTarget, 
                tweenDuration).SetEase(Ease.OutQuad));
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
                tweenDuration).SetEase(Ease.InQuad))
            .Join(DOTween.To(
                () => transform.localPosition,
                x => transform.localPosition = x,
                positionInit,
                tweenDuration).SetEase(Ease.InQuad));
    }
    
    public void SetInteraction(bool value)
    {
        isInteractiveModeEnabled = value;
        //  There has been logic, but now it is artefact of unused logic
        if (value)
        { }
        else
        { }
    }
}
