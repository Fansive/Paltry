using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Extension;

namespace Paltry
{
    public static class TransformUtil  
    {
        /// <summary>
        /// ����������תΪcanvas�µ�������� �����:
        /// <seealso cref="UIRelativeCanvasPos2WorldPos(Vector2, RectTransform)"/>
        /// </summary>
        public static Vector2 WorldPos2UIRelativeCanvasPos(Vector2 worldPos,RectTransform canvas)
        {
            return 0.5f * new Vector2(worldPos.x/canvas.position.x,worldPos.y/canvas.position.y);
        }
        /// <summary>
        /// �������Canvas������((0,0)��(1,1),���½�Ϊԭ��)תΪ��������
        /// Ҫ��Canvas���½�������ԭ��,��Pivot������
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

