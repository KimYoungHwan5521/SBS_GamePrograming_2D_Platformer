using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


// AI�� �� �̻� ��Ƽ� �������� �Ѵ�
// AI�� ������ "ĳ����"�� �Բ� ����.
// AI�� ĳ���Ͱ� �ʿ��ؿ�
// RequireComponent�� ����ϸ� ������Ʈ�� ���� ��쿡�� �ڵ����� �������ش�
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
        // �ٸ� ĳ���Ͷ� �ε�����
        if(collision.collider.TryGetComponent(out Character otherCharacter))
        {
            // �� ���� ���̸�
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
        
        // AI������Ʈ�� ��� ������
        // PlayerInput�� ���ٸ� �� ģ���� �۵��� ��
        // ���� PlayerInput�� �ִٸ� �� ģ���� ���ŵ� ��
        if(GetComponent<PlayerInput>())
        {
            Destroy(this);
        }
        else
        {
            // PlayerInput�� ������ ���� �� ĳ���͸� �����ϰڴ�
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
            // Ÿ���� ������ �̵�
            // MoveToTarget();
            // �÷��̾ ���� ���� �ȿ� ������ �� �ൿ
            if(CheckAttackable(target))
            {
                AttackToTarget();
            }
            else
            {
                Vector2 targetDirection = target.transform.position - controlledCharacter.transform.position;
                // ȸ���� ����
                if(CheckLand())
                {
                    if (target)
                    {
                        // target�� ���⿡ ���� �������� �ٸ��� �ڵ���
                        if(targetDirection.x * controlledCharacter.FaceDirection.x < 0)
                        {
                            controlledCharacter.FaceDirection *= -1;
                        }
                        //                               ������ ����
                        else
                        {
                            // Ÿ���� -1ĭ���� �� ���� �ִ�
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
                        // cast�� �������� ���� : �븻
                        // �븻�� ���� �ٶ󺸰� �־�� ��, �Ʒ��� ���������� õ��
                        // ������ ���� �������� 45�� ���Ϸ� ������ ������ ��
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
                // ������ �� ���� ������ �̵��ϵ���
                controlledCharacter.Move(controlledCharacter.FaceDirection);
            }
            // Ÿ���� ���� ������ ������ �׸� �߰�
            //Debug.Log($"target: {target}, dist: {(target.transform.position - controlledCharacter.transform.position).magnitude}");
            if((target.transform.position - controlledCharacter.transform.position).magnitude > rangeRecognize) 
            {
                target = null;
            }
            // �� �տ� ���� �ִ��� üũ
            // �������� üũ
            if(CheckLand()) controlledCharacter.Jump();
            if(CheckCliff())
            {
                // ������ �� �ִ� Ÿ�̹� : �����ص� ���� ���� �ִ� ���
                if (controlledCharacter.transform.position.Curvecast2D(controlledCharacter.MoveDirection + controlledCharacter.jumpPower * Vector2.up * 2, Physics2D.gravity, 2f, 10, 1))
                {
                    controlledCharacter.Jump();
                }
            }
            // Ÿ���� ������ ���� ������ üũ
        }
        else
        {
            // Ÿ���� ������ Ÿ�� ã��
            CheckAroundEnemy();
            // ã�Ҵµ� Ÿ���� ���ٸ�
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
        // .bounds.center�� �ݶ��̴��� �߾�
        // .bounds.extents�� ���� ���� -> ĸ�� �ٴ� = ĸ�� �߾� + �Ʒ��� extents ���̸�ŭ
        Vector3 stepableHeight = capsule.bounds.center + Vector3.down * (capsule.bounds.extents.y - capsule.bounds.extents.x);

        // ray ���� ���δ�
        // ���� ��� -> �������� �� ��
        Ray stepRay = new Ray(stepableHeight, controlledCharacter.MoveDirection);
        Debug.DrawRay(stepRay.origin, stepRay.direction);

        // Raycast�� ���� ���� ��븦 üũ
        // �ôµ� ������ �� �ʿ䰡 ���� ������Ʈ�鵵 �� üũ�ȴ�
        // Ʈ������ �ֵ��� ����
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(LayerMask.GetMask("Default"));
        RaycastHit2D[] hits = new RaycastHit2D[10];
        // Raycast���� �� ��������� ���ڰ� ���ϵǴ� ��� : �ε��� ����
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
        // ���� ��ü���� üũ -> Cast
        // ���� ģ������ RaycastHit�� ������
        RaycastHit2D[] hits = Physics2D.CircleCastAll(controlledCharacter.transform.position, rangeRecognize, Vector2.right);
        
        Character nearest = null;
        // hits �߿��� ĳ���� ã��
        // 1. hits �ȿ� �ִ� ���ӿ�����Ʈ���� ��ȸ
        foreach (RaycastHit2D hit in hits)
        {
            // 2. Character ������Ʈ�� ������ �ִ�
            if(hit.transform.TryGetComponent(out Character character))
            {
                // 3. Character��� �Ǿƽĺ�
                if(controlledCharacter.CheckEnemy(character))
                {
                    // 4. ���̶�� : ���� ����� �� Ȯ��
                    // 5. ���ݱ��� ���� ������� ���̶�, ���� ���̶� ���ؼ� �� ����� �༮�̸� ����.
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
        // 6. ��� ���� �� ���� ���� ���� ����� ���� target���� ����
        target = nearest;
    }

    protected virtual bool CheckAttackable(Breakable other)
    {
        // �� ������ �Ÿ�
        float distance = Vector3.Distance(other.transform.position, controlledCharacter.transform.position);

        // �� ������ �Ÿ��� ���� ��Ÿ����� ª�� ��� true
        return distance < rangeAttack;
    }

    protected virtual void AttackToTarget()
    {
        controlledCharacter.FaceDirection = target.transform.position - controlledCharacter.transform.position;
        controlledCharacter.Attack();
    }

    protected virtual void MoveToTarget()
    {
        // ���� : ������ - �����
        controlledCharacter.Move(target.transform.position - gameObject.transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, rangeRecognize);
    }


}
