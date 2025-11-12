using UnityEngine;

namespace DefaultNamespace
{
    public class BedHandler : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [SerializeField] private Animator fadeInAnimator;

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
            GameManager.Instance.BedInteract();
            fadeInAnimator.SetTrigger("FadeIn");
        }

        public void StopSleep()
        {
            fadeInAnimator.SetTrigger("FadeOut");
        }
    }
}