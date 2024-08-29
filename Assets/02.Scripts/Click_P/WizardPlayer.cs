using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardPlayer : MonoBehaviour
{
    [Header("���۳�Ʈ")]
    private NavMeshAgent agent;
    private Animator animator;
    private Transform Tr;
    private W_PlayerDamage state;

    [Header("Ŭ��")]
    public float m_DoubleClickSecond = 0.25f; // Ŭ���� ���� Ŭ���� �����ϴ� �ð� (����Ŭ��)

    public double m_Timer = 0.0d; // �ð� ������ ����. (����Ƽ������ ��Ȯ�� �ð��� �����ϱ� ���� double�� ����ϱ⵵ ��.)

    public bool m_IsOneClick = false; // �ѹ� Ŭ���� �ԷµǾ����� �Ǵ��ϴ� ����
    [Header("����")]
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
        UpdateAnimator(); //�� �Լ��� �ִϸ��̼� ��ü ����, move�Լ��� �ִϸ��̼� ���� ��Ȱ��ȭ
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
        ray = Camera.main.ScreenPointToRay(Input.mousePosition); //������� Input.GetTouch(0).position�� ���.
        //������ ī�޶󿡼� ī�޶󿡼� ���̴� ���콺 �������� ���ӻ��� 3D��ǥ�� �����Ͽ� �� ��ġ���� ������ ���.

        #region ���ݻ��¶� ��ġ���� �� �̵� (���� �߰��� �ڵ�)
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

        if (Input.GetMouseButtonDown(0)) //���콺 ����Ŭ���� ���� ��
        {
            if (!isAttack && !isSkill)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << GroundLayer)) //���̾ �ٴ��� ���� �ϰ�
                {
                    if (m_IsOneClick) // ��Ŭ���� �ߴٸ� 
                    {
                        agent.speed = 1.5f;
                        //animator.SetFloat(hashSpeed, agent.speed);
                    }
                    else if (!m_IsOneClick) // ����Ŭ�� �ߴٸ�
                    {
                        agent.speed = 3.0f;
                        //animator.SetFloat(hashSpeed, agent.speed);
                    }
                    targetPos = hit.point; // �������� ���� ��ġ�� ����.
                    agent.SetDestination(targetPos); // �ش� ��ġ�� �̵�. Destination�� �ٸ���?
                    agent.isStopped = false; //���� ���¸� ����, ��������
                }
            }
            #region ���� ��ų ������� �� Ŭ������, ��ġ�������� (���� �߰��� �ڵ�)
            if (isAttack || isSkill) //��ų ���� Ŭ������ ���
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
        else //���콺�� ������ �ʾ��� ��
        {
            #region ������ �Ǻ� 1��° ���
            //if (agent.remainingDistance <= 0.25f) //�������� �Էµ� ��ġ�� �Ÿ��� 0.25������ ��
            //{
            //    agent.speed = 0f;
            //    animator.SetFloat(hashSpeed, agent.speed);
            //    agent.isStopped = true;
            //}
            #endregion
            #region ������ �Ǻ� 2��° ���
            if (Vector3.Distance(Tr.position, targetPos) <= 0.25f) //�ڱ��ڽ� ��ġ�� �Էµ� ��ġ���� �Ÿ��� 0.25 ������ ��
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
        if (m_IsOneClick && (Time.time - m_Timer) > m_DoubleClickSecond) //�ѹ��� Ŭ���ȴٸ�
        {
            //Debug.Log("oneClick"); // ó������ if�� ���� x, ���� Ŭ���� �ϸ� �Ʒ����� Ÿ���� ������Ʈ �ǰ�, 0.25�ʰ� ������ ��Ŭ������ �Ǹ�.
            m_IsOneClick = false; //Ŭ�� �Ǻ����� Ŭ�� ���� false�� ����.
        }
        if (Input.GetMouseButtonDown(0)) //���콺 ���� Ŭ���� �ߴٸ�
        {
            if (!m_IsOneClick) //�̶� �ѹ� Ŭ�������� false���
            {
                m_Timer = Time.time; // ����ð��� ����
                m_IsOneClick = true; // ��Ŭ�� ������ true�� �ٲ�
            }
            else if (m_IsOneClick && (Time.time - m_Timer) < m_DoubleClickSecond) //��Ŭ�� ������ ���̰� 0.25�� �̳��� �ѹ��� Ŭ�� �ߴٸ�
            {
                //Debug.Log("Double Click"); // 0.25�� �̳��� �ٽ� Ŭ���ϸ� ó�� if���� �ɸ��� �ʴ´�. ��, ����Ŭ�� �Ǹ��� �ȴ�.
                m_IsOneClick = false; // ����Ŭ�� �Ǹ��� ���� false
            }
        }
    }
}
