using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommunicate
{
    public bool TryTalk(Character from)
    {
        return default;
    }

    public string GetCommunicateScript(Character from)
    {
        return default;
    }
}
