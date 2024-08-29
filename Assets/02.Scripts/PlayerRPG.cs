using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRPG : MonoBehaviour
{
    public enum PlayerState { IDLE = 0, ATTACK, UNDER_ATTACK, DEAD}
    public PlayerState playerState = PlayerState.IDLE;
    [Tooltip("�ȱ� �ӵ�")] public float walkSpeed = 5.0f;
    [Tooltip("�޸��� �ӵ�")] public float runSpeed = 10.0f;
    [Header("Camera ���� ����")]
    [SerializeField] private Transform CamTr; //ī�޶� ��ġ
    [SerializeField] private Transform CamPivotTr; //ī�޶� �ǹ� ��ġ
    [SerializeField] private float cameraDistance = 0f; //ī�޶���� �Ÿ�
    [SerializeField] private Vector3 mouseMove = Vector3.zero; //���콺�� �̵��� ��ǥ
    [SerializeField] private int playerLayer; //�÷��̾� ���̾�

    [Header("�÷��̾� Move���� ����")]
    [SerializeField] private Transform modelTr;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Vector3 moveVelocity = Vector3.zero; //������ ����

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
            animator.SetBool("isRun", value); //isRun�� ���ε��� �Ҵ��Ͽ� �ִϸ��̼��� �ٲ� �ʿ䰡 ������.
        }
    }
    void Start()
    {
        CamTr = Camera.main.transform;
        CamPivotTr = Camera.main.transform.parent;

        modelTr = GetComponentsInChildren<Transform>()[1]; //�ڱ��ڽ� ���۳�Ʈ�� �ڽ��� 1�ε����� �����ϴ� ������Ʈ�� ������ (0 �ε����� �ڱ��ڽ�)
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
    private void LateUpdate() //ī�޶�� �÷��̾� ������ ���� �ʰ� �۵��ϹǷ� �ش� �Լ����� �ۼ�
    {
        float cameraHeight = 1.3f; //ī�޶� ����
        CamPivotTr.position = transform.position + (Vector3.up * cameraHeight); //ī�޶� �ǹ��� ��ġ�� �÷��̾� �����ʿ��� ��ġ�Ѵ�.
        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y") * 100f * 0.02f, Input.GetAxisRaw("Mouse X") * 100f * 0.02f, 0f);//Time.deltatime�� ���ϸ� �ʹ� �������� ���� ����
        // y������ ���콺�� �����̸� ȭ���� x������ ȸ��(-�� ���� ������ ���콺 �����Ӱ� ȸ�� ������ ���� �ϱ� ����), x������ ���콺�� �����̸� ȭ���� y������ ȸ��
        if (mouseMove.x < -40.0f)
            mouseMove.x = -40.0f;
        else if (mouseMove.x > 40.0f)
            mouseMove.x = 40.0f;

        CamPivotTr.eulerAngles = mouseMove;

        RaycastHit hit;
        Vector3 dir = (CamTr.position - CamPivotTr.position).normalized; //���� ī�޶� ��ġ���� - ī�޶� �ǹ� ��ġ�� ���� ������ ���Ѵ�. (�÷��̾�� ī�޶� ���� ���� ���Ͱ� �ȴ�.)

        if (Physics.Raycast(CamPivotTr.position, dir, out hit, cameraDistance, ~(1 << playerLayer))) //����ĳ��Ʈ�� ������ ���� �÷��̾�̾ �ƴ� ���
            CamTr.localPosition = Vector3.back * hit.distance; //ī�޶� ��ġ�� ��ֹ��� ���� �Ÿ���ŭ �ڷ� ������.
        else
            CamTr.localPosition = Vector3.back * cameraDistance;
    }

    //�Ʒ��� ī�޶� ���� �� ĳ���� �̵� ����
    void CameraDistanceCtrl()
    {
        cameraDistance -= Input.GetAxis("Mouse ScrollWheel"); //���콺 �ٷ� ī�޶� ������ �Ÿ� ����
    }
    void FreezeXZ() //ĳ���� ��Ʈ�ѷ��� ȸ���� y�� ȸ���� �����ϵ��� �����ߴ�.
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
    void RunCheck()
    {
        if (IsRun == false && Input.GetKey(KeyCode.LeftShift)) //�޸��� ���°� �ƴ� �� ���� ����Ʈ�� ������ �޸��Ⱑ �ȴ�.
            IsRun = true;
        else if (IsRun == true && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) //�޸��� ���¿��� wasd�� ������ �ʾҴٸ� isRun�� false�� �ٲ۴�.
            IsRun = false;
    }
    void CalcInputMove() //�������� ����ϴ� �Լ�
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun? runSpeed : walkSpeed);
        //wasd�� �������� �޾Ƽ� ���⺤�ͷ� ��ȯ�� IsRun�� ���̸� runSpeed�� ���ϰ� �ƴҰ�� walkSpeed�� ���Ѵ�. �������� ���Ҷ��� �ٸ��� �ؾ��ϹǷ� GetAxisRaw

        animator.SetFloat("speedX", Input.GetAxis("Horizontal")); //�ִϸ��̼� ��ȯ�� �ε巯���� �ϹǷ� GetAxis�� ����.
        animator.SetFloat("speedY", Input.GetAxis("Vertical"));

        moveVelocity = transform.TransformDirection(moveVelocity); //moveVelocity�� ���� ��ǥ�� ����, �� ĳ������ ���� ��ǥ�谡 ����Ǿ��� ��� �̸� �ٽ� ���� ��ǥ��� ��ȯ�Ͽ� �� ������� ���͸� �����δ�.
        if(0.01f < moveVelocity.sqrMagnitude) //��ü �������� ũ�Ⱑ 0.01���� ũ�ٸ� ī�޶� �����Ѵ�. (�÷��̾ �̵� ���̶��)
        {
            Quaternion cameraRot = CamPivotTr.rotation; //ī�޶� �ǹ��� rotation�� ����
            cameraRot.x = cameraRot.z = 0f; // ī�޶� �ǹ��� x,z�� ���� ���ش�.
            transform.rotation = cameraRot; // �÷��̾��� ������ ī�޶� ī�޶� �ǹ��� ȸ���� �������� �����Ѵ�.
            if(IsRun) //�޸��鼭 �̵����� ��
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity); //�޸��� ������ ����
                characterRot.x = characterRot.z = 0f; //�޸��� ���⿡ x,zȸ�� ���� ���� ���ְ� yȸ������ �����. (���ο����� �ٶ󺸴� ������ ������ ���ش�.)
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f); //������ ���� ȸ������ ���� �ٶ󺸰� �Ѵ�.
            }
            else
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, cameraRot, Time.deltaTime * 10f);
            }
        }
    }
    bool GroundCheck(out RaycastHit hit) //����ĳ��Ʈ�� ���� �������� ��Ƽ� �浹 ����
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f); //0.25�Ÿ��ȿ� ���� �����Ǹ� true��ȯ
    }
    void PlayerIdleAndMove()
    {
        RunCheck();
        if (characterController.isGrounded) //���� ��Ҵٸ� (ĳ���� ��Ʈ�ѷ��� ��ü ����̴�.)
        {
            if (isGrounded == false) isGrounded = true;
            animator.SetBool("isGrounded", true);
            CalcInputMove();

            RaycastHit groundHit;
            if (GroundCheck(out groundHit)) //���� ��Ҵٸ�
            { //���� ���������ϴٸ�
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed;
                //�޸��� ���̶�� �����ǵ带 �־ ���� �����ϰ� ��, �޸��� �ʴ´ٸ� ��ũ���ǵ带 �־ ���� �����ϰ���.
            }//ĳ���͸� ���� ������ ���̱� ���� ���?
            else
            {
                moveVelocity.y = -1f;
            }
            //���� ���� ���·� ������ �ؾ��ϱ� ������ ���⿡�� �����Լ��� �ҷ��´�.
            PlayerAttack();
            ShieldAttack();
        }
        else //���� ���� �ʾҴٸ�
        {
            if (isGrounded == true) isGrounded = false;
            else
                animator.SetBool("isGrounded", false);
            moveVelocity += Physics.gravity * Time.deltaTime; //�߷¿����� x,y,z���� ��� ��ȭ
        }
        characterController.Move(moveVelocity * Time.deltaTime); //������ ������ moveVelocity�� ���ͷ� ĳ���� ��Ʈ�ѷ��� �̵���Ų��.
    }

    //�Ʒ��� �����Լ� ����
    private float nextTime = 0f;
    void AttackTimeState()
    {
        nextTime += Time.deltaTime; //������ ����� �ð��� ����Ͽ� ����.
        if( 1f <= nextTime) //���� ���¿��� ������ �ð��� 1�� �̻��� ��� �ߵ�.
        {
            nextTime += Time.deltaTime;
            playerState = PlayerState.IDLE;
        }
    }
    void PlayerAttack()
    {//�ǵ� ������ ���鶧 Fire2�� ����Ͽ� �����.
        if(Input.GetButtonDown("Fire1")) //���콺 ���ʹ�ư or ���� ��Ʈ��Ű�� ������ ��
        {
            playerState = PlayerState.ATTACK; //���¸� ���� ���·� �ٲ۴�.
            animator.SetTrigger(hashAttack); //���� �ִϸ��̼��� ���� Ʈ���Ÿ� �ִ´�.
            animator.SetFloat(hashSpeedX, 0f); //������ �� ������ �ֱ� ���� x,y �Ѵ� 0���� �����.
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
