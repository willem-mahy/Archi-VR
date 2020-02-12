using TMPro;
using UnityEngine;

namespace WM
{
    public class ReferenceSystem6DOF : MonoBehaviour
    {
        public void SetText(string text)
        {
            this.text.text = text;
        }

        public TextMeshPro text;
    }
}
