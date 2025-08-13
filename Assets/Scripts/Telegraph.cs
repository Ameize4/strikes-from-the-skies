using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Telegraph : MonoBehaviour
{
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
    void Start()
    {
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
        bool isMouseInput = HandleMouseInput();

        if (morseInput.Count > 0 && isMouseInput == false && letterPause < Mathf.Abs(timeOfInputRelease - Time.time))
        {
            var res = string.Join(' ', morseInput);
            Debug.Log(res);
            morseInput.Clear();
        }
    }

    private bool HandleMouseInput()
    {
        bool isProcessed = false;
        if (Input.GetMouseButtonDown(0))
        {
            isProcessed = true;
            mouseInputDownTime = Time.time;
            
            if (isDebug)
            {
                toneClip = GenerateTone(frequency, 1);
                audioSource.clip = toneClip;
            }

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

        if (Input.GetMouseButtonUp(0))
        {
            isProcessed = true;
            timeOfInputRelease = Time.time;
            mouseInputDownTime -= timeOfInputRelease;
            if (dotLen > Mathf.Abs(mouseInputDownTime))
                morseInput.Add(".");
            else
                morseInput.Add("-");
            
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

        if (Input.GetMouseButton(0))
            isProcessed = true;
        return isProcessed;
    }
}
