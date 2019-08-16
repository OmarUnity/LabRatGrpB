using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class ReachTargetSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    public struct ReachTargetJob : IJobForEachWithEntity<LbDistanceToTarget>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref LbDistanceToTarget distanceToTarget)
        {
            if ( distanceToTarget.Value == 0 )
            {
                commandBuffer.AddComponent( index, entity, new LbReachCell() );
                distanceToTarget.Value = 1.0f;
            }
        }
    }

    protected override void OnCreate()
    {
        m_Barrier = World.Active.GetOrCreateSystem<LbSimulationBarrier>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ReachTargetJob {
            commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);

        m_Barrier.AddJobHandleForProducer( job );

        return job;
    }
}
