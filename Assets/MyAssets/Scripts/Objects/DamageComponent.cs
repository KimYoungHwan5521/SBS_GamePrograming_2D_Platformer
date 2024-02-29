using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    // 1. 일단 파일을 여는 것은 굉장히 오래 걸리는 일
    // 캐릭터 스프라이트시트가 하나로 되어있는 것이 맞다
    // 나중에는 여러 캐릭터, 파일 여는데 시간걸림
    // 2. 파일을 여는 것은 최대한 적게
    // 3. 데미지 이펙트 파일을 찾고 싶어요 -> 초기화 할 때 한번만 열게
    public Breakable attacker;
    public int damage;
    public float damageCorrectionValue;
    public float criticalRate;

    GameObject hitEffect;
    public string hitEffectName;

    // 컴포넌트를 상속받는 모든 클래스들은 생성자를 사용할 수 없다.
    public void Initialize(Breakable attacker, float damage, float criticalRate)
    {
        // Resources.Load 처럼 파일이나 미리 정의된 대상을 찾느 경우에는 좀더 빠르게 찾을 수 있는 방법이 있다.
        // 비트코인 -> 블록체인 -> 해시코드
        // 값을 자르고 옮기고 붙이고 ... -> 해시값을 만듦
        // 똑같은 값이 들어오면 -> 똑같은 해시값이 나옴
        // 중간에 정보가 없어져서 다시 되돌릴 수 없음.
        // 로그인할 때 비밀번호
        // 연산 속도를 높이려면 해시를 사용할 수도 있다 : (string).GetHashCode()
        this.attacker = attacker;
        damageCorrectionValue *= damage; 
        this.criticalRate = criticalRate;

        hitEffect = Resources.Load<GameObject>($"Prefabs/Effects/{hitEffectName}");

    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 이렇게도 됨
        OnTriggerEnter2D(collision.collider);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) 
    {
        // 상대방이 Breakable을 가지고 있는지
        // Character 컴포넌트도 Breakable을 상속받았으니까 가져올 수 있음
        // out에 Breakable이 있으니 <Breakable> 생략 가능
        if(other.TryGetComponent<Breakable>(out Breakable victim) && attacker.CheckEnemy(victim))
        {
            int damage = GiveDamage(victim);
            if (damage > 0)
            {
                // 넉백
                // attacker가 victim을 보는 방향
                // 위치 비교
                Vector3 knockbackVector = victim.transform.position - attacker.transform.position;
                knockbackVector.z = knockbackVector.y = 0;
                knockbackVector.Normalize();
                
                // 캐릭터의 이동속도 말고 물리속도도 준비해서 가만히 있을 때 또는 움직일 때에 캐릭터의 속도가 변화
                // 그 타이밍에 물리속도도 같이 받을 수 있게 Character.LaunchCharacter(Vector3 force)
                // 함수를 만들어 주면 좋다.
                victim.GetComponent<Rigidbody2D>().AddForce(knockbackVector * damage * 10 + Vector3.up * damage * 5);

                GameObject inst = Instantiate(hitEffect, other.bounds.center, Quaternion.identity);
                if (inst != null)
                {

                }
                else
                {
                    Debug.Log($"파일 Prefabs/Effects/{hitEffectName} 을(를) 찾을 수 없습니다.");
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
        // 1. 음수 데미지
        totalDamage = Mathf.Clamp(totalDamage, 0, 9999);
        // 2. 올림 / 내림(기본값) / 반올림
        return target.TakeDamage(attacker, Mathf.RoundToInt(totalDamage), isCritical);
    }
}
