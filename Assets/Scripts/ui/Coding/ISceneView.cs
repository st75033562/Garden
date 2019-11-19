using UnityEngine;

public interface ISceneView
{
    /// <summary>
    /// whether the rendering is enabled
    /// </summary>
    bool enabled { get; set; }

    /// <summary>
    /// set the normalized rect of the view on the screen
    /// </summary>
    void SetNormalizedRect(Rect rect);
}
