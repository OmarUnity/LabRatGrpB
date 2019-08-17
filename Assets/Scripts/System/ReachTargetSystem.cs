using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class ReachTargetSystem : JobComponentSystem
{
    private EntityCommandBufferSystem m_Barrier;
    private NativeQueue<Entity> m_Queue;

    [BurstCompile]
    public struct ReachTargetJob : IJobForEachWithEntity<LbDistanceToTarget>
    {
        public NativeQueue<Entity>.ParallelWriter Queue;

        public void Execute(Entity entity, int index, [ReadOnly] ref LbDistanceToTarget distanceToTarget)
        {
            if (distanceToTarget.Value >= 1.0f)
            {   
                Queue.Enqueue(entity);
            }
        }
    }

    public struct ReachTargetCleanJob : IJob
    {
        public NativeQueue<Entity> Queue;
        public EntityCommandBuffer CommandBuffer;
        
        public void Execute()
        {
            while (Queue.Count > 0)
            {
                CommandBuffer.AddComponent(Queue.Dequeue(), new LbReachCell());
            }
        }
    }

    protected override void OnCreate()
    {
        m_Barrier = World.Active.GetOrCreateSystem<LbSimulationBarrier>();
        m_Queue = new NativeQueue<Entity>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        m_Queue.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var handle = new ReachTargetJob {
            Queue = m_Queue.AsParallelWriter(),
        }.Schedule(this, inputDeps);

        handle = new ReachTargetCleanJob()
        {
            Queue = m_Queue,
            CommandBuffer = m_Barrier.CreateCommandBuffer()
        }.Schedule(handle);
        
        m_Barrier.AddJobHandleForProducer( handle );

        return handle;
    }
}
