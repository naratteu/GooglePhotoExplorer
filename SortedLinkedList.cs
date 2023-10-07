using System.Collections;

namespace GooglePhotoExplorer;

public class SortedLinkedList<T> : ICollection<T>
{
    readonly LinkedList<T> list = new();
    public IComparer<T> Comp { get; init; } = Comparer<T>.Default;
    
    public int Count => list.Count;
    public void AddFirst(T value) => _ = Count is 0 ? list.AddFirst(value) : list.First.AndNexts().FirstOrDefault(node => Comp.Compare(value, node.ValueRef) is <= 0)?.AddBefore(value) ?? list.AddLast(value);
    public void AddLast(T value) => _ = Count is 0 ? list.AddLast(value) : list.Last.AndPrevs().FirstOrDefault(node => Comp.Compare(value, node.ValueRef) is >= 0)?.AddAfter(value) ?? list.AddFirst(value);
    void ICollection<T>.Add(T item) => AddFirst(item);

    public bool Remove(T item) => list.Remove(item);
    public void Clear() => list.Clear();
    public bool Contains(T item) => list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    bool ICollection<T>.IsReadOnly => ((ICollection<T>)list).IsReadOnly;
}

file static class LinkedListExtension
{
    public static IEnumerable<LinkedListNode<T>> AndNexts<T>(this LinkedListNode<T>? node) { for (; node is { }; node = node.Next) yield return node; }
    public static IEnumerable<LinkedListNode<T>> AndPrevs<T>(this LinkedListNode<T>? node) { for (; node is { }; node = node.Previous) yield return node; }
    public static LinkedListNode<T> AddBefore<T>(this LinkedListNode<T> node, in T value) => node.List!.AddBefore(node, value);
    public static LinkedListNode<T> AddAfter<T>(this LinkedListNode<T> node, in T value) => node.List!.AddAfter(node, value);
}