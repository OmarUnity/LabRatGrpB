using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class MovementSystem : JobComponentSystem
{
    // IJobChunk <- 
    public struct Move_Job : IJobForEachWithEntity<LbMovementSpeed, Translation>
    {
        public float  deltaTime;
        public float3 direction;

        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            float lastLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);
            //translation.Value.z += movementSpeed.Value * deltaTime;

            translation.Value += direction * movementSpeed.Value * deltaTime;

            float newLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);

            // When the Entity reach a new cell is add the Tag LbReachCell
            if ( Mathf.Abs( (int)(newLerp - lastLerp) ) == 1 )
            {
                commandBuffer.AddComponent( jobIndex, entity, new LbReachCell() );
            }
        }
    }

    EntityCommandBufferSystem   m_Barrier;

    EntityQuery                 m_Group_NorthMovement;
    EntityQuery                 m_Group_SouthMovement;
    EntityQuery                 m_Group_WestMovement;
    EntityQuery                 m_Group_EastMovement;

    protected override void OnCreate()
    {
        // The command buffer is created
        m_Barrier   = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        // It's defined the query for the NORTH JOB
        var query = new EntityQueryDesc
        {
            None = new ComponentType[]{ typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbNorthDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation)
                                      }
        };

        m_Group_NorthMovement = GetEntityQuery( query );

        // It's defined the query for the SOUTH JOB
        query = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbSouthDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation)
                                      }
        };

        m_Group_SouthMovement = GetEntityQuery(query);

        // It's defined the query for the WEST JOB
        query = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbWestDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation)
                                      }
        };

        m_Group_WestMovement = GetEntityQuery(query);

        // It's defined the query for the EAST JOB
        query = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(LbReachCell) },
            All = new ComponentType[] {
                                        ComponentType.ReadOnly<LbEastDirection>(),
                                        ComponentType.ReadOnly<LbMovementSpeed>(),
                                        typeof(Translation)
                                      }
        };

        m_Group_EastMovement = GetEntityQuery(query);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var moveNorth_Job = new Move_Job
        {
            deltaTime       = deltaTime,
            direction       = new float3(0, 0, 1),
            commandBuffer   = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_NorthMovement, inputDeps);

        var moveSouth_Job = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(0, 0, -1),
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_SouthMovement, moveNorth_Job);

        var moveWest_Job = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(-1, 0, 0),
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_WestMovement, moveSouth_Job);

        var moveEast_Job = new Move_Job
        {
            deltaTime = deltaTime,
            direction = new float3(1, 0, 0),
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_EastMovement, moveWest_Job);

        m_Barrier.AddJobHandleForProducer( moveNorth_Job );
        m_Barrier.AddJobHandleForProducer( moveSouth_Job );
        m_Barrier.AddJobHandleForProducer( moveWest_Job );
        m_Barrier.AddJobHandleForProducer( moveEast_Job );

        return moveEast_Job;
    }
}
