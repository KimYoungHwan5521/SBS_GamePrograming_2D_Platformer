using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    protected float timeScale;

    // 이 "컴포넌트"가 켜졌을 때
    private void OnEnable()
    {
        timeScale= Time.timeScale;
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = timeScale;
    }

    // 버튼에 등록할 함수에 대한 제약조건
    // 1. static이면 안됨
    // 2. public이어야 함
    // 3. 매개변수는 int, float, string, bool 중에 하나만 가능
    public void GoTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
