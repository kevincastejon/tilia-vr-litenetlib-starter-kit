using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntitySetting
{
    public int maxInstance = -1;
    public bool destroyFirst = false;
    public GameObject prefab;
}

public class EntitiesSettings : MonoBehaviour
{
    public List<EntitySetting> settings = new List<EntitySetting>();
    public int maxEntitiesStateSend = 12;
}
