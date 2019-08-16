using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class PlayerCursor_Authoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum Player { One, Two, Three, Four };
    public Player PlayerId = Player.One;
    public float3 Position = float3.zero;
    public bool isPlayer = true;
    
    // The MonoBehaviour data is converted to ComponentData on the entity.
    // We are specifically transforming from a good editor representation of the data (Represented in degrees)
    // To a good runtime representation (Represented in radians)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (isPlayer)
        {
            if (PlayerId == Player.One)
                dstManager.AddComponentData(entity, new LbPlayer {Value = 0});
            if (PlayerId == Player.Two)
                dstManager.AddComponentData(entity, new LbPlayer {Value = 1});
            if (PlayerId == Player.Three)
                dstManager.AddComponentData(entity, new LbPlayer {Value = 2});
            if (PlayerId == Player.Four)
                dstManager.AddComponentData(entity, new LbPlayer {Value = 3});
            dstManager.AddComponentData(entity, new LbArrow());
            dstManager.AddComponentData(entity, new LbMovementSpeed {Value = 1f});
            dstManager.AddComponentData(entity, new LbArrowPosition {Value = Position});
        }
        else
        {
            dstManager.AddComponentData(entity, new LbBoard {SizeX = 10, SizeY = 10});
        }
    }
}
