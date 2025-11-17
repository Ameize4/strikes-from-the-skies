using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private Button continueButton, quitButton;
    [SerializeField] private Image fade;

    
    private void Start()
    {
        continueButton.onClick.AddListener(LoadGameScene);
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit(0);
        });
        fade.DOFade(0f, 1f);
    }

    private void LoadGameScene()
    {
        fade.DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene(gameSceneName));
    }
}