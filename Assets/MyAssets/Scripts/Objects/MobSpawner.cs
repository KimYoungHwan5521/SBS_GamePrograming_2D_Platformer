using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    // 뭘 소환
    public GameObject mob;
    // 몇마리 소환
    public int maxSpawn;
    public int curSpawn;
    // 소환 쿨
    public float delaySpawn;
    // 소환까지 몇 초 남았는지
    public float waitSpawn;
    // 어디에 소환
    public float rangeSpawn;
    // 인식 범위 : 어디까지 왔으면 소환할 것인지
    public float rangeRecognize;

    // 플레이어가 소환 범위 안에 들어왔는가
    public bool isPlayerInRange;

    private void Update()
    {
        // 태그로 플레이어를 찾음
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 배열에서 범위안에 있는 대상을 찾아서 하나라도 있으면
        GameObject inRange = System.Array.Find(players, target => (target.transform.position - transform.position).magnitude < rangeRecognize);

        // 범위 안에 없으면 리턴
        if (inRange == null)
        {
            isPlayerInRange = false;
            waitSpawn = delaySpawn;
            return;
        }
        isPlayerInRange = true;

        if (curSpawn >= maxSpawn) return; 
        // 쿨타임: 시간이 지나는 걸 체크해야됨
        if (waitSpawn <= 0)
        {
            Vector2 spawnRotation = transform.position;
            // 1. 정사각형
            // spawnRotation.x += Random.Range(-rangeSpawn, rangeSpawn);
            // spawnRotation.y += Random.Range(-rangeSpawn, rangeSpawn);
            // 2. 원
            //                  단위 원 안에 랜덤으로
            spawnRotation += Random.insideUnitCircle * rangeSpawn;

            GameObject inst = Instantiate(mob, spawnRotation, transform.rotation);
            // 버프 / 디버프같은 경우도 컴포넌트 형태로 추가해주면 편하다
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
