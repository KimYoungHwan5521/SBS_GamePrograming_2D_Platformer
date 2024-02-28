using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    protected float timeScale;

    // �� "������Ʈ"�� ������ ��
    private void OnEnable()
    {
        timeScale= Time.timeScale;
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = timeScale;
    }

    // ��ư�� ����� �Լ��� ���� ��������
    // 1. static�̸� �ȵ�
    // 2. public�̾�� ��
    // 3. �Ű������� int, float, string, bool �߿� �ϳ��� ����
    public void GoTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
