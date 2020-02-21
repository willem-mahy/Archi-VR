using UnityEngine;

namespace WM.Unity
{
    /// <summary>
    /// Add this component to a gameobject, to render a wireframe box
    /// of given Color and Size around the origin of the gameobject.
    /// </summary>
    public class LineRendererBox : MonoBehaviour
    {
        #region Fields

        /// <summary>
        ///  The diagonal length.
        /// </summary>
        public Vector3 Size;

        /// <summary>
        /// The Color
        /// </summary>
        public Color Color = Color.white;

        /// <summary>
        /// The alpha value.
        /// </summary>
        public float Alpha = 1.0f;


        private Gradient _gradient;

        private Material _material;


        private LineRenderer _lineRendererTop;

        private LineRenderer _lineRendererBottom;

        private LineRenderer _lineRendererEdge0;

        private LineRenderer _lineRendererEdge1;

        private LineRenderer _lineRendererEdge2;

        private LineRenderer _lineRendererEdge3;

        #endregion Fields

        #region GameObject overrides

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            float widthMultiplier = 0.01f;

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            _gradient = new Gradient();
            _gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color, 0.0f), new GradientColorKey(Color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(Alpha, 0.0f), new GradientAlphaKey(Alpha, 1.0f) }
            );

            _material = new Material(Shader.Find("Sprites/Default"));

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererBottom = go.AddComponent<LineRenderer>();
                _lineRendererBottom.material = _material;
                _lineRendererBottom.widthMultiplier = widthMultiplier;
                _lineRendererBottom.positionCount = 5;
                _lineRendererBottom.colorGradient = _gradient;
            }

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererTop = go.AddComponent<LineRenderer>();
                _lineRendererTop.material = _material;
                _lineRendererTop.widthMultiplier = widthMultiplier;
                _lineRendererTop.positionCount = 5;
                _lineRendererTop.colorGradient = _gradient;
            }

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererEdge0 = go.AddComponent<LineRenderer>();
                _lineRendererEdge0.material = _material;
                _lineRendererEdge0.widthMultiplier = widthMultiplier;
                _lineRendererEdge0.positionCount = 2;
                _lineRendererEdge0.colorGradient = _gradient;
            }

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererEdge1 = go.AddComponent<LineRenderer>();
                _lineRendererEdge1.material = _material;
                _lineRendererEdge1.widthMultiplier = widthMultiplier;
                _lineRendererEdge1.positionCount = 2;
                _lineRendererEdge1.colorGradient = _gradient;
            }

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererEdge2 = go.AddComponent<LineRenderer>();
                _lineRendererEdge2.material = _material;
                _lineRendererEdge2.widthMultiplier = widthMultiplier;
                _lineRendererEdge2.positionCount = 2;
                _lineRendererEdge2.colorGradient = _gradient;
            }

            {
                var go = new GameObject();
                go.transform.parent = gameObject.transform;
                _lineRendererEdge3 = go.AddComponent<LineRenderer>();
                _lineRendererEdge3.material = _material;
                _lineRendererEdge3.widthMultiplier = widthMultiplier;
                _lineRendererEdge3.positionCount = 2;
                _lineRendererEdge3.colorGradient = _gradient;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            _gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color, 0.0f), new GradientColorKey(Color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(Alpha, 0.0f), new GradientAlphaKey(Alpha, 1.0f) }
            );

            _lineRendererBottom.colorGradient = _gradient;
            _lineRendererTop.colorGradient = _gradient;
            _lineRendererEdge0.colorGradient = _gradient;
            _lineRendererEdge1.colorGradient = _gradient;
            _lineRendererEdge2.colorGradient = _gradient;
            _lineRendererEdge3.colorGradient = _gradient;

            var s = Size * 0.5f;

            var localCorners = new Vector3[]
            {
            new Vector3(-s.x, -s.y, -s.z),
            new Vector3(+s.x, -s.y, -s.z),
            new Vector3(+s.x, -s.y, +s.z),
            new Vector3(-s.x, -s.y, +s.z),
            new Vector3(-s.x, +s.y, -s.z),
            new Vector3(+s.x, +s.y, -s.z),
            new Vector3(+s.x, +s.y, +s.z),
            new Vector3(-s.x, +s.y, +s.z),
            };

            var corners = new Vector3[localCorners.Length];

            for (int i = 0; i < localCorners.Length; ++i)
            {
                corners[i] = gameObject.transform.TransformPoint(localCorners[i]);
            }

            {
                var points = new Vector3[5]
                {
                corners[0],
                corners[1],
                corners[2],
                corners[3],
                corners[0]
                };

                _lineRendererBottom.SetPositions(points);
            }

            {
                var points = new Vector3[5]
                {
                corners[4],
                corners[5],
                corners[6],
                corners[7],
                corners[4]
                };

                _lineRendererTop.SetPositions(points);
            }

            {
                var points = new Vector3[2]
                {
                corners[0],
                corners[4]
                };

                _lineRendererEdge0.SetPositions(points);
            }

            {
                var points = new Vector3[2]
                {
                corners[1],
                corners[5]
                };

                _lineRendererEdge1.SetPositions(points);
            }

            {
                var points = new Vector3[2]
                {
                corners[2],
                corners[6]
                };

                _lineRendererEdge2.SetPositions(points);
            }

            {
                var points = new Vector3[2]
                {
                corners[3],
                corners[7]
                };

                _lineRendererEdge3.SetPositions(points);
            }
        }

        #endregion GameObject overrides
    }
}
