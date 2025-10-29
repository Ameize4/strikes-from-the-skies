using UnityEngine;

namespace DefaultNamespace
{
    public class PhoneHandler : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private AudioClip phoneAudioClip, plasticAC;

        private Transform player;
        private AudioSource AS;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;

            AS = GetComponent<AudioSource>();
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
            GameManager.Instance.AnswerPhone();
        }

        public void playAudioRing()
        {
            AS.clip = phoneAudioClip;
            AS.Play();
        }

        public void playAudioPlasticImpact()
        {
            AS.clip = plasticAC;
            AS.Play();
        }
    }
}