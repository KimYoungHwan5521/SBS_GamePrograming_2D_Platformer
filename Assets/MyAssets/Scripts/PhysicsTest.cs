using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{
    public Rigidbody2D unity;
    public Transform dummy;

    public Vector3 velocity;
    public float time;

    // Start is called before the first frame update
    void Start()
    {
        unity.velocity = velocity;
        Time.timeScale = 0.25f;
    }

    // Update is called once per frame
    void Update()
    {
        // Ű���� ������ ����Ű�� �ӵ��� 0.25�辿 �ø��� ���� ����Ű�� 0.25�辿 ���̰�
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Time.timeScale += 0.25f;
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(Time.timeScale < 0.25f)
            {
                time -= Time.deltaTime;
            }
            else
            {
                Time.timeScale -= 0.25f;
            }
        }

        
        // ������ ��ġ  ��  ��ġ����      ���� �ð� �� ��ġ
        dummy.position = transform.position.GetPositionAfterTime(velocity, Physics2D.gravity, time);
        time += Time.deltaTime;
        //Debug.Log($"���� x : {dummy.transform.position.x - unity.transform.position.x}, y : {dummy.transform.position.y - unity.transform.position.y}, �ð��� ��� ���� : {(dummy.transform.position - unity.transform.position).magnitude / time}");

        // �ð����� ������ �׸���
        // �� �� ���� �̷����� �� ���ΰ�
        // 1.5�� 30��
        /*
        float totalTime = 5f;
        int split = 10;
        Vector3 formerPredictposition = unity.transform.position;
        for(int i=1; i<=split; i++) 
        {
            float predictTime = time + (totalTime * i / split);
            Vector3 predictPosition = transform.position.GetPositionAfterTime(velocity, Physics2D.gravity, predictTime);

            Debug.DrawLine(formerPredictposition, predictPosition);

            formerPredictposition= predictPosition;
        }
        */
        
        RaycastHit2D ray = dummy.transform.position.Curvecast2D(velocity.GetVelocityAfterTime(Physics2D.gravity, time), Physics2D.gravity, 5f, 10, 1);
        Debug.Log(ray.collider);
        
    }
}
