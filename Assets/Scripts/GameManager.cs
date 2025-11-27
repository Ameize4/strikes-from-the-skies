using System;
using System.Linq;
using DG.Tweening;
using Sonity;
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
            public GameObject lightObject;
            // public Texture2D ShadowMask;
        }
        [Space]
        [SerializeField] private LightingInfo[] _lightingInfos;

        [Space]
        [SerializeField]
        private Map.HeadquarterData headquarterData;
        public Map.Headquarter headquarter;
        
        [Space]
        public Map.Grid grid;

        public Map.Chapters chapters;
        [SerializeField] private int currentChapterIdx = 0;

        [Space]
        [SerializeField] private PhoneHandler phoneHandler;
        [SerializeField] private BedHandler bedHandler;
        
        [Space]
        [SerializeField] private CameraShake.CameraShakeProperties cameraShakeProperties;
        private CameraShake _cameraShake;

        [Space] [SerializeField] private Volume _volume;
        [SerializeField] float maxVignette, minVignette;
        private Vignette vignette;
        
        [SerializeField] private PlayableDirector _timelinePlayable, loseTimeline;

        [Space] [SerializeField] public SoundEvent enemyShowedUpSE;
        [SerializeField] public SoundEvent enemyMovedSE;
        [SerializeField] public SoundEvent enemyDestroyedSE;

        [Space] [SerializeField] public SoundEvent canonShotSE;
        [SerializeField] public SoundEvent canonHitSE;
        [SerializeField] public SoundEvent canonMissedSE;

        [Space] [SerializeField] private ParticleSystem p1, p2, p3;

        [Serializable]
        private struct YarnSoundBox
        {
            public string name;
            public SoundEvent sound;
            public Transform transform;

            public void Play() => sound.Play(transform);
            public void Stop() => sound.Stop(transform);
        }
        [Space] [SerializeField] private YarnSoundBox[] yarnSounds;
        
        [Space]
        [SerializeField] private MeshRenderer tableMesh;
        [SerializeField] private Material darkTableMaterial;


        // int values of KeyCode Enum of keyboard numbers
        private int alphaKeyCode1 = 49;
        private int alphaKeyCode9 = 57;

        private float trauma;

        private bool waitingForCall;
        private string callJumpDialogueName;

        private bool waitingForBed;
        private string bedJumpDialogueName;

        private bool waitingForDoor;
        private string doorJumpDialogueName;

        public bool twoLetterTelegraphLimitEnabled;

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

            headquarter = new Map.Headquarter(headquarterData);
            headquarter.OnZeroHealth += () =>
            {
                loseTimeline.Play();
                var c = FindFirstObjectByType<CreditsController>();
                _timelinePlayable.stopped += (director => c.EndCredits());
            };
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
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P))
            {
                grid.BeginEnemyWave(chapters.debugEnemyData);
            }
            if (Input.GetKey(KeyCode.O))
            {
                _cameraShake.SetTrauma(1f);
                trauma = 1f;
            }
            #endif
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SetLight(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2)) SetLight(1);
                else if (Input.GetKeyDown(KeyCode.Alpha3)) SetLight(2);
            }

            trauma = Mathf.Clamp01(trauma - cameraShakeProperties.recoverySpeed * Time.deltaTime);
            var vignetteTrauma = (trauma * (maxVignette-minVignette)) + minVignette;
            if (trauma != 0) vignette.intensity.value = vignetteTrauma;

            _cameraShake.Process();
        }

        #region Yarn commands

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
        
        [YarnCommand("InitBedInteract")]
        public static void Yarn_InitBedInteract(string nodeName)
        {
            Instance.bedJumpDialogueName = nodeName;
            Instance.waitingForBed = true;
        }
        
        [YarnCommand("InitDoorInteract")]
        public static void Yarn_InitDoorInteract(string nodeName)
        {
            Instance.doorJumpDialogueName = nodeName;
            Instance.waitingForDoor = true;
        }
        
        [YarnCommand("DelayCallInAttack")]
        public static void Yarn_DelayCallInAttack(int value, string nodeName)
        {
            DOTween.Sequence().AppendInterval(value).AppendCallback(() =>
            {
                if (!Instance.grid.InActiveWave) { return; }
                if (Instance.waitingForCall) { return; }
                Instance.phoneHandler.playAudioRing();
                Instance.callJumpDialogueName = nodeName;
                Instance.waitingForCall = true;
            });
        }
        
        [YarnCommand("StopSleep")]
        public static void Yarn_StopSleep()
        {
            Instance.bedHandler.StopSleep();
        }

        [YarnCommand("SpawnTimeline")]
        public static void Yarn_SpawnTimeline()
        {
            Instance._timelinePlayable.Play();
            var c = FindFirstObjectByType<CreditsController>();
            // Instance._timelinePlayable.stopped += (director => c.StartCredits());
        }
        
        [YarnCommand("ShowInvisibleEnemies")]
        public static void Yarn_ShowInvisibleEnemies()
        {
            DOTween.Sequence().AppendInterval(60f).AppendCallback(Instance.grid.ShowAllEnemies);
        }
        
        [YarnCommand("PlaySound")]
        public static void Yarn_PlaySound(string name)
        {
            foreach (var yarnSoundBox in Instance.yarnSounds)
            {
                if (yarnSoundBox.name == name) {yarnSoundBox.Play(); return;}
            }
            Debug.LogWarning($"There is no yarn sound with {name} name");
        }
        
        [YarnCommand("StopSound")]
        public static void Yarn_StopSound(string name)
        {
            foreach (var yarnSoundBox in Instance.yarnSounds)
            {
                if (yarnSoundBox.name == name) {yarnSoundBox.Stop(); return;}
            }
            Debug.LogWarning($"There is no yarn sound with {name} name");
        }
        
        [YarnCommand("BlockInteractiveObjects")]
        public static void Yarn_BlockInteractiveObjects(bool value)
        {
            InteractiveObject.isBlocked = value;
        }
        
        [YarnCommand("SetTrauma")]
        public static void Yarn_SetTrauma(float value)
        {
            Instance._cameraShake.SetTrauma(value);
            Instance.trauma = value;
        }
        
        [YarnCommand("BackToDialogue")]
        public static void Yarn_BackToDialogue()
        {
            Instance.AllEnemiesDestroyed();
        }
        
        [YarnCommand("ChangeLight")]
        public static void Yarn_ChangeLight(int value)
        {
            Instance.SetLight(value - 1);
        }
        [YarnCommand("DisableMapObject")]
        public static void Yarn_DisableMapObject()
        {
            Instance.grid.KillAllEnemies();
            var m = Instance.tableMesh.materials;
            m[1] = Instance.darkTableMaterial;
            Instance.tableMesh.SetMaterials(m.ToList());
        }
        #endregion

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
            
                DOTween.Sequence()
                    .AppendInterval(0.3f).AppendCallback(() =>
                    {
                        canonShotSE.Play(transform);
                    })
                    .AppendInterval(2.5f).AppendCallback(() =>
                {
                    if (success) canonHitSE.Play(transform);
                    else canonMissedSE.Play(transform);
                    
                    trauma = 1f;
                    _cameraShake.SetTrauma(0.5f);
                    grid.CleanDiedEnemies();
                    p1.Play();
                    p2.Play();
                    p3.Play();
                });
            }
        }

        public void AllEnemiesDestroyed()
        {
            string? reached = headquarter.IsThresholdReached();
            if (reached != null)
            {
                dialogueRunner.StartDialogue(reached);
                return;
            }
            
            if (chapters.chapters[currentChapterIdx].dialogueAfterWave != "")
            {
                dialogueRunner.StartDialogue(chapters.chapters[currentChapterIdx].dialogueAfterWave);
            }
        }
        
        public void SetLight(int idx)
        {
            foreach (var info in _lightingInfos)
            {
                info.lightObject.SetActive(false);
            }
            SetLightmaps(_lightingInfos[idx]);
        }
        
        public void SetLightmaps(LightingInfo info)
        {
            LightmapData data = new LightmapData();
            data.lightmapColor = info.Color;
            // data.shadowMask = info.ShadowMask;
            LightmapSettings.lightmaps = new LightmapData[] { data };
            info.lightObject.SetActive(true);
        }
        
        public void AnswerPhone()
        {
            if (!waitingForCall) return;
            
            waitingForCall = false;
            dialogueRunner.StartDialogue(callJumpDialogueName);
            callJumpDialogueName = "";
            phoneHandler.playAudioPlasticImpact();
        }
        
        public void BedInteract()
        {
            if (!waitingForBed) return;
            
            waitingForBed = false;
            dialogueRunner.StartDialogue(bedJumpDialogueName);
            bedJumpDialogueName = "";
            bedHandler.StartSleep();
        }

        public void DoorInteract()
        {
            if (!waitingForDoor)
            {
                return;
            }

            waitingForDoor = false;
            dialogueRunner.StartDialogue(doorJumpDialogueName);
            doorJumpDialogueName = "";
        }
    }
}