using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ShowDamage : MonoBehaviour
{
    // ������ �޴� �ִ� Breakable
    // ���� ��ġ���� �����ݽô�.
    public Breakable victim;

    public TextMeshProUGUI damageText;

    public GameObject normalPrefab;
    public GameObject criticalPrefab;

    public int damageValue;
    public bool isCritical;

    private void Start()
    {
        SetValues(victim, damageValue, isCritical);
    }

    private void Update()
    {
        if (victim == null) return;
        // �� ���� ������Ʈ�� ��ġ�� Victim�� ��ġ�� �Ű��ּ���
        // UI������Ʈ�� ��ġ�� �Ϲ� ���� ������Ʈ�� ��ġ ������ ����
        // ī�޶� �ִ� ĳ���� ��ġ�� -> ��ũ�� ��ġ��
        // World : ���� ������ġ
        // ScreenPoint : ȭ��� ��ǥ (������ǥ)
        // Viewport : ȭ��� ��ǥ (����)
        // To Ray : ȭ�鿡�� ���� �� (���콺 Ŭ�� üũ, UI���� �������� �ָ� ã�� �� ����)
        // transform.position = Camera.main.WorldToScreenPoint(victim.transform.position);
    }

    // �Լ��� ���� ������, ������, bũ��Ƽ�� �־��ֱ�
    public void SetValues(Breakable victim, int damage, bool isCritical)
    {
        this.victim = victim;
        damageValue= damage;
        this.isCritical = isCritical;

        // �����ڰ� �������� �˾Ҵ�.
        // ��ġ�� �̵�
        transform.position = Camera.main.WorldToScreenPoint(victim.transform.position);
        // ���� ���� ������Ʈ�� ����
        GameObject activeObject = isCritical ? criticalPrefab: normalPrefab;
        // �ѱ�
        activeObject.SetActive(true);

        // �������� �ִ°� �ƴ϶� �ڽ����� �ִ� ��� : GetComponentInChild
        // (���ε� ���� ���� üũ��)
        damageText = activeObject.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = $"{damageValue}";
    }
}
