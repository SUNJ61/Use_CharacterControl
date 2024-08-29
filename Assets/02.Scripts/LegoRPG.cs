using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegoRPG : MonoBehaviour
{
    [Header("카메라")]
    private Transform CamTr;
    private Transform CamPivotTr;
    private float CamDistance;
    private float Sen = 0.02f;
    private Vector3 mouseMove;
    private int playerLayer;

    public enum State { IDLE = 0, ATTACK };
    public State state = State.IDLE;
    [Header("이동")]
    private Transform modelTr;
    private Animator animator;
    private CharacterController characterController;
    private Vector3 moveVelocity = Vector3.zero;

    private float walkSpeed = 5.0f;
    private float runSpeed = 10.0f;

    private bool isGrounded = false;
    private bool isRun;

    private readonly int hashSword = Animator.StringToHash("swordTrigger");
    private readonly int hashShield = Animator.StringToHash("shieldTrigger");
    private readonly int hashSpeedX = Animator.StringToHash("speedX");
    private readonly int hashSpeedY = Animator.StringToHash("speedY");

    public bool IsRun
    {
        get { return isRun; }
        set
        {
            isRun = value;
            animator.SetBool("isRun", value);
        }
    }
    void Start()
    {
        CamTr = Camera.main.transform;
        CamPivotTr = Camera.main.transform.parent;

        CamDistance = 5.0f;
        playerLayer = LayerMask.NameToLayer("PLAYER");

        modelTr = transform.GetChild(0).GetComponent<Transform>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
        CamDistanceCtrl();

        switch(state)
        {
            case State.IDLE:
                PlayerCtrl();
                break;

            case State.ATTACK:
                AttackTimeState();
                break;
        }

        FreezeXZ();
    }
    #region 카메라 관련 함수들
    private void LateUpdate()
    {
        float PivotHeight = 2.0f;
        CamPivotTr.position = transform.position + (Vector3.up * PivotHeight);

        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y") * 100.0f * Sen, Input.GetAxisRaw("Mouse X") * 100.0f * Sen, 0f);
        if (mouseMove.x < -40.0f) //상하 틸트값 제한
            mouseMove.x = -40.0f;
        else if (mouseMove.x > 40.0f)
            mouseMove.x = 40.0f;

        CamPivotTr.eulerAngles = mouseMove; //마우스 움직이는대로 피벗이 회전한다. (피벗 위치 기준으로 뒤로 가는 카메라는 피벗이 돌면 같이 회전함)

        RaycastHit hit;
        Vector3 dir = (CamTr.position - CamPivotTr.position).normalized;
        if (Physics.Raycast(CamPivotTr.position, dir, out hit, CamDistance, ~(1 << playerLayer)))
            CamTr.localPosition = Vector3.back * hit.distance;
        else
            CamTr.localPosition = Vector3.back * CamDistance;
    }
    void CamDistanceCtrl()
    {
        CamDistance -= Input.GetAxis("Mouse ScrollWheel");
    }
    void FreezeXZ()
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
    #endregion
    void RunCheck()
    {
        if(IsRun == false && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
            IsRun = true;
        else if(IsRun == true && (Input.GetAxis("Vertical") == 0 || Input.GetKeyUp(KeyCode.LeftShift)))
            IsRun = false;
    }
    void CalcMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun ? runSpeed : walkSpeed);

        animator.SetFloat("speedX", Input.GetAxis("Horizontal"));
        animator.SetFloat("speedY", Input.GetAxis("Vertical"));

        moveVelocity = transform.TransformDirection(moveVelocity);
        if(0.01f < moveVelocity.sqrMagnitude) //움직일 경우
        {
            Quaternion CamPivot = CamPivotTr.rotation;
            CamPivot.x = CamPivot.z = 0f;
            transform.rotation = CamPivot; //움직일 때는 카메라 피벗이 회전한 값을 캐릭터도 따라간다. 즉 카메라에서는 캐릭터에 뒤만 보이게한다.
            if(IsRun == true)
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity); //이동하는 방향 저장
                characterRot.x = characterRot.z = 0f; //달릴 때 달리는 방향을 저장, x,z 로테이션값 제거, 즉 캐릭터 뒤만 보이게 한다.
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f);
            }
            else //걸을 때
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, CamPivot, Time.deltaTime * 10f);
            } //움직일 때는 카메라 피벗이 회전한 값을 캐릭터도 따라간다. 즉 카메라에서는 캐릭터에 뒤만 보이게한다.
        }
    }
    bool GroundCheck(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f);
    }
    void PlayerCtrl()
    {
        RunCheck();
        if(characterController.isGrounded) //캐릭터 컨트롤러 자체기능으로 땅에 닿았다 판단.
        {
            if(isGrounded == false) isGrounded = true;
            animator.SetBool("isGrounded", true);
            CalcMove();

            RaycastHit groundHit;
            if(GroundCheck(out groundHit)) //레이저를 아래로 쏘았는데 땅에 닿았다
            {
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed; //달리는 중이라면 moveVelocity의 y값을 -runSpeed / 걷는중이라면 moveVelocity의 y값을 -walkSpeed
            }
            else //레이저를 아래로 쏘았는데 땅에 닿지 않았다면 
            {
                moveVelocity.y = -1.0f; //moveVelocity의 y값을 -1로 바꾼다.
            }
            SwordAttack();
            ShieldAttack();
        }
        else //캐릭터 컨트롤러 자체 기능으로 땅에 닿지 않았다 판단.
        {
            if (isGrounded == true) isGrounded = false;
            else
                animator.SetBool("isGrounded", false);
            moveVelocity += Physics.gravity * Time.deltaTime; //중력에의해 x,y,z값이 모두 변화
        }
        characterController.Move(moveVelocity * Time.deltaTime); //위에서 결정된 moveVelocity의 벡터로 컨트롤러를 이동시킨다.
    }

    private float nextTime = 0f;
    void AttackTimeState()
    {
        nextTime += Time.deltaTime;
        if (1f <= nextTime)
        {
            nextTime += Time.deltaTime;
            state = State.IDLE;
        }
    }
    void SwordAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            state = State.ATTACK;
            animator.SetTrigger(hashSword);
            animator.SetFloat(hashSpeedX, 0f);
            animator.SetFloat(hashSpeedY, 0f);

            nextTime = 0f;
        }
    }
    void ShieldAttack()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            state = State.ATTACK;
            animator.SetTrigger(hashShield);
            animator.SetFloat(hashSpeedX, 0f);
            animator.SetFloat(hashSpeedY, 0f);

            nextTime = 0f;
        }
    }
}
