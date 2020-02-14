﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WM
{
    public class PointWithCaption : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// The caption text.
        /// </summary>
        /// <param name="text"></param>
        public string CaptionText
        {
            get
            {
                return captionText.text;
            }
            set
            {
                captionText.text = value;
            }
        }

        /// <summary>
        /// The caption color.
        /// </summary>
        public Color32 CaptionColor
        {
            get
            {
                return captionText.faceColor;
            }
            set
            {
                captionText.faceColor = value;
            }
        }

        /// <summary>
        /// The caption alpha.
        /// </summary>
        public float CaptionAlpha
        {
            get
            {
                return captionText.alpha;
            }
            set
            {
                captionText.alpha = value;
            }
        }

        #endregion Public API

        #region Fields

        /// <summary>
        /// The text.
        /// </summary>
        public TextMeshPro captionText;

        #endregion Fields
    }
}
