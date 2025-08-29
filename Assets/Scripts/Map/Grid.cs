using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace.Map
{
    [Serializable]
    public struct GridPos
    {
        public int posX, posY;

        public GridPos(int posX, int posY)
        {
            this.posX = posX;
            this.posY = posY;
        }
    }
    
    public class Grid : MonoBehaviour
    {
        public int sizeX, sizeY;

        public Cell[] cells;
        public GameObject Cube;
        public GameObject EnemyPrefab;
        public TMP_Text textLabel;
        public Vector3 OffsetUp, OffsetRight;

        private Enemy[] enemies;
        
        private bool inActiveWave = false;
        
        static string[] alphabet = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};

        private string eventNameOnFinish;

        private void Start()
        {
            // Init cells
            cells = new Cell[sizeX * sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    cells[i*sizeX + j] = new Cell(new GridPos(i, j), this);
                }
            }

            // Create cell object
            foreach (var cell in cells)
            {
                var newCube = Instantiate(Cube, transform);
                newCube.transform.position = GetCellPosition(cell);
                cell.gameObject = newCube;
                cell.gameObject.transform.name = $"X{cell.gridPos.posX}Y{cell.gridPos.posY}";
            }
            

            AddHelpers();
        }

        private void Update()
        {
            UpdateEnemies();
        }

        private void UpdateEnemies()
        {
            if (inActiveWave == false) return;

            foreach (var e in enemies) e.Process();
        }

        public Vector3 GetCellPosition(Cell cell)
        {
            var x = cell.gridPos.posX * transform.localScale.x;
            var y = cell.gridPos.posY * transform.localScale.y;
            return transform.position + transform.TransformDirection(new Vector3(x, 0, y));
        }

        public void TryKillCell(int TargetX, string TargetYLetter)
        {
            int targetY = Array.FindIndex(alphabet, x => x == TargetYLetter);
            TryKillCell(TargetX, targetY);
        }
        
        public void TryKillCell(int TargetX, int TargetY)
        {
            if (enemies == null) return;
            
            var targetCell = cells[TargetX * sizeX + TargetY];

            bool isAllKilled = true;
            foreach (var enemy in enemies)
            {
                // Try kill cells
                if (enemy.IsOnCell(targetCell)) enemy.Die();

                if (!enemy.isDead) isAllKilled = false;
            }

            if (isAllKilled)  FinalizeEnemyWave();
        }

        private void AddHelpers()
        {
            for (int i = 0; i < sizeX; i++)
            {
                var pos = cells[i].gameObject.transform.position;
                var label = Instantiate(textLabel, transform);
                label.transform.position = pos + OffsetUp;
                label.transform.rotation = Quaternion.LookRotation(-transform.right);
                label.text = alphabet[i];
            }

            for (int i = 0; i < sizeY; i++)
            {
                var pos = cells[i*sizeY].gameObject.transform.position;
                var label = Instantiate(textLabel, transform);
                label.transform.position = pos + OffsetRight;
                label.transform.rotation = Quaternion.LookRotation(-transform.right);
                label.text = i.ToString();
            }
        }

        public void BeginEnemyWave(EnemyData[] enemiesData, string eventNameOnFinish)
        {
            if (inActiveWave) return;

            // Init enemies
            enemies = new Enemy[enemiesData.Length];
            for (var enemyIdx = 0; enemyIdx < enemiesData.Length; enemyIdx++)
            {
                var enemyData = enemiesData[enemyIdx];
                var enemyGO = Instantiate(EnemyPrefab, transform);

                var localScale = transform.localScale;
                enemyGO.transform.localScale = new Vector3(1/localScale.x, 1/localScale.y, 1/localScale.z);
                
                enemies[enemyIdx] = new Enemy(this, enemyData, enemyGO.transform);
                enemies[enemyIdx].SetPath(enemyData.path);
            }

            this.eventNameOnFinish = eventNameOnFinish;
            inActiveWave = true;
        }

        private void FinalizeEnemyWave()
        {
            inActiveWave = false;
            foreach (var enemy in enemies)
                enemy.Clean();

            enemies = null;

            GameManager.Instance.AllEnemiesDestroyed(eventNameOnFinish);
        }
    }
}