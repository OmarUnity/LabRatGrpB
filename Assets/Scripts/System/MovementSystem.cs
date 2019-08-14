using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;

public class MovementSystem : JobComponentSystem
{
    public struct MoveUp_Job : IJobForEach<LbNorthDirection, LbMovementSpeed, Translation>
    {
        public float deltaTime;

        public void Execute([ReadOnly] ref LbNorthDirection upDirection, [ReadOnly] ref LbMovementSpeed movementSpeed, ref Translation translation)
        {
            translation.Value.z += movementSpeed.Value * deltaTime;

            Debug.Log(Mathf.Lerp(translation.Value.z, Mathf.Round(translation.Value.z), movementSpeed.Value) );
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moveUp_Job = new MoveUp_Job {
            deltaTime = Time.deltaTime
        };

        return moveUp_Job.Schedule(this, inputDeps);
    }
}
