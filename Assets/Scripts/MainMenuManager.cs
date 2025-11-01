using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class pause : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button continueButton, quitButton;
    
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
}