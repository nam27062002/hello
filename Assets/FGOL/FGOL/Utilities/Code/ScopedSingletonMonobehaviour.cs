using UnityEngine;

namespace FGOL.Utilities
{
    public class ScopedSingletonMonobehaviour<T> : MonoBehaviour where T : ScopedSingletonMonobehaviour<T>
    {
        private static T s_instance = null;
        private static object _lock = new object();

        public static T Instance
        {
            get { return s_instance; }
        }

        protected virtual void Awake()
        {
            Assert.Warn(s_instance == null, "Shouldn't instantiate multiple Singletons! (typeof " + typeof(T) + ")");
            lock (_lock)
            {                
                if (s_instance == null)
                {
                    s_instance = this as T;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            lock (_lock)
            {
                s_instance = null;
            }
        }
    }
}