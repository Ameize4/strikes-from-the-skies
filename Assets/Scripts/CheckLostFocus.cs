using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class YarnOptionSelector : MonoBehaviour
{
    [SerializeField] DialoguePresenterBase dialogueView;
    public InputActionReference lClick;

    private void OnEnable()
    {
        lClick.action.performed += Focus;
    }
    private void OnDisable()
    {
        lClick.action.performed -= Focus;
    }

    void Focus(InputAction.CallbackContext context)
    {
        var options = dialogueView.transform.GetComponentInChildren<OptionItem>();
        if (options)
        {
            EventSystem.current.SetSelectedGameObject(options.gameObject);
        }
    }
}