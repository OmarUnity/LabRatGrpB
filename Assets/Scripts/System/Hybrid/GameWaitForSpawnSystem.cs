using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Update the game camera
/// </summary>
public class GameWaitForSpawnSystem : ComponentSystem
{
    GameIntro m_UIIntro;

    EntityQuery m_Query;
    EntityQuery m_GeneratorQuery;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(typeof(LbGameWaitForSpawn));
        m_GeneratorQuery = GetEntityQuery(typeof(LbBoardGenerator));
    }

    /// <summary>
    /// Update the game
    /// </summary>
    protected override void OnUpdate()
    {
        var generator = m_GeneratorQuery.GetSingleton<LbBoardGenerator>();

        bool spawnCreated = false;

        Entities.ForEach((ref LbGameWaitForSpawn waiter) =>
        {
            waiter.Value -= Time.deltaTime;
            if (waiter.Value <= 0.0f)
            {
                var spawnAll = EntityManager.CreateEntity();
                EntityManager.AddComponentData(spawnAll, new LbGameSpawnAll());

                spawnCreated = true;
            }
        });

        if (spawnCreated)
        {
            EntityManager.DestroyEntity(m_Query);
        }
    }
}