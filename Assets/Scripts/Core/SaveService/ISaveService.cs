public interface ISaveService
{
    public void Save<T>(string key, T value);
    public T Load<T>(string key);
    public bool HasKey(string key);
    void DeleteKey(string key);
}
