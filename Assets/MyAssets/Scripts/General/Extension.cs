using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


// Ȯ�� �޼���� static
public static class Extension
{   
    public static Vector3 GetVelocityAfterTime(this Vector3 velocity, Vector3 gravity, float time)
    {
        return velocity + gravity * time;
    }

    public static Vector3 GetPositionAfterTime(this Vector3 origin, Vector3 velocity, Vector3 gravity, float time)
    {
        // ���������� 1��ŭ ���� �ֽ��ϴ�.
        // 30�� ����
        // velocity * time <= �⺻ �̸�ŭ�� ��
        // 30�� �������
        // �߷��� �Ʒ��� 9.81
        // 30�� ������ ���� �߷��� �󸶳� ���� �޾������
        // �߷� * �ð�^2 / 2
        return origin + (velocity * time) + (gravity * time * time) / 2;
    }

    //                                                                                                                                  -1 : 1111 1111 1111 1111(bin)
    public static RaycastHit2D Curvecast2D(this Vector3 origin, Vector3 velocity, Vector3 gravity, float totalTime, int split, int layerMask = -1, bool checkTrigger = false)
    {
        Vector3 formerPredictposition = origin;
        Ray ray = new Ray();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = checkTrigger;
        filter.SetLayerMask(layerMask);
        RaycastHit2D[] hits = new RaycastHit2D[10];
        for (int i = 1; i <= split; i++)
        {
            float predictTime = (totalTime * i / split);
            Vector3 predictPosition = origin.GetPositionAfterTime(velocity, gravity, predictTime);
            //Debug.DrawLine(formerPredictposition, predictPosition);
            Vector3 direction = predictPosition - formerPredictposition;
            ray.origin = formerPredictposition;
            ray.direction = direction;
            Debug.DrawRay(ray.origin, ray.direction * direction.magnitude);
            int hitAmount = Physics2D.Raycast(ray.origin, ray.direction, filter, hits, direction.magnitude);
            if (hitAmount > 0) return hits[0];

            formerPredictposition = predictPosition;
        }
        return default;
    }
}
