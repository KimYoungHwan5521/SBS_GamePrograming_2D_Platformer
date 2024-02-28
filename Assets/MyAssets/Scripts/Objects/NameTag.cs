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
        // �̼���
        // ���� 2��
        // �θ�� �ö󰬴ٰ� �ڽ� ã��
        hpDelta = hpDelta.parent.GetComponent<RectTransform>();
        // �θ� ���忡�� �ڽ��� �ٶ󺾽ô�.
        // �ڽ� 2��
        // hpDelta�� ù°
        // ù°�� ã�ƿͼ� hpDelta�� ���
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
