using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class pause : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button 
        continueButton, 
        twoLetterMaxButton, 
        quitButton;
    
    [Space] [SerializeField] private Volume _volume;
    private DepthOfField depthOfField;

    private bool canBePaused = true;

    bool paused = false;

    private void SetPaused()
    {
        canBePaused = InteractiveObject.CurrentFocus != null;
    }
    
    private void OnEnable()
    {
        InteractiveObject._currentFocusChanged += SetPaused;
    }
    private void OnDisable()
    {
        InteractiveObject._currentFocusChanged -= SetPaused;
    }

    private void Start()
    {
        continueButton.onClick.AddListener(() =>
        {
            togglePause();
        });
        twoLetterMaxButton.onClick.AddListener(toggleTwoLetterMax);
        toggleTwoLetterMax(); // Run once to prepare values
        toggleTwoLetterMax(); // Run twice because i want lazily rewert bool value
        
        quitButton.onClick.AddListener(() => Application.Quit(0));
        
        
        _volume.sharedProfile.TryGet(out DepthOfField depthOfField);

        this.depthOfField = depthOfField;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && canBePaused)
        {
            paused = togglePause();
        }
    }
	
    bool togglePause()
    {
        if(Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            mainMenu.SetActive(false);
            depthOfField.active = false;
            return false;
        }
        else
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            mainMenu.SetActive(true);
            depthOfField.active = true;
            return true;	
        }
    }

    
    private string tlmString;
    
    private void toggleTwoLetterMax()
    {
        var textLabel = twoLetterMaxButton.GetComponentInChildren<TMP_Text>();
        tlmString ??= textLabel.text;
        
        GameManager.Instance.twoLetterTelegraphLimitEnabled = !GameManager.Instance.twoLetterTelegraphLimitEnabled;
        bool value = GameManager.Instance.twoLetterTelegraphLimitEnabled;
        if (value)
            textLabel.text = $"{tlmString} <b>{value.ToString()}</b>";
        else
            textLabel.text = $"{tlmString} <b>{value.ToString()}</b>";

    }
}