using ArchiVR.Application.Editable;
using ArchiVR.Application.PickInitialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.Application;
using WM.Util;

namespace ArchiVR.Application
{
    /// <summary>
    /// Application state in which the user creates an object.
    /// </summary>
    public class ApplicationStateCreate<E, D> : ApplicationState<ApplicationArchiVR>
    {
        public ApplicationStateCreate(
            ApplicationArchiVR application,
            ApplicationArchiVR.TypedEditData<E, D> EditData) : base(application)
        {
            this.EditData = EditData;
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Init"/> implementation.
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Enter"/> implementation.
        /// </summary>
        public override void Enter()
        {
            // Load all prefabs.
            foreach (var objectPrefabDefinition in _editSettings.ObjectPrefabDefinitions)
            {
                _objectPrefabs.Add(Resources.Load<GameObject>(objectPrefabDefinition.PrefabPath));
            }

            Resume();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Exit"/> implementation.
        /// </summary>
        public override void Exit()
        {
            //m_application.LocomotionEnabled = true;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(false);

            m_application.HudInfoPanel.SetActive(false);
            m_application.HudInfoText.text = "";

            if (null != _previewGO)
            {
                UtilUnity.Destroy(_previewGO);
            }
        }

        public override void Resume()
        {
            m_application.LocomotionEnabled = true;// false;

            // Make editable objects visible
            EditData.ContainerGameObject.SetActive(true);

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            m_application.HudInfoText.text = "Create " + EditData.Settings.ObjectTypeName;
            m_application.HudInfoPanel.SetActive(true);

            OnActiveObjectTypeChanged();
        }

        /// <summary>
        /// The rotation of the preview along its anchoring axis, as defined by 'B' button presses.
        /// </summary>
        private int _rotationIndex = 0;

        /// <summary>
        /// The step with which the rotation of the preview along its anchoring axis is increased, with each 'B' button press.
        /// </summary>
        private float _rotationStep = 45;

        /// <summary>
        /// Rotation of the preview around its anchoring axis.
        /// </summary>
        private float Rotation { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        private bool _pressToDefineRotation = true;

        /// <summary>
        /// 
        /// </summary>
        private bool PositionPinned => _pickedInfos.Count >= 1;

        /// <summary>
        /// <see cref="ApplicationState{T}.Update"/> implementation.
        /// </summary>
        public override void Update()
        {
            UpdateControllerUI();

            m_application.Fly();
            //m_application.UpdateTrackingSpace();  // Tempoarily disabled manipulation of the tracking space, since it collides with the input mapping for toggling active prefab type below.

            var controllerState = m_application.m_controllerInput.m_controllerState;

            // Exiting edit mode is done by:
            // - Pressing left controller index trigger.
            var returnToParentState = controllerState.lIndexTriggerDown;

            if (returnToParentState)
            {
                m_application.PopApplicationState();
                return;
            }

            // - Flip L controller thumb stick Left => Activate previous prefab type
            if (controllerState.lThumbstickDirectionLeftDown)
            {
                ActivatePreviousObjectType();
                return;
            }

            // - Flip L controller thumb stick Right => Activate next prefab type
            if (controllerState.lThumbstickDirectionRightDown)
            {
                ActivateNextObjectType();
                return;
            }

            // Controller 'B' button pressed => Rotate preview 90 deg.
            if (m_application.m_controllerInput.m_controllerState.bButtonDown)
            {
                _rotationIndex = (int)(Math.Floor((Rotation + _rotationStep) / _rotationStep));

                Rotation = _rotationIndex * _rotationStep;

                UpdatePreview();
                return;
            }

            UpdateHoveredPoint();

            if (!PositionPinned)
            {
                UpdatePreview();

                // R Index trigger down => Pick a point:
                if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
                {
                    AddHoveredPointAsPickedPoint();
                }
            }
            else
            {
                // R controller hand trigger down => Unpin position.
                if (controllerState.rHandTriggerDown)
                {
                    UnpinPosition();
                    return;
                }
                
                if (_pressToDefineRotation)
                {
                    // R Index trigger down => update preview, so that it rotates to the hovered position
                    if (controllerState.rIndexTriggerPressed)
                    {
                        UpdateRotationFromHoveredPoint();
                    }
                }
                else
                {
                    // Continuously update preview, so that it rotates to the hovered position
                    UpdateRotationFromHoveredPoint();
                }

                // Controller 'A' button pressed => Create object from preview
                if (m_application.m_controllerInput.m_controllerState.aButtonDown)
                {
                    CreateObject();
                }
            }
        }

        private void UpdateRotationFromHoveredPoint()
        {
            var angle = GetRotationAngleFromHoveredPoint();

            if (float.IsNaN(angle))
            {
                return;
            }

            Rotation += angle;

            UpdatePreview();
        }
        
        private float GetRotationAngleFromHoveredPoint()
        {
            if (!_hitInfo.HasValue)
            {
                return float.NaN;
            }

            var offset = (_hitInfo.Value.point - _previewGO.transform.position).normalized;

            var offset_Local = _previewGO.transform.InverseTransformDirection(offset);

            var pi = _previewGO.GetComponent<PickInitializable>();

            var anchoringAxis_Local = pi.GetAnchoringAxis_Local(_pickedInfos);
            var upAxis_Local = pi.GetUpAxis_Local(_pickedInfos);

            var anchorPlane = new Plane(anchoringAxis_Local, 0);

            offset_Local = anchorPlane.ClosestPointOnPlane(offset_Local);

            offset_Local = offset_Local.normalized;

            var angle = (float)UtilMath.ToDegrees(Math.Acos(Vector3.Dot(offset_Local, upAxis_Local)));

            var c = Vector3.Cross(offset_Local, upAxis_Local);

            if (c.magnitude == 0)
            {
                return float.NaN;
            }

            if (Vector3.Dot(c, anchoringAxis_Local) > 0)
            {
                angle = -angle;
            }

            return angle;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UnpinPosition()
        {
            while (PositionPinned)
            {
                RemovePickedPoint();
            }
        }

        /// <summary>
        /// Removes the last added picked point (if any).
        /// </summary>
        private void RemovePickedPoint()
        {
            if (_pickedInfos.Count == 0)
            {
                return;
            }

            _pickedInfos.RemoveAt(_pickedInfos.Count - 1);
        }

        /// <summary>
        /// Update the hovered point.
        /// </summary>
        private void UpdateHoveredPoint()
        {
            var pickRay = m_application.RPickRay.GetRay();

            var hitInfo = new RaycastHit();
            hitInfo.distance = float.NaN;

            GameObject pickedGO = null;

            foreach (var layer in m_application.GetLayers())
            {
                UtilUnity.PickRecursively(
                    layer.Model,
                    pickRay,
                    layer.Model,
                    ref pickedGO,
                    ref hitInfo);
            }

            m_application.RPickRay.HitDistance = hitInfo.distance;

            if (pickedGO != null)
            {
                _hitInfo = hitInfo;
            }
            else
            {
                _hitInfo = null;
            }
        }

        /// <summary>
        /// If there is a hovered point, add it as a picked point.
        /// </summary>
        private void AddHoveredPointAsPickedPoint()
        {
            if (!_hitInfo.HasValue)
            {
                return;
            }

            _pickedInfos.Add(_hitInfo.Value);

            if (null != _previewGO)
            {
                var pi = _previewGO.GetComponent<PickInitializable>();

                // If the prefab has no pick init logic,
                if (null == pi)
                {
                    // instantiate it at the first picked position with identity rotation.
                    CreateObject();
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatePreview()
        {
            if (null == _previewGO)
            {
                return;
            }

            // Compose a list of picks, including the currently hovered point (if any)
            var picks = new List<RaycastHit>(_pickedInfos);

            if (picks.Count == 0)
            {
                if (_hitInfo.HasValue)
                {
                    picks.Add(_hitInfo.Value);
                }
            }

            if (0 == picks.Count)
            {
                // Without picks, the object cannot be located => hide preview.
                _previewGO.SetActive(false);
            }
            else
            {
                // Locate the object.
                var pi = _previewGO.GetComponent<PickInitializable>();

                if (null != pi)
                {
                    pi.Initialize(picks, Rotation);
                }

                // Show preview.
                _previewGO.SetActive(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateDefineObject.UpdateControllerButtonUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.IndexTrigger.Text = "Back";
                leftControllerButtonMapping.HandTrigger.Text = "";

                leftControllerButtonMapping.ButtonStart.Text = "";

                leftControllerButtonMapping.ButtonX.Text = "";
                leftControllerButtonMapping.ButtonY.Text = "";

                leftControllerButtonMapping.ThumbUp.Text = "";
                leftControllerButtonMapping.ThumbDown.Text = "";
                leftControllerButtonMapping.ThumbLeft.Text = "Prev Type";
                leftControllerButtonMapping.ThumbRight.Text = "Next Type";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.IndexTrigger.Text = "Pick";
                rightControllerButtonMapping.HandTrigger.Text = (_pickedInfos.Count == 0) ? "" : "Remove Pick";

                rightControllerButtonMapping.ButtonOculusStart.Text = "";

                rightControllerButtonMapping.ButtonA.Text = _pickedInfos.Count == 0 ? "" : "Place";
                rightControllerButtonMapping.ButtonB.Text = "Step Rotate";

                m_application.DisplayFlyControls();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivatePreviousObjectType()
        {
            _editSettings.ActiveObjectPrefabIndex = UtilIterate.MakeCycle(--_editSettings.ActiveObjectPrefabIndex, 0, _editSettings.ObjectPrefabDefinitions.Count);

            OnActiveObjectTypeChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivateNextObjectType()
        {
            _editSettings.ActiveObjectPrefabIndex = UtilIterate.MakeCycle(++_editSettings.ActiveObjectPrefabIndex, 0, _editSettings.ObjectPrefabDefinitions.Count);

            OnActiveObjectTypeChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnActiveObjectTypeChanged()
        {
            if (null != _previewGO)
            {
                UtilUnity.Destroy(_previewGO);

                _previewGO = null;
            }

            _previewGO = GameObject.Instantiate(
                ActiveObjectPrefab,
                Vector3.zero,
                Quaternion.identity);

            if (null != _previewGO.GetComponent<BoxCollider>())
            {
                _previewGO.AddComponent<PlacementGuides>();
            }

            m_application.m_leftControllerText.text = ActiveObjectPrefabDefinition.Name;
            m_application.m_leftControllerText.gameObject.SetActive(true);

            var pi = _previewGO.GetComponent<PickInitializable>();

            if (pi == null)
            {
                m_application.m_leftControllerText.color = Color.red;
                pi = _previewGO.AddComponent<PickInitializable>();
                pi.PickInitializationTypes.Add(PickInitializable.PickInitializationType.Default);
            }
            else
            {
                m_application.m_leftControllerText.color = Color.black;
            }
        }

        /// <summary>
        /// Create an instance of the active prefab,
        /// position it at the location of the preview,
        /// and add it persistently to the project content.
        /// </summary>
        private void CreateObject()
        {
            // Create the new editable object.
            var objectGO = GameObject.Instantiate(
                ActiveObjectPrefab,
                _previewGO.transform.position,
                _previewGO.transform.rotation);

            var editable = objectGO.GetComponent<E>();

            if (editable is IPrefabInstantiation prefabInstantiation)
            {
                prefabInstantiation.PrefabPath = ActiveObjectPrefabDefinition.PrefabPath;
            }
            else
            {
                m_application.Logger.Error("Trying to create an editable that is not an IPrefabInstantiation!" + editable.ToString());

                UtilUnity.Destroy(objectGO);
                return;
            }

            if (editable is ILayerContent layerContent)
            {
                layerContent.LayerName = m_application.EstimateLayer(objectGO);
            }

            // Give the new editable object a unique name.
            objectGO.name = ActiveObjectPrefab.name + " (" + Guid.NewGuid().ToString() + ")";

            // Add the new editable object to the project content.
            EditData.Add(objectGO);

            // Clear the picked points.
            _pickedInfos.Clear();
        }

        #region Fields

        private ApplicationArchiVR.TypedEditData<E, D> EditData;

        private List<GameObject> _gameObjects => EditData.GameObjects;

        private ApplicationArchiVR.ObjectEditSettings _editSettings => EditData.Settings;

        public ObjectPrefabDefinition ActiveObjectPrefabDefinition => _editSettings.ObjectPrefabDefinitions[_editSettings.ActiveObjectPrefabIndex];

        /// <summary>
        /// The position where the pick ray is currently picking.
        /// </summary>
        private List<RaycastHit> _pickedInfos = new List<RaycastHit>();

        /// <summary>
        /// The position where the pick ray is currently picking.
        /// </summary>
        private RaycastHit? _hitInfo;

        private GameObject ActiveObjectPrefab => _objectPrefabs[_editSettings.ActiveObjectPrefabIndex];

        private GameObject _previewGO;

        private List<GameObject> _objectPrefabs = new List<GameObject>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion