using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class ArrowManagerSystem : JobComponentSystem
{
    private EntityQuery m_ArrowSpawerQuery;
    //private EntityQuery m_BoardQuery;
    EntityCommandBufferSystem m_EntityCommandBufferSystem;
        
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.Active.GetOrCreateSystem<LbSimulationBarrier>();
        m_ArrowSpawerQuery = GetEntityQuery(typeof(LbArrowSpawner),typeof(LbArrow));
       // m_BoardQuery = GetEntityQuery(typeof(LbBoard), typeof(LbArrowDirectionMap));
    }

    struct SpawnArrow : IJob
    {        
        [DeallocateOnJobCompletion]public NativeArray<Entity> Entities;
        [DeallocateOnJobCompletion]public NativeArray<LbArrowSpawner> ArrowSpawners;
        public EntityCommandBuffer CommandBuffer;

        public void Execute()
        {
            for (int i = 0; i < Entities.Length; i++)
            {
                var arrowSpawner = ArrowSpawners[i];
                var rotationDegrees = ArrowSpawners[i].Direction * 90;
                var instance = CommandBuffer.Instantiate(ArrowSpawners[i].Prefab);
                    
                ArrowSpawners[i] = arrowSpawner;
                CommandBuffer.AddComponent(instance, new LbArrow());
                CommandBuffer.AddComponent( instance, new LbLifetime { Value = 10f});
                CommandBuffer.SetComponent( instance, new Translation{Value = new float3(ArrowSpawners[i].Location.x,0.6f,ArrowSpawners[i].Location.z)});
                CommandBuffer.SetComponent( instance, new Rotation{Value = quaternion.EulerXYZ(math.radians(90),math.radians(rotationDegrees),math.radians(0))});
                CommandBuffer.RemoveComponent<LbArrow>(Entities[i]);
            }
        }
    }

    struct CheckArrow : IJob
    {
        public void Execute()
        {
            
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var entities = m_ArrowSpawerQuery.ToEntityArray(Allocator.TempJob);
        var arrowSpawners = m_ArrowSpawerQuery.ToComponentDataArray<LbArrowSpawner>(Allocator.TempJob);
        
        var handle = new SpawnArrow
        {
            Entities = entities,
            ArrowSpawners = arrowSpawners,
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer()
        }.Schedule(inputDeps);
        
        m_EntityCommandBufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }
}
