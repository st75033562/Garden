using System.Collections;
using System.Collections.Generic;
using System;

public class IntrusiveListNode<T> where T : IntrusiveListNode<T>
{
    public IntrusiveList<T> list { get; internal set; }

    public T prev { get; internal set; }

    public T next { get; internal set; }

    /// <summary>
    /// add the given node before the current node
    /// </summary>
    public void AddBefore(T node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        if (node.list != null)
        {
            throw new ArgumentException("node is already in a list");
        }

        if (prev != null)
        {
            prev.next = node;
        }
        node.prev = prev;
        node.next = (T)this;
        prev = node;
        node.list = list;
    }

    /// <summary>
    /// add the given node after the current node
    /// </summary>
    public void AddAfter(T node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        if (node.list != null)
        {
            throw new ArgumentException("node is already in a list");
        }

        if (next != null)
        {
            next.prev = node;
        }
        node.prev = (T)this;
        node.next = next;
        next = node;
        node.list = list;
    }

    /// <summary>
    /// remove the node from the list
    /// </summary>
    public void Remove()
    {
        if (prev != null)
        {
            prev.next = next;
        }
        if (next != null)
        {
            next.prev = prev;
        }
        next = prev = null;
        list = null;
    }
}

public class IntrusiveList<T> : IEnumerable<T> where T : IntrusiveListNode<T>
{
    public T first { get; private set; }

    public T last { get; private set; }

    /// <summary>
    /// add the value to the head of the list
    /// </summary>
    public void AddFirst(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (value.list != null)
        {
            throw new ArgumentException("value is already inserted in a list");
        }

        if (first != null)
        {
            first.AddBefore(value);
        }
        else
        {
            value.list = this;
            last = value;
        }
        first = value;
    }

    /// <summary>
    /// add the value to the end of the list
    /// </summary>
    /// <param name="value">value to add</param>
    public void AddLast(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (value.list != null)
        {
            throw new ArgumentException("value is already inserted in a list");
        }

        if (last != null)
        {
            last.AddAfter(value);
        }
        else
        {
            first = value;
            value.list = this;
        }
        last = value;
    }

    /// <summary>
    /// add the value before the given node
    /// </summary>
    /// <param name="node">insert position</param>
    /// <param name="value">value to insert</param>
    public void AddBefore(T node, T value)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }
        if (node.list != this)
        {
            throw new ArgumentException("node does not belong to this list");
        }

        node.AddBefore(value);
        if (node == first)
        {
            first = node;
        }
    }

    /// <summary>
    /// add the value after the given node
    /// </summary>
    /// <param name="node">insert position</param>
    /// <param name="value">value to insert</param>
    public void AddAfter(T node, T value)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }
        if (node.list != this)
        {
            throw new ArgumentException("node does not belong to this list");
        }

        node.AddAfter(value);
        if (node == last)
        {
            last = value;
        }
    }

    /// <summary>
    /// remove the node from the list
    /// </summary>
    /// <param name="node">node to remove</param>
    /// <returns>the next node in the list</returns>
    public T Remove(T node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }
        if (node.list != this)
        {
            throw new ArgumentException("node does not belong to this list");
        }

        if (node == first)
        {
            first = node.next;
        }
        if (node == last)
        {
            last = node.prev;
        }
        var next = node.next;
        node.Remove();
        return next;
    }

    /// <summary>
    /// clear the list, the complexity is O(n) where n is the number of elements in the list
    /// </summary>
    public void Clear()
    {
        for (var cur = first; cur != null; )
        {
            var next = cur.next;
            cur.list = null;
            cur.prev = cur.next = null;
            cur = next;
        }
        first = last = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (IntrusiveListNode<T> cur = first; cur != null; cur = cur.next)
        {
            yield return (T)cur;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
