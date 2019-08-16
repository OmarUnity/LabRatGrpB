﻿using System.Collections.Generic;
using System.Collections;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class BoardAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Vector2Int Size;

    private readonly byte[] m_DirList =
    {
        0x1B,
        0x5B,
        0x1F,
        0x5F,
        0x18,
        0x5A,
        0x14,
        0x55,
        0x2B,
        0xEB,
        0x0F,
        0xFF,
        0x28,
        0xAA,
        0x00,
    };

    private const short kHoleFlag = 0x100;
    private const short kHomebaseFlag = 0x800;

    // The MonoBehaviour data is converted to ComponentData on the entity.
    // We are specifically transforming from a good editor representation of the data (Represented in degrees)
    // To a good runtime representation (Represented in radians)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new LbBoard
        {
            SizeX = (byte)Size.x,
            SizeY = (byte)Size.y
        });

		var arrowMapBuffer = dstManager.AddBuffer<LbArrowMap>(entity);
		var dirMapbuffer = dstManager.AddBuffer<LbDirectionMap>(entity);
		

        var cells = GetCellMap();
        var walls = GetWalls();

        for (int y = 0; y < Size.y; ++y)
            for (int x=0; x<Size.x; ++x)
        {

                var cellLocation = new Vector2Int(x, y);
                var cell = cells[cellLocation];

            var hasWallArray = new[]
            {
                HasWall(walls, cellLocation, Directions.North),
                HasWall(walls, cellLocation, Directions.South),
                HasWall(walls, cellLocation, Directions.West),
                HasWall(walls, cellLocation, Directions.East)
            };

            var bitArrayWalls = new BitArray(hasWallArray);

            var bytesWalls = new byte[1];
            bitArrayWalls.CopyTo(bytesWalls, 0);

            short bufferValue = (short)m_DirList[bytesWalls[0]];
            if (cell.isHole)
            {
                bufferValue |= kHoleFlag;
            }

            if (cell.homebase != null)
            {
                var homebase = cell.homebase.GetComponent<HomebaseAuthoring>();
                switch (homebase.Player)
                {
                    case Players.Player2:
                        bufferValue |= LbPlayer.kPlayer2Flag;
                        break;

                    case Players.Player3:
                        bufferValue |= LbPlayer.kPlayer3Flag;
                        break;

                    case Players.Player4:
                        bufferValue |= LbPlayer.kPlayer4Flag;
                        break;
                }

                bufferValue |= kHomebaseFlag;
            }

            dirMapbuffer.Add(bufferValue);
		}
    }

    #region BOARD_MAP_GENERATION
    /// <summary>
    /// Data to track walls in a board location
    /// </summary>
    public struct WallData
    {
        public Wall Vertical;
        public Wall Horizontal;
    }

    /// <summary>
    /// Get the map of cells
    /// </summary>
    /// <returns></returns>
    public Dictionary<Vector2Int, Cell> GetCellMap()
    {
        var map = new Dictionary<Vector2Int, Cell>();

        var cellObjs = GetComponentsInChildren<Cell>();
        foreach (var cell in cellObjs)
            map.Add(cell.location, cell);

        return map;
    }

    /// <summary>
    /// Get the walls map
    /// </summary>
    public Dictionary<Vector2Int, WallData> GetWalls()
    {
        Dictionary<Vector2Int, WallData> wallMap = new Dictionary<Vector2Int, WallData>();

        var walls = GetComponentsInChildren<Wall>();
        foreach (var wall in walls)
            AddWallToDictionary(ref wallMap, wall);

        return wallMap;
    }

    /// <summary>
    /// Add a wall to the dictionary
    /// </summary>
    /// <param name="Map"></param>
    /// <param name="wall"></param>
    public static void AddWallToDictionary(ref Dictionary<Vector2Int, WallData> Map, Wall wall)
    {
        var location = wall.location;
        if (!Map.ContainsKey(location))
        {
            Map.Add(location, new WallData());
        }

        var data = Map[location];
        if (wall.isHorizontal)
        {
            data.Horizontal = wall;
        }
        else
        {
            data.Vertical = wall;
        }

        Map[location] = data;
    }

    /// <summary>
    /// Return true if there is a wall in the given direction starting in the given location
    /// </summary>
    public static bool HasWall(Dictionary<Vector2Int, WallData> walls, Vector2Int location, Directions direction)
    {
        switch(direction)
        {
            case Directions.North:
                location += Vector2Int.up;
                break;

            case Directions.East:
                location += Vector2Int.right;
                break;
        }

        if (walls.ContainsKey(location))
        {
            var data = walls[location];
            var wall = (direction == Directions.North || direction == Directions.South) ? data.Horizontal : data.Vertical;
            return wall != null;
        }
        return false;
    }

    /// <summary>
    /// Call some tests
    /// </summary>
    private void DoTests(Dictionary<Vector2Int, WallData> walls)
    {
        TestWall(walls, 0, 0, Directions.South, true);
        TestWall(walls, 0, 0, Directions.East, false);
        TestWall(walls, 0, 0, Directions.West, true);
        TestWall(walls, 0, 0, Directions.North, false);

        TestWall(walls, 1, 0, Directions.South, true);
        TestWall(walls, 1, 0, Directions.East, false);
        TestWall(walls, 1, 0, Directions.West, false);
        TestWall(walls, 1, 0, Directions.North, false);

        TestWall(walls, 0, 1, Directions.South, false);
        TestWall(walls, 0, 1, Directions.East, false);
        TestWall(walls, 0, 1, Directions.West, true);
        TestWall(walls, 0, 1, Directions.North, false);

        TestWall(walls, Size.x - 1, 0, Directions.South, true);
        TestWall(walls, Size.x - 1, 0, Directions.East, true);
        TestWall(walls, Size.x - 1, 0, Directions.West, false);
        TestWall(walls, Size.x - 1, 0, Directions.North, false);

        TestWall(walls, 0, Size.y - 1, Directions.South, false);
        TestWall(walls, 0, Size.y - 1, Directions.East, false);
        TestWall(walls, 0, Size.y - 1, Directions.West, true);
        TestWall(walls, 0, Size.y - 1, Directions.North, true);

        TestWall(walls, Size.x - 1, Size.y - 1, Directions.South, false);
        TestWall(walls, Size.x - 1, Size.y - 1, Directions.East, true);
        TestWall(walls, Size.x - 1, Size.y - 1, Directions.West, false);
        TestWall(walls, Size.x - 1, Size.y - 1, Directions.North, true);
    }

    /// <summary>
    /// Test is the wall generation was right
    /// </summary>
    private void TestWall(Dictionary<Vector2Int, WallData> walls, int X, int Y, Directions direction, bool hasWall)
    {
        var location = new Vector2Int(X, Y);
        var has = HasWall(walls, location, direction);

        if (has != hasWall)
        {
            Debug.LogError("Invalid WallTest in " + location + " for direction: " + direction + " expected: " + hasWall + " but received:" + has);
        }
    }

    #endregion // BOARD_MAP_GENERATION
}
