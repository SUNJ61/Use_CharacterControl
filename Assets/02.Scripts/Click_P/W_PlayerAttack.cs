using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_PlayerAttack : MonoBehaviour
{
    private WizardPlayer state;
    private Transform Tr;

    private float AttackDis = 100.0f;
    private float AttackDelay = 1.2f;
    private float SkillDelay = 2.2f;
    private float prevTime;
    private float attackDam = 10.0f;
    private float skillDam = 50.0f;

    private readonly string enemyTag = "Enemy";
    void Start()
    {
        state = GetComponent<WizardPlayer>();
        Tr = transform;
        prevTime = Time.time;
    }
    void Update()
    {
        if(state.isAttack &&  Time.time - prevTime > AttackDelay)
        {
            Ray ray = new Ray(Tr.position + (Vector3.up * 1.5f), Tr.forward);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * AttackDis, Color.green);
            if(Physics.Raycast(ray, out hit, AttackDis))
            {
                if (hit.collider.gameObject.tag == enemyTag)
                {
                    object[] _params = new object[2];
                    _params[0] = hit.point;
                    _params[1] = attackDam;
                    hit.collider.gameObject.SendMessage("OnDamage", _params, SendMessageOptions.DontRequireReceiver);
                }
            }
            prevTime = Time.time;
        }
        else if(state.isSkill && Time.time - prevTime > SkillDelay)
        {
            Ray ray = new Ray(Tr.position + (Vector3.up * 1.5f), Tr.forward);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * AttackDis, Color.red);
            if (Physics.Raycast(ray, out hit, AttackDis))
            {
                object[] _params = new object[2];
                _params[0] = hit.point;
                _params[1] = skillDam;
                hit.collider.gameObject.SendMessage("OnDamage", _params, SendMessageOptions.DontRequireReceiver);
            }
            prevTime = Time.time;
        }
    }
}
