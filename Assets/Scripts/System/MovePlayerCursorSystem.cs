//using System;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using UnityEngine;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Random = Unity.Mathematics.Random;

//[UpdateInGroup(typeof(SimulationSystemGroup))]
//public class MovePlayerCursorSystem : JobComponentSystem
//{
//    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
//    private EntityQuery m_PlayerQuery;

//    protected override void OnCreate()
//    {
//        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        
//        var query = new EntityQueryDesc
//        {
//            None = new ComponentType[] {typeof(LbArrow)},
//            All = new ComponentType[] {typeof(LbPlayer), typeof(Translation),typeof(LbMovementSpeed),typeof(LbArrowPosition)}
//        };
//        m_PlayerQuery = GetEntityQuery(query);
//    }

//    struct MovePlayerCursorJob : IJobForEachWithEntity<LbPlayer,Translation,LbMovementSpeed,>
//    {
//        public float DeltaTime;
        
//        public EntityCommandBuffer.Concurrent CommandBuffer;

//        [BurstCompile]
//        public void Execute(Entity entity, int index, [ReadOnly] ref LbPlayer player, ref Translation translation,
//            [ReadOnly] ref LbMovementSpeed speed)
//        {
//            translation.Value += speed.Value * DeltaTime * (position.Value - translation.Value);
            
//            if (math.distancesq(translation.Value, position.Value) < 0.05f)
//            {
//                translation.Value = position.Value;
//                CommandBuffer.AddComponent(index, entity, new LbArrow());
//            }
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        var job = new MovePlayerCursorJob
//        {
//            DeltaTime = Time.deltaTime,
//            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(m_PlayerQuery, inputDependencies);
        
//        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

//        return job;
//    }
//}
