using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DefaultNamespace.Map
{
    [Serializable]
    public struct EnemyData
    {
        public float speed;
        public float delay;
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
        public bool isDelayed;

        public Enemy(Grid grid, EnemyData data, Transform transform)
        {
            this.grid = grid;
            this.data = data;
            this.transform = transform;
            posIdx = 0;
            isDelayed = data.delay > 0;
            coundDown = isDelayed ? data.delay : data.speed;
            
            if (isDelayed) transform.gameObject.SetActive(false);
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
            if (isDelayed)
            {
                transform.gameObject.SetActive(true);
                isDelayed = false;
                return;
            }

            posIdx = (posIdx + 1) % cellPath.Length;
            transform.position = grid.GetCellPosition(cellPath[posIdx]);
        }

        public bool IsOnCell(Cell cell)
        {
            return cellPath[posIdx] == cell;
        }

        public void Die()
        {
            isDead = true;
            transform.gameObject.SetActive(false);
        }

        public void Clean()
        {
            Object.Destroy(transform.gameObject);
        }
    }
}