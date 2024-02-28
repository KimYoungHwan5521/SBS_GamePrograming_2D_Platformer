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
            case 0: // 전체화면
                screenMode = FullScreenMode.FullScreenWindow; break;
            case 1: // 테두리 없음
                screenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 2: // 창모드
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
        // 유니티에서 제작하고 있을 때에는 유니티가 어플리케이션이니까 꺼버리면 안됨
        // 전처리기
        // 먼저 처리한다
        // [코드 -> 실행파일] 하기 전에 진행
        // "코드를 다뤄야 하는 경우"란?
        // 비주얼스튜디오에서는 잘 보였는데, 빌드하면 안보이는 이유
        // 유니티 에디터는 실제 게임파일에 포함이 되면 안됨
        // 게임 코드가 아예 빠져야 하는 경우가 생김
        // 전처리기는 #으로 시작
        // 전처리기 if는 "대상이 있는가?"라고 하는 것으로 생각해주시면 됩니다.
        // if - endif
        // if - elif - else - endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


}
