using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
public sealed class NewsMemory
{
    private readonly List<NewsItem> _items = new List<NewsItem>();

    public IReadOnlyList<NewsItem> Items => _items;

    public void Publish(NewsItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public void Restore(IEnumerable<NewsItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        _items.Clear();
        foreach (NewsItem item in items)
        {
            if (item == null) throw new ArgumentException("A news item is required.", nameof(items));
            _items.Add(item);
        }
    }

    public void AdvanceDay()
    {
        foreach (NewsItem item in _items)
        {
            item.AgeOneDay();
        }
    }
}
}