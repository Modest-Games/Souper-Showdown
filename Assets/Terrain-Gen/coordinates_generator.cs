using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using NaughtyAttributes;

public class coordinates_generator: NetworkBehaviour
{
    // required vars
    public Vector2 gridDimensions = new Vector2(60, 40);
    public int numStartingPoints;

    int gridCoordinatesLength;
    int gridCoordinatesIndex;

    public Coords[,] gridCoordinates;
    public NeighbourItem[] thisNeighbourCoords;
    private float[] straightLine;

    // Object Types:
    private const string none = "none";
    private const string startingPoint = "starting point";
    private const string soup = "soup";
    private const string blender = "blender";
    private const string chefKnife = "chef's knife";
    private const string smallKnife = "small knife";

    // Prefabs:
    public GameObject smallKnifePrefab;
    public GameObject chefKnifePrefab;
    public GameObject blenderPrefab;

    // Object spawning sensitivies
    public int blenderRate;
    public int chefKnifeRate;
    public int saltPepperRate;

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


    public void GenerateTerrain()
    {
        
        gridCoordinates = new Coords[(int)gridDimensions.x, (int)gridDimensions.y];
        
        Coords startingCell = new Coords();
        startingCell.aliveBool = false;
        startingCell.objectType = none;

        // fill grid coordinates with coordinates
        for (int j = 0; j < (int)gridDimensions.y; j++ ) 
        {
            for (int i = 0; i < (int)gridDimensions.x ;  i++) 
            {
                gridCoordinates[i, j] = startingCell;
            }
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
            // set this cell to alive
            gridCoordinates[(int)coords.x, (int)coords.y].aliveBool = true;

            // define the object here to a starting point
            gridCoordinates[(int)coords.x, (int)coords.y].objectType = startingPoint;
        }

        // The algorithm will return this list of structs
        List<SpawnItem> envObjectsList = new List<SpawnItem>(); 

        // ================================ //
        //   Spawn in blenders (3x3 items)  //
        // ================================ //
        for (int j = 0 ; j < (int)gridDimensions.y ; j++ ) 
        {
            for (int i = 0 ; i < (int)gridDimensions.x ;  i++) 
            {
                // only check neighbours if this point is a starting point
                if (gridCoordinates[i, j].objectType == startingPoint) 
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
                        if (coordInfo.objectType == startingPoint)
                        {
                            numStartPoints++;
                        }
                        if (coordInfo.objectType == startingPoint || coordInfo.objectType == none)
                        {
                            numEmptyTiles++;
                        }
                    }

                    // spawn in a blender if there is an appropriate amount of empty cells as well as starting points in the 3x3 area
                    if (numStartPoints >= blenderRate && numEmptyTiles == 8) {

                        // update grid coordinates array
                        // set each neighbour's cell to alive and blender
                        foreach(NeighbourItem coordInfo in thisNeighbourCoords) 
                        {   
                            gridCoordinates[(int)coordInfo.location.x, (int)coordInfo.location.y].aliveBool = true;
                            gridCoordinates[(int)coordInfo.location.x, (int)coordInfo.location.y].objectType = blender;
                        }

                        // update the center cell in the gridCoordinates
                        gridCoordinates[i, j].aliveBool = false;
                        gridCoordinates[i, j].objectType = blender;

                        // Add this center cell to the spawn items list
                        SpawnItem thisItem = new SpawnItem();
                        thisItem.location = new Vector2(i, j);
                        thisItem.orientation = 0;
                        thisItem.objectType = blender;
                        envObjectsList.Add(thisItem);

                    }

                }
            }
        }
        // ================================ //
        //     End of Spawn Blender         //
        // ================================ //



        // ================================ //
        //     Spawn in chef's knife        //
        // ================================ //
        for (int j = 0 ; j < (int)gridDimensions.y ; j++ ) 
        {

            for (int i = 0 ; i < (int)gridDimensions.x ;  i++) 
            {
                // only check neighbours if this point is a starting point
                if (gridCoordinates[i, j].objectType == startingPoint) 
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
                    // we need this because we're only going to spawn a knife here if the points are colinear
                    List<Vector2> straightLine = new List<Vector2>();

                    foreach(NeighbourItem coordInfo in thisNeighbourCoords) 
                    {
                        if (coordInfo.objectType == startingPoint)
                        {
                            numStartPoints++;

                            // add this coordinate to the straight line (needs to be checked if it is colinear with other points)
                            Vector2 cellCoords = new Vector2(coordInfo.location.x, coordInfo.location.y);
                            straightLine.Add(cellCoords);

                        }
                        if (coordInfo.objectType == startingPoint || coordInfo.objectType == none)
                        {
                            numEmptyTiles++;
                        }
                    }

                    //Debug.Log(numStartPoints);

                    // if there are two starting points in the 3x3 area, check if they are colinear
                    if (numStartPoints == chefKnifeRate)
                    {

                        // add the current coords to be checked if they are colinear
                        Vector2 cellCoords = new Vector2(i, j);
                        straightLine.Add(cellCoords);

                        // check if these three points are colinear
                        if ((straightLine[2].y - straightLine[1].y) * (straightLine[1].x - straightLine[0].x) == (straightLine[1].y - straightLine[0].y) * (straightLine[2].x - straightLine[1].x)) 
                        {

                            int slope;

                            // get the slope (this will be used for the orientation)
                            if (straightLine[0].x == straightLine[1].x && straightLine[1].x == straightLine[2].x) 
                            {
                                // horizontal
                                slope = 0;
                            }
                            else if (straightLine[0].y == straightLine[1].y && straightLine[1].y == straightLine[2].y)
                            {
                                // vertical
                                slope = 2;
                            }
                            else 
                            {
                                // any other slope
                                slope = ((int)straightLine[2].y - (int)straightLine[1].y) / ((int)straightLine[2].x - (int)straightLine[1].x);
                            }

                            int orientation;

                            // get the orientation from the slope
                            switch(slope)
                            {
                                case 0:
                                    orientation = 0;
                                    break;
                                
                                case 2:
                                    orientation = 90;
                                    break;

                                case 1:
                                    orientation = 45;
                                    break;

                                case -1:
                                    orientation = 135;
                                    break;
                                
                                default:
                                    orientation = 0;
                                    break;
                            }

                            // update grid coordinates array
                            foreach(Vector2 coords in straightLine) 
                            {   
                                gridCoordinates[(int)coords.x, (int)coords.y].aliveBool = false;
                                gridCoordinates[(int)coords.x, (int)coords.y].objectType = chefKnife;
                            }

                            // add this item to the list of item to be spawned
                            // Add this center cell to the spawn items list 
                            SpawnItem thisItem = new SpawnItem();
                            thisItem.location = new Vector2(i, j);
                            thisItem.orientation = orientation;
                            thisItem.objectType = chefKnife;
                            envObjectsList.Add(thisItem);
                        }

                    }



                }
            }
        }
        // ================================ //
        //    End of Spawn chef's knife     //
        // ================================ //

        // ================================ //
        //       Spawn in 2x1 items         //
        // ================================ //
        for (int j = 0 ; j < (int)gridDimensions.y ; j++ ) 
        {

            for (int i = 0 ; i < (int)gridDimensions.x ;  i++) 
            {
                // only check neighbours if this point is a starting point
                if (gridCoordinates[i, j].objectType == startingPoint) 
                {
                    thisNeighbourCoords = new NeighbourItem[8];
                    int k = 0;
                    int neighbourCoordsIndex = 0;
                    
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

                            // if this neighbour cell is a starting point, remember its index. This will be used later for the orientation
                            if (cellInfo.objectType == startingPoint)
                            {
                                neighbourCoordsIndex = k;
                            }
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
                        if (coordInfo.objectType == startingPoint)
                        {
                            numStartPoints++;
                        }
                        if (coordInfo.objectType == startingPoint || coordInfo.objectType == none)
                        {
                            numEmptyTiles++;
                        }
                    }

                    if (numStartPoints == saltPepperRate)
                    {  
                        // get the orientation of the object
                        int orientation;
                        switch(neighbourCoordsIndex)
                        {
                            case 0:
                                orientation = 135;
                                break;
                            case 1:
                                orientation = 90;
                                break;
                            case 2:
                                orientation = 45;
                                break;
                            case 3:
                                orientation = 0;
                                break;
                            case 4:
                                orientation = 315;
                                break;
                            case 5:
                                orientation = 270;
                                break;
                            case 6:
                                orientation = 225;
                                break;
                            case 7:
                                orientation = 180;
                                break;
                            default:
                                orientation = 0;
                                break;
                        }

                        // update grid coordinates array
                        gridCoordinates[(int)thisNeighbourCoords[neighbourCoordsIndex].location.x, (int)thisNeighbourCoords[neighbourCoordsIndex].location.y].aliveBool = false;
                        gridCoordinates[(int)thisNeighbourCoords[neighbourCoordsIndex].location.x, (int)thisNeighbourCoords[neighbourCoordsIndex].location.y].objectType = "small knife";

                        gridCoordinates[i, j].aliveBool = false;
                        gridCoordinates[i, j].objectType = smallKnife;

                        // add this item to the list of item to be spawned
                        SpawnItem thisItem = new SpawnItem();
                        thisItem.location = new Vector2(i, j);
                        thisItem.orientation = orientation;
                        thisItem.objectType = smallKnife;
                        envObjectsList.Add(thisItem);
                    }


                }
            }
        }

        // ================================ //
        //     End of spawn 2x1 items       //
        // ================================ //

        // final object list
        //return envObjectsList;

        // print list of items

        foreach (SpawnItem item in envObjectsList) 
        {
            SpawnTerrain(item.location, item.orientation, item.objectType);
            SpawnTerrainClientRpc(item.location, item.orientation, item.objectType);
        }

        PositionTerrain();
        PositionTerrainClientRpc();
    }

    private void SpawnTerrain(Vector2 location, int orientation, string type)
    {
        GameObject toSpawn = null;
        switch (type)
        {
            case smallKnife:
                toSpawn = smallKnifePrefab;
                break;

            case chefKnife:
                toSpawn = chefKnifePrefab;
                break;

            case blender:
                toSpawn = blenderPrefab;
                break;
        }

        GameObject terrainPiece = Instantiate(toSpawn, new Vector3(location.x - (gridDimensions.x / 2), toSpawn.transform.position.y, location.y - (gridDimensions.y / 2)), Quaternion.Euler(0, orientation, 0));
        terrainPiece.transform.SetParent(transform);
    }

    private void PositionTerrain()
    {
        if (gameObject.name == "TerrainLeft")
        {
            transform.position = new Vector3(-17.5f, 0f, 0f);
        }

        else
        {
            transform.position = new Vector3(17.5f, 0f, 0f);
        }
    }

    [ClientRpc]
    private void SpawnTerrainClientRpc(Vector2 location, int orientation, string type)
    {
        if (IsServer && IsClient)
            return;

        SpawnTerrain(location, orientation, type);
    }

    [ClientRpc]
    private void PositionTerrainClientRpc()
    {
        if (IsServer && IsClient)
            return;

        PositionTerrain();
    }
}
