using System;
using System.Collections.Generic;
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

        [Space]
        [SerializeField] private GridPos[] mainTownCells;
        [SerializeField] private GridPos[] secondTown, thirdTown;
        

        public Cell[] cells;
        public GameObject Cube;
        public GameObject EnemyPrefab;
        public TMP_Text textLabel;
        public Vector3 OffsetUp, OffsetRight;

        private Enemy[] enemies;
        
        private bool inActiveWave = false;
        
        static string[] alphabet = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"};

        private Queue<Cell> searchFrontier = new Queue<Cell>();

        private void Start()
        {
            // Init cells
            cells = new Cell[sizeX * sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    var cell = new Cell(new GridPos(i, j), this);
                    var cellIndex = i * sizeX + j;
                    cells[cellIndex] = cell;

                    cell._isAlternative = (j % 2 == 0);
                    if (i % 2 == 0)
                    {
                        cell._isAlternative = !cell._isAlternative;
                    }

                    if (j > 0)
                    {
                        Cell.MakeNorthSouthNeighbors(cell, cells[cellIndex - 1]);
                    }
                    if (i > 0)
                    {
                        Cell.MakeEastWestNeighbors(cell, cells[cellIndex - sizeY]);
                    }
                }
            }

            var n = 1;
            foreach (var cell in cells)
            {
                var newCube = Instantiate(Cube, transform);
                newCube.transform.position = GetCellPosition(cell);
                cell.gameObject = newCube;
                cell.gameObject.transform.name = $"X{cell.gridPos.posX}Y{cell.gridPos.posY}n{n}";
                n++;
            }
            
            AddHelpers();
            foreach (GridPos cell in mainTownCells)
            {
                ToggleDestination(cell.posX, cell.posY);
            }
        }

        private void Update()
        {
            UpdateEnemies();
        }

        public void ToggleDestination(int x, int y)
        {
            var cell = cells[GetCellIdxByCoordinates(x, y)];
            ToggleDestination(cell);
        }
        
        private void ToggleDestination(Cell cell)
        {
            if (cell.contentType == CellContentType.Destination) {
                cell.contentType = CellContentType.Empty;
                if (!FindPath())
                {
                    cell.contentType = CellContentType.Destination;
                    FindPath();
                }
            }
            else
            {
                cell.contentType = CellContentType.Destination;
                FindPath();
            }
        }


        public void ToggleWall(int x, int y)
        {
            var cell = cells[GetCellIdxByCoordinates(x, y)];
            ToggleDestination(cell);
        }
        
        private void ToggleWall(Cell cell)
        {
            if (cell.contentType == CellContentType.Wall) {
                cell.contentType = CellContentType.Empty;
                FindPath();
            }
            else if (cell.contentType == CellContentType.Empty)
            {
                cell.contentType = CellContentType.Wall;
                if (!FindPath())
                {
                    cell.contentType = CellContentType.Wall;
                    FindPath();
                }
            }
        }

        private bool FindPath()
        {
            foreach (Cell cell in cells)
            {
                if (cell.contentType == CellContentType.Destination)
                {
                    cell.BecomeDestination();
                    searchFrontier.Enqueue(cell);
                }
                else
                {
                    cell.ClearPath();
                }
            }

            if (searchFrontier.Count == 0) return false;

                
            while (searchFrontier.Count > 0) {
                Cell cell = searchFrontier.Dequeue();
                if (cell != null)
                {
                    if (cell._isAlternative)
                    {
                        searchFrontier.Enqueue(cell.GrowPathNorth());
                        searchFrontier.Enqueue(cell.GrowPathSouth());
                        searchFrontier.Enqueue(cell.GrowPathEast());
                        searchFrontier.Enqueue(cell.GrowPathWest());
                    }
                    else
                    {
                        searchFrontier.Enqueue(cell.GrowPathWest());
                        searchFrontier.Enqueue(cell.GrowPathEast());
                        searchFrontier.Enqueue(cell.GrowPathSouth());
                        searchFrontier.Enqueue(cell.GrowPathNorth());
                    }
                }
            }

            foreach (Cell cell in cells)
            {
                if (!cell.HasPath)
                    return false;
            }

            foreach (Cell cell in cells) cell.ShowPath();

            return true;
        }

        private void UpdateEnemies()
        {
            if (inActiveWave == false) return;

            foreach (var e in enemies) e.Process();
            
            if (enemies.Length == 0) FinalizeEnemyWave();
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

            if (isAllKilled) FinalizeEnemyWave();
        }

        public int GetCellIdxByCoordinates(int x, int y)
        {
            return x * sizeX + y;
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

        public void BeginEnemyWave(EnemyData[] enemiesData)
        {
            if (inActiveWave) return;

            // Init enemies
            enemies = new Enemy[enemiesData.Length];
            for (var enemyIdx = 0; enemyIdx < enemiesData.Length; enemyIdx++)
            {
                var enemyData = enemiesData[enemyIdx];
                var enemyGO = Instantiate(EnemyPrefab, transform);
                var audioSource =  enemyGO.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = GameManager.Instance.enemyAudioClip;
                audioSource.volume = 0.2f;

                var localScale = transform.localScale;
                enemyGO.transform.localScale = new Vector3(1/localScale.x, 1/localScale.y, 1/localScale.z);
                
                enemies[enemyIdx] = new Enemy(this, enemyData, enemyGO.transform);
            }

            inActiveWave = true;
        }

        private void FinalizeEnemyWave()
        {
            inActiveWave = false;
            foreach (var enemy in enemies)
                enemy.Clean();

            enemies = null;

            GameManager.Instance.AllEnemiesDestroyed();
        }
    }
}