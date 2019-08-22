using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class PlayerCursor_Authoring : MonoBehaviour,IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public enum Player { One = 0, Two = 1, Three = 2, Four = 3 };
    public GameObject PlayerPrefab;
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
            dstManager.AddComponentData(entity, new LbPlayer {Value = (byte)PlayerId, PrefabArrow = conversionSystem.GetPrimaryEntity(PlayerPrefab)});
            dstManager.AddComponentData(entity, new LbArrow());
            dstManager.AddComponentData(entity, new LbMovementSpeed {Value = 1f});
            dstManager.AddComponentData(entity, new LbArrowPosition {Value = Position});
        }
        else
        {
            dstManager.AddComponentData(entity, new LbBoard {SizeX = 10, SizeY = 10});
        }
    }
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(PlayerPrefab);
    }
}
