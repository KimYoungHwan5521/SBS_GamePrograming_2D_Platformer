using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    public MobSpawner owner;

    private void OnDestroy()
    {
        owner.curSpawn--;
    }
}
