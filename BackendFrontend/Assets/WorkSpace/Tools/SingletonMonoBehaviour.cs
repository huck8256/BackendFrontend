using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
            Instance = this as T;
        else
        {
            Debug.LogWarning($"[Singleton] 중복된 인스턴스 {typeof(T)} 삭제됨");
            Destroy(gameObject); // 중복된 GameObject 자체를 삭제
        }
    }
}