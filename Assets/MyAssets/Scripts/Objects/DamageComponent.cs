using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    // 1. �ϴ� ������ ���� ���� ������ ���� �ɸ��� ��
    // ĳ���� ��������Ʈ��Ʈ�� �ϳ��� �Ǿ��ִ� ���� �´�
    // ���߿��� ���� ĳ����, ���� ���µ� �ð��ɸ�
    // 2. ������ ���� ���� �ִ��� ����
    // 3. ������ ����Ʈ ������ ã�� �;�� -> �ʱ�ȭ �� �� �ѹ��� ����
    public Breakable attacker;
    public int damage;
    public float damageCorrectionValue;
    public float criticalRate;

    GameObject hitEffect;
    public string hitEffectName;

    // ������Ʈ�� ��ӹ޴� ��� Ŭ�������� �����ڸ� ����� �� ����.
    public void Initialize(Breakable attacker, float damage, float criticalRate)
    {
        // Resources.Load ó�� �����̳� �̸� ���ǵ� ����� ã�� ��쿡�� ���� ������ ã�� �� �ִ� ����� �ִ�.
        // ��Ʈ���� -> ���ü�� -> �ؽ��ڵ�
        // ���� �ڸ��� �ű�� ���̰� ... -> �ؽð��� ����
        // �Ȱ��� ���� ������ -> �Ȱ��� �ؽð��� ����
        // �߰��� ������ �������� �ٽ� �ǵ��� �� ����.
        // �α����� �� ��й�ȣ
        // ���� �ӵ��� ���̷��� �ؽø� ����� ���� �ִ� : (string).GetHashCode()
        this.attacker = attacker;
        damageCorrectionValue *= damage; 
        this.criticalRate = criticalRate;

        hitEffect = Resources.Load<GameObject>($"Prefabs/Effects/{hitEffectName}");

    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // �̷��Ե� ��
        OnTriggerEnter2D(collision.collider);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) 
    {
        // ������ Breakable�� ������ �ִ���
        // Character ������Ʈ�� Breakable�� ��ӹ޾����ϱ� ������ �� ����
        // out�� Breakable�� ������ <Breakable> ���� ����
        if(other.TryGetComponent<Breakable>(out Breakable victim) && attacker.CheckEnemy(victim))
        {
            int damage = GiveDamage(victim);
            if (damage > 0)
            {
                // �˹�
                // attacker�� victim�� ���� ����
                // ��ġ ��
                Vector3 knockbackVector = victim.transform.position - attacker.transform.position;
                knockbackVector.z = knockbackVector.y = 0;
                knockbackVector.Normalize();
                
                // ĳ������ �̵��ӵ� ���� �����ӵ��� �غ��ؼ� ������ ���� �� �Ǵ� ������ ���� ĳ������ �ӵ��� ��ȭ
                // �� Ÿ�ֿ̹� �����ӵ��� ���� ���� �� �ְ� Character.LaunchCharacter(Vector3 force)
                // �Լ��� ����� �ָ� ����.
                victim.GetComponent<Rigidbody2D>().AddForce(knockbackVector * damage * 10 + Vector3.up * damage * 5);

                GameObject inst = Instantiate(hitEffect, other.bounds.center, Quaternion.identity);
                if (inst != null)
                {

                }
                else
                {
                    Debug.Log($"���� Prefabs/Effects/{hitEffectName} ��(��) ã�� �� �����ϴ�.");
                }

            }

        }

    }
    protected virtual int GiveDamage(Breakable target)
    {
        float totalDamage = damage + damageCorrectionValue;
        bool isCritical = false;
        if (Random.value < criticalRate)
        {
            isCritical = true;
            totalDamage *= 2;
        }
        // 1. ���� ������
        totalDamage = Mathf.Clamp(totalDamage, 0, 9999);
        // 2. �ø� / ����(�⺻��) / �ݿø�
        return target.TakeDamage(attacker, Mathf.RoundToInt(totalDamage), isCritical);
    }
}
