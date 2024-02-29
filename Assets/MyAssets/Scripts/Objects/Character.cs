using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;


public enum CharacterState { Normal, Action, Stun, Death, Invicible }

// ���� �ð� �Ŀ� �ش� ������Ʈ�� ���ִ� ���
// �Ű������� �� ������ ��� �ִٰ� Ÿ�̹��� �Ǹ� �����Ű��?
// �� ����� �ٽ� �ǵ����� �Լ��� �ǽð����� ����� => ����
public struct DelayAction
{
    public float time;
    // �Լ��� �����س��� �� �ִ�.
    // Action ��ȯ���� void�� �Լ��� ����
    // Func ��ȯ���� �ִ� �Լ����� ����
    public System.Action lambda;
}


// ���� �ε�����.
[System.Serializable]
public struct ContactInfo
{
    // ������ �ε�������?
    public GameObject other;
    // �ε��� ����
    public ContactPoint2D contact;
    // �ε��� �ð� -> �ڿ��� Ÿ��! 
    public float time;

    public ContactInfo(GameObject other, ContactPoint2D contact, float time)
    {
        this.other = other;
        this.contact = contact;
        this.time = time;
    }
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class Character : Breakable
{
    public const float coyoteTime = 0.08f;

    public static GameObject pause;

    protected Rigidbody2D rigid;
    protected Animator animator;
    [SerializeField] protected CharacterState currentState = CharacterState.Normal;

    [SerializeField] int _mpCurrent;
    public int MpCurrent
    { 
        get => _mpCurrent; 
        protected set => Mathf.Clamp(value, 0, MpMax); 
    }

    [SerializeField] int _mpMax;
    public int MpMax
    { 
        get => _mpMax;
        protected set
        {
            _mpMax = value;
            MpCurrent = MpCurrent;
        } 
    }

    // ���� "���� �ִ�" ����
    public Vector2 MoveDirection { get; protected set; }
    // ���� "���� ����" ����
    public Vector2 PreferredDirection { get; set; }
    // �ٶ󺸴� ����
    public Vector2 FaceDirection
    {
        // �� eulerAngles
        get => transform.rotation.eulerAngles.y < 90 ? Vector2.right : Vector2.left;
        set
        {
            // value ������ �ٶ������ ���ڴ�transform.LookAt(value);
            // value�� x������ 0���� ũ�ų� ������ rotation y = 0 �ƴϸ� rotation y = 180
            transform.rotation = Quaternion.Euler(0, value.x >= 0 ? 0 : 180, 0);
        }
    }
    [Header("Move Values")]
    [SerializeField] public float moveSpeed;
    [SerializeField] public float jumpPower;
    private float jumpTime;

    [Header("Attack Values")]
    [SerializeField] protected float attackPower;
    [SerializeField] protected float attackMaxDelay;
    [SerializeField] protected float attackLeftDelay;

    [Header("Rates")]
    [Range(0, 1), SerializeField] protected float dodgeRate;
    [Range(0, 1), SerializeField] protected float criticalRate;

    [Header("Effects")]
    [SerializeField] GameObject DeathEffect;

    protected List<DelayAction> delayList = new List<DelayAction>();
    [SerializeField]protected List<ContactInfo> collisionList = new List<ContactInfo>();

    // ���� ����ִ��� AI�� ����ؾ� ��
    public bool IsGround { get; protected set; }
    protected virtual CharacterState CheckState()
    {
        // ������ ���� �߿���
        // �켱����
        // ������¿� ���� ���� �޶��� �� ����

        // Death
        if(IsBreak)
        {
            return CharacterState.Death;
        }
        // Stun

        // Action
        if (animator.GetBool("isAction")) return CharacterState.Action;
        // Normal
        return CharacterState.Normal;
    }

    protected void Start()
    {
        // gameobject. �� ���� ����
        rigid = gameObject.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (pause == null)
        {
            pause = GameObject.Find("Pause");
            pause.SetActive(false);
        }
    }

    protected void Update()
    {
        currentState = CheckState();
    }

    // Fixed Update : 0.02�ʸ��� �ݺ�. ���������� Fixed Update���� �϶�
    protected void FixedUpdate()
    {
        // delayList�� �� ���ƾ� ��
        for(int i=0; i<delayList.Count; i++)
        {
            // struct���� ���� "��ü"�� �ٲ��ִ� �������� �غ�����߉�
            delayList[i] = new DelayAction() {time = delayList[i].time - Time.fixedDeltaTime, lambda = delayList[i].lambda };
            if (delayList[i].time < 0)
            {
                // �����س��� �Լ� �����ϱ� '()'
                delayList[i].lambda();
            }
        }
        delayList.RemoveAll((target) => target.time <= 0);
        IsGround = CheckGround();
        animator.SetFloat("Speed", Mathf.Abs(rigid.velocity.x));
        animator.SetBool("isGround", IsGround);
        animator.SetBool("isFall", MoveDirection.y < 0);

        switch(currentState)
        {
            case CharacterState.Normal:
                MoveDirection = (PreferredDirection * Vector2.right).normalized;
                break;
            case CharacterState.Death:
            case CharacterState.Stun:
            case CharacterState.Action:
                MoveDirection = Vector2.zero;
                break;
        }
        if(MoveDirection.magnitude > 0)
        {
            rigid.AddForce(MoveDirection * 100, ForceMode2D.Impulse);


            // rigid.velocity.x (x) ������Ƽ�� �Ϻθ� �ٲܼ��� ����.
            // ��°�δ� �ٲ� �� �ִ�.
            Vector2 speedLimitedVelocity = rigid.velocity;
            speedLimitedVelocity.x = Mathf.Clamp(speedLimitedVelocity.x, -moveSpeed, moveSpeed);
            rigid.velocity = speedLimitedVelocity;



            if (MoveDirection.x > 0)
            {
                transform.rotation = Quaternion.identity;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        else
        {
            // ����Ű�� ������ ���߰�
            /*
            Vector2 speedLimitedVelocity = rigid.velocity;
            speedLimitedVelocity.x = 0;
            //Vector2 speedLimitedVelocity = rigid.velocity;
            //speedLimitedVelocity.x = Mathf.Lerp(speedLimitedVelocity.x, 0, 0.5f);
            rigid.velocity = speedLimitedVelocity;
            */
            // rigid.velocity = new Vector2(0, rigid.velocity.y);
            rigid.velocity *= Vector2.up;
        }
    }

    protected void OnCollisionStay2D(Collision2D collision)
    {
        // ��Ȯ�� ���� �ε��� ����(ContactPoint2D)��?
        // ������ ���� �迭�� ����µ�, �� ������ ������ ����̶� �ε����� ������?
        ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
        // �� contacts�� �ε��� ������ �� �� �ֶ�
        collision.GetContacts(contacts);

        // ���� ������ "ã�ƾ�"�Ѵ�
        // ���ٽ�(Lambda) : �Լ��� �ٷ� ����, �� �� ���� ���� ������ �Լ�
        // ( �Ű����� ) => { ���� }
        // �̹� �־���ϴ� �Ű������� �ִ� ���, �ڷ����� �����ص� ����
        ContactPoint2D myContact = System.Array.Find(contacts, target => target.otherCollider.gameObject == gameObject);
        // Debug.Log($"collider: {collision.GetContact(0).collider} / otherCollider: {collision.GetContact(0).otherCollider}");

        // �Ȱ��� ���� ������ �� ���� �ʵ���
        // "�Ȱ���"��� ���� gameObject�� Ȯ���� �� => �ٸ� ���� �ٸ� �� ����.(���� �ٸ�)
        // collisionList.Find(target => target.contact.otherCollider.gameObject == myContact.otherCollider.gameObject);
        // collisionList.Find(target => target.other == collision.gameObject);
        int collisionIndex = collisionList.FindIndex(target => target.other == collision.gameObject);
        // �� ��° ���� "��ȣ"�� �����ִ� �Լ���. ã�� ����� ������ -1 ��ȯ
        if(collisionIndex < 0)
        {
            // �Ȱ��� ���� ���ٴ� ���̹Ƿ� �ε�����
            // �ε������ϱ� ContactInfo ����                                    Time.time : ���� �ð�
            collisionList.Add(new ContactInfo(collision.gameObject, myContact, Time.time));
            
        }
        else
        {
            // ����
            collisionList[collisionIndex] = new ContactInfo(collision.gameObject, myContact, Time.time);
        }


        /*
        // ����ִ� ���� ������ 45�� �̸��̶��
        float angleDiff = Vector2.Angle(Vector2.up, collision.GetContact(0).normal);
        if (angleDiff < 45)
        {
            isGround = true;

        }
        */
    }

    //protected void OnCollisionExit2D(Collision2D collision)
    //{
    //    isGround = false;
    //}

    protected bool CheckGround()
    {
        // �ڿ��� Ÿ���̶�� �ϴ� ���� �����ð� ���� ������ ǥ��
        // ���� �ð� ���� �ֵ��� ����
        collisionList.RemoveAll(target => Time.time - target.time > coyoteTime);

        if (rigid.velocity.y == 0) return true;

        // ���� �ִٶ�� ��
        // ���� ����Ʈ �߿� "�� ���� ������Ʈ"�� ã�ƾ� �Ѵ�.

        //                                                              = Vector2.up
        return collisionList.FindIndex(target => Vector2.Angle(-Physics2D.gravity, target.contact.normal) < 46.0f) >= 0;
    }

    // On: �Է� �޴� �뵵
    protected virtual void OnJump()
    {
        Jump();
    }

    // ���� �����ϴ� �뵵
    public virtual void Jump()
    {
        // switch���� break�� case���̸� �����ϴ� ���
        // break�� �ɸ��� ������ ��� case���� �� ����.
        switch(currentState)
        {
            case CharacterState.Death:
            case CharacterState.Stun:
                return;
        }

        // �Ʒ�Ű�� ������ �ִ�
        if(PreferredDirection.y < 0)
        {
            Collider2D myCol = collisionList[0].contact.collider;
            Collider2D otherCol = collisionList[0].contact.otherCollider;
            // �ΰ��� �ݶ��̴��� �浹���� �ʾ����� ���ڵ�
            Physics2D.IgnoreCollision(myCol, otherCol);
            delayList.Add(new DelayAction() {time = 0.5f, lambda = () => 
                { 
                    Physics2D.IgnoreCollision(myCol, otherCol, false);
                }
            });
            return;
        }

        if (!IsGround) return;
        // 2. ������ jumpTime�� �������� coyoteTime �̸��̸� ����
        //                       coyoteTime�� Update���� ����ϹǷ� ���� FixedUpdate�� ��ٷ����Ѵ�.
        if (Time.time - jumpTime < coyoteTime + Time.fixedDeltaTime) return;

        // 1. ������ �����ϸ� jumpTime�� ���� �ð�����
        jumpTime = Time.time;
        rigid.AddForce(Vector2.up * jumpPower * 100);
    }

    protected virtual void OnAttack()
    {
        Attack();
    }
    public virtual void Attack()
    {
        switch (currentState)
        {
            case CharacterState.Death:
            case CharacterState.Stun:
                return;
        }
        animator.SetTrigger("doAttack");
    }

    // ���ݿ� ���� ������
    // �������� ����� ���� �޼���
    protected virtual void GenerateHitBox(string name)
    {
        // ����Ƽ���� "�̹� �����ϴ� ���"�� �ڵ�� �����ϴ� ���
        // �������� �����ؼ� ���ӿ� �ø���
        // Instantiate(�ν��Ͻ�ȭ �ϴ�) -> ������ ���� ���ο� ��ü �����
        GameObject HitBox = Instantiate(Resources.Load<GameObject>($"Prefabs/Hitboxes/{name}"), transform.position, transform.rotation);
        // �� TryGetComponent�� prefab�� �ƴ� Instantiate�� ������Ʈ�� �� ��
        if(HitBox.TryGetComponent(out DamageComponent damage))
        {
            // �׻� �ʱ�ȭ
            damage.Initialize(this, attackPower, criticalRate);
        }
        Destroy(HitBox, 0.04f);
    }

    protected override bool OnBreak()
    {
        animator.SetTrigger("doDeath");
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        // bitMask : ����ȭ -> ���� 0.01�� : 12�� ���� -> 120��
        // 00000000 00000000 00000000 00000000 => int�� 32bit
        //                                   1  2^0( 1<<0 ) Default
        //                            10000000  2^7( 1<<7 ) Corpse
        //                            10010001  Corpse + Water + Default ( 1<<7 | 1<<4 | 1<<0 )
        //                                      1<<LayerMask.NameToLayer("Corpse");
        

        // �ڷ�ƾ�� ����ٰ� ������ ���� �ƴϴ�
        // �ڷ�ƾ�� Update���� ������ �������� ������ �־� �� ª��
        IEnumerator co = CreateEffect(DeathEffect, transform.position, transform.rotation, 1.9f);
        StartCoroutine(co);
        // StopCoroutine(co);


        Destroy(gameObject, 2f);
        return base.OnBreak();
    }

    protected virtual void OnMove(InputValue value)
    {
        Move(value.Get<Vector2>());
    }

    public virtual void Move(Vector2 direction)
    {
        switch (currentState)
        {
            case CharacterState.Death:
                PreferredDirection = Vector2.zero;
                return;
        }
        // �����̴� ����
        // direction.normalized => ������ �ȹٲ�, ���� ��ȯ
        // direction.Normalize() => ������ �ٲ�
        direction.Normalize();

        // Move�� �����̶�� �Լ�����
        // ������ ����, �� ������ �ൿ�� ������Ʈ
        PreferredDirection = direction;
    }

    protected virtual void OnDodge()
    {
        Dodge();
    }
    public virtual void Dodge()
    {
        
    }

    protected virtual void OnRun(InputValue value)
    {
        Run(value.Get<bool>());
    }
    public virtual void Run(bool isActivated)
    {

    }

    protected virtual void OnSkill()
    {
        Skill();
    }
    public virtual void Skill()
    {

    }

    public void OnPause()
    {
        // activeInHierarchy vs activeSelf
        // activeSelf        : �θ� ������� �ڱ� �ڽ��� active�Ǿ��ִ���
        // activeInHierarchy : �ڽ��� �����־ �θ� ���������� false
        // pause.SetActive(!pause.activeSelf);
        pause.SetActive(!pause.activeInHierarchy);
    }

    //                     On : �÷��̾ ���� �Է����� ��
    //                                                      ���콺 ��ġ, �ٵ� �ȼ� ������
    //                                                      ��������Ʈ�� �ٲ������.
    protected virtual void OnRangeAttack() => RangeAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition));

    protected virtual void RangeAttack(Vector2 wantPosition)
    {
        Vector2 wantDirection = wantPosition - (Vector2)transform.position;
        wantDirection.Normalize();
        Quaternion wantRotation = Quaternion.Euler(0, 0, Mathf.Atan2(wantDirection.y, wantDirection.x)*Mathf.Rad2Deg);
        // ����ü �ҷ�����
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Projectiles/Projectile");
        
        Vector2 spawnPosition = transform.position;
        if(TryGetComponent(out Collider2D myCol))
        {
            spawnPosition = myCol.ClosestPoint(wantPosition);
        }

        
        GameObject inst = Instantiate(prefab, spawnPosition, wantRotation);



        if(inst.TryGetComponent<DamageComponent>(out var damage))
        {
            damage.Initialize(this, attackPower, criticalRate);

        }
        if(inst.TryGetComponent(out Rigidbody2D otherRigid))
        {
            otherRigid.AddForce(wantDirection * 500);
        }
    }

    // enumerator �ڷ�ƾ�� ���� ����
    // �ڷ�ƾ�� �˰�� ��ž� �ؼ� �ϴ� �ſ���!
    // ���࿡ ��Ʈ���������� �ڷ�ƾ�� ���̾��� �� �������!
    // �Ϲ����� �ڵ庸�� 100�� ���� ����.
    IEnumerator CreateEffect(GameObject prefab, Vector3 position, Quaternion rotation, float startTime = 0)
    {
        // IEnumerator : �ݺ��� -> �ٸ� �۾� �ϴٰ� �� ��ٷȴ��� Ȯ���Ϸ� ���� �ðſ���
        // �������� ��ȯ�� ����� �մϴ�. �ٸ� �ֵ� ���� �϶�� ��ȯ�Ѱ�
        // yield(�纸)
        // ����� ���� �����ð� ��ٸ��� �Լ�
        yield return new WaitForSeconds(startTime);

        GameObject instance = Instantiate(prefab, position, rotation);
        

        // DestroyTimer ��ũ��Ʈ���� ó��
        // ���� ������ ��ٸ���
        //yield return new WaitForSeconds(lifeTime);

        // �����
        //Destroy(instance);
    }

}
