using System;
using UnityEngine;
using Yarn.Unity;

namespace DefaultNamespace.Map
{
    [Serializable]
    public class Chapter
    {
        public string name;
        public EnemyData[] enemiesData;
        [SerializeField] 
        public DialogueReference dialogueAfterWave;
    }
    
    [CreateAssetMenu]
    public class Chapters : ScriptableObject
    {
        public Chapter[] chapters;
        public EnemyData[] debugEnemyData;
    }
}