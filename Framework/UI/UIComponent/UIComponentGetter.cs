using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Paltry
{
    public class UIComponentGetter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        public RectTransform Rect => _rectTransform ??= GetComponent<RectTransform>();
        private Image _image;
        public Image Image => _image ??= GetComponent<Image>();
        private TMP_Text _tmpText;
        public TMP_Text TMPText => _tmpText ??= GetComponent<TMP_Text>();
    }
}

