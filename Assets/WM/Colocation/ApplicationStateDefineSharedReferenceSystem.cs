using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.Colocation
{
    /// <summary>
    /// Application state in which the user defines the shared reference system to be used for colocation.
    /// 
    /// Defining the SharedReferenceSystem is a 3-step progress:
    /// - Measure first reference point
    /// - Measure second reference point
    /// - Review the resulting SharedreferenceSystem.
    /// 
    /// Some profixed acronyms are used in this code, to denote the Reference system in which geometrical data is expressed:
    /// _W : expressed in world space.
    /// _T : expressed in tracking system space.
    /// </summary>
    public class ApplicationStateDefineSharedReferenceSystem : ApplicationState
    {
        private Color32 _newObjectsColor = new Color32(0, 255, 0, 255);
        private Color32 _defaultObjectsColor = new Color32(255, 255, 255, 255);

        /// <summary>
        /// The measured positions for the current SharedReferenceFrame.
        /// These are the positions that were measured in the previous iteration of this procedure.
        /// </summary>
        private List<PointWithCaption> _points = new List<PointWithCaption>();

        /// <summary>
        /// The positions being measured in this iteration of the procedure.
        /// </summary>
        private List<PointWithCaption> _newPoints = new List<PointWithCaption>();

        /// <summary>
        /// The shared reference system being defined in this procedure.
        /// </summary>
        private ReferenceSystem6DOF _newSharedReferenceSystem;

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
            m_application.m_controllerInput.Reset(); // To prevent a spurious point measurement when starting the procedure.

            OVRManager.instance.AllowRecenter = false;

            OVRManager.boundary.SetVisible(true);

            // Show current measured points.
            foreach (var point in _points)
            {
                point.gameObject.SetActive(true);
            }

            m_application.HudInfoPanel.SetActive(true);
            
            // Show bounds of tracking system
            
            InitButtonMappingUI_MeasurePoint();

            UpdateInfoText();
        }

        /// <summary>
        /// Called when the application exits the application state.
        /// </summary>
        public override void Exit()
        {
            AcceptNewReferenceSystem();

            OVRManager.instance.AllowRecenter = m_application.ColocationEnabled;

            OVRManager.boundary.SetVisible(false);

            m_application.HudInfoPanel.SetActive(false);
            m_application.HudInfoText.text = "";

            // Hide current measured points.
            foreach (var newPoint in _points)
            {
                newPoint.gameObject.SetActive(false);
            }

            // Clear new measured points.
            foreach (var point in _newPoints)
            {
                UtilUnity.Destroy(point.gameObject);
            }

            _newPoints.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        private void AcceptNewReferenceSystem()
        {
            // Clear current measured points.
            foreach (var point in _points)
            {
                UtilUnity.Destroy(point.gameObject);
            }
            _points.Clear();

            // Set the new measured points as current measured points.
            foreach (var point in _newPoints)
            {
                _points.Add(point);
                
                var pointWithCaption = point.GetComponent<PointWithCaption>();
                pointWithCaption.CaptionColor = _defaultObjectsColor;
            }
            _newPoints.Clear();

            // Set the new SharedReferenceSystem as the current SharedReferenceSystem.
            var sharedReferenceSystemGO = m_application.SharedReferenceSystem.gameObject;
            var newSharedReferenceSystemGO = _newSharedReferenceSystem.gameObject;

            m_application.SetSharedReferenceSystemLocation(
                newSharedReferenceSystemGO.transform.position,
                newSharedReferenceSystemGO.transform.rotation);

            // Destroy the new shared reference system.
            UtilUnity.Destroy(newSharedReferenceSystemGO);
            _newSharedReferenceSystem = null;
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

            UpdateControllerInfos();

            var back = m_application.m_controllerInput.m_controllerState.button1Down || m_application.m_controllerInput.m_controllerState.button3Down;

            if (_newPoints.Count < 2)
            {
                if (m_application.m_controllerInput.m_controllerState.button7Down)
                {
                    MeasurePoint(m_application.m_leftHandAnchor.transform);
                }
                else if (m_application.m_controllerInput.m_controllerState.button8Down)
                {
                    MeasurePoint(m_application.m_rightHandAnchor.transform);
                }
                else if (back)
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
                else if (back)
                {
                    ErasePoint();
                }
            }
        }

        /// <summary>
        /// Update the displayed position / rotation of the controllers.
        /// </summary>
        private void UpdateControllerInfos()
        {
            UpdateControllerInfo(m_application.m_leftHandAnchor, m_application.m_leftControllerText);
            UpdateControllerInfo(m_application.m_rightHandAnchor, m_application.m_rightControllerText);
        }

        /// <summary>
        /// Displayed position / rotation of the given hand anchor in the given text.
        /// </summary>
        private void UpdateControllerInfo(
           GameObject handAnchor,
           Text controllerInfoText)
        {
            var p = handAnchor.transform.localPosition;
            controllerInfoText.text = string.Format("{0:F3}, {1:F3}, {2:F3}", p.x, p.y, p.z);
        }

        #region TODO: Remove from Application state interface?

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

        #endregion TODO: Remove from Application state interface?

        /// <summary>
        /// 
        /// </summary>
        public void InitButtonMappingUI_MeasurePoint()
        {
            m_application.Logger.Debug("ApplicationStateDefineSharedReferenceSystem.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.IndexTrigger.Text = "Measure";
                leftControllerButtonMapping.HandTrigger.Text = "";

                leftControllerButtonMapping.ButtonStart.Text = "";

                leftControllerButtonMapping.ButtonX.Text = "Erase";
                leftControllerButtonMapping.ButtonY.Text = "";

                leftControllerButtonMapping.ThumbUp.Text = "";
                leftControllerButtonMapping.ThumbDown.Text = "";
                leftControllerButtonMapping.ThumbLeft.Text = "";
                leftControllerButtonMapping.ThumbRight.Text = "";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.IndexTrigger.Text = "Measure";
                rightControllerButtonMapping.HandTrigger.Text = "";

                rightControllerButtonMapping.ButtonOculusStart.Text = "Exit";

                rightControllerButtonMapping.ButtonXA.Text = "Erase";
                rightControllerButtonMapping.ButtonYB.Text = "";

                rightControllerButtonMapping.ThumbUp.Text = (isEditor ? "Beweeg vooruit (ArrowUp)" : "");
                rightControllerButtonMapping.ThumbDown.Text = (isEditor ? "Beweeg achteruit (ArrowDown)" : "");
                rightControllerButtonMapping.ThumbLeft.Text = (isEditor ? "Beweeg links (ArrowLeft)" : "");
                rightControllerButtonMapping.ThumbRight.Text = (isEditor ? "Beweeg rechts (ArrowRight)" : "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitButtonMappingUI_AcceptSRF()
        {
            m_application.Logger.Debug("ApplicationStateDefineSharedReferenceSystem.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.IndexTrigger.Text = "Accept";
                leftControllerButtonMapping.HandTrigger.Text = "";

                leftControllerButtonMapping.ButtonStart.Text = "";

                leftControllerButtonMapping.ButtonX.Text = "Erase";
                leftControllerButtonMapping.ButtonY.Text = "";

                leftControllerButtonMapping.ThumbUp.Text = "";
                leftControllerButtonMapping.ThumbDown.Text = "";
                leftControllerButtonMapping.ThumbLeft.Text = "";
                leftControllerButtonMapping.ThumbRight.Text = "";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.IndexTrigger.Text = "Accept";
                rightControllerButtonMapping.HandTrigger.Text = "";

                rightControllerButtonMapping.ButtonOculusStart.Text = "Exit";

                rightControllerButtonMapping.ButtonXA.Text = "Erase";
                rightControllerButtonMapping.ButtonYB.Text = "";

                rightControllerButtonMapping.ThumbUp.Text = (isEditor ? "Beweeg vooruit (ArrowUp)" : "");
                rightControllerButtonMapping.ThumbDown.Text = (isEditor ? "Beweeg achteruit (ArrowDown)" : "");
                rightControllerButtonMapping.ThumbLeft.Text = (isEditor ? "Beweeg links (ArrowLeft)" : "");
                rightControllerButtonMapping.ThumbRight.Text = (isEditor ? "Beweeg rechts (ArrowRight)" : "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        private void MeasurePoint(Transform t)
        {
            if (_newPoints.Count == 1)
            {
                float distanceFromFirstPoint = (t.position - _newPoints[0].transform.position).magnitude;

                if (distanceFromFirstPoint < 1)
                {
                    return; // Do not accept new point: too close to the first point!
                }
            }

            var newPointGO = UnityEngine.GameObject.Instantiate(
                Resources.Load("WM/Prefab/Geometry/PointWithCaption"),
                t.position,
                t.rotation) as GameObject;

            var newPoint = newPointGO.GetComponent<PointWithCaption>();

            var pointPositionText = UtilUnity.ToString(t.localPosition);
            var pointNumber = _newPoints.Count + 1;
            var captionText = string.Format("Point {0} {1}", pointNumber, pointPositionText);
            newPoint.CaptionText = captionText;

            newPoint.CaptionColor = _newObjectsColor;

            _newPoints.Add(newPoint);

            if (_newPoints.Count == 2)
            {
                CreateNewSharedReferenceSystem();
                InitButtonMappingUI_AcceptSRF();
            }

            UpdateInfoText();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateInfoText()
        {
            if (_newPoints.Count == 2)
            {
                m_application.HudInfoText.text = "Measuring complete";
            }
            else
            {
                var numberOfPointToMeasure = _newPoints.Count + 1;
                m_application.HudInfoText.text = "Measure Point " + numberOfPointToMeasure;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        private void ErasePoint()
        {
            if (_newSharedReferenceSystem != null)
            {
                UtilUnity.Destroy(_newSharedReferenceSystem.gameObject);
                _newSharedReferenceSystem = null;
            }

            if (_newPoints.Count > 0)
            {
                int pointToEraseIndex = _newPoints.Count - 1;
                UtilUnity.Destroy(_newPoints[pointToEraseIndex].gameObject);
                _newPoints.RemoveAt(pointToEraseIndex);
            }

            UpdateInfoText();
            InitButtonMappingUI_MeasurePoint();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateNewSharedReferenceSystem()
        {
            // Compute position.
            var pos0 = _newPoints[0].transform.position;
            pos0.y = m_application.trackingSpace.transform.position.y;
            
            var pos1 = _newPoints[1].transform.position;
            pos1.y = m_application.trackingSpace.transform.position.y;

            var position = (pos0 + pos1) / 2;

            // Compute orientation.
            var axis0 = Vector3.up;
            var axis1 = Vector3.Normalize(pos1 - pos0);
            var axis2 = Vector3.Cross(axis0, axis1);
            axis1 = Vector3.Cross(axis2, axis0);

            var rotation = Quaternion.LookRotation(axis1, axis0);

            //
            var referenceSystemPrefab = Resources.Load("WM/Prefab/Geometry/ReferenceSystem6DOF");

            // Create the new SharedReferenceSystem GameObject.
            var newSharedReferenceSystemGO = UnityEngine.GameObject.Instantiate(
                    referenceSystemPrefab,
                    position,
                    rotation) as GameObject;

            _newSharedReferenceSystem = newSharedReferenceSystemGO.GetComponent<ReferenceSystem6DOF>();
            
            // Give it a descriptive name.
            newSharedReferenceSystemGO.name = "New SRF";

            // Attach it as a child to the tracking space.
            newSharedReferenceSystemGO.transform.SetParent(m_application.trackingSpace.transform, true);
            
            // Initialize its caption.
            var newSharedReferenceSystemLocalPosition = newSharedReferenceSystemGO.transform.localPosition;
            var captionText = string.Format("{0} {1}", newSharedReferenceSystemGO.name, UtilUnity.ToString(newSharedReferenceSystemLocalPosition));
            _newSharedReferenceSystem.CaptionText = captionText;

            _newSharedReferenceSystem.CaptionColor = _newObjectsColor;
        }
    }
} // namespace WM.Colocation