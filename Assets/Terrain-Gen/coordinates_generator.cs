using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class coordinates_generator: MonoBehaviour
{
    // required vars
    public Vector2 gridDimensions = new Vector2(30, 25);
    public int numStartingPoints;

    int gridCoordinatesLength;
    int gridCoordinatesIndex;

    public Coords[,] gridCoordinates;
    public NeighbourItem[] thisNeighbourCoords;

    public struct Coords 
    {
        public bool aliveBool;
        public string objectType;
    }

    public struct NeighbourItem 
    {
        public Vector2 location;
        public bool aliveBool;
        public string objectType;
    }

    public struct SpawnItem
    {
        public Vector2 location;
        public int orientation;
        public string objectType;
    }

    // neighbour coordinates of every cell
    //int[,] neighbourCoords = new int[,] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { -1, 0 }, { 1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 } };
    Vector2[,] neighbourCoords = { 
        {new Vector2(-1, -1)},  {new Vector2(0, -1)},       {new Vector2(1, -1)}, 
        {new Vector2(-1, 0)},                               {new Vector2(1, 0 )}, 
        {new Vector2(-1, 1)},   {new Vector2(0, 1)},        {new Vector2(1, 1)}
    };


    [Button]

    // Start is called before the first frame update
    void GenerateTerrain()
    {

        gridCoordinates = new Coords[(int)gridDimensions.x, (int)gridDimensions.y];
        
        Coords startingCell = new Coords();
        startingCell.aliveBool = false;
        startingCell.objectType = "none";

        // fill grid coordinates with coordinates
        for (int j = 0 ; j < (int)gridDimensions.y ; j++ ) 
        {
            for (int i = 0 ; i < (int)gridDimensions.x ;  i++) 
            {
                gridCoordinates[i, j] = startingCell;
            }
        }

        // soup coordinates and add these to the grid array
        // top left corner of the soup
        float soupX = Mathf.Ceil((int)gridDimensions.x / 2) - 1;
        float soupY = Mathf.Ceil((int)gridDimensions.y / 2) - 1;

        Vector2[,] soupCoords = {
            {new Vector2(soupX, soupY)}, {new Vector2(soupX + 1, soupY)}, {new Vector2(soupX + 2, soupY)}, {new Vector2(soupX + 3, soupY)},
            {new Vector2(soupX, soupY + 1)}, {new Vector2(soupX + 1, soupY + 1)}, {new Vector2(soupX + 2, soupY)}, {new Vector2(soupX + 3, soupY)},
            {new Vector2(soupX, soupY + 2)}, {new Vector2(soupX + 1, soupY + 2)}, {new Vector2(soupX + 2, soupY)}, {new Vector2(soupX + 3, soupY)},
            {new Vector2(soupX, soupY + 3)}, {new Vector2(soupX + 1, soupY + 3 )}, {new Vector2(soupX + 2, soupY)}, {new Vector2(soupX + 3, soupY)}
        };

        // update gridCoordinates array to reflect the soup coordinates
        // this will be used later to ensure that no item is spawned in the soup area
        foreach(Vector2 coords in soupCoords)
        {
            // set this cell to alive
            gridCoordinates[(int)coords.x, (int)coords.y].aliveBool = true;

            // define the object here to the soup
            gridCoordinates[(int)coords.x, (int)coords.y].objectType = "soup";

            //Debug.Log(gridCoordinates[(int)coords.x, (int)coords.y].aliveBool);
        }

        // initial spawn points
        List<Vector2> startingPoints = new List<Vector2>();
        for (int i = 0 ; i < numStartingPoints ; i++)
        {
            Vector2 newPoint = new Vector2(Random.Range(0, (int)gridDimensions.x), Random.Range(0, (int)gridDimensions.y));
            startingPoints.Add(newPoint);
        }

        // update grid coordinates
        foreach(Vector2 coords in startingPoints) 
        {
            // this point only becomes a starting point if it's not in the soup (we don't want anything spawning where the soup is)
            if (gridCoordinates[(int)coords.x, (int)coords.y].objectType == "none") {

                // set this cell to alive
                gridCoordinates[(int)coords.x, (int)coords.y].aliveBool = true;

                // define the object here to a starting point
                gridCoordinates[(int)coords.x, (int)coords.y].objectType = "starting point";
            }
        }

        // The algorithm will return this array of structs
        List<SpawnItem> envObjectsList = new List<SpawnItem>(); 

        // ================================ //
        //   Spawn in blenders (3x3 items)  //
        // ================================ //
        for (int j = 0 ; j < (int)gridDimensions.y ; j++ ) 
        {
            for (int i = 0 ; i < (int)gridDimensions.x ;  i++) 
            {
                // only check neighbours if this point is a starting point
                if (gridCoordinates[i, j].objectType == "starting point") 
                {
                    thisNeighbourCoords = new NeighbourItem[8];
                    int k = 0;

                    // construct neighbour coords list
                    foreach(Vector2 coords in neighbourCoords) 
                    {
                        int cellsNeighboursX = (int)coords.x + i;
                        int cellsNeighboursY = (int)coords.y + j;

                        // clamp values so that non-existing points are not added to the list
                        if (cellsNeighboursX >= 0 && cellsNeighboursX < (int)gridDimensions.x && cellsNeighboursY >= 0 && cellsNeighboursY < (int)gridDimensions.y)
                        {
                            NeighbourItem cellInfo = new NeighbourItem();
                            cellInfo.location = new Vector2(cellsNeighboursX, cellsNeighboursY);
                            cellInfo.aliveBool = gridCoordinates[cellsNeighboursX, cellsNeighboursY].aliveBool;
                            cellInfo.objectType = gridCoordinates[cellsNeighboursX, cellsNeighboursY].objectType;
                            thisNeighbourCoords[k] = cellInfo;
                        }

                        k++;
                        
                    }

                    // loop through neighbour coordinates
                    // if they are a starting point, increment 
                    int numStartPoints = 0;
                    // we have to check if all the coords around are empty. If not, we can't spawn a blender there
                    int numEmptyTiles = 0;

                    foreach(NeighbourItem coordInfo in thisNeighbourCoords) 
                    {
                        if (coordInfo.objectType == "starting point")
                        {
                            numStartPoints++;
                        }
                        if (coordInfo.objectType == "starting point" || coordInfo.objectType == "none")
                        {
                            numEmptyTiles++;
                        }
                    }

                    // spawn in a blender if there is an appropriate amount of empty cells as well as starting points in the 3x3 area
                    if (numStartPoints >= 4 && numEmptyTiles == 8) {

                        // update grid coordinates array
                        // set each neighbour's cell to alive and blender
                        foreach(NeighbourItem coordInfo in thisNeighbourCoords) 
                        {   
                            gridCoordinates[(int)coordInfo.location.x, (int)coordInfo.location.y].aliveBool = true;
                            gridCoordinates[(int)coordInfo.location.x, (int)coordInfo.location.y].objectType = "blender";
                        }

                        // update the center cell in the gridCoordinates
                        gridCoordinates[i, j].aliveBool = true;
                        gridCoordinates[i, j].objectType = "blender";

                        // Add this center cell to the spawn items list
                        SpawnItem thisItem = new SpawnItem();
                        thisItem.location = new Vector2(i, j);
                        thisItem.orientation = 0;
                        thisItem.objectType = "blender";

                        Debug.Log(thisItem.location);

                    }

                }
            }
        }



        
    }


}
