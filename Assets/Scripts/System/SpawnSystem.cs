using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SpawnSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<LbSpawner, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float DeltaTime;

        public void Execute(Entity entity, int index, ref LbSpawner lbSpawner, ref Translation translation, ref Rotation rotation)
        {
            lbSpawner.ElapsedTimeForMice += DeltaTime;
            lbSpawner.ElapsedTimeForCats += DeltaTime;
            var random = new Random(1).NextInt(0, 3);
            Debug.Log(random);
            
            if (lbSpawner.ElapsedTimeForMice > lbSpawner.MouseFrequency)
            {
                lbSpawner.ElapsedTimeForMice = 0;
                var mouseInstance = CommandBuffer.Instantiate(index, lbSpawner.MousePrefab);
                CommandBuffer.SetComponent(index, mouseInstance, new Translation{Value = translation.Value});
                CommandBuffer.SetComponent(index, mouseInstance, new Rotation{Value = rotation.Value});
                CommandBuffer.AddComponent<LbReachCell>(index, mouseInstance);
                if (random == 0)
                {
                    CommandBuffer.AddComponent<LbNorthDirection>(index, mouseInstance);
                }
                else if (random == 1)
                {
                    CommandBuffer.AddComponent<LbSouthDirection>(index, mouseInstance);
                }
                else if (random == 2)
                {
                    CommandBuffer.AddComponent<LbEastDirection>(index, mouseInstance);
                }
                else
                {
                    CommandBuffer.AddComponent<LbWestDirection>(index, mouseInstance);
                }
            }
            
            if (lbSpawner.ElapsedTimeForCats > lbSpawner.CatFrequency)
            {
                lbSpawner.ElapsedTimeForCats = 0;
                var catInstance = CommandBuffer.Instantiate(index, lbSpawner.CatPrefab);
                CommandBuffer.SetComponent(index, catInstance, new Translation{Value = translation.Value});
                CommandBuffer.SetComponent(index, catInstance, new Rotation{Value = rotation.Value});
                CommandBuffer.AddComponent<LbReachCell>(index, catInstance);
                if (random == 0)
                {
                    CommandBuffer.AddComponent<LbNorthDirection>(index, catInstance);
                }
                else if (random == 1)
                {
                    CommandBuffer.AddComponent<LbSouthDirection>(index, catInstance);
                }
                else if (random == 2)
                {
                    CommandBuffer.AddComponent<LbEastDirection>(index, catInstance);
                }
                else
                {
                    CommandBuffer.AddComponent<LbWestDirection>(index, catInstance);
                }
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var jobHandle = new SpawnJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);
        
        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
