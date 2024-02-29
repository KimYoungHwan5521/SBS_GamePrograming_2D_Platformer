using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : DamageComponent
{
    // 공격하면 사라져야 자연스럽다.
    // 캐릭터를 때렸다. 사라진다.
    // 땅을 때렸다. 사라진다.

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
