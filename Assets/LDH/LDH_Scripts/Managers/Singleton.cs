using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPattern
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        protected void SingletonInit()
        {
            //Debug.Log("SingletonInit 호출");
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
           
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
        
        
        public static void Release()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }
        
    }
}