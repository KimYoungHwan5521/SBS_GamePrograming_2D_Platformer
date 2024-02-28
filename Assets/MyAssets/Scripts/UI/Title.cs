using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Title : MonoBehaviour
{
    public TMP_Dropdown resolution;

    public FullScreenMode screenMode = FullScreenMode.FullScreenWindow;
    public void ResolutionChange(int value)
    {
        Debug.Log($"Set Resolution to {resolution.options[value].text}");
        switch(value)
        {
            case 0: // 1920x1080
                Screen.SetResolution(1920, 1080, screenMode);
                break;
            case 1: // 1440x960
                Screen.SetResolution(1440, 960, screenMode);
                break;
            case 2: // 800x600
                Screen.SetResolution(800, 600, screenMode);
                break;
            default:
                Screen.SetResolution(1920, 1080, screenMode);
                break;
        }
    }

    public void ScreenModeChange(int value)
    {
        switch(value)
        {
            case 0: // ��üȭ��
                screenMode = FullScreenMode.FullScreenWindow; break;
            case 1: // �׵θ� ����
                screenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 2: // â���
                screenMode = FullScreenMode.Windowed; break;
            default:
                screenMode = FullScreenMode.FullScreenWindow; break;
        }
    }

    public void GameStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Laboratory");
    }

    public void GameQuit()
    {
        // ����Ƽ���� �����ϰ� ���� ������ ����Ƽ�� ���ø����̼��̴ϱ� �������� �ȵ�
        // ��ó����
        // ���� ó���Ѵ�
        // [�ڵ� -> ��������] �ϱ� ���� ����
        // "�ڵ带 �ٷ�� �ϴ� ���"��?
        // ���־�Ʃ��������� �� �����µ�, �����ϸ� �Ⱥ��̴� ����
        // ����Ƽ �����ʹ� ���� �������Ͽ� ������ �Ǹ� �ȵ�
        // ���� �ڵ尡 �ƿ� ������ �ϴ� ��찡 ����
        // ��ó����� #���� ����
        // ��ó���� if�� "����� �ִ°�?"��� �ϴ� ������ �������ֽø� �˴ϴ�.
        // if - endif
        // if - elif - else - endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


}
