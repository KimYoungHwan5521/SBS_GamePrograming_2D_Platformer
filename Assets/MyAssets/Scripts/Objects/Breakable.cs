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
    // ������Ƽ�� ����� ���� ������ ��!
    // ����Ƽ���� ������ �Ұ���(Ŀ���� �ν����͸� ����� ������)
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
            // �ٲ�� �� �����   �ٲ� �����
            if (_hpCurrent > 0 && value <= 0) OnBreak();
            _hpCurrent = Mathf.Clamp(value, 0, HpMax);
        }
    }

    [SerializeField]  int _hpMax;
    public int HpMax
    { 
        get => _hpMax; 
        // �ִ�ü���� �ö󰡸� ����ü�µ� �ö󰡴� ���
        protected set
        {
            _hpMax = value;
            // �� �ִ�ü���� ������ �� ����ü�� üũ
            HpCurrent = HpCurrent;
        }
    }

    // ���� true, �Ʊ� false
    public virtual bool CheckEnemy(Breakable other)
    {
        if(other == null || other == this) return false;
        if (team == other.team)
        {
            // �߸��̳� ������Ʈ���� ���� �����ϰ�
            if(other.team == Team.Neutral || other.team == Team.Object)
            {
                return true;
            }
            return false;
        }
        else
        {
            // ���� �ٸ����ε� ������ NPC�̰�, �ٸ� ������ Ally
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
        // ������ �ޱ� ��
        if (IsBreak) return 0;
        // 1. ������ �������� ����
        GameObject inst = Resources.Load<GameObject>("Prefabs/UI/ShowDamage");
        // 2. �������� ����
        // 5. Canvas�� ������ �ν��Ͻ��� �־���
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        GameObject prefab = Instantiate(inst, mainCanvas.transform);
        prefab.transform.position = Camera.main.WorldToScreenPoint(transform.position);
        // 3. ������ ������ �������� ShowDamage�� ����
        // 4. ������ ������ �������� ShowDamage�� ���� �־���
        prefab.GetComponent<ShowDamage>().SetValues(this, damage, isCritical);
        // extra: ���ҽ��� ���� �����ϰ� ���� �ѹ��� ��������

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
