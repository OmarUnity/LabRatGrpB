using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct DestroyTag : IComponentData
{
}

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.
public class DestroySystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // Use the [BurstCompile] attribute to compile a job with Burst.
    // You may see significant speed ups, so try it!
    [BurstCompile]
    struct RotationSpeedJob : IJobForEachWithEntity<DestroyTag>
    {
        //public float DeltaTime;

        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref DestroyTag destroyTag)
        {
            CommandBuffer.DestroyEntity(jobIndex, entity);
        }
    }

    // OnUpdate runs on the main thread.
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();

        var job = new RotationSpeedJob
        {
            //DeltaTime = Time.deltaTime,
            CommandBuffer = commandBuffer,

        }.Schedule(this, inputDependencies);

        m_Barrier.AddJobHandleForProducer(job);

        return job;
    }
}
