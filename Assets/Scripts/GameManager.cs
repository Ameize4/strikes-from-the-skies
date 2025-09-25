using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [FormerlySerializedAs("camera")] [SerializeField] private Transform followPoint;
        
        [Space]
        [SerializeField] DialogueRunner dialogueRunner;
        [SerializeField] DialogueReference dialogue;

        [Space]
        [SerializeField] private LightmapSwapper _lightmapSwapper;
        [SerializeField] private LightingInfo _lightingInfo1, _lightingInfo2;
        
        [Space]
        public Map.Grid grid;

        public Map.Chapters chapters;
        [SerializeField] private int currentChapterIdx = 0;

        public AudioClip enemyAudioClip;
        
        public Map.EnemyData[] enemiesData;

        private Vector3 localPosition, localRotation;
        private Transform newTrackingObject;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // dialogueRunner.StartDialogue(dialogue.nodeName);
            localPosition = followPoint.localPosition;
            localRotation = followPoint.localRotation.eulerAngles;
            
            CinemachineCore.CameraActivatedEvent.AddListener(Call);
        }

        private void Call(ICinemachineCamera.ActivationEventParams arg0)
        {
            var camera = (CinemachineCamera)arg0.IncomingCamera;
            newTrackingObject = camera.Target.TrackingTarget;
            if (newTrackingObject == null) newTrackingObject = camera.transform;
        }

        [SerializeField]
        float frequency = 25;
        
        [SerializeField]
        private Vector3 maximumTranslationShake = Vector3.one;
        [SerializeField]
        private Vector3 maximumAngularShake = Vector3.one;

        private float trauma = 0;
        private float recoverySpeed = 1;
        private float traumaExponent = 2;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                grid.BeginEnemyWave(enemiesData);
            }
            if (Input.GetKey(KeyCode.O))
            {
                trauma = 1f;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    _lightmapSwapper.SetLightmaps(_lightingInfo1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    _lightmapSwapper.SetLightmaps(_lightingInfo2);
                }
            }

            var shake = Mathf.Pow(trauma, traumaExponent);
            followPoint.localPosition = localPosition + new Vector3(
                maximumTranslationShake.x * Mathf.PerlinNoise(0, Time.time*frequency) * 2 - 1, 
                maximumTranslationShake.y * Mathf.PerlinNoise(1, Time.time*frequency) * 2 - 1,
                maximumTranslationShake.z * Mathf.PerlinNoise(2, Time.time*frequency) * 2 - 1
            ) * shake;
            followPoint.localRotation = Quaternion.Euler(localRotation + new Vector3(
                maximumAngularShake.x * (Mathf.PerlinNoise(3, Time.time*frequency) * 2 - 1),
                maximumAngularShake.y * (Mathf.PerlinNoise(4, Time.time*frequency) * 2 - 1),
                maximumAngularShake.z * (Mathf.PerlinNoise(5, Time.time*frequency) * 2 - 1)
            ) * shake);
            trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (newTrackingObject)
            {
                followPoint.localPosition = localPosition;
                followPoint.localRotation = Quaternion.Euler(localRotation);

                followPoint = newTrackingObject;
                localPosition = newTrackingObject.localPosition;
                localRotation = newTrackingObject.localRotation.eulerAngles;

                newTrackingObject = null;
            }
        }

        [YarnCommand("SpawnWave")]
        public static void Yarn_SpawnWave()
        {
            Instance.grid.BeginEnemyWave(Instance.chapters.chapters[Instance.currentChapterIdx].enemiesData);
        }
        
        [YarnCommand("NextChapter")]
        public static void Yarn_NextChapter()
        {
            Instance.currentChapterIdx += 1;
        }
        
        public void SendMorseCoordinates(string message)
        {
            if (message.Length == 2)
            {
                string left = message[0].ToString();
                string right = message[1].ToString();
                
                if (left.All(char.IsDigit) && right.All(char.IsDigit))
                {
                    Debug.Log("Both digit");
                    return;
                } 
                else if(!left.All(char.IsDigit) && !right.All(char.IsDigit))
                {
                    Debug.Log("Both non digit");
                    return;
                }
                // Make sure order is right and digit is first
                else if(right.All(char.IsDigit))
                {
                    (left, right) = (right, left);
                }

                grid.TryKillCell(int.Parse(left), right);
            }
        }

        public void AllEnemiesDestroyed()
        {
            if (chapters.chapters[currentChapterIdx].dialogueAfterWave != "")
            {
                dialogueRunner.StartDialogue(chapters.chapters[currentChapterIdx].dialogueAfterWave);
            }
        }
    }
}