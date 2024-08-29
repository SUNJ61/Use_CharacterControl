using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyCtrl : MonoBehaviour
{
    private Transform Tr;
    private Transform PlayerTr;
    private List<Transform> WayList = new List<Transform>();
    private Animator animator;
    private NavMeshAgent agent;
    private enemyDamage state;

    private float attackDis = 3.5f;
    private float traceDis = 10.0f;
    private float traceSpeed = 2.0f;
    private float patrolSpeed = 1.0f;

    private int Index = 0;

    private bool isAttack = false;

    private readonly int hashAttack = Animator.StringToHash("isAttack");
    private readonly int hashTrace = Animator.StringToHash("isTrace");
    void Start()
    {
        Tr = transform;
        PlayerTr = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        state = GetComponent<enemyDamage>();

        var Point = GameObject.Find("WayPoint");
        if (Point != null)
            Point.GetComponentsInChildren<Transform>(WayList);
        WayList.RemoveAt(0);
    }
    void Update()
    {
        if (state.isDie) return;

        PatrolMove();
        PointUpdate();
    }

    private void PointUpdate()
    {
        float P_Dis = Vector3.Distance(Tr.position, WayList[Index].position);
        if (P_Dis < 1.5f)
        {
            if (Index == WayList.Count - 1)
                Index = 0;
            else
                Index++;
        }
    }

    private void PatrolMove()
    {
        float Dis = Vector3.Distance(PlayerTr.position, Tr.position);
        if (Dis < attackDis)
        {
            isAttack = true;
            agent.isStopped = true;
            animator.SetBool(hashAttack, true);
            animator.SetBool(hashTrace, true);
        }
        else if (Dis < traceDis)
        {
            isAttack = false;
            agent.isStopped = false;
            animator.SetBool(hashAttack, false);
            animator.SetBool(hashTrace, true);
            agent.SetDestination(PlayerTr.position);
            agent.speed = traceSpeed;
        }
        else
        {
            isAttack = false;
            agent.isStopped = false;
            animator.SetBool(hashAttack, false);
            animator.SetBool(hashTrace, false);
            agent.SetDestination(WayList[Index].position);
            agent.speed = patrolSpeed;
        }
    }
}
