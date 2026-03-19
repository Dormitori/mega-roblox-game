using System.Collections.Generic;
using UnityEngine;

public interface IPool<T> where T : Component
{
    T Rent(bool  setActive = true);
    void Return(T item);
    void Prewarm(int count);
}

public sealed class ObjectPool<T> : IPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly bool _autoExpand;
    private readonly int _maxSize;

    private readonly Stack<T> _inactive = new();
    private int _created;

    public ObjectPool(T prefab, Transform parent = null, int prewarm = 0, bool autoExpand = true, int maxSize = int.MaxValue)
    {
        _prefab = prefab;
        _parent = parent;
        _autoExpand = autoExpand;
        _maxSize = maxSize;

        if (prewarm > 0) Prewarm(prewarm);
    }

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
            Return(CreateNew());
    }

    public T Rent(bool setActive = true)
    {
        if (_inactive.Count > 0)
        {
            var item = _inactive.Pop();
            item.gameObject.SetActive(setActive);
            return item;
        }

        if (!_autoExpand || _created >= _maxSize)
            return null;

        var created = CreateNew();
        created.gameObject.SetActive(setActive);
        return created;
    }

    public void Return(T item)
    {
        if (item == null) return;

        item.gameObject.SetActive(false);
        if (_parent != null)
            item.transform.SetParent(_parent, false);

        _inactive.Push(item);
    }

    T CreateNew()
    {
        _created++;
        var item = Object.Instantiate(_prefab, _parent);
        item.gameObject.SetActive(false);
        return item;
    }
}