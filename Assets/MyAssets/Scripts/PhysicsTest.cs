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
        // 키보드 오른쪽 방향키로 속도를 0.25배씩 늘리고 왼쪽 방향키로 0.25배씩 줄이고
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

        
        // 더미의 위치  는  위치에서      일정 시간 후 위치
        dummy.position = transform.position.GetPositionAfterTime(velocity, Physics2D.gravity, time);
        time += Time.deltaTime;
        //Debug.Log($"오차 x : {dummy.transform.position.x - unity.transform.position.x}, y : {dummy.transform.position.y - unity.transform.position.y}, 시간당 평균 오차 : {(dummy.transform.position - unity.transform.position).magnitude / time}");

        // 시간으로 포물선 그리기
        // 몇 초 뒤의 미래까지 볼 것인가
        // 1.5초 30개
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
