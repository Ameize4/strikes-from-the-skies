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

        private Enemy[] enemies;
        public GridPos[] path;

        private void Start()
        {
            // Init enemies
            enemies = new Enemy[1];
            var enemyGO = Instantiate(EnemyPrefab);
            enemies[0] = new Enemy(this, enemyGO.transform);
            
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
            }
            
            //
            var cellPath = new Cell[path.Length];
            for (var i = 0; i < path.Length; i++)
            {
                var gridPos = path[i];
                var cell = cells[gridPos.posX * sizeX + gridPos.posY];
                cellPath[i] = cell;
            }
            enemies[0].SetPath(cellPath);
        }

        private void Update()
        {
            foreach (var e in enemies)
                e.Process();
        }

        public Vector3 GetCellPosition(Cell cell)
        {
            return transform.position + transform.TransformDirection(new Vector3(cell.gridPos.posX, 0, cell.gridPos.posY));
        }
    }
}