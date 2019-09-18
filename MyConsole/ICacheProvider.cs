namespace MyConsole
{
    public interface ICacheProvider
    {
        bool Contains(string key);
        object Get(string key);
        void Put(string key, object result, int duration);
    }
}