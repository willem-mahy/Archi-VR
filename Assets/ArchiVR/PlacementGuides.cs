using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WM.Unity;

namespace ArchiVR
{
    public class Guide
    {
        public GameObject _gameObject;

        public GameObject _gameObject2;

        public LineRenderer _lineRenderer;

        public TextMeshPro _text;

        public Guide(
            string name,
            Vector3 position,
            Vector3 forward,
            Material material,
            float widthMultiplier,
            Gradient colorGradient)
        {
            _gameObject = new GameObject(name + " LaserLine");
            
            _gameObject.transform.position = position;
            _gameObject.transform.LookAt(position + forward);

            _lineRenderer = _gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = material;
            _lineRenderer.widthMultiplier = widthMultiplier;
            _lineRenderer.positionCount = 2;
            _lineRenderer.colorGradient = colorGradient;

            _gameObject2 = new GameObject(name + " TextBillBoard");
            _gameObject2.transform.position = position + 0.1f * forward;
            _gameObject2.transform.LookAt(_gameObject2.transform.position + forward);
            //var billBoard = _gameObject2.AddComponent<BillBoard>();
            
            var gameObject3 = new GameObject(name + " Text");
            //gameObject3.transform.rotation = Quaternion.Euler(0, 180, 0);
            gameObject3.transform.LookAt(Vector3.back);
            gameObject3.transform.position = position + 0.2f * forward + 0.01f * Vector3.forward;
            gameObject3.transform.SetParent(_gameObject2.transform, true);

            _text = gameObject3.AddComponent<TextMeshPro>();
            _text.transform.localScale = 0.02f * Vector3.one;
            _text.fontSize = 48;
            _text.color = Color.black;

            //gameObject3.AddComponent<RenderOnTop>();
        }

        /// <summary>
        /// Make the guide measure the distance from its origin, to the first object its laser line encounters.
        /// </summary>
        public void Update()
        {
            var points = new Vector3[2]
            {
                _gameObject.transform.position,
                _gameObject.transform.forward
            };

            RaycastHit hit;

            if (Physics.Raycast(_gameObject.transform.position, _gameObject.transform.forward, out hit))
            {
                points[1] = hit.point;

                _text.text = (points[1] - points[0]).magnitude.ToString("F2") + "m";
            }
            else
            {
                _text.text = "";
            }
            
            _lineRenderer.SetPositions(points);
        }
    }

    /// <summary>
    /// Add this component to a gameobject, to render placement guides from it.
    /// </summary>
    public class PlacementGuides : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The Color
        /// </summary>
        public Color PositiveColor = Color.green;

        /// <summary>
        /// The Color
        /// </summary>
        public Color NegativeColor = Color.blue;

        /// <summary>
        /// The alpha value.
        /// </summary>
        public float Alpha = 1.0f;


        private Gradient _positiveGradient;

        private Gradient _negativeGradient;

        private Material _material;
        
        private List<Guide> _guides = new List<Guide>();

        #endregion Fields

        #region GameObject overrides

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            float widthMultiplier = 0.01f;

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            _positiveGradient = new Gradient();
            _positiveGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(PositiveColor, 0.0f), new GradientColorKey(PositiveColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(Alpha, 0.0f), new GradientAlphaKey(Alpha, 1.0f) }
            );

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            _negativeGradient = new Gradient();
            _negativeGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(NegativeColor, 0.0f), new GradientColorKey(NegativeColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(Alpha, 0.0f), new GradientAlphaKey(Alpha, 1.0f) }
            );

            _material = new Material(Shader.Find("Sprites/Default"));

            var boxCollider = gameObject.GetComponent<BoxCollider>();
            
            var pointPosX =
                0.01f * Vector3.forward +
                (boxCollider.center.x + 0.5f * boxCollider.size.x) * Vector3.right +
                (boxCollider.center.y + 0.5f * boxCollider.size.y) * Vector3.up;

            var pointNegX =
                0.01f * Vector3.forward +
                (boxCollider.center.x - 0.5f * boxCollider.size.x) * Vector3.right +
                (boxCollider.center.y + 0.5f * boxCollider.size.y) * Vector3.up;

            var pointPosY =
                0.01f * gameObject.transform.forward +
                (boxCollider.center.x) * Vector3.right +
                (boxCollider.center.y + 0.5f * boxCollider.size.y) * Vector3.up;

            var pointNegY_Top =
                boxCollider.center +
                (0.05f * boxCollider.size.x) * Vector3.left +
                (0.5f * boxCollider.size.y) * Vector3.up +
                ((0.5f * boxCollider.size.z) + 0.01f)  * Vector3.forward;

            var pointNegY_Bottom =
                boxCollider.center +
                (0.05f * boxCollider.size.x) * Vector3.right +
                (0.5f * boxCollider.size.y) * Vector3.down +
                ((0.5f * boxCollider.size.z) + 0.01f) * Vector3.forward;

            _guides.Add(new Guide("Guide PosX", pointPosX, Vector3.right, _material, widthMultiplier, _positiveGradient));
            _guides.Add(new Guide("Guide NegX", pointNegX, Vector3.left, _material, widthMultiplier, _negativeGradient));
            _guides.Add(new Guide("Guide PosY", pointPosY, Vector3.up, _material, widthMultiplier, _positiveGradient));
            _guides.Add(new Guide("Guide NegY Top", pointNegY_Top, Vector3.down, _material, widthMultiplier, _negativeGradient));
            _guides.Add(new Guide("Guide NegY Bottom", pointNegY_Bottom, Vector3.down, _material, widthMultiplier, _negativeGradient));

            foreach (var guide in _guides)
            {
                guide._gameObject.transform.SetParent(gameObject.transform, false);
                guide._gameObject2.transform.SetParent(gameObject.transform, false);
            }
        }

        #endregion GameObject overrides

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            foreach (var guide in _guides)
            {
                guide.Update();
            }
        }
    }
}
