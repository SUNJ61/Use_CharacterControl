using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemyDamage : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capCol;

    [SerializeField]private float HP = 100.0f;
    private float MaxHp = 100.0f;

    public bool isDie = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capCol = GetComponent<CapsuleCollider>();
        HP = MaxHp;
        Mathf.Clamp(HP, 0f,MaxHp);
    }
    private void DieAni()
    {
        animator.SetTrigger("isDie");
        rb.isKinematic = true;
        capCol.enabled = false;
    }

    private void OnDamage(object[] _params)
    {
        HP -= (float)_params[1];
        if ((float)_params[1] == 50.0f) //스킬로 맞았을 때 맞은 이펙트
        {

        }
        else //일반 공격으로 맞았을 때 맞은 이펙트
        {

        }
        if (HP <= 0)
        {
            isDie = true;
            DieAni();
        }
    }
}
