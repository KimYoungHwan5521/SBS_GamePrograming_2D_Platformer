using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


// AI가 들어간 이상 살아서 움직여야 한다
// AI는 무조건 "캐릭터"와 함께 간다.
// AI는 캐릭터가 필요해요
// RequireComponent를 사용하면 컴포넌트가 없는 경우에는 자동으로 생성해준다
[RequireComponent(typeof(Character))] 
public class AIBase : MonoBehaviour
{
    public CapsuleCollider2D capsule;
    public Character controlledCharacter;
    public Breakable target;
    public Vector3 preferredLocation;
    public float rangeAttack;
    public float rangeRecognize;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 다른 캐릭터랑 부딪히면
        if(collision.collider.TryGetComponent(out Character otherCharacter))
        {
            // 내 기준 적이면
            if (controlledCharacter.CheckEnemy(otherCharacter))
            {
                target = otherCharacter;
            }
            else
            {
                controlledCharacter.FaceDirection = controlledCharacter.transform.position - otherCharacter.transform.position;
            }
        }
    }

    protected virtual void Start()
    {
        //target = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Character>();
        
        // AI컴포넌트는 어디에 놓을까
        // PlayerInput이 없다면 이 친구가 작동할 것
        // 만약 PlayerInput이 있다면 이 친구가 제거될 것
        if(GetComponent<PlayerInput>())
        {
            Destroy(this);
        }
        else
        {
            // PlayerInput이 없으면 내가 이 캐릭터를 조종하겠다
            controlledCharacter = gameObject.GetComponent<Character>();
            capsule = controlledCharacter.GetComponent<CapsuleCollider2D>();
        }
    }

    protected virtual void Update()
    {
        if(controlledCharacter.IsGround == false)
        {
            controlledCharacter.Move(controlledCharacter.FaceDirection);
        }
        if(target)
        {
            // 타겟이 있을때 이동
            // MoveToTarget();
            // 플레이어가 공격 범위 안에 들어왔을 때 행동
            if(CheckAttackable(target))
            {
                AttackToTarget();
            }
            else
            {
                Vector2 targetDirection = target.transform.position - controlledCharacter.transform.position;
                // 회전은 언제
                if(CheckLand())
                {
                    if (target)
                    {
                        // target의 방향에 현재 진행방향과 다르면 뒤돌기
                        if(targetDirection.x * controlledCharacter.FaceDirection.x < 0)
                        {
                            controlledCharacter.FaceDirection *= -1;
                        }
                        //                               같으면 점프
                        else
                        {
                            // 타겟이 -1칸보다 더 위에 있다
                            if(targetDirection.y >= -1)
                            {
                                controlledCharacter.Jump();
                            }
                        }
                    }
                }
                else if(CheckCliff())
                {
                    if(targetDirection.y >= -1)
                    {
                        RaycastHit2D hit = controlledCharacter.transform.position.Curvecast2D(controlledCharacter.MoveDirection * controlledCharacter.moveSpeed + controlledCharacter.jumpPower * 2 * Vector2.up, Physics2D.gravity, 2f, 30, 1);
                        // cast가 닿은면의 방향 : 노말
                        // 노말이 위를 바라보고 있어야 땅, 아래를 보고있으면 천장
                        // 각도가 위를 기준으로 45도 이하로 기울어져 있으면 땅
                        if(hit.collider != null && Vector2.Angle(Vector2.up, hit.normal) < 45)
                        {
                            controlledCharacter.Jump();
                        }
                        else
                        {
                            controlledCharacter.FaceDirection *= -1;
                        }
                    }
                }
                // 공격할 수 없을 때에만 이동하도록
                controlledCharacter.Move(controlledCharacter.FaceDirection);
            }
            // 타겟이 범위 밖으로 나가면 그만 추격
            //Debug.Log($"target: {target}, dist: {(target.transform.position - controlledCharacter.transform.position).magnitude}");
            if((target.transform.position - controlledCharacter.transform.position).magnitude > rangeRecognize) 
            {
                target = null;
            }
            // 눈 앞에 벽이 있는지 체크
            // 낭떠러지 체크
            if(CheckLand()) controlledCharacter.Jump();
            if(CheckCliff())
            {
                // 점프할 수 있는 타이밍 : 점프해도 닿을 곳이 있는 경우
                if (controlledCharacter.transform.position.Curvecast2D(controlledCharacter.MoveDirection + controlledCharacter.jumpPower * Vector2.up * 2, Physics2D.gravity, 2f, 10, 1))
                {
                    controlledCharacter.Jump();
                }
            }
            // 타겟이 나보다 위에 있을때 체크
        }
        else
        {
            // 타겟이 없으면 타겟 찾기
            CheckAroundEnemy();
            // 찾았는데 타겟이 없다면
            if(target == null)
            {
                if(controlledCharacter.IsGround && (CheckLand() || CheckCliff()))
                {
                    controlledCharacter.FaceDirection *= -1;
                }
                controlledCharacter.Move(controlledCharacter.FaceDirection);
            }
        }
    }

    protected virtual bool CheckCliff()
    {
        Vector3 stepableHeight = capsule.bounds.center + Vector3.down * (capsule.bounds.extents.y - capsule.bounds.extents.x) + (Vector3)controlledCharacter.MoveDirection * capsule.bounds.extents.x * 0.5f;
        Ray cliffRay = new Ray(stepableHeight, Vector3.down);
        Debug.DrawRay(cliffRay.origin, cliffRay.direction * 3);

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Default"));
        RaycastHit2D[] hits = new RaycastHit2D[10];
        int hitAmount = Physics2D.Raycast(cliffRay.origin, cliffRay.direction, filter, hits, 3);
        return hitAmount == 0;
    }

    protected virtual bool CheckLand()
    {
        // .bounds.center는 콜라이더의 중앙
        // .bounds.extents는 절반 길이 -> 캡슐 바닥 = 캡슐 중앙 + 아래로 extents 높이만큼
        Vector3 stepableHeight = capsule.bounds.center + Vector3.down * (capsule.bounds.extents.y - capsule.bounds.extents.x);

        // ray 선이 보인다
        // 선의 요소 -> 시작점과 끝 점
        Ray stepRay = new Ray(stepableHeight, controlledCharacter.MoveDirection);
        Debug.DrawRay(stepRay.origin, stepRay.direction);

        // Raycast를 통해 맞은 상대를 체크
        // 봤는데 점프를 할 필요가 없는 오브젝트들도 다 체크된다
        // 트리거인 애들은 배제
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(LayerMask.GetMask("Default"));
        RaycastHit2D[] hits = new RaycastHit2D[10];
        // Raycast같은 걸 사용했을때 숫자가 리턴되는 경우 : 부딪힌 갯수
        int hitAmount = Physics2D.Raycast(stepRay.origin, stepRay.direction, filter, hits, 1);
        // Debug.Log(hits[0].collider);
        if (hitAmount > 0)
        {
            return true;
        }
        else return false;
    }

    protected virtual void CheckAroundEnemy()
    {
        // 물리 객체들을 체크 -> Cast
        // 맞은 친구들을 RaycastHit로 돌려줌
        RaycastHit2D[] hits = Physics2D.CircleCastAll(controlledCharacter.transform.position, rangeRecognize, Vector2.right);
        
        Character nearest = null;
        // hits 중에서 캐릭터 찾기
        // 1. hits 안에 있는 게임오브젝트들을 순회
        foreach (RaycastHit2D hit in hits)
        {
            // 2. Character 컴포넌트를 가지고 있는
            if(hit.transform.TryGetComponent(out Character character))
            {
                // 3. Character라면 피아식별
                if(controlledCharacter.CheckEnemy(character))
                {
                    // 4. 적이라면 : 가장 가까운 적 확인
                    // 5. 지금까지 가장 가까웠던 적이랑, 지금 적이랑 비교해서 더 가까운 녀석이면 저장.
                    if(nearest == null)
                    {
                        nearest = character;
                    }
                    else if((nearest.transform.position - controlledCharacter.transform.position).magnitude > (character.transform.position - controlledCharacter.transform.position).magnitude)
                    {
                        nearest = character;
                    }
                }
            }
        }
        // 6. 모든 것을 다 돌고 나서 가장 가까운 적을 target으로 지정
        target = nearest;
    }

    protected virtual bool CheckAttackable(Breakable other)
    {
        // 둘 사이의 거리
        float distance = Vector3.Distance(other.transform.position, controlledCharacter.transform.position);

        // 둘 사이의 거리가 공격 사거리보다 짧은 경우 true
        return distance < rangeAttack;
    }

    protected virtual void AttackToTarget()
    {
        controlledCharacter.FaceDirection = target.transform.position - controlledCharacter.transform.position;
        controlledCharacter.Attack();
    }

    protected virtual void MoveToTarget()
    {
        // 방향 : 목적지 - 출발지
        controlledCharacter.Move(target.transform.position - gameObject.transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, rangeRecognize);
    }


}
