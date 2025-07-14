using Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonPool : IPunPrefabPool
{
    private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {      
        GameObject obj = null;

        if (_pool.ContainsKey(prefabId) && _pool[prefabId].Count > 0)
        {
            obj = _pool[prefabId].Dequeue();

            PhotonView view = Provider.Get<PhotonView>(obj);
            if (view != null && view.IsMine)
            {
                view.RPC("RemoteSetactive", RpcTarget.All, position, rotation);
            }
            obj.SetActive(true);
        }
        else
        {
            GameObject prefab = Manager.Resources.Load<GameObject>(prefabId);
            obj = Object.Instantiate(prefab, position, rotation);
        }

        return obj;
    }

    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();

        if (!_pool.ContainsKey(prefabId))
            _pool[prefabId] = new Queue<GameObject>();

        _pool[prefabId].Enqueue(gameObject);

        PhotonView view = Provider.Get<PhotonView>(gameObject);
        if (view != null && view.IsMine)
        {
            view.RPC("RemoteSetInactive", RpcTarget.All);
        }
    }
}
