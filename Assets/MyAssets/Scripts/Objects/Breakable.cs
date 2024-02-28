using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum Team 
{ 
    Ally, Enemy, NPC, Neutral, Object
}

public abstract class Breakable : MonoBehaviour
{
    // 프로퍼티를 사용할 때에 주의할 점!
    // 유니티에서 수정이 불가능(커스텀 인스펙터를 만들기 전까지)
    [SerializeField] public Team team;
    public Team GetTeam() => team;


    protected bool _isBreak;
    public bool IsBreak
    {
        get => _isBreak;
        protected set => _isBreak = value; 
    }

    [SerializeField] int _hpCurrent;
    public int HpCurrent
    { 
        get => _hpCurrent;
        protected set
        { 
            // 바뀌기 전 생명력   바뀔 생명력
            if (_hpCurrent > 0 && value <= 0) OnBreak();
            _hpCurrent = Mathf.Clamp(value, 0, HpMax);
        }
    }

    [SerializeField]  int _hpMax;
    public int HpMax
    { 
        get => _hpMax; 
        // 최대체력이 올라가면 현재체력도 올라가는 경우
        protected set
        {
            _hpMax = value;
            // ※ 최대체력이 내려갈 때 현재체력 체크
            HpCurrent = HpCurrent;
        }
    }

    // 적군 true, 아군 false
    public virtual bool CheckEnemy(Breakable other)
    {
        if(other == null || other == this) return false;
        if (team == other.team)
        {
            // 중립이나 오브젝트들은 서로 적대하게
            if(other.team == Team.Neutral || other.team == Team.Object)
            {
                return true;
            }
            return false;
        }
        else
        {
            // 나랑 다른팀인데 한쪽이 NPC이고, 다른 한쪽이 Ally
            /*
            if(team == Team.NPC && other.team == Team.Ally || team == Team.Ally && other.team == Team.NPC)
            {
                return false;
            }
            */
            if(team == Team.NPC)
            {
                if(other.team == Team.Ally)
                {
                    return false;
                }
            }
            if(team == Team.Ally)
            {
                if(other.team == Team.NPC)
                {
                    return false;
                }
            }
            return true;
        }
    }
    
    protected virtual bool OnBreak()
    {
        return IsBreak = true;
    }
    public virtual bool Break()
    {
        if (IsBreak) return false;
        OnBreak();
        return default;
    }

    public virtual int TakeDamage(Breakable obj, int damage, bool isCritical = false)
    {
        // 데미지 받기 전
        if (IsBreak) return 0;
        // 1. 데미지 프리팹을 생성
        GameObject inst = Resources.Load<GameObject>("Prefabs/UI/ShowDamage");
        // 2. 프리팹을 생성
        // 5. Canvas에 데미지 인스턴스를 넣어줌
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        GameObject prefab = Instantiate(inst, mainCanvas.transform);
        prefab.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        // 3. 생성된 데미지 프리팹의 ShowDamage를 저장
        // 4. 생성된 데미지 프리팹의 ShowDamage에 값을 넣어줌
        prefab.GetComponent<ShowDamage>().SetValues(this, damage, isCritical);
        // extra: 리소스는 게임 시작하고 나서 한번만 가져오게

        HpCurrent -= damage;
        return damage;
    }

    public virtual int TakeHeal(Breakable from, int heal)
    {
        if (IsBreak) return 0;
        return default;
    }

    public virtual bool Repair()
    {
        if(IsBreak) return false;
        return default;
    }

}
