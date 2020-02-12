using System.Collections.Generic;
using UnityEngine;
using WM.Application;

namespace WM.Colocation
{
    /// <summary>
    /// Application state in which the user defines the shared reference system to be used for colocation.
    /// 
    /// Some profixed acronyms are used in this code, to denote the Reference system in which geometrical data is expressed:
    /// _W : expressed in world space.
    /// _T : expressed in tracking system space.
    /// </summary>
    public class ApplicationStateDefineSharedReferenceSystem : ApplicationState
    {
        /// <summary>
        /// The measured positions.
        /// </summary>
        private List<GameObject> _measuredPoints_W = new List<GameObject>();

        /// <summary>
        /// The shared reference system.
        /// </summary>
        private GameObject _referenceSystem;

        /// <summary>
        /// Called once, right after construction.
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// Called when the application enters the application state.
        /// </summary>
        public override void Enter()
        {
            OVRManager.boundary.SetVisible(true);

            m_application.CreateTrackingSpaceReferenceSystem();

            m_application.HudInfoPanel.SetActive(true);
            m_application.HudInfoText.text = "Measure Point 1";

            // Show bounds of tracking system
            // Show visualisation of the position/rotation of the controllers on screen.

            InitButtonMappingUI();
        }

        /// <summary>
        /// Called when the application exits the application state.
        /// </summary>
        public override void Exit()
        {
            OVRManager.boundary.SetVisible(false);

            m_application.DestroyTrackingSpaceReferenceSystem();

            m_application.HudInfoPanel.SetActive(false);
            m_application.HudInfoText.text = "";

            // Hide visualisation of the position/rotation of the controllers on screen.

            foreach (var point in _measuredPoints_W)
            {
                UtilUnity.Destroy(point);
            }
            _measuredPoints_W.Clear();
        }

        /// <summary>
        /// Called every frame while the application is in the application state.
        /// </summary>
        public override void Update()
        {
            if (UnityEngine.Application.isEditor)
            {
                m_application.Fly();
                m_application.UpdateTrackingSpace();
            }

            {
                var p = m_application.m_leftHandAnchor.transform.localPosition;
                m_application.m_leftControllerText.text = string.Format("{0:F3}, {1:F3}, {2:F3}", p.x, p.y, p.z);
            }

            {
                var p = m_application.m_rightHandAnchor.transform.localPosition;
                m_application.m_rightControllerText.text = string.Format("{0:F3}, {1:F3}, {2:F3}", p.x, p.y, p.z);
            }

            // TODO: Update the position/rotation of the controllers on screen.

            if (_measuredPoints_W.Count < 2)
            {
                if (m_application.m_controllerInput.m_controllerState.button7Down)
                {
                    MeasurePoint(m_application.m_leftHandAnchor.transform);
                }
                else if (m_application.m_controllerInput.m_controllerState.button8Down)
                {
                    MeasurePoint(m_application.m_rightHandAnchor.transform);
                }
                else if (m_application.m_controllerInput.m_controllerState.button5Down || m_application.m_controllerInput.m_controllerState.button6Down)
                {
                    ErasePoint();
                }
            }
            else
            {
                if (m_application.m_controllerInput.m_controllerState.button7Down || m_application.m_controllerInput.m_controllerState.button8Down)
                {
                    m_application.SetActiveApplicationState(0);
                }
                else if (m_application.m_controllerInput.m_controllerState.button5Down || m_application.m_controllerInput.m_controllerState.button6Down)
                {
                    ErasePoint();
                }
            }
        }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public override void UpdateModelLocationAndScale() { }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public override void UpdateTrackingSpacePosition() { }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public override void OnTeleportFadeOutComplete()
        {
        }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public override void OnTeleportFadeInComplete()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitButtonMappingUI()
        {
            m_application.Logger.Debug("ApplicationStateDefineSharedReferenceSystem.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.textLeftIndexTrigger.text = "Measure";
                leftControllerButtonMapping.textLeftHandTrigger.text = "Erase";

                leftControllerButtonMapping.textButtonStart.text = "";

                leftControllerButtonMapping.textButtonX.text = "";
                leftControllerButtonMapping.textButtonY.text = "";

                leftControllerButtonMapping.textLeftThumbUp.text = "";
                leftControllerButtonMapping.textLeftThumbDown.text = "";
                leftControllerButtonMapping.textLeftThumbLeft.text = "";
                leftControllerButtonMapping.textLeftThumbRight.text = "";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.textRightIndexTrigger.text = "Measure";
                rightControllerButtonMapping.textRightHandTrigger.text = "Erase";

                rightControllerButtonMapping.textButtonOculus.text = "Exit";

                rightControllerButtonMapping.textButtonA.text = "";
                rightControllerButtonMapping.textButtonB.text = "";

                rightControllerButtonMapping.textRightThumbUp.text = (isEditor ? "Beweeg vooruit (ArrowUp)" : "");
                rightControllerButtonMapping.textRightThumbDown.text = (isEditor ? "Beweeg achteruit (ArrowDown)" : "");
                rightControllerButtonMapping.textRightThumbLeft.text = (isEditor ? "Beweeg links (ArrowLeft)" : "");
                rightControllerButtonMapping.textRightThumbRight.text = (isEditor ? "Beweeg rechts (ArrowRight)" : "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        private void MeasurePoint(Transform t)
        {
            if (_measuredPoints_W.Count == 1)
            {
                float distanceFromFirstPoint = (t.position - _measuredPoints_W[0].transform.position).magnitude;

                if (distanceFromFirstPoint < 1)
                {
                    return; // Do not accept new point: too close to the first point!
                }
            }

            var measuredPoint = UnityEngine.GameObject.Instantiate(
                Resources.Load("WM/Prefab/Geometry/PointWithCaption"),
                t.position,
                t.rotation) as GameObject;

            var pointWithCaption = measuredPoint.GetComponent<PointWithCaption>();
            pointWithCaption.SetText("Point " + (_measuredPoints_W.Count + 1));
            
            _measuredPoints_W.Add(measuredPoint);

            if (_measuredPoints_W.Count == 2)
            {
                m_application.HudInfoText.text = "Measuring complete"; 
                UpdateSharedReferenceSystem();
            }
            else
            {
                m_application.HudInfoText.text = "Measure Point " + (_measuredPoints_W.Count + 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        private void ErasePoint()
        {
            if (_referenceSystem != null)
            {
                UtilUnity.Destroy(_referenceSystem);
                _referenceSystem = null;
            }

            if (_measuredPoints_W.Count > 0)
            {
                int pointToEraseIndex = _measuredPoints_W.Count - 1;
                UtilUnity.Destroy(_measuredPoints_W[pointToEraseIndex]);
                _measuredPoints_W.RemoveAt(pointToEraseIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateSharedReferenceSystem()
        {
            // Compute position.
            var pos0 = _measuredPoints_W[0].transform.position;
            var pos1 = _measuredPoints_W[1].transform.position;

            var position = (pos0 + pos1) / 2;

            // Compute orientation.
            var axis0 = Vector3.up;
            var axis1 = Vector3.Normalize(pos1 - pos0);
            var axis2 = Vector3.Cross(axis0, axis1);
            axis1 = Vector3.Cross(axis2, axis0);

            var rotation = Quaternion.LookRotation(axis1, axis0);

            // Create the shared reference system.
            var sharedReferenceSystem = m_application.CreateSharedReferenceSystem();

            // Put it to the correct location.
            sharedReferenceSystem.transform.position = position;
            sharedReferenceSystem.transform.rotation = rotation;

            {
                var p = sharedReferenceSystem.transform.localPosition;
                sharedReferenceSystem.GetComponent<ReferenceSystem6DOF>().SetText("SRF " +  string.Format("({0:F3}, {1:F3}, {2:F3})", p.x, p.y, p.z));
            }
        }
    }
} // namespace WM.Colocation