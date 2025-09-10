using DefaultNamespace.Interfaces;using UnityEngine;

public class HintHandler : MonoBehaviour, IInteractive
{
    public void SetInteraction(bool value)
    {
        Debug.Log(value);
        gameObject.SetActive(value);
    }
}
