using DefaultNamespace.Interfaces;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;
    [SerializeField] private GameObject outlineObject;
    [SerializeField] private GameObject virtualCamera;
    [SerializeField] private MonoBehaviour[] interactiveComponents;

    [SerializeField] private InteractiveObject rightObject, leftObject;

    private Transform player;
    private bool isFocused;
    private int defaultCameraPriority;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        SetOutline(false);
        SetInteractiveComponents(false);
    }

    private void Update()
    {
        if (isFocused)
        {
            if (Input.GetKeyDown(exitKey))
                StopInteraction();
            
            if (Input.GetKeyDown(KeyCode.D) && rightObject != null)
            {
                StopInteraction();
                rightObject.StartInteraction();
            }
            else if (Input.GetKeyDown(KeyCode.A) && leftObject != null)
            {
                StopInteraction();
                leftObject.StartInteraction();
            }
            
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool isClose = distance <= interactionDistance;

        SetOutline(isClose);

        if (isClose && Input.GetKeyDown(interactKey))
            StartInteraction();
    }

    private void StartInteraction()
    {
        isFocused = true;
        
        if (virtualCamera != null)
            virtualCamera.SetActive(true);
        
        SetInteractiveComponents(true);
    }

    private void StopInteraction()
    {
        isFocused = false;
        SetOutline(false);
        
        if (virtualCamera != null)
            virtualCamera.SetActive(false);
        
        SetInteractiveComponents(false);
    }

    private void SetOutline(bool state)
    {
        if (outlineObject != null)
            outlineObject.SetActive(state);
    }

    private void SetInteractiveComponents(bool state)
    {
        foreach (var component in interactiveComponents)
        {
            if (component != null && component is IInteractive interactive)
            {
                interactive.SetInteraction(state);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}