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
public class ArrowPositionSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    private EntityQuery m_BoardQuery;
    private EntityQuery m_PlayerQuery;
    private Random _random;
    
    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        var query = new EntityQueryDesc
        {
            //None = new ComponentType[] {typeof(LbArrow)},
            All = new ComponentType[] {typeof(LbPlayer), typeof(LbArrowPosition), typeof(LbArrow)}
        };
        m_PlayerQuery = GetEntityQuery(query);

        m_BoardQuery = GetEntityQuery(typeof(LbBoard));
        
        _random = new Random();
        _random.InitState();
    }

    struct MovePlayerCursorJob : IJobForEachWithEntity<LbArrowPosition,LbPlayer>
    {
        public int Seed;
        public int Size;
        
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, ref LbArrowPosition arrowPosition,[ReadOnly] ref LbPlayer player)
        {
            var random = new Random((uint) ( Seed+index ));
            var position = new float3(random.NextInt(0, Size), 1, random.NextInt(0, Size));
            arrowPosition.Value = position;
            
            CommandBuffer.SetComponent(index,entity,arrowPosition);
            CommandBuffer.RemoveComponent<LbArrow>(index,entity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        LbBoard board = m_BoardQuery.GetSingleton<LbBoard>();

        var sRand = new System.Random();
        
        var job = new MovePlayerCursorJob
        {
            Seed = sRand.Next(int.MaxValue),
            Size = board.SizeY,
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_PlayerQuery, inputDependencies);
        
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
