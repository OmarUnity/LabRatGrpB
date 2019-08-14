using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class BoardAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Vector2Int Size;

    private readonly Dictionary<byte, short> m_DirMap = new Dictionary<byte, short>()
    {
        {0x0, 0x1B},
        {0x1, 0x5B},
        {0x8, 0x2B},
        {0x2, 0x1F},
        {0x4, 0x18},
        {0x3, 0x5F},
        {0xC, 0x28},
        {0xD, 0xAA},
        {0xB, 0xFF},
        {0xE, 0x00},
        {0x7, 0x55},
        {0x9, 0xEB},
        {0xA, 0x0F},
        {0x6, 0x14},
        {0x5, 0x5A}
    };

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
        
        var dirMapEntity = dstManager.CreateEntity(typeof(LbDirectionMap));
        var dirMapbuffer = dstManager.GetBuffer<LbDirectionMap>(dirMapEntity);

        var cells = GetCells();
        var walls = GetWalls();

        foreach(var cell in cells)
        {
            var hasNorthWall = HasWall(walls, cell, Directions.North);
            var hasSouthWall = HasWall(walls, cell, Directions.South);
            var hasWestWall = HasWall(walls, cell, Directions.West);
            var hasEasrsWall = HasWall(walls, cell, Directions.East);

            var hasWallArray = new[]
            {
                HasWall(walls, cell, Directions.North),
                HasWall(walls, cell, Directions.South),
                HasWall(walls, cell, Directions.West),
                HasWall(walls, cell, Directions.East)
            };

            /*
             bool[] hasWallArray = new []
             {
                 true,
                 false,
                 true,
                 false
             };*/


            var bitArrayWalls = new BitArray(hasWallArray);

            var bytesWalls = new byte[1];
            bitArrayWalls.CopyTo(bytesWalls, 0);

            dirMapbuffer.Add(m_DirMap[bytesWalls[0]]);
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
    /// Get the list of all cell locations
    /// </summary>
    /// <returns></returns>
    private List<Vector2Int> GetCells()
	{
		List<Vector2Int> cells = new List<Vector2Int>();

        var cellObjs = GetComponentsInChildren<Cell>();
        foreach (var cell in cellObjs)
            cells.Add(cell.location);

		return cells;
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
