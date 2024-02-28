using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Rigidbody2D rigid;

    // Start is called before the first frame update
    void Start()
    {
        // Test라는 기능을 가지고 있는 이 컴포넌트와 같은 오브젝트에 있는 Rigidbody2D를 가지고 오기
        rigid = gameObject.GetComponent<Rigidbody2D>();
        Debug.Log("npnpwkdkklwll");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump()
    {
        Debug.Log("jump");
        rigid.AddForce(new Vector2(0, 1) * 300);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Boom");
    }


}
