using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomAreaEditor
{
    private const float EDITOR_WINDOW_TAB_HEIGHT = 21.0f;
    private static Matrix4x4 m_previewGUIMatrix; 

    /// <summary>
    /// Begin a zommed Area with a certain zoomscale within a certain Rect
    /// </summary>
    /// <param name="_zoomScale">Zoom Scale</param>
    /// <param name="_screenCoordArea">Zoom Area's Rect</param>
    /// <returns></returns>
    public static Rect Begin(float _zoomScale, Rect _screenCoordArea)
    {
        GUI.EndGroup();

        Rect _clippedArea = _screenCoordArea.ScaleSizeBy(1.0f / _zoomScale, _screenCoordArea.GetTopLeft());
        _clippedArea.y += EDITOR_WINDOW_TAB_HEIGHT;
        GUI.BeginGroup(_clippedArea);

        m_previewGUIMatrix = GUI.matrix;
        Matrix4x4 _translation = Matrix4x4.TRS(_clippedArea.GetTopLeft(), Quaternion.identity, Vector3.one);
        Matrix4x4 _scale = Matrix4x4.Scale(new Vector3(_zoomScale, _zoomScale, 1.0f));
        GUI.matrix = _translation * _scale * _translation.inverse * GUI.matrix;

        return _clippedArea; 
    }

    /// <summary>
    /// End the zoomed Area
    /// Close the group and start a new one in order to avoid Errors
    /// </summary>
    public static void End()
    {
        GUI.matrix = m_previewGUIMatrix;
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0.0f, EDITOR_WINDOW_TAB_HEIGHT, Screen.width, Screen.height));
    }
}

public static class RectExtension
{
    /// <summary>
    /// Get the top left coordinates of a rect
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector2 GetTopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin); 
    }

    /// <summary>
    /// Scale the this of a rect according to a float
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    /// <summary>
    /// Scale the this of a rect according to a float relative to a pivot point
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="scale"></param>
    /// <param name="pivotPoint"></param>
    /// <returns></returns>
    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    /// <summary>
    /// Scale the this of a rect according to a Vector2
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    /// <summary>
    /// Scale the this of a rect according to a Vector2 and relative to a pivot point
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="scale"></param>
    /// <param name="pivotPoint"></param>
    /// <returns></returns>
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale.x;
        result.xMax *= scale.x;
        result.yMin *= scale.y;
        result.yMax *= scale.y;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }
}