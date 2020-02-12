using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WM
{
    public class PointWithCaption : MonoBehaviour
    {
        public void SetText(string text)
        {
            this.text.text = text;
        }

        public TextMeshPro text;
    }
}
