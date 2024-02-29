using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : DamageComponent
{
    // �����ϸ� ������� �ڿ�������.
    // ĳ���͸� ���ȴ�. �������.
    // ���� ���ȴ�. �������.

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if(attacker.gameObject != collision.gameObject) Destroy(gameObject);
    }

    protected override void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);
        if (attacker.gameObject != collider.gameObject && collider.tag != "CameraBoundary") Destroy(gameObject);
    }
}
