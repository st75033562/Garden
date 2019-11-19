using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System;

public static class UnitTestUtils
{
    public static Comparison<Vector2> Vector2Comparision(float delta)
    {
        return (a, b) => {
            return a.Approximate(b, delta) ? 0 : 1;
        };
    }

    public static void AreEqual(IEnumerable<Vector2> expected, IEnumerable<Vector2> actual, float delta)
    {
        Assert.That(expected, Is.EqualTo(actual).Using(Vector2Comparision(delta)));
    }

    public static void AreEqual(Vector2 expected, Vector2 actual, float delta)
    {
        Assert.That(expected, Is.EqualTo(actual).Using(Vector2Comparision(delta)));
    }
}
