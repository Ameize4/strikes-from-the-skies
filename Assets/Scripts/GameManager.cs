using System;
using System.Linq;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private Transform followPoint;
        
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
        [SerializeField] private PhoneHandler phoneHandler;
        
        [Space]
        [SerializeField] private CameraShake.CameraShakeProperties cameraShakeProperties;
        private CameraShake _cameraShake;

        [Space] [SerializeField] private Volume _volume;
        [SerializeField] float maxVignette, minVignette;
        private Vignette vignette;
        
        public AudioClip enemyAudioClip;

        // int values of KeyCode Enum of keyboard numbers
        private int alphaKeyCode1 = 49;
        private int alphaKeyCode9 = 57;

        private float trauma;

        private bool waitingForCall;
        private string callJumpDialogueName;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            dialogueRunner.StartDialogue(dialogue.nodeName);
            
            CinemachineCore.CameraActivatedEvent.AddListener(CameraChangedListener);

            _cameraShake = new CameraShake(cameraShakeProperties);
            _cameraShake.SetNewTarget(followPoint);
            
            _volume.sharedProfile.TryGet(out Vignette vignette);

            this.vignette = vignette;
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
                grid.BeginEnemyWave(chapters.debugEnemyData);
            }
            if (Input.GetKey(KeyCode.O))
            {
                _cameraShake.SetTrauma(1f);
                trauma = 1f;
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

            trauma = Mathf.Clamp01(trauma - cameraShakeProperties.recoverySpeed * Time.deltaTime);
            var vignetteTrauma = (trauma * (maxVignette-minVignette)) + minVignette;
            if (trauma != 0) vignette.intensity.value = vignetteTrauma;

            _cameraShake.Process();
        }

        [YarnCommand("SpawnWave")]
        public static void Yarn_SpawnWave()
        {
            var enemiesData = Instance.chapters.chapters[Instance.currentChapterIdx].enemiesData;
            if (enemiesData?.Length <= 0)
            {
                Instance.AllEnemiesDestroyed();
            }
            Instance.grid.BeginEnemyWave(Instance.chapters.chapters[Instance.currentChapterIdx].enemiesData);
        }
        
        [YarnCommand("NextChapter")]
        public static void Yarn_NextChapter()
        {
            Instance.currentChapterIdx += 1;
        }
        
        [YarnCommand("JumpToCall")]
        public static void Yarn_JumpToCall(string nodeName)
        {
            Instance.phoneHandler.playAudioRing();
            Instance.callJumpDialogueName = nodeName;
            Instance.waitingForCall = true;
        }
        

        [SerializeField] private PlayableDirector _timelinePlayable;
        [YarnCommand("SpawnTimeline")]
        public static void Yarn_SpawnTimeline()
        {
            Instance._timelinePlayable.Play();
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

                bool success = grid.TryKillCell(int.Parse(left), right);
            
                DOTween.Sequence().AppendInterval(2.5f).OnComplete(() =>
                {
                    trauma = 1f;
                    _cameraShake.SetTrauma(0.5f);
                    grid.CleanDiedEnemies();
                });
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
        
        public void AnswerPhone()
        {
            if (!waitingForCall) return;
            
            waitingForCall = false;
            dialogueRunner.StartDialogue(callJumpDialogueName);
            callJumpDialogueName = "";
            phoneHandler.playAudioPlasticImpact();
        }
    }
}