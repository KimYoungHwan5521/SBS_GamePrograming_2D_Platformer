using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Rigidbody2D rigid;

    // Start is called before the first frame update
    void Start()
    {
        // Test��� ����� ������ �ִ� �� ������Ʈ�� ���� ������Ʈ�� �ִ� Rigidbody2D�� ������ ����
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
