using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class BoardAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Vector2Int Size;

    // The MonoBehaviour data is converted to ComponentData on the entity.
    // We are specifically transforming from a good editor representation of the data (Represented in degrees)
    // To a good runtime representation (Represented in radians)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new LbBoard { Size = new int2(Size.x, Size.y) });

        var cells = GetCells();
        var walls = GetWalls();

        // Example
        foreach(var cell in cells)
        {
            bool hasNorthWall = HasWall(walls, cell, Directions.North);
            bool hasSouthWall = HasWall(walls, cell, Directions.South);
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
