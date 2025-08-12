using System;
using UnityEngine;

namespace DefaultNamespace.Map
{
    [Serializable]
    public struct EnemyData
    {
        public float speed;
        public GridPos[] path;
    }
    
    public class Enemy
    {
        private Transform transform;
        private EnemyData data;
        private Grid grid;

        private int posIdx;
        private Cell[] cellPath;
        
        private float coundDown;
        public bool isDead;

        public Enemy(Grid grid, EnemyData data, Transform transform)
        {
            this.grid = grid;
            this.data = data;
            this.transform = transform;
            coundDown = data.speed;
            posIdx = 0;
        }

        public void SetPath(GridPos[] path)
        {
            cellPath = new Cell[path.Length];
            for (var pathIdx = 0; pathIdx < path.Length; pathIdx++)
            {
                var gridPos = path[pathIdx];
                var cell = grid.cells[gridPos.posX * grid.sizeX + gridPos.posY];
                cellPath[pathIdx] = cell;
            }
            transform.position = grid.GetCellPosition(cellPath[0]);
        }
        
        public void Process()
        {
            if (isDead) return;
            
            coundDown -= Time.deltaTime;
            if (coundDown > 0) return;
            
            coundDown = data.speed;
            posIdx = (posIdx + 1) % cellPath.Length;
            transform.position = grid.GetCellPosition(cellPath[posIdx]);
        }

        public bool IsOnCell(Cell cell)
        {
            return cellPath[posIdx] == cell;
        }
    }
}