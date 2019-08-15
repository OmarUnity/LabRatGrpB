using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Transforms;

// This system updates all entities in the scene with Translation, LbMovementSpeed and LbFall component.
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class FallSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // Use the [BurstCompile] attribute to compile a job with Burst.
    //[BurstCompile]
    struct FallJob : IJobForEachWithEntity<Translation, LbMovementSpeed, LbFall>
    {
        public float DeltaTime;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int jobIndex, ref Translation translation, ref LbMovementSpeed speed, [ReadOnly] ref LbFall fallTag)
        {
            speed.Value -= 4.9f * DeltaTime; // 9.8/2
            translation.Value.y += speed.Value * DeltaTime;
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new FallJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = commandBuffer,
        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
