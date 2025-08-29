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
        [SerializeField] DialogueReference dialogue_afterWave1;
        [SerializeField] DialogueReference dialogue_afterWave2;

        [Space]
        public Map.Grid grid;
        
        public Map.EnemyData[] enemiesData;
        public Map.EnemyData[] enemiesData_wave1;
        public Map.EnemyData[] enemiesData_wave2;

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
                grid.BeginEnemyWave(enemiesData, "");
            }
        }

        [YarnCommand("SpawnWave")]
        public static void Yarn_SpawnWave()
        {
            Instance.grid.BeginEnemyWave(Instance.enemiesData_wave1, Instance.dialogue_afterWave1.nodeName);
        }

        [YarnCommand("SpawnWave1")]
        public static void Yarn_SpawnWave1()
        {
            Instance.grid.BeginEnemyWave(Instance.enemiesData_wave2, Instance.dialogue_afterWave2.nodeName);
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

        public void AllEnemiesDestroyed(string eventName)
        {
            if (eventName != "")
            {
                dialogueRunner.StartDialogue(eventName);
            }
        }
    }
}