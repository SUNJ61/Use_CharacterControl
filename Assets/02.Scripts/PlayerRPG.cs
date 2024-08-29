using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRPG : MonoBehaviour
{
    public enum PlayerState { IDLE = 0, ATTACK, UNDER_ATTACK, DEAD}
    public PlayerState playerState = PlayerState.IDLE;
    [Tooltip("걷기 속도")] public float walkSpeed = 5.0f;
    [Tooltip("달리기 속도")] public float runSpeed = 10.0f;
    [Header("Camera 관련 변수")]
    [SerializeField] private Transform CamTr; //카메라 위치
    [SerializeField] private Transform CamPivotTr; //카메라 피벗 위치
    [SerializeField] private float cameraDistance = 0f; //카메라와의 거리
    [SerializeField] private Vector3 mouseMove = Vector3.zero; //마우스가 이동한 좌표
    [SerializeField] private int playerLayer; //플레이어 레이어

    [Header("플레이어 Move관련 변수")]
    [SerializeField] private Transform modelTr;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Vector3 moveVelocity = Vector3.zero; //움직일 방향

    private bool isGrounded = false;
    private bool isRun;

    private readonly int hashAttack = Animator.StringToHash("swordAttackTrigger");
    private readonly int hashShield = Animator.StringToHash("shieldAttackTrigger");
    private readonly int hashSpeedX = Animator.StringToHash("speedX");
    private readonly int hashSpeedY = Animator.StringToHash("speedY");
    public bool IsRun
    {
        get { return isRun; } set
        {
            isRun = value;
            animator.SetBool("isRun", value); //isRun을 따로따로 할당하여 애니메이션을 바꿀 필요가 없어짐.
        }
    }
    void Start()
    {
        CamTr = Camera.main.transform;
        CamPivotTr = Camera.main.transform.parent;

        modelTr = GetComponentsInChildren<Transform>()[1]; //자기자신 컴퍼넌트의 자식의 1인덱스에 존재하는 오브젝트를 가져옴 (0 인덱스는 자기자신)
        animator = transform.GetChild(0).GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        cameraDistance = 5.0f;
        playerLayer = LayerMask.NameToLayer("PLAYER");
    }
    void Update()
    {
        FreezeXZ();
        switch(playerState)
        {
            case PlayerState.IDLE:
                PlayerIdleAndMove();
                break;

            case PlayerState.ATTACK:
                AttackTimeState();
                break;

            case PlayerState.UNDER_ATTACK:

                break;

            case PlayerState.DEAD:

                break;
        }

        CameraDistanceCtrl();
    }
    private void LateUpdate() //카메라는 플레이어 움직임 보다 늦게 작동하므로 해당 함수에서 작성
    {
        float cameraHeight = 1.3f; //카메라 높이
        CamPivotTr.position = transform.position + (Vector3.up * cameraHeight); //카메라 피벗의 위치를 플레이어 가슴쪽에다 위치한다.
        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y") * 100f * 0.02f, Input.GetAxisRaw("Mouse X") * 100f * 0.02f, 0f);//Time.deltatime을 곱하면 너무 느려져서 값을 곱함
        // y축으로 마우스를 움직이면 화면은 x축으로 회전(-를 붙인 이유는 마우스 움직임과 회전 방향을 같게 하기 위해), x축으로 마우스를 움직이면 화면은 y축으로 회전
        if (mouseMove.x < -40.0f)
            mouseMove.x = -40.0f;
        else if (mouseMove.x > 40.0f)
            mouseMove.x = 40.0f;

        CamPivotTr.eulerAngles = mouseMove;

        RaycastHit hit;
        Vector3 dir = (CamTr.position - CamPivotTr.position).normalized; //실제 카메라 위치에서 - 카메라 피벗 위치를 빼서 방향을 구한다. (플레이어와 카메라 사이 방향 벡터가 된다.)

        if (Physics.Raycast(CamPivotTr.position, dir, out hit, cameraDistance, ~(1 << playerLayer))) //레이캐스트로 감지된 것이 플레이어레이어가 아닐 경우
            CamTr.localPosition = Vector3.back * hit.distance; //카메라 위치를 장애물에 맞은 거리만큼 뒤로 보낸다.
        else
            CamTr.localPosition = Vector3.back * cameraDistance;
    }

    //아래는 카메라 무빙 및 캐릭터 이동 선언
    void CameraDistanceCtrl()
    {
        cameraDistance -= Input.GetAxis("Mouse ScrollWheel"); //마우스 휠로 카메라가 떨어진 거리 조정
    }
    void FreezeXZ() //캐릭터 컨트롤러의 회전을 y축 회전만 가능하도록 제한했다.
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
    void RunCheck()
    {
        if (IsRun == false && Input.GetKey(KeyCode.LeftShift)) //달리기 상태가 아닐 때 왼쪽 쉬프트를 누르면 달리기가 된다.
            IsRun = true;
        else if (IsRun == true && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) //달리기 상태에서 wasd를 누르지 않았다면 isRun을 false로 바꾼다.
            IsRun = false;
    }
    void CalcInputMove() //움직임을 계산하는 함수
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun? runSpeed : walkSpeed);
        //wasd의 움직임을 받아서 방향벡터로 전환후 IsRun이 참이면 runSpeed를 곱하고 아닐경우 walkSpeed를 곱한다. 움직임을 곱할때는 바르게 해야하므로 GetAxisRaw

        animator.SetFloat("speedX", Input.GetAxis("Horizontal")); //애니메이션 전환은 부드러워야 하므로 GetAxis를 쓴다.
        animator.SetFloat("speedY", Input.GetAxis("Vertical"));

        moveVelocity = transform.TransformDirection(moveVelocity); //moveVelocity를 절대 좌표로 변경, 즉 캐릭터의 로컬 좌표계가 변경되었을 경우 이를 다시 월드 좌표계로 반환하여 그 정보대로 벡터를 움직인다.
        if(0.01f < moveVelocity.sqrMagnitude) //전체 움직임의 크기가 0.01보다 크다면 카메라를 제한한다. (플레이어가 이동 중이라면)
        {
            Quaternion cameraRot = CamPivotTr.rotation; //카메라 피벗의 rotation을 저장
            cameraRot.x = cameraRot.z = 0f; // 카메라 피벗의 x,z의 값을 없앤다.
            transform.rotation = cameraRot; // 플레이어의 방향을 카메라가 카메라 피벗이 회전한 방향으로 변경한다.
            if(IsRun) //달리면서 이동중일 때
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity); //달리는 방향을 저장
                characterRot.x = characterRot.z = 0f; //달리는 방향에 x,z회전 값을 전부 없애고 y회전값만 남긴다. (경사로에서의 바라보는 방향의 영향을 없앤다.)
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f); //위에서 얻은 회전값을 모델이 바라보게 한다.
            }
            else
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, cameraRot, Time.deltaTime * 10f);
            }
        }
    }
    bool GroundCheck(out RaycastHit hit) //레이캐스트를 땅쪽 방향으로 쏘아서 충돌 감지
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f); //0.25거리안에 땅이 감지되면 true반환
    }
    void PlayerIdleAndMove()
    {
        RunCheck();
        if (characterController.isGrounded) //땅에 닿았다면 (캐릭터 컨트롤러의 자체 기능이다.)
        {
            if (isGrounded == false) isGrounded = true;
            animator.SetBool("isGrounded", true);
            CalcInputMove();

            RaycastHit groundHit;
            if (GroundCheck(out groundHit)) //땅에 닿았다면
            { //땅이 울퉁불퉁하다면
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed;
                //달리는 중이라면 런스피드를 주어서 땅에 착지하게 함, 달리지 않는다면 워크스피드를 주어서 땅에 착지하게함.
            }//캐릭터를 땅에 완전히 붙이기 위해 사용?
            else
            {
                moveVelocity.y = -1f;
            }
            //땅에 닿은 상태로 공격을 해야하기 때문에 여기에서 공격함수를 불러온다.
            PlayerAttack();
            ShieldAttack();
        }
        else //땅에 닿지 않았다면
        {
            if (isGrounded == true) isGrounded = false;
            else
                animator.SetBool("isGrounded", false);
            moveVelocity += Physics.gravity * Time.deltaTime; //중력에의해 x,y,z값이 모두 변화
        }
        characterController.Move(moveVelocity * Time.deltaTime); //위에서 결정된 moveVelocity의 벡터로 캐릭터 컨트롤러를 이동시킨다.
    }

    //아래는 어택함수 선언
    private float nextTime = 0f;
    void AttackTimeState()
    {
        nextTime += Time.deltaTime; //게임이 실행된 시간의 비례하여 증가.
        if( 1f <= nextTime) //어택 상태에서 지나간 시간이 1초 이상일 경우 발동.
        {
            nextTime += Time.deltaTime;
            playerState = PlayerState.IDLE;
        }
    }
    void PlayerAttack()
    {//실드 어택을 만들때 Fire2를 사용하여 만들기.
        if(Input.GetButtonDown("Fire1")) //마우스 왼쪽버튼 or 왼쪽 컨트롤키를 눌렀을 때
        {
            playerState = PlayerState.ATTACK; //상태를 어택 상태로 바꾼다.
            animator.SetTrigger(hashAttack); //공격 애니메이션을 위해 트리거를 넣는다.
            animator.SetFloat(hashSpeedX, 0f); //공격할 때 가만히 있기 위해 x,y 둘다 0으로 만든다.
            animator.SetFloat(hashSpeedY, 0f);

            nextTime = 0f;
        }
    }
    void ShieldAttack()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            playerState = PlayerState.ATTACK;
            animator.SetTrigger(hashShield);
            animator.SetFloat(hashSpeedX, 0f);
            animator.SetFloat(hashSpeedY, 0f);

            nextTime = 0f;
        }
    }
}
