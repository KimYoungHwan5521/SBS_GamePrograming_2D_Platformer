using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;


public enum CharacterState { Normal, Action, Stun, Death, Invicible }

// 일정 시간 후에 해당 오브젝트를 없애는 기능
// 매개변수를 그 때까지 들고 있다가 타이밍이 되면 실행시키기?
// 그 대상을 다시 되돌리는 함수를 실시간으로 만들기 => 람다
public struct DelayAction
{
    public float time;
    // 함수를 저장해놓을 수 있다.
    // Action 반환값이 void인 함수를 저장
    // Func 반환값이 있는 함수들을 저장
    public System.Action lambda;
}


// 나는 부딪혔다.
[System.Serializable]
public struct ContactInfo
{
    // 누구랑 부딪혔는지?
    public GameObject other;
    // 부딪힌 정보
    public ContactPoint2D contact;
    // 부딪힌 시간 -> 코요테 타임! 
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

    // 내가 "가고 있는" 방향
    public Vector2 MoveDirection { get; protected set; }
    // 내가 "가고 싶은" 방향
    public Vector2 PreferredDirection { get; set; }
    // 바라보는 방향
    public Vector2 FaceDirection
    {
        // ※ eulerAngles
        get => transform.rotation.eulerAngles.y < 90 ? Vector2.right : Vector2.left;
        set
        {
            // value 방향을 바라봤으면 좋겠다transform.LookAt(value);
            // value의 x방향이 0보다 크거나 같으면 rotation y = 0 아니면 rotation y = 180
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

    // 땅을 밟고있는지 AI가 사용해야 함
    public bool IsGround { get; protected set; }
    protected virtual CharacterState CheckState()
    {
        // 서순이 제일 중요함
        // 우선순위
        // 현재상태에 따라 조금 달라질 수 있음

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
        // gameobject. 은 생략 가능
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

    // Fixed Update : 0.02초마다 반복. 물리연산은 Fixed Update에서 하라
    protected void FixedUpdate()
    {
        // delayList를 다 돌아야 함
        for(int i=0; i<delayList.Count; i++)
        {
            // struct같은 경우는 "전체"를 바꿔주는 형식으로 준비해줘야됌
            delayList[i] = new DelayAction() {time = delayList[i].time - Time.fixedDeltaTime, lambda = delayList[i].lambda };
            if (delayList[i].time < 0)
            {
                // 저장해놓은 함수 실행하기 '()'
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


            // rigid.velocity.x (x) 프로퍼티는 일부만 바꿀수는 없다.
            // 통째로는 바꿀 수 있다.
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
            // 방향키를 땠을때 멈추게
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
        // 정확히 내가 부딪힌 정보(ContactPoint2D)는?
        // 정보를 얻어올 배열을 만드는데, 그 개수는 상대방이 몇명이랑 부딪히고 있을까?
        ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
        // 그 contacts에 부딪힌 정보를 싹 다 주라
        collision.GetContacts(contacts);

        // 나의 정보를 "찾아야"한다
        // 람다식(Lambda) : 함수를 바로 만듦, 딱 한 번만 쓰고 버리는 함수
        // ( 매개변수 ) => { 내용 }
        // 이미 넣어야하는 매개변수가 있는 경우, 자료형은 생략해도 무방
        ContactPoint2D myContact = System.Array.Find(contacts, target => target.otherCollider.gameObject == gameObject);
        // Debug.Log($"collider: {collision.GetContact(0).collider} / otherCollider: {collision.GetContact(0).otherCollider}");

        // 똑같은 것이 있으면 더 하지 않도록
        // "똑같다"라는 것은 gameObject를 확인할 것 => 다른 값은 다를 수 있음.(거의 다름)
        // collisionList.Find(target => target.contact.otherCollider.gameObject == myContact.otherCollider.gameObject);
        // collisionList.Find(target => target.other == collision.gameObject);
        int collisionIndex = collisionList.FindIndex(target => target.other == collision.gameObject);
        // 몇 번째 인지 "번호"로 돌려주는 함수들. 찾는 대상이 없으면 -1 반환
        if(collisionIndex < 0)
        {
            // 똑같은 것이 없다는 뜻이므로 부딪혔다
            // 부딪혔으니까 ContactInfo 생성                                    Time.time : 현재 시간
            collisionList.Add(new ContactInfo(collision.gameObject, myContact, Time.time));
            
        }
        else
        {
            // 갱신
            collisionList[collisionIndex] = new ContactInfo(collision.gameObject, myContact, Time.time);
        }


        /*
        // 밟고있는 땅의 각도가 45도 미만이라면
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
        // 코요테 타임이라고 하는 것은 일정시간 땅에 있음을 표현
        // 일정 시간 지난 애들은 삭제
        collisionList.RemoveAll(target => Time.time - target.time > coyoteTime);

        if (rigid.velocity.y == 0) return true;

        // 땅에 있다라는 뜻
        // 닿은 리스트 중에 "땅 판정 오브젝트"를 찾아야 한다.

        //                                                              = Vector2.up
        return collisionList.FindIndex(target => Vector2.Angle(-Physics2D.gravity, target.contact.normal) < 46.0f) >= 0;
    }

    // On: 입력 받는 용도
    protected virtual void OnJump()
    {
        Jump();
    }

    // 실제 실행하는 용도
    public virtual void Jump()
    {
        // switch에서 break는 case사이를 구분하는 경우
        // break가 걸리기 전까지 모든 case들을 다 실행.
        switch(currentState)
        {
            case CharacterState.Death:
            case CharacterState.Stun:
                return;
        }

        // 아래키를 누르고 있다
        if(PreferredDirection.y < 0)
        {
            Collider2D myCol = collisionList[0].contact.collider;
            Collider2D otherCol = collisionList[0].contact.otherCollider;
            // 두개의 콜라이더가 충돌하지 않았으면 좋겠따
            Physics2D.IgnoreCollision(myCol, otherCol);
            delayList.Add(new DelayAction() {time = 0.5f, lambda = () => 
                { 
                    Physics2D.IgnoreCollision(myCol, otherCol, false);
                }
            });
            return;
        }

        if (!IsGround) return;
        // 2. 마지막 jumpTime과 비교했을때 coyoteTime 미만이면 리턴
        //                       coyoteTime은 Update에서 계산하므로 다음 FixedUpdate를 기다려야한다.
        if (Time.time - jumpTime < coyoteTime + Time.fixedDeltaTime) return;

        // 1. 점프에 성공하면 jumpTime을 현재 시간으로
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

    // 공격에 대한 프리팹
    // 프리팹을 만들기 위한 메서드
    protected virtual void GenerateHitBox(string name)
    {
        // 유니티에서 "이미 존재하는 대상"을 코드로 복사하는 경우
        // 프리팹을 복사해서 게임에 올리기
        // Instantiate(인스턴스화 하다) -> 원본을 토대로 새로운 개체 만들기
        GameObject HitBox = Instantiate(Resources.Load<GameObject>($"Prefabs/Hitboxes/{name}"), transform.position, transform.rotation);
        // ※ TryGetComponent를 prefab이 아닌 Instantiate된 오브젝트에 쓸 것
        if(HitBox.TryGetComponent(out DamageComponent damage))
        {
            // 항상 초기화
            damage.Initialize(this, attackPower, criticalRate);
        }
        Destroy(HitBox, 0.04f);
    }

    protected override bool OnBreak()
    {
        animator.SetTrigger("doDeath");
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        // bitMask : 최적화 -> 서버 0.01초 : 12만 동접 -> 120초
        // 00000000 00000000 00000000 00000000 => int는 32bit
        //                                   1  2^0( 1<<0 ) Default
        //                            10000000  2^7( 1<<7 ) Corpse
        //                            10010001  Corpse + Water + Default ( 1<<7 | 1<<4 | 1<<0 )
        //                                      1<<LayerMask.NameToLayer("Corpse");
        

        // 코루틴은 만든다고 끝나는 것이 아니다
        // 코루틴이 Update보다 성능이 떨어져서 오차가 있어 더 짧게
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
        // 움직이는 방향
        // direction.normalized => 본인은 안바뀜, 값만 반환
        // direction.Normalize() => 본인이 바뀜
        direction.Normalize();

        // Move는 움직이라는 함수지만
        // 생각이 먼저, 그 다음에 행동은 업데이트
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
        // activeSelf        : 부모 상관없이 자기 자신이 active되어있는지
        // activeInHierarchy : 자신이 켜져있어도 부모가 꺼져있으면 false
        // pause.SetActive(!pause.activeSelf);
        pause.SetActive(!pause.activeInHierarchy);
    }

    //                     On : 플레이어가 직접 입력했을 때
    //                                                      마우스 위치, 근데 픽셀 기준임
    //                                                      월드포인트로 바꿔줘야함.
    protected virtual void OnRangeAttack() => RangeAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition));

    protected virtual void RangeAttack(Vector2 wantPosition)
    {
        Vector2 wantDirection = wantPosition - (Vector2)transform.position;
        wantDirection.Normalize();
        Quaternion wantRotation = Quaternion.Euler(0, 0, Mathf.Atan2(wantDirection.y, wantDirection.x)*Mathf.Rad2Deg);
        // 투사체 불러오기
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

    // enumerator 코루틴에 대한 내용
    // 코루틴을 알고는 계셔야 해서 하는 거에요!
    // 만약에 포트폴리오가서 코루틴을 많이쓰면 다 지울거임!
    // 일반적인 코드보다 100배 정도 느림.
    IEnumerator CreateEffect(GameObject prefab, Vector3 position, Quaternion rotation, float startTime = 0)
    {
        // IEnumerator : 반복자 -> 다른 작업 하다가 다 기다렸는지 확이하러 종종 올거예요
        // 나가려면 반환을 해줘야 합니다. 다른 애들 먼저 하라고 반환한것
        // yield(양보)
        // 만들기 전에 일정시간 기다리는 함수
        yield return new WaitForSeconds(startTime);

        GameObject instance = Instantiate(prefab, position, rotation);
        

        // DestroyTimer 스크립트에서 처리
        // 지울 때까지 기다리기
        //yield return new WaitForSeconds(lifeTime);

        // 지우기
        //Destroy(instance);
    }

}
