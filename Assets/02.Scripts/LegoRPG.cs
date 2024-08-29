using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegoRPG : MonoBehaviour
{
    [Header("ī�޶�")]
    private Transform CamTr;
    private Transform CamPivotTr;
    private float CamDistance;
    private float Sen = 0.02f;
    private Vector3 mouseMove;
    private int playerLayer;

    public enum State { IDLE = 0, ATTACK };
    public State state = State.IDLE;
    [Header("�̵�")]
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
    #region ī�޶� ���� �Լ���
    private void LateUpdate()
    {
        float PivotHeight = 2.0f;
        CamPivotTr.position = transform.position + (Vector3.up * PivotHeight);

        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y") * 100.0f * Sen, Input.GetAxisRaw("Mouse X") * 100.0f * Sen, 0f);
        if (mouseMove.x < -40.0f) //���� ƿƮ�� ����
            mouseMove.x = -40.0f;
        else if (mouseMove.x > 40.0f)
            mouseMove.x = 40.0f;

        CamPivotTr.eulerAngles = mouseMove; //���콺 �����̴´�� �ǹ��� ȸ���Ѵ�. (�ǹ� ��ġ �������� �ڷ� ���� ī�޶�� �ǹ��� ���� ���� ȸ����)

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
        if(0.01f < moveVelocity.sqrMagnitude) //������ ���
        {
            Quaternion CamPivot = CamPivotTr.rotation;
            CamPivot.x = CamPivot.z = 0f;
            transform.rotation = CamPivot; //������ ���� ī�޶� �ǹ��� ȸ���� ���� ĳ���͵� ���󰣴�. �� ī�޶󿡼��� ĳ���Ϳ� �ڸ� ���̰��Ѵ�.
            if(IsRun == true)
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity); //�̵��ϴ� ���� ����
                characterRot.x = characterRot.z = 0f; //�޸� �� �޸��� ������ ����, x,z �����̼ǰ� ����, �� ĳ���� �ڸ� ���̰� �Ѵ�.
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f);
            }
            else //���� ��
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, CamPivot, Time.deltaTime * 10f);
            } //������ ���� ī�޶� �ǹ��� ȸ���� ���� ĳ���͵� ���󰣴�. �� ī�޶󿡼��� ĳ���Ϳ� �ڸ� ���̰��Ѵ�.
        }
    }
    bool GroundCheck(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f);
    }
    void PlayerCtrl()
    {
        RunCheck();
        if(characterController.isGrounded) //ĳ���� ��Ʈ�ѷ� ��ü������� ���� ��Ҵ� �Ǵ�.
        {
            if(isGrounded == false) isGrounded = true;
            animator.SetBool("isGrounded", true);
            CalcMove();

            RaycastHit groundHit;
            if(GroundCheck(out groundHit)) //�������� �Ʒ��� ��Ҵµ� ���� ��Ҵ�
            {
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed; //�޸��� ���̶�� moveVelocity�� y���� -runSpeed / �ȴ����̶�� moveVelocity�� y���� -walkSpeed
            }
            else //�������� �Ʒ��� ��Ҵµ� ���� ���� �ʾҴٸ� 
            {
                moveVelocity.y = -1.0f; //moveVelocity�� y���� -1�� �ٲ۴�.
            }
            SwordAttack();
            ShieldAttack();
        }
        else //ĳ���� ��Ʈ�ѷ� ��ü ������� ���� ���� �ʾҴ� �Ǵ�.
        {
            if (isGrounded == true) isGrounded = false;
            else
                animator.SetBool("isGrounded", false);
            moveVelocity += Physics.gravity * Time.deltaTime; //�߷¿����� x,y,z���� ��� ��ȭ
        }
        characterController.Move(moveVelocity * Time.deltaTime); //������ ������ moveVelocity�� ���ͷ� ��Ʈ�ѷ��� �̵���Ų��.
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
