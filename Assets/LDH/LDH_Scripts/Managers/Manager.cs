using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// 매니저 등록 및 초기화 방법
    /// 1. Manager.cs 내부에 아래와 같은 방식으로 접근용 프로퍼티(static)를 등록한다.:
    ///     public static TestManager Test => TestManager.Instance;
    /// 2. 각 매니저 클래스는 Singleton<T>를 상속받고, Awake()에서 반드시 SingletonInit()를 호출해야 한다.:
    ///     public class TestManager : Singleton<TestManager>
    ///     {
    ///         private void Awake() => SingletonInit();
    ///     }
    ///
    /// 3. 해당 매니저 컴포넌트는 @Manager 프리팹에 다음 중 하나의 방식으로 등록해야 한다. :
    ///     - 프리팹 에디터에서 직접 컴포넌트를 추가 (프리팹 위치 : Assets/Resources/Prefabs/@Manager)
    ///     - 또는 Manager.cs의 Initialize() 내에서 manager.AddComponent<T>로 동적으로 추가
    ///
    /// 4. Manager는 게임 실행 시 자동으로 초기화되기 때문에 씬에 배치할 필요가 없다.
    /// 5. 매니저 접근 및 사용 예시 : Manager.Test.Test();
    ///
    /// 6. Add Component 대신 하위 자식 오브젝트로 붙여넣는 경우,
    /// 해당 매니저는 Awake에서 SingletonInit 호출하지 않고, Instance만 자기 자신으로 넣어줌녀 된다! (DontDestroy 관련Warning 발생 때문)
    /// </summary>
    public static class Manager
    {
        public static GameObject manager;


        //-----접근용 프로퍼티 등록------//
        public static PoolManager Pool => PoolManager.Instance;
        public static ResourcesManager Resources => ResourcesManager.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var prefab = UnityEngine.Resources.Load<GameObject>("Prefabs/@Manager");
            manager = GameObject.Instantiate(prefab);
            manager.gameObject.name = "@Manager";
            GameObject.DontDestroyOnLoad(manager);


            //todo: 주석해제하기
            //manager.AddComponent<PoolManager>();
            manager.GetOrAddComponent<ResourcesManager>();

            //각각의 매니저 스크립트를 프리팹에 스크립트를 직접 추가해두거나 아래와 같이 AddComponent로 동적으로 추가한다.
            // 예시 : manager.AddComponent<RandomManager>();

        }

        public static void InitPool() => manager.AddComponent<PoolManager>();
    }
}

