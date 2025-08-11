using UnityEngine;

namespace DefaultNamespace.Map
{
    public class Enemy
    {
        public Transform transform;

        public int posIdx;
        public Cell position;
        public Cell[] path;
        public float coundDown;
        private Grid grid;

        public Enemy(Grid grid, Transform transform)
        {
            this.grid = grid;
            this.transform = transform;
            coundDown = 5;
            posIdx = 0;
        }

        public void SetPath(Cell[] path)
        {
            this.path = path;
            position = path[0];
            transform.position = grid.GetCellPosition(position);
        }
        
        public void Process()
        {
            coundDown -= Time.deltaTime;
            if (coundDown > 0) return;
            
            coundDown = 5;
            posIdx = (posIdx + 1) % path.Length;
            position = path[posIdx];
            transform.position = grid.GetCellPosition(position);
        }
    }
}