using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class MainMenuLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private Button 
        continueButton, 
        localButton, 
        quitButton;
    [SerializeField] private Image fade;

    
    private void Start()
    {
        continueButton.onClick.AddListener(LoadGameScene);
        localButton.onClick.AddListener(toggleLocal);
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit(0);
        });
        
        fade.DOFade(0f, 1f).OnComplete(() =>
        {
            var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedString("ui_language");
            var textLabel = localButton.GetComponentInChildren<TMP_Text>();
            textLabel.text = $"{stringOperation} <b>{LocalizationSettings.SelectedLocale.Identifier.Code.ToUpper()}</b>";
            print(textLabel.text);
        });
    }

    private void LoadGameScene()
    {
        fade.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene(gameSceneName));
    }
    
    
    private void toggleLocal()
    {
        var availableLocales = LocalizationSettings.AvailableLocales.Locales;

        int currentLocaleIndex = availableLocales.IndexOf(LocalizationSettings.SelectedLocale);
        int nextLocaleIndex = (currentLocaleIndex + 1) % availableLocales.Count;
        var nextLocale = availableLocales[nextLocaleIndex];
        LocalizationSettings.SelectedLocale = nextLocale;

        var textLabel = localButton.GetComponentInChildren<TMP_Text>();
        
        var localizedStringEvent = textLabel.GetComponent<LocalizeStringEvent>();
        localizedStringEvent.enabled = false;

        var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedString("ui_language");
        textLabel.text = $"{stringOperation} <b>{nextLocale.Identifier.Code.ToUpper()}</b>";
    }
}