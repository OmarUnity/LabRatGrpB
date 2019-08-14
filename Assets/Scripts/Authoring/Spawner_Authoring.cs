﻿using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequiresEntityConversion]
public class Spawner_Authoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject prefab;
    public int maxAmount;
    public int frecuency;


    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new LbSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(prefab),
            MaxAmount = maxAmount,
            Frequency = frecuency
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}