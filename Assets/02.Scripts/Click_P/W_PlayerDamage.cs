using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_PlayerDamage : MonoBehaviour
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
        Mathf.Clamp(HP, 0f, MaxHp);
    }
    private void DieAni()
    {
        animator.SetTrigger("isDie");
        rb.isKinematic = true;
        capCol.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("DamBox")) //여기다가 이펙트 발생,삭제 넣기
        {
            HP -= 10.0f;
            if (HP <= 0)
            {
                isDie = true;
                DieAni();
            }
        }
    }
}
