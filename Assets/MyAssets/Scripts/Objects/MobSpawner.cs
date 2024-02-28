using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    // �� ��ȯ
    public GameObject mob;
    // ��� ��ȯ
    public int maxSpawn;
    public int curSpawn;
    // ��ȯ ��
    public float delaySpawn;
    // ��ȯ���� �� �� ���Ҵ���
    public float waitSpawn;
    // ��� ��ȯ
    public float rangeSpawn;
    // �ν� ���� : ������ ������ ��ȯ�� ������
    public float rangeRecognize;

    // �÷��̾ ��ȯ ���� �ȿ� ���Դ°�
    public bool isPlayerInRange;

    private void Update()
    {
        // �±׷� �÷��̾ ã��
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // �迭���� �����ȿ� �ִ� ����� ã�Ƽ� �ϳ��� ������
        GameObject inRange = System.Array.Find(players, target => (target.transform.position - transform.position).magnitude < rangeRecognize);

        // ���� �ȿ� ������ ����
        if (inRange == null)
        {
            isPlayerInRange = false;
            waitSpawn = delaySpawn;
            return;
        }
        isPlayerInRange = true;

        if (curSpawn >= maxSpawn) return; 
        // ��Ÿ��: �ð��� ������ �� üũ�ؾߵ�
        if (waitSpawn <= 0)
        {
            Vector2 spawnRotation = transform.position;
            // 1. ���簢��
            // spawnRotation.x += Random.Range(-rangeSpawn, rangeSpawn);
            // spawnRotation.y += Random.Range(-rangeSpawn, rangeSpawn);
            // 2. ��
            //                  ���� �� �ȿ� ��������
            spawnRotation += Random.insideUnitCircle * rangeSpawn;

            GameObject inst = Instantiate(mob, spawnRotation, transform.rotation);
            // ���� / ��������� ��쵵 ������Ʈ ���·� �߰����ָ� ���ϴ�
            inst.AddComponent<SpawnedObject>().owner = this;
            curSpawn++;
            waitSpawn = delaySpawn;
        }
        else
        {
            waitSpawn -= Time.deltaTime;
        }
            
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = isPlayerInRange? Color.red : Color.white;
        if(isPlayerInRange)
        {
            // Gizmos.color = Color.red;
            Gizmos.color = new Color(1, waitSpawn / delaySpawn, waitSpawn / delaySpawn);
        }
        else
        {
            Gizmos.color = Color.white;
        }
        
        //Gizmos.DrawWireCube(transform.position, new Vector3(rangeRecognize, rangeRecognize));
        Gizmos.DrawWireSphere(transform.position, rangeRecognize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangeSpawn);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeRecognize);

    }

}
