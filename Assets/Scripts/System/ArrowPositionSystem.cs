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
        //m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        //var query = new EntityQueryDesc
        //{
        //    //None = new ComponentType[] {typeof(LbArrow)},
        //    All = new ComponentType[] {typeof(LbPlayer), typeof(LbArrowPosition), typeof(LbArrow)}
        //};
        //m_PlayerQuery = GetEntityQuery(query);

        //m_BoardQuery = GetEntityQuery(typeof(LbBoard));
        
        //_random = new Random();
        //_random.InitState();
    }

    struct SpawnArrow : IJob//ForEachWithEntity<LbArrowPosition,LbPlayer,LbArrow>
    {
        public int Seed;
        public int Size;
        public int RandomNumber;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        //[DeallocateOnJobCompletion]public NativeArray<Entity> Entities;
        //[DeallocateOnJobCompletion]public NativeArray<LbArrowPosition> ArrowPositions;
        //[DeallocateOnJobCompletion]public NativeArray<LbPlayer> Players;
        //[DeallocateOnJobCompletion] public NativeArray<LbCursorArrow> ;
        //[DeallocateOnJobCompletion] public NativeArray<LbDirection> Players;


        public void Execute()
        {
            //for (int i = 0; i < Entities.Length; i++)
            {
                
//                var random = new Random((uint) ( Seed+i ));
//                var position = new float3(random.NextInt(0, Size), 1, random.NextInt(0, Size));
            
//                var instance = CommandBuffer.Instantiate(i,Players[i].PrefabArrow);
//                CommandBuffer.AddComponent(i,instance,new LbLifetime { Value = 10f});
           
//                CommandBuffer.SetComponent(i, instance, new Translation{Value = new float3(ArrowPositions[i].Value.x,0.6f,ArrowPositions[i].Value.z)});
//                var arrowPos = ArrowPositions[i];
//                arrowPos.Value = position;
//                ArrowPositions[i] = arrowPos;
                
//                CommandBuffer.SetComponent(i,Entities[i],ArrowPositions[i]);
////               
//                CommandBuffer.SetComponent(i, instance, new Rotation{Value = quaternion.EulerXYZ(math.radians(90),math.radians(90*RandomNumber),math.radians(0))});
//                CommandBuffer.RemoveComponent<LbArrow>(i,Entities[i]);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return inputDependencies;
        //LbBoard board = m_BoardQuery.GetSingleton<LbBoard>();
        //var entities = m_PlayerQuery.ToEntityArray(Allocator.TempJob);
        //var arrowPositions = m_PlayerQuery.ToComponentDataArray<LbArrowPosition>(Allocator.TempJob);
        //var players = m_PlayerQuery.ToComponentDataArray<LbPlayer>(Allocator.TempJob);

        //var sRand = new System.Random();
        
        //var job = new SpawnArrow
        //{
        //    Seed = sRand.Next(int.MaxValue),
        //    Size = board.SizeY,
        //    RandomNumber = _random.NextInt(0, 4),
        //    Entities = entities,
        //    ArrowPositions = arrowPositions,
        //    Players = players,
        //    CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        //}.Schedule(inputDependencies);
        
        //m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
        //return job;
    }
}
