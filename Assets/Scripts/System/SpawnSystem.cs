using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SpawnSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem _commandBufferSystem;
    private Random _random = new Random(1);
    
    protected override void OnCreate()
    {
        _commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<LbSpawner, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float DeltaTime;
        public int randomNumber;

        public void Execute(Entity entity, int index, ref LbSpawner lbSpawner, ref Translation translation, ref Rotation rotation)
        {
            lbSpawner.ElapsedTimeForMice += DeltaTime;
            lbSpawner.ElapsedTimeForCats += DeltaTime;
            
            if (lbSpawner.ElapsedTimeForMice > lbSpawner.MouseFrequency)
            {
                lbSpawner.ElapsedTimeForMice = 0;
                DoSpawn(index, ref translation, ref rotation, ref lbSpawner.MousePrefab, 2.0f);
                
            }
            
            if (lbSpawner.ElapsedTimeForCats > lbSpawner.CatFrequency)
            {
                lbSpawner.ElapsedTimeForCats = 0;
                DoSpawn(index, ref translation, ref rotation, ref lbSpawner.CatPrefab, 1.0f);
            }
        }

        private void DoSpawn(int index, ref Translation translation, ref Rotation rotation, ref Entity entityType, float speed)
        {
            var instance = CommandBuffer.Instantiate(index, entityType);
            CommandBuffer.SetComponent(index, instance, new Translation{Value = translation.Value});
            CommandBuffer.SetComponent(index, instance, new Rotation{Value = rotation.Value});
            CommandBuffer.AddComponent<LbReachCell>(index, instance);
            if (randomNumber == 0)
            {
                CommandBuffer.AddComponent<LbNorthDirection>(index, instance);
            }
            else if (randomNumber == 1)
            {
                CommandBuffer.AddComponent<LbSouthDirection>(index, instance);
            }
            else if (randomNumber == 2)
            {
                CommandBuffer.AddComponent<LbEastDirection>(index, instance);
            }
            else
            {
                CommandBuffer.AddComponent<LbWestDirection>(index, instance);
            }
                
            CommandBuffer.AddComponent<LbMovementSpeed>( index, instance);
            CommandBuffer.AddComponent<LbDistanceToTarget>( index, instance );
                
            CommandBuffer.SetComponent(index, instance, new LbMovementSpeed{ Value = speed});
            CommandBuffer.SetComponent(index, instance, new LbDistanceToTarget{ Value = 1});
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var jobHandle = new SpawnJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = _commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            randomNumber = _random.NextInt(0, 4)
        }.Schedule(this, inputDeps);
        
        _commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
