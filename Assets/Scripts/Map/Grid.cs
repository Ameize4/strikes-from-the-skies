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

        public EnemyData[] enemiesData;
        
        private Enemy[] enemies;

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
            }
            
            // Init enemies
            enemies = new Enemy[enemiesData.Length];
            for (var enemyIdx = 0; enemyIdx < enemiesData.Length; enemyIdx++)
            {
                var enemyData = enemiesData[enemyIdx];
                var enemyGO = Instantiate(EnemyPrefab);
                enemies[enemyIdx] = new Enemy(this, enemyData, enemyGO.transform);
                enemies[enemyIdx].SetPath(enemyData.path);
            }

            AddHelpers();
        }

        private void Update()
        {
            foreach (var e in enemies) e.Process();
            if (Input.GetMouseButton(0))
            {
                TryKillCell();
            }
        }

        public Vector3 GetCellPosition(Cell cell)
        {
            return transform.position + transform.TransformDirection(new Vector3(cell.gridPos.posX, 0, cell.gridPos.posY));
        }

        // Until we can throw cell by morse
        public int TargetX, TargetY;
        private void TryKillCell()
        {
            var targetCell = cells[TargetX * sizeX + TargetY];
            foreach (var enemy in enemies)
            {
                if (enemy.IsOnCell(targetCell))
                {
                    enemy.isDead = true;
                }
            }
        }

        private void AddHelpers()
        {
            for (int i = 0; i < sizeX; i++)
            {
                var pos = cells[i].gameObject.transform.position;
                var label = Instantiate(textLabel, transform);
                label.transform.position = pos + OffsetUp;
                label.text = i.ToString();
            }

            string[] alphabet = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};
            for (int i = 0; i < sizeY; i++)
            {
                var pos = cells[i*sizeY+sizeX-1].gameObject.transform.position;
                var label = Instantiate(textLabel, transform);
                label.transform.position = pos + OffsetRight;
                label.text = alphabet[i];
            }
        }
    }
}