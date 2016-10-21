public class ScopedSingleton<T> where T : ScopedSingleton<T>, new()
{
    private static T s_instance = null;
    private static object _lock = new object();

    public static void Init()
    {
        lock(_lock)
        { 
        if(s_instance == null)
        {
            s_instance = new T();
        }
        }
    }

    public static void DeInit()
    {
        lock (_lock)
        {
            if (s_instance != null)
            {
                s_instance.Destroy();
                s_instance = null;
            }
        }
    }

    public static T Instance
    {
        get { return s_instance; }
    }

    protected virtual void Destroy()
    {
    }
}