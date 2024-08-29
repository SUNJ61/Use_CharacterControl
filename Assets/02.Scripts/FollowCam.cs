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
        //초기 카메라 위치 설정
        CamPivot.position = target.position + (Vector3.up * targetOffset);
        var camPos = target.position - (Vector3.forward * distance) + (Vector3.up * height);
        Vector3 camDir = (CamPivot.position - CamTr.position).normalized;
        CamTr.position = camPos;
        CamTr.rotation = Quaternion.LookRotation(camDir);

        //카메라 움직임 설정
        Vector3 rayDir = (CamTr.position - CamPivot.position).normalized;
        float camDis = Vector3.Distance(CamPivot.position, CamTr.position);
        RaycastHit hit;
        if(Physics.Raycast(CamPivot.position, rayDir, out hit, camDis, ~(1 << 3)))
            CamTr.localPosition = rayDir * hit.distance;
        else
            CamTr.localPosition = rayDir * camDis;


        //var camPos = target.position - (Vector3.forward * distance) + (Vector3.up * height); //캠 위치를 타겟 뒤에 위치 시킨다.
        //CamPivot.Position = Vector3.Slerp(CamPivot.position, camPos, moveDamping * Time.deltaTime); //캠 위치를 camPos 위치로 서서히 moveDamping * Time.deltaTime 속도로 움직인다.
        //CamPivot.Rotation = Quaternion.Slerp(CamPivot.rotation, target.rotation, rotDamping * Time.deltaTime); //캠이 바라보는 방향을 target.rotation 방향으로 rotDamping * Time.deltaTime속도로 바꾼다.
        //CamPivot.LookAt(target.position + (Vector3.up * targetOffset)); //캠이 바라보는 위치 값 설정. (타겟의 1높이 위치를 바라봄)
    }
}
