using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Unity.Collections;

public class SpawnSystem : JobComponentSystem
{
    enum InstanceType
    {
        Cat, Mouse
    }
    
    private EntityCommandBufferSystem _commandBufferSystem;
    private Random _random = new Random(1);
    
    protected override void OnCreate()
    {
        _commandBufferSystem = World.GetOrCreateSystem<LbSimulationBarrier>();
    }

    struct SpawnJob : IJobForEachWithEntity<LbSpawner, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float DeltaTime;
        public int RandomNumber;

        public void Execute(Entity entity, int index, ref LbSpawner lbSpawner, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
        {
            var location = translation.Value;

            lbSpawner.ElapsedTimeForMice += DeltaTime;
            lbSpawner.ElapsedTimeForCats += DeltaTime;

            if (lbSpawner.ElapsedTimeForMice > lbSpawner.MouseFrequency)
            {
                lbSpawner.ElapsedTimeForMice = 0;
                DoSpawn(index, location, ref rotation, ref lbSpawner.MousePrefab, 4.0f, InstanceType.Mouse);
            }
            
            if (lbSpawner.ElapsedTimeForCats > lbSpawner.CatFrequency)
            {
                lbSpawner.ElapsedTimeForCats = 0;
                DoSpawn(index, location, ref rotation, ref lbSpawner.CatPrefab, 1.0f, InstanceType.Cat);
            }
        }

        private void DoSpawn(int index, float3 translation, ref Rotation rotation, ref Entity entityType, float speed, InstanceType instanceType)
        {
            var instance = CommandBuffer.Instantiate(index, entityType);

            CommandBuffer.SetComponent(index, instance, new Translation{ Value = translation });
            CommandBuffer.SetComponent(index, instance, new Rotation{ Value = rotation.Value });

            CommandBuffer.AddComponent<LbRotationSpeed>(index, instance);
            CommandBuffer.AddComponent(index, instance, new LbMovementSpeed { Value = speed });
            CommandBuffer.AddComponent(index, instance, new LbMovementTarget() { From = translation, To = translation });
            CommandBuffer.AddComponent(index, instance, new LbDistanceToTarget { Value = 1.0f });

            CommandBuffer.AddComponent(index, instance, new LbDirection() { Value = (byte)RandomNumber });
                
            if (instanceType == InstanceType.Cat)
            {
                CommandBuffer.AddComponent<LbCat>(index, instance);
                CommandBuffer.AddComponent(index, instance, new LbLifetime() {Value = 30.0f });
            }
            else if (instanceType == InstanceType.Mouse)
            {
                CommandBuffer.AddComponent<LbRat>(index, instance);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var jobHandle = new SpawnJob
        {
            DeltaTime = Time.deltaTime,
            CommandBuffer = _commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            RandomNumber = _random.NextInt(0, 4)
        }.Schedule(this, inputDeps);
        
        _commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
