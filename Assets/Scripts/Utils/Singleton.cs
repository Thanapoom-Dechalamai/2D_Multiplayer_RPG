using UnityEngine;

namespace Utils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;
        public static T Instance { get => _instance; set => _instance = value; }
        private bool _isRemoveParent = true;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                return;
            }
            else
            {
                _instance = (T)(object)this;
            }

            if (_isRemoveParent && transform.parent)
            {
                transform.SetParent(null);
            }

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        protected virtual void OnDestroy()
        {
            if (_instance == this as T)
                _instance = null;
        }
    }

    public class SceneSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        //Logger.LogError("No instance of " + typeof(T) + " found in the scene.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                //Logger.Log($"Destroy Duplicate Instance: {gameObject}");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            _instance.gameObject.name = _instance.gameObject.name + " (Scene Singleton)";
        }
        protected virtual void OnDestroy()
        {
            if (_instance == this as T)
                _instance = null;
        }
    }
}