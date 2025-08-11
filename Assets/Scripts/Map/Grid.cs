using System;
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

        private Cell[] cells;
        public GameObject Cube;
        public GameObject EnemyPrefab;

        public EnemyData[] enemiesData;
        
        private Enemy[] enemies;

        private void Start()
        {
            // Init enemies
            enemies = new Enemy[enemiesData.Length];
            for (var i = 0; i < enemiesData.Length; i++)
            {
                var enemyData = enemiesData[i];
                var enemyGO = Instantiate(EnemyPrefab);
                enemies[i] = new Enemy(this, enemyData, enemyGO.transform);
            }
            
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
            
            //
            for (var enemyIdx = 0; enemyIdx < enemiesData.Length; enemyIdx++)
            {
                var enemyData = enemiesData[enemyIdx];
                var cellPath = new Cell[enemyData.path.Length];
                for (var pathIdx = 0; pathIdx < enemyData.path.Length; pathIdx++)
                {
                    var gridPos = enemyData.path[pathIdx];
                    var cell = cells[gridPos.posX * sizeX + gridPos.posY];
                    cellPath[pathIdx] = cell;
                }

                enemies[enemyIdx].SetPath(cellPath);
            }
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
    }
}