using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private Transform CamTr;
    [SerializeField] private Transform CamPivot;
    [SerializeField] private Transform target;

    [SerializeField] private float height = 5.0f;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float targetOffset = 1.0f;
    //[SerializeField] private float moveDamping = 5.0f;
    //[SerializeField] private float rotDamping = 10.0f;

    void Start()
    {
        CamTr = Camera.main.transform;
        CamPivot = GetComponent<Transform>();
    }
    void LateUpdate()
    {
        //�ʱ� ī�޶� ��ġ ����
        CamPivot.position = target.position + (Vector3.up * targetOffset);
        var camPos = target.position - (Vector3.forward * distance) + (Vector3.up * height);
        Vector3 camDir = (CamPivot.position - CamTr.position).normalized;
        CamTr.position = camPos;
        CamTr.rotation = Quaternion.LookRotation(camDir);

        //ī�޶� ������ ����
        Vector3 rayDir = (CamTr.position - CamPivot.position).normalized;
        float camDis = Vector3.Distance(CamPivot.position, CamTr.position);
        RaycastHit hit;
        if(Physics.Raycast(CamPivot.position, rayDir, out hit, camDis, ~(1 << 3)))
            CamTr.localPosition = rayDir * hit.distance;
        else
            CamTr.localPosition = rayDir * camDis;


        //var camPos = target.position - (Vector3.forward * distance) + (Vector3.up * height); //ķ ��ġ�� Ÿ�� �ڿ� ��ġ ��Ų��.
        //CamPivot.Position = Vector3.Slerp(CamPivot.position, camPos, moveDamping * Time.deltaTime); //ķ ��ġ�� camPos ��ġ�� ������ moveDamping * Time.deltaTime �ӵ��� �����δ�.
        //CamPivot.Rotation = Quaternion.Slerp(CamPivot.rotation, target.rotation, rotDamping * Time.deltaTime); //ķ�� �ٶ󺸴� ������ target.rotation �������� rotDamping * Time.deltaTime�ӵ��� �ٲ۴�.
        //CamPivot.LookAt(target.position + (Vector3.up * targetOffset)); //ķ�� �ٶ󺸴� ��ġ �� ����. (Ÿ���� 1���� ��ġ�� �ٶ�)
    }
}
