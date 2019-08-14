using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;

public class MovementSystem : JobComponentSystem
{
    public struct MoveNorth_Job : IJobForEachWithEntity<LbNorthDirection, LbMovementSpeed, Translation>
    {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbNorthDirection northDirection, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            float lastLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);
            translation.Value.z += movementSpeed.Value * deltaTime;
            float newLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);

            // When the Entity reach a new cell is add the Tag LbReachCell
            if ( Mathf.Abs( (int)(newLerp - lastLerp) ) == 1 )
            {
                commandBuffer.AddComponent( jobIndex, entity, new LbReachCell() );
            }
        }
    }

    public struct MoveSouth_Job : IJobForEachWithEntity<LbSouthDirection, LbMovementSpeed, Translation>
    {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbSouthDirection southDirection, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            float lastLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);
            translation.Value.z -= movementSpeed.Value * deltaTime;
            float newLerp = Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value);

            // When the Entity reach a new cell is add the Tag LbReachCell
            if (Mathf.Abs( (int)(newLerp - lastLerp) ) == 1)
            {
                commandBuffer.AddComponent(jobIndex, entity, new LbReachCell());
            }
        }
    }

    public struct MoveWest_Job : IJobForEachWithEntity<LbWestDirection, LbMovementSpeed, Translation>
    {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbWestDirection westDirection, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            float lastLerp = Mathf.Lerp(translation.Value.x, Mathf.Round(translation.Value.x), movementSpeed.Value);
            translation.Value.x -= movementSpeed.Value * deltaTime;
            float newLerp = Mathf.Lerp(translation.Value.x, Mathf.Round(translation.Value.x), movementSpeed.Value);

            // When the Entity reach a new cell is add the Tag LbReachCell
            if (Mathf.Abs((int)(newLerp - lastLerp)) == 1)
            {
                commandBuffer.AddComponent(jobIndex, entity, new LbReachCell());
            }
        }
    }

    public struct MoveEast_Job : IJobForEachWithEntity<LbEastDirection, LbMovementSpeed, Translation>
    {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbEastDirection eastDirection, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            float lastLerp = Mathf.Lerp(translation.Value.x, Mathf.Round(translation.Value.x), movementSpeed.Value);
            translation.Value.x += movementSpeed.Value * deltaTime;
            float newLerp = Mathf.Lerp(translation.Value.x, Mathf.Round(translation.Value.x), movementSpeed.Value);

            // When the Entity reach a new cell is add the Tag LbReachCell
            if (Mathf.Abs((int)(newLerp - lastLerp)) == 1)
            {
                commandBuffer.AddComponent(jobIndex, entity, new LbReachCell());
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

        var moveNorth_Job = new MoveNorth_Job
        {
            deltaTime       = deltaTime,
            commandBuffer   = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_NorthMovement, inputDeps);

        var moveSouth_Job = new MoveSouth_Job
        {
            deltaTime = deltaTime,
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_SouthMovement, moveNorth_Job);

        var moveWest_Job = new MoveWest_Job
        {
            deltaTime = deltaTime,
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_WestMovement, moveSouth_Job);

        var moveEast_Job = new MoveEast_Job
        {
            deltaTime = deltaTime,
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_Group_EastMovement, moveWest_Job);

        m_Barrier.AddJobHandleForProducer( moveNorth_Job );
        m_Barrier.AddJobHandleForProducer( moveSouth_Job );
        m_Barrier.AddJobHandleForProducer( moveWest_Job );
        m_Barrier.AddJobHandleForProducer( moveEast_Job );

        return moveEast_Job;
    }
}
