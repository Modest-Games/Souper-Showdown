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
    public struct Coords 
    {
        public bool aliveBool;
        public string objectType;
    }

    public struct SpawnItems
    {
        public Vector2 location;
        public int orientation;
        public string objectType;
    }

    // neighbour coordinates of every cell
    int[,] neighbourCoords = new int[,] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { -1, 0 }, { 1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 } };

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

        // rest of the soup coordinates
        // float[,] soupCoords = new float[,] { 
        //     {soupX, soupY}, {soupX + 1, soupY}, {soupX + 2, soupY}, {soupX + 3, soupY},
        //     {soupX, soupY + 1}, {soupX + 1, soupY + 1}, {soupX + 2, soupY + 1}, {soupX + 3, soupY + 1},
        //     {soupX, soupY + 2}, {soupX + 1, soupY + 2}, {soupX + 2, soupY + 2}, {soupX + 3, soupY + 2},
        //     {soupX, soupY + 3}, {soupX + 1, soupY + 3}, {soupX + 2, soupY + 3}, {soupX + 3, soupY + 3}
        // };

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

        // ================================ //
        //   Spawn in blenders (3x3 items)  //
        // ================================ //




        Debug.Log(startingPoints[0]);
    }


}
