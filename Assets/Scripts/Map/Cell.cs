using System;
using UnityEngine;

namespace DefaultNamespace.Map
{
    public enum CellContentType
    {
        Empty, Destination, Wall
    }
    
    public class Cell
    {
        public GridPos gridPos;
        public Grid grid;
        public GameObject gameObject;
        
        private Cell north, east, south, west, nextOnPath;

        private int distance;
        
        public bool _isAlternative { get; set; }

        public CellContentType contentType = CellContentType.Empty;

        public bool HasPath => distance != int.MaxValue;
        
        public Cell NextCellOnPath => nextOnPath;

        public bool isEnemyHere;

        public Cell(GridPos gridPos, Grid grid)
        {
            this.gridPos = gridPos;
            this.grid = grid;
        }
        
        static Quaternion
            northRotation = Quaternion.Euler(90f, 0f, 0f),
            eastRotation = Quaternion.Euler(90f, 90f, 0f),
            southRotation = Quaternion.Euler(90f, 180f, 0f),
            westRotation = Quaternion.Euler(90f, 270f, 0f);

        public static void MakeEastWestNeighbors(Cell east, Cell west)
        {
            Debug.Assert(west.east == null && east.west == null, "Redefined neighbors!");
            west.east = east;
            east.west = west;
        }

        public static void MakeNorthSouthNeighbors(Cell north, Cell south)
        {
            Debug.Assert(south.north == null && north.south == null, "Redefined neighbors!");
            south.north = north;
            north.south = south;
        }

        public void ClearPath()
        {
            distance = int.MaxValue;
            nextOnPath = null;
        }

        public void BecomeDestination()
        {
            distance = 0;
            nextOnPath = null;
            // Debug.Log($"X{gridPos.posX}Y{gridPos.posY} ||| nextOnPath X{nextOnPath?.gridPos.posX}Y{nextOnPath?.gridPos.posY} ||| distance {distance}");
        }

        public Cell GrowPathNorth() => GrowPathTo(north);
        public Cell GrowPathEast() => GrowPathTo(east);
        public Cell GrowPathWest() => GrowPathTo(west);
        public Cell GrowPathSouth() => GrowPathTo(south);

        private Cell GrowPathTo(Cell neighbor)
        {
            Debug.Assert(HasPath, "No Path!");
            if (neighbor == null || neighbor.HasPath) return null;
            neighbor.distance = distance + 1;
            neighbor.nextOnPath = this;
            return neighbor.contentType != CellContentType.Wall ? neighbor : null;
        }

        public void ShowPath()
        {
            var arrow = gameObject.transform.Find("arrow");
            if (arrow == null) return;
            
            // Debug.Log($"X{gridPos.posX}Y{gridPos.posY} ||| nextOnPath X{nextOnPath?.gridPos.posX}Y{nextOnPath?.gridPos.posY} ||| distance {distance}");
            if (distance == 0)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            if (contentType == CellContentType.Wall)
                arrow.gameObject.SetActive(false);
            else
                arrow.gameObject.SetActive(true);
            
            arrow.localRotation =
                nextOnPath == north ? northRotation :
                nextOnPath == east ? eastRotation :
                nextOnPath == south ? southRotation :
                westRotation;
        }
    }
}