using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshAgentExtension
{
    
    //Option to set destination and stop the coroutine
    public static bool SetDestination(this NavMeshAgent agent, MonoBehaviour sender, Vector3 target, bool stopFollowCoroutine = true)
    {
        if (stopFollowCoroutine)
        {
            // Stop any existing follow coroutine
            sender.StopCoroutine("FollowTargetCoroutine");
        }

        // Call the original SetDestination method
        return agent.SetDestination(target);
    }

    // Follow based on frames
    public static void FollowTransform(this NavMeshAgent agent, MonoBehaviour sender, Transform target, float stoppingDistance, int updateEveryFrames = 10)
    {
        sender.StopCoroutine("FollowTargetCoroutine");
        sender.StartCoroutine(FollowTargetCoroutine(agent, target, stoppingDistance, updateEveryFrames));
    }

    // Follow based on time interval
    public static void FollowTransform(this NavMeshAgent agent, MonoBehaviour sender, Transform target, float stoppingDistance, float updateIntervalSeconds)
    {
        sender.StopCoroutine("FollowTargetCoroutine");
        sender.StartCoroutine(FollowTargetCoroutine(agent, target, stoppingDistance, updateIntervalSeconds));
    }

    // Coroutine for following based on frames
    private static IEnumerator FollowTargetCoroutine(NavMeshAgent agent, Transform target, float stoppingDistance, int updateEveryFrames)
    {
        while (agent != null && target != null)
        {
            if (agent.isStopped || !agent.isOnNavMesh || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                yield break;
            }

            agent.SetDestination(target.position);

            if (agent.remainingDistance <= stoppingDistance && !agent.pathPending)
            {
                agent.SetDestination(agent.transform.position);
                yield break;
            }

            // Wait for the specified number of frames
            for (int i = 0; i < updateEveryFrames; i++)
            {
                yield return null;
            }
        }
    }

    // Coroutine for following based on time interval
    private static IEnumerator FollowTargetCoroutine(NavMeshAgent agent, Transform target, float stoppingDistance, float updateIntervalSeconds)
    {
        while (agent != null && target != null)
        {
            if (agent.isStopped || !agent.isOnNavMesh || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                yield break;
            }

            agent.SetDestination(target.position);

            if (agent.remainingDistance <= stoppingDistance && !agent.pathPending)
            {
                agent.SetDestination(agent.transform.position);
                yield break; break;
            }

            // Wait for the specified time interval
            yield return new WaitForSeconds(updateIntervalSeconds);
        }
    }
}
