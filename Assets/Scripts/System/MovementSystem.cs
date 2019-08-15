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

    EntityQuery m_Group_NorthMovement;
    EntityQuery m_Group_SouthMovement;
    EntityQuery m_Group_WestMovement;
    EntityQuery m_Group_EastMovement;

    protected override void OnCreate()
    {
        // The command buffer is created
        m_Barrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // It's defined the query for the NORTH JOB
        var query_North = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbNorthDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation),
                                        ComponentType.ReadWrite<LbDistanceToTarget>()
                                      }
        };

        m_Group_NorthMovement = GetEntityQuery(query_North);

        // It's defined the query for the SOUTH JOB
        var query_South = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbSouthDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation),
                                        ComponentType.ReadWrite<LbDistanceToTarget>()
                                      }
        };

        m_Group_SouthMovement = GetEntityQuery(query_South);

        // It's defined the query for the WEST JOB
        var query_West = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbWestDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation),
                                        ComponentType.ReadWrite<LbDistanceToTarget>()
                                      }
        };

        m_Group_WestMovement = GetEntityQuery(query_West);

        // It's defined the query for the EAST JOB
        var query_East = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbEastDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation),
                                        ComponentType.ReadWrite<LbDistanceToTarget>()
                                      }
        };

        m_Group_EastMovement = GetEntityQuery(query_East);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var job_North = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(0, 0, 1),
            //commandBuffer       = m_Barrier.CreateCommandBuffer().ToConcurrent(),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_NorthMovement, inputDeps);

        var job_South = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(0, 0, -1),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_SouthMovement, inputDeps);

        var job_West = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(-1, 0, 0),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_WestMovement, inputDeps);

        var job_East = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(1, 0, 0),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        }.Schedule(m_Group_EastMovement, inputDeps);

        var finalHandle = JobHandle.CombineDependencies(JobHandle.CombineDependencies(job_North, job_South, job_East), job_West);
        m_Barrier.AddJobHandleForProducer(finalHandle);

        return finalHandle;
    }
}

[BurstCompile]
public struct Move_Job : IJobChunk
{
    public float deltaTime;
    public float3 direction;

    [NativeDisableContainerSafetyRestriction] public ArchetypeChunkComponentType<Translation> translationType;
    [ReadOnly] public ArchetypeChunkComponentType<LbMovementSpeed> movementSpeedType;
    [NativeDisableContainerSafetyRestriction] public ArchetypeChunkComponentType<LbDistanceToTarget> distanceToTargetType;

    // If you need to reach some Entity from an IJobChunk, 
    //[ReadOnly] public ArchetypeChunkComponentType<Entity> entityType;

    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    {
        // Get information of a certain chunk
        var chunkTranslations = chunk.GetNativeArray(translationType);
        var chunkMovementSpeed = chunk.GetNativeArray(movementSpeedType);
        var chunkDistanceToTarget = chunk.GetNativeArray(distanceToTargetType);
        //var chunkEntity             = chunk.GetNativeArray( entityType );

        for (var i = 0; i < chunk.Count; i++)
        {
            var translation = chunkTranslations[i];
            var movementSpeed = chunkMovementSpeed[i];
            var distanceToTarget = chunkDistanceToTarget[i];

            translation.Value += direction * movementSpeed.Value * deltaTime;
            distanceToTarget.Value -= movementSpeed.Value * deltaTime;

            chunkDistanceToTarget[i] = distanceToTarget;
            chunkTranslations[i] = translation;

            if (distanceToTarget.Value <= 0)
            {
                translation.Value = math.round(translation.Value);
                chunkTranslations[i] = translation;

                distanceToTarget.Value = 0;
                chunkDistanceToTarget[i] = distanceToTarget;
            }
        }
    }
}