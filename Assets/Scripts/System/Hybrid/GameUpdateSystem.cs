﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// Update the game camera
/// </summary>
[UpdateAfter(typeof(DestroySystem))]
public class GameUpdateSystem : ComponentSystem
{
    GameTimer m_Timer;
    GameWinner m_Winner;
    EntityQuery m_Query;

    EntityQuery m_SpawnersQuery;
    EntityQuery m_MovementQuery;
    EntityQuery m_PlayerQuery;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(typeof(LbGameTimer));

        m_MovementQuery = GetEntityQuery(typeof(LbMovementTarget));
        m_SpawnersQuery = GetEntityQuery(typeof(LbSpawner));

        m_PlayerQuery = GetEntityQuery(typeof(LbPlayer), typeof(LbPlayerScore));
    }

    private bool Initialize<T>(ref T t) where T : MonoBehaviour
    {
        if (t == null)
        {
            t = GameObject.FindObjectOfType<T>();
            if (t == null)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Update the game
    /// </summary>
    protected override void OnUpdate()
    {
        if (Initialize<GameTimer>(ref m_Timer) || Initialize<GameWinner>(ref m_Winner))
            return;

        if (m_Timer == null)
        {
            m_Timer = GameObject.FindObjectOfType<GameTimer>();
            if (m_Timer == null)
                return;
        }

        bool hasFinish = false;
        Entities.ForEach((ref LbGameTimer timer) =>
        {
            timer.Value -= Time.deltaTime;
            if (timer.Value <= 0.0f)
            {
                timer.Value = 0.0f;
                hasFinish = true;
            }
            m_Timer.SetTime(timer.Value);
        });

        if (hasFinish)
        {
            m_Timer.SetTime(0.0f);
            EntityManager.DestroyEntity(m_Query);
            EntityManager.DestroyEntity(m_MovementQuery);
            EntityManager.DestroyEntity(m_SpawnersQuery);

            var resetTime = 20.0f;

            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new LbGameRestarter() { Value = resetTime });

            var winners = GetBestPlayerAndCleanUp();
            m_Winner.ShowWinners(winners, resetTime);
        }
    }

    private List<Players> GetBestPlayerAndCleanUp()
    {
        var winners = new List<Players>();

        var bestScore = -1;
        Entities.ForEach((ref LbPlayer player, ref LbPlayerScore score) =>
        {
            if (score.Value == bestScore)
            {
                winners.Add((Players)player.Value);
            }
            else if (score.Value > bestScore)
            {
                bestScore = score.Value;

                winners.Clear();
                winners.Add((Players)player.Value);
            }

            score.Value = 0;
        });

        return winners;
    }
}