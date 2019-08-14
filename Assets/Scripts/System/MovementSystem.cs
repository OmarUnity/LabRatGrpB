using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class MovementSystem : JobComponentSystem
{
    EntityCommandBufferSystem   m_Barrier;

    EntityQuery                 m_Group;
    EntityQuery                 m_Group_NorthMovement;
    EntityQuery                 m_Group_SouthMovement;
    EntityQuery                 m_Group_WestMovement;
    EntityQuery                 m_Group_EastMovement;

    protected override void OnCreate()
    {
        // The command buffer is created
        m_Barrier   = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // It's defined the query for the NORTH JOB
        var query_North = new EntityQueryDesc
        {
            None = new ComponentType[]{ typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbNorthDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation),
                                        ComponentType.ReadWrite<LbDistanceToTarget>()
                                      }
        };

        m_Group_NorthMovement = GetEntityQuery( query_North );

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

        m_Group_SouthMovement = GetEntityQuery( query_South );

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

        m_Group_WestMovement = GetEntityQuery( query_West );

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

        m_Group_EastMovement = GetEntityQuery( query_East );

        m_Group = GetEntityQuery(new EntityQueryDesc[] { query_North, query_South, query_West, query_East });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var job_North = new Move_Job
        {
            deltaTime           = deltaTime,
            direction           = new float3(0, 0, 1),
            //commandBuffer       = m_Barrier.CreateCommandBuffer().ToConcurrent(),

            translationType     = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType   = GetArchetypeChunkComponentType<LbMovementSpeed>( true ),
            distanceToTargetType     = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        };

        var job_South = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(0, 0, -1),
            
            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType     = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        };

        var job_West = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(-1, 0, 0),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType     = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        };

        var job_East = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(1, 0, 0),

            translationType = GetArchetypeChunkComponentType<Translation>(),
            movementSpeedType = GetArchetypeChunkComponentType<LbMovementSpeed>(true),
            distanceToTargetType     = GetArchetypeChunkComponentType<LbDistanceToTarget>()
        };

        inputDeps = job_North.Schedule( m_Group_NorthMovement, inputDeps );
        inputDeps = job_South.Schedule( m_Group_SouthMovement, inputDeps );
        inputDeps = job_West.Schedule ( m_Group_WestMovement, inputDeps );
        inputDeps = job_East.Schedule ( m_Group_EastMovement, inputDeps );

        return inputDeps;
    }
}

public struct Move_Job : IJobChunk
{
    public float deltaTime;
    public float3 direction;

    public ArchetypeChunkComponentType<Translation>                 translationType;
    [ReadOnly] public ArchetypeChunkComponentType<LbMovementSpeed>  movementSpeedType;
    public ArchetypeChunkComponentType<LbDistanceToTarget>          distanceToTargetType;
    
    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    {
        // Get information of a certain chunk
        var chunkTranslations   = chunk.GetNativeArray( translationType );
        var chunkMovementSpeed  = chunk.GetNativeArray( movementSpeedType );
        var chunkDistanceToTarget  = chunk.GetNativeArray( distanceToTargetType );

        for (var i = 0; i < chunk.Count; i++)
        {
            var translation         = chunkTranslations[ i ];
            var movementSpeed       = chunkMovementSpeed[ i ];
            var distanceToTarget    = chunkDistanceToTarget[ i ];
            
            translation.Value += direction * movementSpeed.Value * deltaTime;
            distanceToTarget.Value -= movementSpeed.Value * deltaTime;
            
            chunkDistanceToTarget[i] = distanceToTarget;
            chunkTranslations[i] = translation;
            
            if (distanceToTarget.Value <= 0)
            {
                translation.Value = math.round(translation.Value);
                chunkTranslations[i] = translation;
                // TODO LbReachCell
            }
        }
    }
}
