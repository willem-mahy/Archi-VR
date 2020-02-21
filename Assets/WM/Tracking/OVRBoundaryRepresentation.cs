using UnityEngine;

namespace WM.Unity.Tracking
{
    public class OVRBoundaryRepresentation : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private LineRendererBox _playAreaBoundary;

        /// <summary>
        /// 
        /// </summary>
        private LineRendererBox _outerBoundary;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            _outerBoundary = gameObject.AddComponent<LineRendererBox>();
            _outerBoundary.Color = Color.yellow;

            _playAreaBoundary = gameObject.AddComponent<LineRendererBox>();
            _playAreaBoundary.Color = Color.green;
        }

        private void Update()
        {
            var playAreaSize = OVRManager.boundary.GetConfigured() ?
                OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea) :
                Vector3.one;

            if (!UnityEngine.Application.isEditor)
            {
                _playAreaBoundary.transform.localPosition = new Vector3(0, 0.5f * playAreaSize.y, 0);
            }
            _playAreaBoundary.Size = playAreaSize;

            var outerSize = OVRManager.boundary.GetConfigured() ?
                OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary) :
                Vector3.one + 0.5f * Vector3.forward + Vector3.right;

            if (!UnityEngine.Application.isEditor)
            {
                _outerBoundary.transform.localPosition = new Vector3(0, 0.5f * outerSize.y, 0);
            }
            _outerBoundary.Size = outerSize;
        }
    }
}