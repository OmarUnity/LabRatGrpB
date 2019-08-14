using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.
public class FallSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // Use the [BurstCompile] attribute to compile a job with Burst.
    //[BurstCompile]
    struct FallTimeJob : IJobForEachWithEntity<LbLifetime>
    {
        public float DeltaTime;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int jobIndex, ref LbLifetime fallTime)
        {
            fallTime.Value -= DeltaTime;

            if (fallTime.Value < 0.0f)
            {
                CommandBuffer.AddComponent(jobIndex, entity, new LbDestroy());
                CommandBuffer.RemoveComponent<LbLifetime>(jobIndex,entity);
            }
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new FallTimeJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = commandBuffer,
        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
