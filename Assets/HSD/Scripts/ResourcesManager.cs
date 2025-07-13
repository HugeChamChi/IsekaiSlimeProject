using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using DesignPattern;
using Managers;
using Object = UnityEngine.Object;
using Photon.Pun;
using System.IO;

public class ResourcesManager : Singleton<ResourcesManager>
{
    private static Dictionary<string, Object> resources = new();
    private PhotonView pv;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        pv.ViewID = 94; // 임시로 설정
    }

    public T Load<T>(string path) where T : Object
    {
        string _path = $"{typeof(T).Name}{path}";

        if (resources.ContainsKey(_path))
            return resources[_path] as T;

        T resource = Resources.Load(path) as T;

        if(resource != null)
            resources.Add(_path, resource);

        return resource;
    }

    public void Unload(string path)
    {
        if (resources.ContainsKey(path))
        {
            Resources.UnloadAsset(resources[path]);
            resources.Remove(path);
        }
    }

    public void UnloadAll()
    {
        foreach (var res in resources.Values)
            Resources.UnloadAsset(res);
        resources.Clear();
    }

    #region Synchronous
    public T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent, bool isPool = false) where T : Object
    {
        GameObject obj = original as GameObject;

        if (isPool)
            return Manager.Pool.Get(obj, position, rotation, parent) as T;
        else
            return Object.Instantiate(obj, position, rotation, parent) as T;
    }
    public T Instantiate<T>(T original, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        return Instantiate(original, position, rotation, null, isPool);
    }
    public T Instantiate<T>(T original, Vector3 position, bool isPool = false) where T : Object
    {
        return Instantiate(original, position, Quaternion.identity, null, isPool);
    }
    public T Instantiate<T>(string path, Vector3 position, Quaternion rotation, Transform parent, bool isPool = false) where T : Object
    {
        T obj = Load<T>(path);
        return Instantiate(obj, position, rotation, parent, isPool);
    }
    public T Instantiate<T>(string path, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        return Instantiate<T>(path, position, rotation, null, isPool);
    }
    public T Instantiate<T>(string path, Vector3 postion, bool isPool = false) where T : Object
    {
        return Instantiate<T>(path, postion, Quaternion.identity, null, isPool);
    }
    
    public void Destroy(GameObject obj)
    {
        if (obj == null || !obj.activeSelf) return;

        if (Manager.Pool.ContainsKey(obj.name))
            Manager.Pool.Release(obj);   
        else
            Object.Destroy(obj);
    }

    public void Destroy(GameObject obj, float delay)
    {
        if (obj == null || !obj.activeSelf) return;

        if (Manager.Pool.ContainsKey(obj.name))
            Manager.Pool.Release(obj, delay);     
        else
            Object.Destroy(obj, delay);
    }
    #endregion
    #region Network
    [PunRPC]
    public void NetworkInstantiate_RPC(string name, Vector3 pos, Quaternion rot)
    {
        //Manager.Pool.GetNetwork<GameObject>(name, pos, rot);
        Manager.Pool.Get<GameObject>(name, pos, rot);
    }    

    public void NetworkInstantiate<T>(T original, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("NetworkInstantiate는 마스터 클라이언트에서만 호출해야 합니다.");
            return;
        }

        if (isPool)
        {
            pv.RPC("NetworkInstantiate_RPC", RpcTarget.All, (original as GameObject).name, position, rotation);
        }
        else
        {
            PhotonNetwork.Instantiate($"Prefabs/{(original as GameObject).name}", position, rotation);
        }
    }
    public void NetworkInstantiate<T>(T original, Vector3 position, bool isPool = false) where T : Object
    {
        NetworkInstantiate<T>(original, position, Quaternion.identity, isPool);
    }
    public void NetworkInstantiate<T>(T original, bool isPool = false) where T : Object
    {
        NetworkInstantiate<T>(original, Vector3.zero, Quaternion.identity, isPool);
    }
    public void NetworkInstantiate<T>(string path, Vector3 position, Quaternion rotation, bool isPool = false) where T : Object
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("NetworkInstantiate는 마스터 클라이언트에서만 호출해야 합니다.");
            return;
        }

        pv.RPC("NetworkInstantiate_RPC", RpcTarget.All, path, position, rotation, isPool);
    }
    public void NetworkInstantiate<T>(string path, Vector3 position, bool isPool = false) where T : Object
    {
        NetworkInstantiate<T>(path, position, Quaternion.identity, isPool);
    }
    public void NetworkInstantiate<T>(string path, bool isPool = false) where T : Object
    {
        NetworkInstantiate<T>(path, Vector3.zero, Quaternion.identity, isPool);
    }

    // 네트워크 용 Destroy함수 들
    public void NetworkDestroy(int viewID, string name)
    {
       if (PhotonNetwork.IsMasterClient && Manager.Pool.NetworkContainsKey(name))
            pv.RPC("Destroy_RPC", RpcTarget.All, viewID);
    }
    public void NetworkDestroy(int viewID, string name, float delay)
    {       
        if (PhotonNetwork.IsMasterClient && Manager.Pool.NetworkContainsKey(name))
            pv.RPC("Destroy_RPC", RpcTarget.All, viewID, delay);
    }

    [PunRPC]
    public void Destroy_RPC(int viewID, float delay)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        Manager.Pool.ReleaseNetwork(obj, delay);
    }
    [PunRPC]
    public void Destroy_RPC(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        Manager.Pool.ReleaseNetwork(obj);
    }
    #endregion
}