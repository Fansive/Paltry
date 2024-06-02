using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Paltry.EditorExtension
{
    //[CustomEditor(typeof(RectTransform))]
    //public class Inspector_RectTransform : Editor
    //{
    //    RectTransform canvas;
    //    RectTransform rect;
    //    const string LabelName = "在Canvas的相对坐标";
    //    private void OnEnable()
    //    {
    //        rect = (RectTransform)target;
    //        canvas = TransformUtil.FindCanvasRectTransform(rect);
    //        if (canvas?.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace)
    //            canvas = null;
    //    }
    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();
    //        if (canvas == null)
    //            return;

    //        Vector2 relativePos = TransformUtil.WorldPos2UIRelativeCanvasPos(rect.position, canvas);
    //        Vector2 newValue = EditorGUILayout.Vector2Field(LabelName, relativePos);
    //        if (GUI.changed)
    //            rect.position = TransformUtil.UIRelativeCanvasPos2WorldPos(newValue, canvas);
    //        base.OnInspectorGUI();
    //    }
    //}
}
