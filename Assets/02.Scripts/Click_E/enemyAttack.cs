using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class enemyAttack : MonoBehaviour
{
    [SerializeField]private GameObject DamBox;
    void Start()
    {
        DamBox = transform.GetChild(2).GameObject();
    }
    public void OnDamBox()
    {
        DamBox.SetActive(true);
    }
    public void OffDamBox()
    {
        DamBox.SetActive(false);
    }
}
