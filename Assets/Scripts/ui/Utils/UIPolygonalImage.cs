using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class UIPolygonalImage : Image
{
    private Collider2D m_collider;

    protected override void Awake()
    {
        m_collider = GetComponent<Collider2D>();
    }

    public override bool Raycast(UnityEngine.Vector2 sp, UnityEngine.Camera eventCamera)
    {
        return m_collider.OverlapPoint(sp);
    }
}
