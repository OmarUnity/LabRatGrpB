using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

public class MovementSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    EntityQuery m_BoardQuery;
    EntityQuery m_Group_NorthMovement;
    EntityQuery m_Group_SouthMovement;
    EntityQuery m_Group_WestMovement;
    EntityQuery m_Group_EastMovement;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<LbSimulationBarrier>();
        m_BoardQuery = GetEntityQuery(typeof(LbBoard));

        m_Group_NorthMovement = GetEntityQuery(CreateQueryFor<LbNorthDirection>());
        m_Group_SouthMovement = GetEntityQuery(CreateQueryFor<LbSouthDirection>());
        m_Group_WestMovement = GetEntityQuery(CreateQueryFor<LbWestDirection>()); 
        m_Group_EastMovement = GetEntityQuery(CreateQueryFor<LbEastDirection>());
    }

    private EntityQueryDesc CreateQueryFor<T>() where T : IComponentData
    {
        var queryDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[]
            {
                ComponentType.ReadOnly<T>(),
                ComponentType.ReadOnly<LbMovementSpeed>(),
                typeof(Translation),
                ComponentType.ReadWrite<LbDistanceToTarget>()
            }
        };
        return queryDesc;
    }

    [BurstCompile]
    public struct Move_Job : IJobChunk
    {
        public float DeltaTime;
        public float3 Direction;
        public int2 BoardSize;

        [NativeDisableContainerSafetyRestriction] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<LbMovementSpeed> MovementSpeedType;
        [NativeDisableContainerSafetyRestriction] public ArchetypeChunkComponentType<LbDistanceToTarget> DistanceToTargetType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(TranslationType);
            var chunkMovementSpeed = chunk.GetNativeArray(MovementSpeedType);
            var chunkDistanceToTarget = chunk.GetNativeArray(DistanceToTargetType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var movementSpeed = chunkMovementSpeed[i].Value;

                var translation = chunkTranslations[i];
                var distanceToTarget = chunkDistanceToTarget[i];

                var position = translation.Value + Direction * movementSpeed * DeltaTime;
                position.x = math.clamp(position.x, 0.0f, BoardSize.x - 1);
                position.z = math.clamp(position.z, 0.0f, BoardSize.y - 1);
                translation.Value = position;
                chunkTranslations[i] = translation;

                distanceToTarget.Value -= movementSpeed * DeltaTime;
                if (distanceToTarget.Value < 0.0f)
                {
                    distanceToTarget.Value = 0.0f;
                }
                chunkDistanceToTarget[i] = distanceToTarget; 
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);
        var board = m_BoardQuery.GetSingleton<LbBoard>();
        var boardSize = new int2(board.SizeX, board.SizeY);

        var job_North = new Move_Job
        {
            DeltaTime = deltaTime,
            Direction = new float3(0, 0, 1),
            BoardSize = boardSize,

            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            MovementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            DistanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_NorthMovement, inputDeps);

        var job_South = new Move_Job
        {
            DeltaTime = deltaTime,
            Direction = new float3(0, 0, -1),
            BoardSize = boardSize,

            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            MovementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            DistanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_SouthMovement, inputDeps);

        var job_West = new Move_Job
        {
            DeltaTime = deltaTime,
            Direction = new float3(-1, 0, 0),
            BoardSize = boardSize,

            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            MovementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            DistanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_WestMovement, inputDeps);

        var job_East = new Move_Job
        {
            DeltaTime = deltaTime,
            Direction = new float3(1, 0, 0),
            BoardSize = boardSize,

            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            MovementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            DistanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_EastMovement, inputDeps);

        var finalHandle = JobHandle.CombineDependencies(JobHandle.CombineDependencies(job_North, job_South, job_East), job_West);
        m_Barrier.AddJobHandleForProducer(finalHandle);

        return finalHandle;
    }
}