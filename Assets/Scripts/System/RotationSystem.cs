using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

public class RotationSystem : JobComponentSystem
{
    EntityQuery m_Group_EastRotation;
    EntityQuery m_Group_WestRotation;
    EntityQuery m_Group_NorthRotation;
    EntityQuery m_Group_SouthRotation;

    protected override void OnCreate()
    {
        // It's defined the query for turning east (right) JOB
        var query_TurnEast = new EntityQueryDesc
        {
            //None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        typeof(Rotation),
                                        ComponentType.ReadOnly<LbEastDirection>(),
                                        ComponentType.ReadOnly<LbRotationSpeed>(),
                                        ComponentType.ReadOnly<Translation>()
                                      }
        };

        m_Group_EastRotation = GetEntityQuery( query_TurnEast );

        // It's defined the query for turning west (left) JOB
        var query_TurnWest = new EntityQueryDesc
        {
            //None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        typeof(Rotation),
                                        ComponentType.ReadOnly<LbWestDirection>(),
                                        ComponentType.ReadOnly<LbRotationSpeed>(),
                                        ComponentType.ReadOnly<Translation>()
                                      }
        };

        m_Group_WestRotation = GetEntityQuery( query_TurnWest );

        // It's defined the query for turning north (up) JOB
        var query_TurnNorth = new EntityQueryDesc
        {
            //None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        typeof(Rotation),
                                        ComponentType.ReadOnly<LbNorthDirection>(),
                                        ComponentType.ReadOnly<LbRotationSpeed>(),
                                        ComponentType.ReadOnly<Translation>()
                                      }
        };

        m_Group_NorthRotation = GetEntityQuery( query_TurnNorth );

        // It's defined the query for turning south (down) JOB
        var query_TurnSouth = new EntityQueryDesc
        {
            //None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        typeof(Rotation),
                                        ComponentType.ReadOnly<LbSouthDirection>(),
                                        ComponentType.ReadOnly<LbRotationSpeed>(),
                                        ComponentType.ReadOnly<Translation>()
                                      }
        };

        m_Group_SouthRotation = GetEntityQuery( query_TurnSouth );
    }

    [BurstCompile]
    public struct RotationJob : IJobChunk
    {
        public float    deltaTime;
        public float3   rotationDirection;

        [NativeDisableContainerSafetyRestriction] public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<LbRotationSpeed> rotationSpeedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            // Get information of a certain chunk
            var chunkRotations          = chunk.GetNativeArray( rotationType );
            var chunkRotationSpeed      = chunk.GetNativeArray( rotationSpeedType );

            for (var i = 0; i < chunk.Count; i++)
            {
                var rotation            = chunkRotations[ i ];
                var rotationSpeed       = chunkRotationSpeed[ i ];

                float3 forward          = math.forward( rotation.Value );
                float3 dir              = forward - (rotationDirection * deltaTime * rotationSpeed.Value);
                dir.y                   = 0.0f;

                rotation.Value = quaternion.LookRotation( dir, math.up() );

                chunkRotations[ i ] = rotation;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var job_RotationEast = new RotationJob
        {
            deltaTime               = deltaTime,
            rotationDirection       = new float3( 1f, 0, 0 ),

            rotationType            = GetArchetypeChunkComponentType<Rotation>(),
            rotationSpeedType       = GetArchetypeChunkComponentType<LbRotationSpeed>(true)
        }.Schedule(m_Group_EastRotation, inputDeps);

        
        var job_RotationWest = new RotationJob
        {
            deltaTime           = deltaTime,
            rotationDirection   = new float3( -1f, 0, 0 ),

            rotationType        = GetArchetypeChunkComponentType<Rotation>(),
            rotationSpeedType   = GetArchetypeChunkComponentType<LbRotationSpeed>(true)
        }.Schedule(m_Group_WestRotation, inputDeps);

        var job_RotationNorth = new RotationJob
        {
            deltaTime = deltaTime,
            rotationDirection = new float3(0, 0, 1),

            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            rotationSpeedType = GetArchetypeChunkComponentType<LbRotationSpeed>(true)
        }.Schedule(m_Group_NorthRotation, inputDeps);

        var job_RotationSouth = new RotationJob
        {
            deltaTime = deltaTime,
            rotationDirection = new float3(0, 0, -1),

            rotationType = GetArchetypeChunkComponentType<Rotation>(),
            rotationSpeedType = GetArchetypeChunkComponentType<LbRotationSpeed>(true)
        }.Schedule(m_Group_SouthRotation, inputDeps);

        var finalHandle = JobHandle.CombineDependencies(JobHandle.CombineDependencies(job_RotationEast, job_RotationWest, job_RotationNorth), job_RotationSouth);

        return finalHandle;
    }
}
