using System.IO; // in out 파일 입출력
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

// JSON으로 저장하는 것은 '클래스'나 '구조체'를 저장하는 것이 기본
public class SaveData
{
    // 일반 Vector2는 float. int만 받으려면 Vector2Int
    // 타일기반 게임(바둑)에 많이 쓰임
    public Vector2Int resolution;
    public FullScreenMode fullScreenMode;
    public int stage;
    // 수직동기화
    // public int vSync;
    // public byte vSync; // -128 ~ 127
    public bool vSync; // 실제로 vSync에 넣을 값은 0 또는 1

    public SaveData
    (
        Vector2Int resolution,
        FullScreenMode fullScreenMode,
        int stage,
        bool vSync
    )
    {
        this.resolution = resolution;
        this.fullScreenMode = fullScreenMode;
        this.stage = stage;
        this.vSync = vSync;
    }
}

public class DataManager : MonoBehaviour
{

    // private void OnApplicationQuit() { }

    private void OnDestroy()
    {
        Save(new SaveData(new Vector2Int(1920,1080), FullScreenMode.FullScreenWindow, 1, false));
    }


    public bool Save(SaveData data)
    {
        // 1. 데이터가 null 인지 확인
        if(data == null) return false;
        // 2. 이미 있는지 확인
        // 폴더가 있어야 파일이 있다.
        string folderPath = $"{Application.persistentDataPath}/SaveData";
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = $"{folderPath}/savadata.json";
        if(!File.Exists(filePath))
        {
            // 3. 파일을 만듦
            // 파일 쓰고있는 상태를 받아온것 -> 다시 닫아야됨
            File.Create(filePath).Close();
        }
        // 4. 데이터를 JSON으로 변환
        string jsonData = JsonUtility.ToJson(data);
        // 5. 변환한 데이터를 파일에 넣어줌
        File.WriteAllText(filePath, jsonData);
        return true;
    }

    public SaveData Load()
    {
        // 1. 폴더 경로를  folderPath로 선언
        string folderPath = $"{Application.persistentDataPath}/SaveData";
        // 2. 폴더가 있는지 확인
        if(!Directory.Exists(folderPath))
        {
            // 3. 폴더가 없으면 null값을 리턴하고 종료
            return null;
        }
        // 4. 폴더가 있으면 파일경로를 filePath로 선언
        string filePath = $"{folderPath}/savadata.json";
        if(!File.Exists(filePath))
        {
            // 5. 파일이 없으면 null값을 리턴하고 종료
            return null;
        }
        // 6. 데이터를 저장할 data를 선언
        SaveData data;
        // 7. 파일의 데이터를 전부 가져옴 ReadAllText
        string jsonData = File.ReadAllText(filePath);
        // 8. 받은 json 데이터를 data에 파싱
        data = JsonUtility.FromJson<SaveData>(jsonData);
        // 9. data를 반환
        return data;
    }

    // 시작하는 함수는 총 세개
    // Awake - 이 친구가 게임에 처음 진입하는 순간
    // OnEnable - 활성화 될 때마다 실행
    // Start - Update를 하려고 처음 시도하는 순간
    private void Awake()
    {
        SaveData data = Load();
        if(data == null)
        {
            data = new SaveData(new Vector2Int(1920,1080), FullScreenMode.FullScreenWindow, 1, false);
        
        }

        Screen.SetResolution(data.resolution.x, data.resolution.y, data.fullScreenMode);
        // 옵션을 설정할 때 수직동기화같은 옵션은 어디에
        QualitySettings.vSyncCount = data.vSync ? 1 : 0;
    }
}
