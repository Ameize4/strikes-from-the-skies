using System.Linq;
using UnityEngine;
using Yarn.Unity;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        [SerializeField] DialogueRunner dialogueRunner;
        [SerializeField] DialogueReference dialogue;

        [Space]
        public Map.Grid grid;

        public Map.Chapters chapters;
        [SerializeField] private int currentChapterIdx = 0;

        public AudioClip enemyAudioClip;
        
        public Map.EnemyData[] enemiesData;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            dialogueRunner.StartDialogue(dialogue.nodeName);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                grid.BeginEnemyWave(enemiesData);
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