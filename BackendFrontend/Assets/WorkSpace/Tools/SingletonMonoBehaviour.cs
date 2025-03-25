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
            Debug.LogWarning($"[Singleton] �ߺ��� �ν��Ͻ� {typeof(T)} ������");
            Destroy(gameObject); // �ߺ��� GameObject ��ü�� ����
        }
    }
}