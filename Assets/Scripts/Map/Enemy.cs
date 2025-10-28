using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Map
{
    [Serializable]
    public struct EnemyData
    {
        public float speed;
        public float delay;
        public GridPos beginPosition;
        public bool isInvisible;
    }
    
    public class Enemy
    {
        private Transform transform;
        private EnemyData data;
        private Grid grid;

        private int posIdx;
        
        private float coundDown;
        public bool isDead;
        public bool isDelayed;

        private float randomMin = -0.5f;
        private float randomMax =  0.5f;

        private Cell cellFrom, cellTo;
        private float progress;

        public Enemy(Grid grid, EnemyData data, Transform transform)
        {
            this.grid = grid;
            this.data = data;
            this.transform = transform;
            posIdx = 0;
            isDelayed = data.delay > 0;
            coundDown = isDelayed ? data.delay : data.speed;
            
            coundDown += Random.Range(randomMin, randomMax);
            
            SetPath(data);
            
            if (isDelayed) transform.gameObject.SetActive(false);
            else PlayAudioStep();
        }

        public void SetPath(EnemyData data)
        {
            cellFrom = grid.GetCellByCoordinates(data.beginPosition);
            cellFrom.isEnemyHere = true;
            cellTo = cellFrom.NextCellOnPath;

            transform.position = grid.GetCellPosition(cellFrom);
        }
        
        public void Process()
        {
            if (isDead) return;
            
            coundDown -= Time.deltaTime;
            if (coundDown > 0) return;
            
            coundDown = data.speed;
            coundDown += Random.Range(randomMin, randomMax);

            if (isDelayed)
            {
                transform.gameObject.SetActive(!data.isInvisible);
                PlayAudioStep();
                isDelayed = false;
                return;
            }
            
            if (cellTo.isEnemyHere) return;

            if (cellTo.contentType == CellContentType.Destination)
            {
                Debug.Log("bruh");
                return;
            }
            
            cellFrom.isEnemyHere = false;
            cellFrom = cellTo;
            cellFrom.isEnemyHere = true;
            cellTo = cellFrom.NextCellOnPath ?? grid.GetCellByCoordinates(data.beginPosition);
            transform.position = grid.GetCellPosition(cellFrom);

            PlayAudioStep();
        }

        private void PlayAudioStep()
        {
            var audioSource = transform.GetComponent<AudioSource>();
            audioSource.time = 0.6f;
            audioSource.pitch = Random.Range(0.95f, 1.1f);
            audioSource.Play();
        }

        public bool IsOnCell(Cell cell)
        {
            return cellFrom == cell;
        }

        public void MarkDead()
        {
            isDead = true;
        }

        public void RemoveFromBoard()
        {
            cellFrom.isEnemyHere = false;
            transform.gameObject.SetActive(false);
        }

        public void Clean()
        {
            Object.Destroy(transform.gameObject);
        }
    }
}