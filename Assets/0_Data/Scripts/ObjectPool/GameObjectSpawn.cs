using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSpawn : MonoBehaviour
{

    public static GameObjectSpawn Instance;

    [SerializeField] protected GameObjectListData gameObjectListData;
    protected Transform holder;
    protected Dictionary<string, Queue<GameObject>> objDic = new Dictionary<string, Queue<GameObject>>();
    protected int count = 0;

    //get
    public int Count => count;
    public GameObjectListData GameObjectListData => gameObjectListData;

    protected virtual void Awake()
    {
        if (Instance == null) Instance = this;
        holder = transform.Find("holder");
    }
    void Reset()
    {
        if (holder == null)
        {
            holder = new GameObject("holder").transform;
            holder.parent = transform;
        }
    }
    //DeSapwn
    public void DeSapwn(GameObject obj)
    {
        string key = obj.name;
        if (!objDic.ContainsKey(key))
        {
            objDic[key] = new Queue<GameObject>();
        }

        SetParent(obj.transform, holder);
        objDic[key].Enqueue(obj);
        obj.gameObject.SetActive(false);
        count--;
    }
    //Spawn
    public GameObject Spawn(int id, Vector3 pos, Quaternion rot)
    {
        GameObject obj = gameObjectListData.GetObjById(id); // Lấy prefab từ ID
        if (obj == null)
        {
            Debug.LogWarning($"[GameObjectSpawn] Object with id {id} not found in GameObjectListData.");
            return null;
        }
        return Spawn(obj, pos, rot);
    }
    public GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot)
    {
        GameObject newObj = GetObjFromPool(obj);

        newObj.gameObject.SetActive(true);
        newObj.transform.SetPositionAndRotation(pos, rot);
        //SetParent(newObj.transform, holder);

        count++;
        return newObj;
    }

    private GameObject GetObjFromPool(GameObject obj)
    {
        string key = obj.name;
        if (objDic.ContainsKey(key) && objDic[key].Count > 0)
        {
            GameObject pooledObj = objDic[key].Dequeue();
            SetParent(pooledObj.transform, null);
            return pooledObj;
        }

        GameObject newObj = Instantiate(obj);
        newObj.name = key;
        return newObj;
    }
    public void SetParentUI(RectTransform child, RectTransform parent)
    {
        child.SetParent(parent,false);
    }
    public void SetParent(Transform child, Transform parent)
    {
        child.transform.parent = parent;
    }
    public void SetHolderParent(Transform child)
    {
        SetParent(child, holder);
    }
    
}
