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

    private Dictionary<byte, short> dirmap = new Dictionary<byte, short>()
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
        dstManager.AddComponentData(entity, new LbBoard { Size = new int2(Size.x, Size.y) });
        Entity DirMapEntity = dstManager.CreateEntity(typeof(LbDirectionMap));
        
        DynamicBuffer<LbDirectionMap> dirMapbuffer = dstManager.GetBuffer<LbDirectionMap>(DirMapEntity);


        var cells = GetCells();
        var walls = GetWalls();

        // Example
        foreach(var cell in cells)
        {
            bool hasNorthWall = HasWall(walls, cell, Directions.North);
            bool hasSouthWall = HasWall(walls, cell, Directions.South);
            bool hasWestWall = HasWall(walls, cell, Directions.West);
            bool hasEasrsWall = HasWall(walls, cell, Directions.East);

           bool[] hasWallArray = new []
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
            
            
            BitArray bitArrayWalls = new BitArray(hasWallArray);
            
            byte[] bytesWalls = new byte[1];
            bitArrayWalls.CopyTo(bytesWalls, 0);
            
            Debug.Log(bytesWalls[0]);
            dirMapbuffer.Add(dirmap[bytesWalls[0]]);
            // TODO ....
        }
    }

    #region BOARD_MAP_GENERATION
    /// <summary>
    /// Get the list of all cell locations
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetCells()
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
    public Dictionary<Vector2Int, Wall> GetWalls()
    {
        Dictionary<Vector2Int, Wall> wallMap = new Dictionary<Vector2Int, Wall>();

        // TODO

        return wallMap;
    }

    /// <summary>
    /// Return true if there is a wall in the given direction starting in the given location
    /// </summary>
    public bool HasWall(Dictionary<Vector2Int, Wall> walls, Vector2Int location, Directions direction)
    {
        return true;
    }
    #endregion // BOARD_MAP_GENERATION
}
