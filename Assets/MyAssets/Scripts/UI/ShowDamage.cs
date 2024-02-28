using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ShowDamage : MonoBehaviour
{
    // 공격을 받는 애는 Breakable
    // 얘의 위치에서 보여줍시다.
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
        // 이 게임 오브젝트의 위치를 Victim의 위치로 옮겨주세요
        // UI오브젝트의 위치와 일반 게임 오브젝트의 위치 기준점 차이
        // 카메라에 있는 캐릭터 위치를 -> 스크린 위치로
        // World : 실제 게임위치
        // ScreenPoint : 화면상 좌표 (절대좌표)
        // Viewport : 화면상 좌표 (비율)
        // To Ray : 화면에서 선을 쏨 (마우스 클릭 체크, UI밑을 지나가는 애를 찾을 수 있음)
        // transform.position = Camera.main.WorldToScreenPoint(victim.transform.position);
    }

    // 함수를 통해 피해자, 데미지, b크리티컬 넣어주기
    public void SetValues(Breakable victim, int damage, bool isCritical)
    {
        this.victim = victim;
        damageValue= damage;
        this.isCritical = isCritical;

        // 피해자가 누구인지 알았다.
        // 위치를 이동
        transform.position = Camera.main.WorldToScreenPoint(victim.transform.position);
        // 지금 켜질 오브젝트를 저장
        GameObject activeObject = isCritical ? criticalPrefab: normalPrefab;
        // 켜기
        activeObject.SetActive(true);

        // 본인한테 있는게 아니라 자식한테 있는 경우 : GetComponentInChild
        // (본인도 제일 먼저 체크함)
        damageText = activeObject.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = $"{damageValue}";
    }
}
