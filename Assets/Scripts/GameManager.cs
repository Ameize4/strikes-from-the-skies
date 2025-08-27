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

        public Map.Grid grid;
        
        public Map.EnemyData[] enemiesData;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                grid.BeginEnemyWave(enemiesData);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                dialogueRunner.StartDialogue(dialogue.nodeName);
            }
        }

        [YarnCommand("SpawnWave")]
        public static void Yarn_SpawnWave()
        {
            Instance.grid.BeginEnemyWave(Instance.enemiesData);
            
        }
        public void SendMorseCoordinates(string message)
        {
            print(message);
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
    }
}