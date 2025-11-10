using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectListData", menuName = "Data/GameObjectListData", order = 1)]
public class GameObjectListData : ScriptableObject
{
    [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();
    public List<GameObject> GameObjects => gameObjects;

    public GameObject GetObjById(int id)
    {
        foreach (var obj in gameObjects)
        {
            if (obj == null) continue;
            if (obj.GetInstanceID() == id) return obj;
        }
        return null;
    }
    public GameObject GetRandomObj()
    {
        return gameObjects[Random.Range(0, gameObjects.Count)];
    }
}
