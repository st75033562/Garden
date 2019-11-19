using System;
using UnityEngine.Assertions;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class PlistUtils
{
    /// <summary>
    /// Merge source to target
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    public static void Merge(this PlistDocument target, PlistDocument source)
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(source);

        Merge(target.root, source.root);
    }

    private static void Merge(PlistElementDict target, PlistElementDict source)
    {
        foreach (var value in source.values)
        {
            PlistElement targetElem;
            if (!target.values.TryGetValue(value.Key, out targetElem))
            {
                target[value.Key] = value.Value.Clone();
            }
            else if (targetElem.GetType() == value.Value.GetType())
            {
                if (targetElem is PlistElementInteger)
                {
                    (targetElem as PlistElementInteger).value = value.Value.AsInteger();
                }
                else if (targetElem is PlistElementBoolean)
                {
                    (targetElem as PlistElementBoolean).value = value.Value.AsBoolean();
                }
                else if (targetElem is PlistElementString)
                {
                    (targetElem as PlistElementString).value = value.Value.AsString();
                }
                else if (targetElem is PlistElementArray)
                {
                    Merge(value.Key, targetElem.AsArray(), value.Value.AsArray());
                }
                else if (targetElem is PlistElementDict)
                {
                    Merge(targetElem.AsDict(), value.Value.AsDict());
                }
                else
                {
                    throw new ArgumentException("unknown type: " + targetElem.GetType());
                }
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("cannot merge different type of keys, name: {0}, target {1}, source {2}",
                        value.Key, targetElem.GetType(), value.Value.GetType()));
            }
        }
    }

    private static void Merge(string name, PlistElementArray target, PlistElementArray source)
    {
        // only support for merging primitive array
        foreach (var value in source.values)
        {
            if (value is PlistElementArray || value is PlistElementDict)
            {
                Debug.LogWarning("non primitive array element is not supported, key: " + name);
                continue;
            }

            int index = -1;
            if (value is PlistElementInteger)
            {
                index = target.values.FindIndex(x => x.AsInteger() == value.AsInteger());
            }
            else if (value is PlistElementBoolean)
            {
                index = target.values.FindIndex(x => x.AsBoolean() == value.AsBoolean());
            }
            else if (value is PlistElementString)
            {
                index = target.values.FindIndex(x => x.AsString() == value.AsString());
            }
            if (index == -1)
            {
                target.values.Add(value.Clone());
            }
            else
            {
                target.values[index] = value.Clone();
            }
        }
    }

    // create a deep clone of the element
    public static PlistElement Clone(this PlistElement elem)
    {
        if (elem is PlistElementInteger)
        {
            return new PlistElementInteger(elem.AsInteger());
        }
        else if (elem is PlistElementString)
        {
            return new PlistElementString(elem.AsString());
        }
        else if (elem is PlistElementBoolean)
        {
            return new PlistElementBoolean(elem.AsBoolean());
        }
        else if (elem is PlistElementArray)
        {
            var clone = new PlistElementArray();
            foreach (var childElem in elem.AsArray().values)
            {
                clone.values.Add(childElem.Clone());
            }
            return clone;
        }
        else if (elem is PlistElementDict)
        {
            var clone = new PlistElementDict();
            foreach (var childElem in elem.AsDict().values)
            {
                clone[childElem.Key] = childElem.Value.Clone();
            }
            return clone;
        }
        else
        {
            throw new ArgumentException("unknown type: " + elem.GetType());
        }
    }
}
