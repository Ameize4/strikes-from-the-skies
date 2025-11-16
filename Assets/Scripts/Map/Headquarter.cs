using System;
using UnityEngine;

namespace DefaultNamespace.Map
{
    
    [Serializable]
    public struct HeadquarterData
    {
        public int health;
        public int firstHealthThreshold;
    }
    
    public class Headquarter
    {
        private HeadquarterData data;

        private int currentHealth;
        
        public Headquarter(HeadquarterData data)
        {
            this.data = data;

            currentHealth = data.health;
        }

        public void TakeDamage(int value)
        {
            currentHealth -= value;
            Debug.Log(currentHealth);
        }
    }
}