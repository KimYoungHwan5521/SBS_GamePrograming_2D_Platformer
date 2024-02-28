using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusWindow : MonoBehaviour
{
    public Character player;
    public Image hpBar;
    public Image justBeforeHpBar;
    public Image mpBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    // Start is called before the first frame update
    void Start()
    {
        justBeforeHpBar.fillAmount = (float) player.HpCurrent / player.HpMax;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
        hpBar.fillAmount = (float)player.HpCurrent / player.HpMax;
        hpText.text = $"{player.HpCurrent} / {player.HpMax}";
        mpBar.fillAmount = (float)player.MpCurrent / player.MpMax;
        mpText.text = $"{player.MpCurrent} / {player.MpMax}";

        // 차이나는 양의 몇 %만 따라가기
        // 선형보간
        // 1.
        // justBeforeHpBar.fillAmount = Mathf.Max(Mathf.Lerp(justBeforeHpBar.fillAmount, hpBar.fillAmount, Time.deltaTime), hpBar.fillAmount);

        // 2. 
        justBeforeHpBar.fillAmount = Mathf.SmoothStep(justBeforeHpBar.fillAmount, hpBar.fillAmount, Time.deltaTime * 10);

    }
}
