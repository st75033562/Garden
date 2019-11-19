using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : Component{
	private static T _instance = null;
    private static bool _created;

	public static T instance{
		get{
            EnsureInstance();
			return _instance;
		}
	}

    public static void EnsureInstance() {
		if(_instance == null && !_created){
			_instance = SingletonManager.gameObject.AddComponent(typeof(T)) as T;
            _created = true;
		}
    }
}

public class SingletonManager{
	private static GameObject _gameObject;

	public static GameObject gameObject{
		get{
			if(_gameObject == null){
				_gameObject = new GameObject("singleton");
				Object.DontDestroyOnLoad(_gameObject);
			}
			return _gameObject;
		}
	}
}
