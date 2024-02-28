using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    public float lifeTime; // 수명
    private float leftTime;// 남은 수명

    // Start is called before the first frame update
    void Start()
    {
        leftTime = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        // delta : 변화량
        // 남은 시간    -    지나간 시간      = 남은시간
        //     5                4.5         =    0.5
        leftTime -= Time.deltaTime;
        // Update간의 간격 : Time.deltaTime
        // FixedUpdate간의 간격 : Time.fixedDeltaTime
        if(leftTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
