using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

// This system updates all entities in the scene with LbLifetime component.
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class LifeTimeSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<LbSimulationBarrier>();
    }

    // Use the [BurstCompile] attribute to compile a job with Burst.
    //[BurstCompile]
    struct LifeTimeJob : IJobForEachWithEntity<LbLifetime>
    {
        public float DeltaTime;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int jobIndex, ref LbLifetime lifeTime)
        {
            lifeTime.Value -= DeltaTime;

            if (lifeTime.Value < 0.0f)
            {
                CommandBuffer.AddComponent(jobIndex, entity, new LbDestroy());
            }
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new LifeTimeJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = commandBuffer,
        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
