using System;
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

        [Serializable]
        public struct LightingInfo
        {
            public Texture2D Color;
            // public Texture2D ShadowMask;
        }
        [Space]
        [SerializeField] private LightingInfo[] _lightingInfos;

        [Space]
        public Map.Grid grid;

        public Map.Chapters chapters;
        [SerializeField] private int currentChapterIdx = 0;
        
        [Space]
        [SerializeField] private CameraShake.CameraShakeProperties cameraShakeProperties;
        private CameraShake _cameraShake;

        
        public AudioClip enemyAudioClip;
        
        public Map.EnemyData[] enemiesData;

        // int values of KeyCode Enum of keyboard numbers
        public int alphaKeyCode1 = 49;
        public int alphaKeyCode9 = 57;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // dialogueRunner.StartDialogue(dialogue.nodeName);
            
            CinemachineCore.CameraActivatedEvent.AddListener(CameraChangedListener);

            _cameraShake = new CameraShake(cameraShakeProperties);
            _cameraShake.SetNewTarget(followPoint);
        }

        private void CameraChangedListener(ICinemachineCamera.ActivationEventParams arg0)
        {
            var IncomingCamera = (CinemachineCamera)arg0.IncomingCamera;
            var newTrackingObject = IncomingCamera.Target.TrackingTarget;
            if (newTrackingObject == null) newTrackingObject = IncomingCamera.transform;

            _cameraShake.SetNewTarget(newTrackingObject);
        }

        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                grid.BeginEnemyWave(enemiesData);
            }
            if (Input.GetKey(KeyCode.O))
            {
                _cameraShake.SetTrauma(1f);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                for (var index = 0; index < _lightingInfos.Length; index++)
                {
                    var info = _lightingInfos[index];
                    KeyCode tempKeyCode = (KeyCode)(alphaKeyCode1+index);
                    if (Input.GetKeyDown(tempKeyCode))
                    {
                        SetLightmaps(info);
                    }
                }
            }
            
            _cameraShake.Process();
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
        
        public void SetLightmaps(LightingInfo info)
        {
            LightmapData data = new LightmapData();
            data.lightmapColor = info.Color;
            // data.shadowMask = info.ShadowMask;
            LightmapSettings.lightmaps = new LightmapData[] { data };
        }
    }
}