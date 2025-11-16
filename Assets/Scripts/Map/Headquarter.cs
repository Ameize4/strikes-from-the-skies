using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.Map
{
    
    [Serializable]
    public struct HeadquarterData
    {
        public int health;

        public List<int> healthThresholds;
        public List<string> dialogueToRun;
    }
    
    public class Headquarter
    {
        private HeadquarterData data;

        private int currentHealth;

        private Dictionary<int, bool> thresholdStates;
        private string? lastReachedThreshold;
        public event Action OnZeroHealth;

        public Headquarter(HeadquarterData data)
        {
            this.data = data;
            currentHealth = data.health;

            // Initialize the threshold tracking dictionary.
            thresholdStates = new Dictionary<int, bool>();
            if (data.healthThresholds != null)
            {
                data.healthThresholds.Sort((a, b) => b.CompareTo(a));

                foreach (var threshold in data.healthThresholds)
                {
                    thresholdStates[threshold] = false;
                }
            }
        }

        public void TakeDamage(int value)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - value);

            if (data.healthThresholds != null)
            {
                for (var i = 0; i < data.healthThresholds.Count; i++)
                {
                    int threshold = data.healthThresholds[i];
                    if (currentHealth <= threshold && !thresholdStates[threshold])
                    {
                        thresholdStates[threshold] = true;
                        lastReachedThreshold = data.dialogueToRun[i];

                        break;
                    }
                }
            }

            if (currentHealth <= 0) OnZeroHealth?.Invoke();
        }

        public string? IsThresholdReached()
        {
            string? reachedThreshold = lastReachedThreshold;
            lastReachedThreshold = null;
            return reachedThreshold;
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }
    }
}