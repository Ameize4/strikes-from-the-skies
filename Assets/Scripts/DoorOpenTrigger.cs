using UnityEngine;

namespace DefaultNamespace
{
    public class DoorOpenTrigger : MonoBehaviour
    {
    
        [SerializeField] private float interactionDistance = 1.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private Transform player;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool isClose = distance <= interactionDistance;
            
            if (isClose && Input.GetKeyDown(interactKey))
                StartInteraction();
        }

        private void StartInteraction()
        {
            GameManager.Instance.DoorInteract();
        }
    }
}