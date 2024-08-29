using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardPlayer : MonoBehaviour
{
    [Header("컴퍼넌트")]
    private NavMeshAgent agent;
    private Animator animator;
    private Transform Tr;
    private W_PlayerDamage state;

    [Header("클릭")]
    public float m_DoubleClickSecond = 0.25f; // 클릭후 다음 클릭을 감지하는 시간 (더블클릭)

    public double m_Timer = 0.0d; // 시간 측정용 변수. (유니티에서는 정확한 시간을 측정하기 위해 double을 사용하기도 함.)

    public bool m_IsOneClick = false; // 한번 클릭이 입력되었는지 판단하는 변수
    [Header("변수")]
    public bool isAttack = false;
    public bool isSkill = false;

    private Ray ray;
    private RaycastHit hit;

    private Vector3 targetPos = Vector3.zero;

    private int GroundLayer;

    private bool getPosition_One = false;
    private bool getPosition_Double = false;

    private readonly int hashSpeed = Animator.StringToHash("moveSpeed");
    private readonly int hashSkiil = Animator.StringToHash("SkillTrigger");
    private readonly int hashAttack = Animator.StringToHash("AttackTrigger");
    void Start()
    {
        Tr = transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        state = GetComponent<W_PlayerDamage>();

        GroundLayer = LayerMask.NameToLayer("GROUND");
    }
    void Update()
    {
        if (state.isDie) return;

        ClickCheck();
        ClickMove();
        UpdateAnimator(); //이 함수가 애니매이션 전체 관리, move함수에 애니메이션 전부 비활성화
        AttackStop();
        SkillStop();
    }

    private void SkillStop()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isAttack && !isSkill)
        {
            animator.SetTrigger(hashSkiil);
            StartCoroutine(WaitAniSkill());
        }
    }

    private void AttackStop()
    {
        if (Input.GetKeyDown(KeyCode.A) && !isAttack && !isSkill)
        {
            animator.SetTrigger(hashAttack);
            StartCoroutine(WaitAniAttack());
        }
    }

    IEnumerator WaitAniAttack()
    {

        isAttack = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(1.2f);
        isAttack = false;
        agent.isStopped = false;
    }
    IEnumerator WaitAniSkill()
    {
        isSkill = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(2.2f);
        isSkill = false;
        agent.isStopped = false;
    }
    private void ClickMove()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition); //모바일은 Input.GetTouch(0).position를 사용.
        //광선은 카메라에서 카메라에서 보이는 마우스 포지션을 게임상의 3D좌표로 변경하여 그 위치까지 광선을 쏜다.

        #region 공격상태때 위치저장 후 이동 (내가 추가한 코드)
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << GroundLayer))
        {
            if (getPosition_One && !isAttack && !isSkill)
            {
                agent.speed = 1.5f;
                //animator.SetFloat(hashSpeed, agent.speed);
                agent.SetDestination(targetPos);
                agent.isStopped = false;

                getPosition_One = false;
            }
            else if (getPosition_Double && !isAttack && !isSkill)
            {
                agent.speed = 3.0f;
                //animator.SetFloat(hashSpeed, agent.speed);
                agent.SetDestination(targetPos);
                agent.isStopped = false;

                getPosition_Double = false;
            }
        }
        #endregion

        if (Input.GetMouseButtonDown(0)) //마우스 왼쪽클릭을 했을 때
        {
            if (!isAttack && !isSkill)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << GroundLayer)) //레이어가 바닥을 감지 하고
                {
                    if (m_IsOneClick) // 원클릭만 했다면 
                    {
                        agent.speed = 1.5f;
                        //animator.SetFloat(hashSpeed, agent.speed);
                    }
                    else if (!m_IsOneClick) // 더블클릭 했다면
                    {
                        agent.speed = 3.0f;
                        //animator.SetFloat(hashSpeed, agent.speed);
                    }
                    targetPos = hit.point; // 레이저가 맞은 위치값 저장.
                    agent.SetDestination(targetPos); // 해당 위치로 이동. Destination과 다른점?
                    agent.isStopped = false; //정지 상태를 해제, 추적시작
                }
            }
            #region 공격 스킬 사용했을 때 클릭정보, 위치정보저장 (내가 추가한 코드)
            if (isAttack || isSkill) //스킬 사용시 클릭했을 경우
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << GroundLayer))
                {
                    if (m_IsOneClick)
                    {
                        getPosition_One = true;
                        getPosition_Double = false;
                        targetPos = hit.point;
                    }
                    else if (!m_IsOneClick)
                    {
                        getPosition_Double = true;
                        getPosition_One = false;
                        targetPos = hit.point;
                    }
                }
            }
            #endregion
        }
        else //마우스를 누르지 않았을 때
        {
            #region 도착지 판별 1번째 방법
            //if (agent.remainingDistance <= 0.25f) //추적으로 입력된 위치와 거리가 0.25이하일 때
            //{
            //    agent.speed = 0f;
            //    animator.SetFloat(hashSpeed, agent.speed);
            //    agent.isStopped = true;
            //}
            #endregion
            #region 도착지 판별 2번째 방법
            if (Vector3.Distance(Tr.position, targetPos) <= 0.25f) //자기자신 위치와 입력된 위치사이 거리가 0.25 이하일 때
            {
                agent.speed = 0f;
                //animator.SetFloat(hashSpeed, agent.speed);
                agent.isStopped = true;
            }
            #endregion
        }
    }
    void UpdateAnimator()
    {
        animator.SetFloat(hashSpeed, agent.speed);
    }
    private void ClickCheck()
    {
        if (m_IsOneClick && (Time.time - m_Timer) > m_DoubleClickSecond) //한번만 클릭된다면
        {
            //Debug.Log("oneClick"); // 처음에는 if문 감지 x, 이후 클릭을 하면 아래에서 타임이 업데이트 되고, 0.25초가 지나면 원클릭으로 판명.
            m_IsOneClick = false; //클릭 판별이후 클릭 변수 false로 만듬.
        }
        if (Input.GetMouseButtonDown(0)) //마우스 왼쪽 클릭을 했다면
        {
            if (!m_IsOneClick) //이때 한번 클릭변수가 false라면
            {
                m_Timer = Time.time; // 현재시간을 저장
                m_IsOneClick = true; // 원클릭 변수를 true로 바꿈
            }
            else if (m_IsOneClick && (Time.time - m_Timer) < m_DoubleClickSecond) //원클릭 변수가 참이고 0.25초 이내에 한번더 클릭 했다면
            {
                //Debug.Log("Double Click"); // 0.25초 이내에 다시 클릭하면 처음 if문에 걸리지 않는다. 즉, 더블클릭 판명이 된다.
                m_IsOneClick = false; // 더블클릭 판명후 변수 false
            }
        }
    }
}
