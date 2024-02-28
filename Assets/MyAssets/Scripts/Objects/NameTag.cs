using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameTag : MonoBehaviour
{
    protected Character character;
    protected Slider hpBar;
    protected RectTransform hpDelta;
    protected TextMeshProUGUI nameText;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponentInParent<Character>();
        hpBar = GetComponentInChildren<Slider>();
        nameText= GetComponentInChildren<TextMeshProUGUI>();

        hpDelta = hpBar.fillRect;
        // 촌수로
        // 형제 2촌
        // 부모로 올라갔다가 자식 찾기
        hpDelta = hpDelta.parent.GetComponent<RectTransform>();
        // 부모 입장에서 자식을 바라봅시다.
        // 자식 2명
        // hpDelta는 첫째
        // 첫째를 찾아와서 hpDelta로 등록
        if(hpDelta.childCount > 1)
        {
            hpDelta = hpDelta.GetChild(0).GetComponent<RectTransform>();
        }

        int removeIndex = character.name.LastIndexOf("(Clone)");
        if(removeIndex > -1)
        {
            nameText.text = character.name.Remove(removeIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(character.HpCurrent <= 0) hpBar.gameObject.SetActive(false);
        else
        {
            hpBar.value = (float) character.HpCurrent / character.HpMax;
            hpDelta.anchorMax = new Vector2(Mathf.SmoothStep(hpDelta.anchorMax.x, (float)character.HpCurrent / character.HpMax, Time.deltaTime * 10), hpDelta.anchorMax.y);
        }
    }
}
