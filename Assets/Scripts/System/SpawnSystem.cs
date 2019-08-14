using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class SpawnSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    public struct SpawnJob : IJobForEachWithEntity<LbSpawner, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        public void Execute(Entity entity, int index, ref LbSpawner c0, ref Translation translation, ref Rotation rotation)
        {
            var instance = commandBuffer.Instantiate(index, c0.Prefab);
            commandBuffer.SetComponent(index, instance, new Translation{Value = translation.Value});
            commandBuffer.SetComponent(index, instance, new Rotation{Value = rotation.Value});
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        
        commandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
