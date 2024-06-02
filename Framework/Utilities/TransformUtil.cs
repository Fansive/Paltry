using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Extension;

namespace Paltry
{
    public static class TransformUtil  
    {
        /// <summary>
        /// 将世界坐标转为canvas下的相对坐标 逆操作:
        /// <seealso cref="UIRelativeCanvasPos2WorldPos(Vector2, RectTransform)"/>
        /// </summary>
        public static Vector2 WorldPos2UIRelativeCanvasPos(Vector2 worldPos,RectTransform canvas)
        {
            return 0.5f * new Vector2(worldPos.x/canvas.position.x,worldPos.y/canvas.position.y);
        }
        /// <summary>
        /// 将相对于Canvas的坐标((0,0)到(1,1),左下角为原点)转为世界坐标
        /// 要求Canvas左下角是世界原点,且Pivot在中心
        /// </summary>
        public static Vector2 UIRelativeCanvasPos2WorldPos(Vector2 relativeCanvasPos,RectTransform canvas)
        {
            return Vector2.Scale(relativeCanvasPos,canvas.position*2) ;
        }

        public static RectTransform FindCanvasRectTransform(RectTransform rect)
        {
            Transform parent = rect;
            while(parent != null)
            {
                if (parent.GetComponent<Canvas>())
                    return parent.transform as RectTransform;
                parent = parent.parent;
            }

            return null;
        }
    }
}

