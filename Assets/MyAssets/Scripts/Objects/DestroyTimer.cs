using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    public float lifeTime; // ����
    private float leftTime;// ���� ����

    // Start is called before the first frame update
    void Start()
    {
        leftTime = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        // delta : ��ȭ��
        // ���� �ð�    -    ������ �ð�      = �����ð�
        //     5                4.5         =    0.5
        leftTime -= Time.deltaTime;
        // Update���� ���� : Time.deltaTime
        // FixedUpdate���� ���� : Time.fixedDeltaTime
        if(leftTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
