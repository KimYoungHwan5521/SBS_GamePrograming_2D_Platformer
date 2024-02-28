using System.IO; // in out ���� �����
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

// JSON���� �����ϴ� ���� 'Ŭ����'�� '����ü'�� �����ϴ� ���� �⺻
public class SaveData
{
    // �Ϲ� Vector2�� float. int�� �������� Vector2Int
    // Ÿ�ϱ�� ����(�ٵ�)�� ���� ����
    public Vector2Int resolution;
    public FullScreenMode fullScreenMode;
    public int stage;
    // ��������ȭ
    // public int vSync;
    // public byte vSync; // -128 ~ 127
    public bool vSync; // ������ vSync�� ���� ���� 0 �Ǵ� 1

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
        // 1. �����Ͱ� null ���� Ȯ��
        if(data == null) return false;
        // 2. �̹� �ִ��� Ȯ��
        // ������ �־�� ������ �ִ�.
        string folderPath = $"{Application.persistentDataPath}/SaveData";
        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = $"{folderPath}/savadata.json";
        if(!File.Exists(filePath))
        {
            // 3. ������ ����
            // ���� �����ִ� ���¸� �޾ƿ°� -> �ٽ� �ݾƾߵ�
            File.Create(filePath).Close();
        }
        // 4. �����͸� JSON���� ��ȯ
        string jsonData = JsonUtility.ToJson(data);
        // 5. ��ȯ�� �����͸� ���Ͽ� �־���
        File.WriteAllText(filePath, jsonData);
        return true;
    }

    public SaveData Load()
    {
        // 1. ���� ��θ�  folderPath�� ����
        string folderPath = $"{Application.persistentDataPath}/SaveData";
        // 2. ������ �ִ��� Ȯ��
        if(!Directory.Exists(folderPath))
        {
            // 3. ������ ������ null���� �����ϰ� ����
            return null;
        }
        // 4. ������ ������ ���ϰ�θ� filePath�� ����
        string filePath = $"{folderPath}/savadata.json";
        if(!File.Exists(filePath))
        {
            // 5. ������ ������ null���� �����ϰ� ����
            return null;
        }
        // 6. �����͸� ������ data�� ����
        SaveData data;
        // 7. ������ �����͸� ���� ������ ReadAllText
        string jsonData = File.ReadAllText(filePath);
        // 8. ���� json �����͸� data�� �Ľ�
        data = JsonUtility.FromJson<SaveData>(jsonData);
        // 9. data�� ��ȯ
        return data;
    }

    // �����ϴ� �Լ��� �� ����
    // Awake - �� ģ���� ���ӿ� ó�� �����ϴ� ����
    // OnEnable - Ȱ��ȭ �� ������ ����
    // Start - Update�� �Ϸ��� ó�� �õ��ϴ� ����
    private void Awake()
    {
        SaveData data = Load();
        if(data == null)
        {
            data = new SaveData(new Vector2Int(1920,1080), FullScreenMode.FullScreenWindow, 1, false);
        
        }

        Screen.SetResolution(data.resolution.x, data.resolution.y, data.fullScreenMode);
        // �ɼ��� ������ �� ��������ȭ���� �ɼ��� ���
        QualitySettings.vSyncCount = data.vSync ? 1 : 0;
    }
}
