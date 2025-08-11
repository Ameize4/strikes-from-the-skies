namespace DefaultNamespace.Map
{
    public class Cell
    {
        public GridPos gridPos;
        public Grid grid;

        public Cell(GridPos gridPos, Grid grid)
        {
            this.gridPos = gridPos;
            this.grid = grid;
        }
    }
}