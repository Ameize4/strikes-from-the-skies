using DefaultNamespace.Interfaces;using UnityEngine;

public class HintHandler : MonoBehaviour, IInteractive
{
    public void SetInteraction(bool value)
    {
        gameObject.SetActive(value);
    }
}
