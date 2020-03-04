using ArchiVR.Application.PickInitialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.Application;

namespace ArchiVR.Application
{
    /// <summary>
    /// Application state in which the user creates an object.
    /// </summary>
    public class ApplicationStateCreate : ApplicationState<ApplicationArchiVR>
    {
        public ApplicationStateCreate(
            ApplicationArchiVR application,
            int objectTypeIndex) : base(application)
        {
            _objectTypeIndex = objectTypeIndex;
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

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            m_application.HudInfoText.text = "Create " + EditData.Settings.ObjectTypeName;
            m_application.HudInfoPanel.SetActive(true);

            OnActiveObjectTypeChanged();
        }

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
            var returnToParentState =controllerState.lIndexTriggerDown;

            if (returnToParentState)
            {
                m_application.PopApplicationState();
                return;
            }

            // Removing a picked point is performed by:
            // - Pressing the R controller hand trigger.
            if (m_application.m_controllerInput.m_controllerState.rHandTriggerDown)
            {
                if (_pickedInfos.Count > 0) _pickedInfos.RemoveAt(_pickedInfos.Count - 1);
            }

            // Activating the previous prefab type is performed by:
            // - Flipping the L controller thumb stick Left.
            if (controllerState.lThumbstickDirectionLeftDown)
            {
                ActivatePreviousObjectType();
            }

            // Activating the next prefab type is performed by:
            // - Flipping the L controller thumb stick Right.
            if (controllerState.lThumbstickDirectionRightDown)
            {
                ActivateNextObjectType();
            }

            #region Update picked position

            var pickRay = m_application.RPickRay.GetRay();

            var hitInfo = new RaycastHit();
            hitInfo.distance = float.NaN;

            GameObject pickedGO = null;
            
            foreach (var layer in m_application.GetModelLayers())
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

            #endregion Update picked position

            // Picking a point is performed by:
            //  - Pressing the R controller index trigger.
            if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
            {
                if (_hitInfo.HasValue)
                {
                    _pickedInfos.Add(_hitInfo.Value);

                    if (null != _previewGO)
                    {
                        var pi = _previewGO.GetComponent<PickInitializable>();

                        if (null == pi)
                        {
                            CreateObject();
                            return;
                        }
                    }
                }
            }

            if (null != _previewGO)
            {
                var picks = new List<RaycastHit>(_pickedInfos);

                if (_hitInfo.HasValue)
                {
                    picks.Add(_hitInfo.Value);
                }

                _previewGO.SetActive(0 != picks.Count);

                if (0 != picks.Count)
                {
                    var pi = _previewGO.GetComponent<PickInitializable>();

                    if (null != pi)
                    {
                        pi.Initialize(picks);
                    }
                    else
                    {
                        _previewGO.transform.position = _hitInfo.Value.point;
                        _previewGO.transform.LookAt(_hitInfo.Value.point + _hitInfo.Value.normal, Vector3.up);
                    }
                }
            }

            // Creating an object is performed by:
            //  - Pressing the controller 'X' button.
            if (m_application.m_controllerInput.m_controllerState.aButtonDown)
            {
                CreateObject();
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

                rightControllerButtonMapping.ButtonA.Text = "Place";
                rightControllerButtonMapping.ButtonB.Text = "";

                rightControllerButtonMapping.ThumbUp.Text = "Forward";
                rightControllerButtonMapping.ThumbDown.Text = "Backward";
                rightControllerButtonMapping.ThumbLeft.Text = "Left";
                rightControllerButtonMapping.ThumbRight.Text = "Right";
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
        }

        /// <summary>
        /// Create an instance of the active prefab,
        /// position it at the location of the preview,
        /// and add it persistently to the project content.
        /// </summary>
        private void CreateObject()
        {
            #region 1) Create the new object's GameObject

            var objectGO = GameObject.Instantiate(
                ActiveObjectPrefab,
                Vector3.zero,
                Quaternion.identity);

            // Give the new object a unique name.
            objectGO.name = ActiveObjectPrefab.name + " (" + Guid.NewGuid().ToString() + ")";

            var pi = objectGO.GetComponent<PickInitializable>();

            if (null != pi)
            {
                pi.Initialize(_pickedInfos);
            }
            else
            {
                objectGO.transform.position = _hitInfo.Value.point;
                objectGO.transform.LookAt(_hitInfo.Value.point + _hitInfo.Value.normal, Vector3.up);
            }

            _gameObjects.Add(objectGO);

            #endregion 1) Create the new object's GameObject

            #region 2) Create the new object's ObjectDefinition

            ObjectDefinition objectDefinition; // TODO: = ActiveObjectPrefab.GetDefinition();

            switch (_objectTypeIndex) // Design defect!
            {
                case 0:
                    objectDefinition = new LightDefinition();
                    break;
                case 1:
                    objectDefinition = new PropDefinition();
                    break;
                case 2:
                    objectDefinition = new POIDefinition();
                    break;
                default:
                    objectDefinition = new ObjectDefinition();
                    break;
            }

            objectDefinition.Name = objectGO.name;
            objectDefinition.Position = objectGO.transform.position;
            objectDefinition.Rotation = objectGO.transform.rotation;
            objectDefinition.PrefabPath = ActiveObjectPrefabDefinition.PrefabPath;
            objectDefinition.GameObject = objectGO;

            if (objectDefinition is LightDefinition lightDefinition)
            {
                //lightDefinition.LayerName = ;
                //lightDefinition.LightColor = ;
                //lightDefinition.BodyColor1 = ;
                //lightDefinition.BodyColor2 = ;
            }

            if (objectDefinition is PropDefinition propDefinition)
            {   
                //propDefinition.LayerName = ;
            }

            if (objectDefinition is POIDefinition poiDefinition)
            {
                //poiDefinition.LayerName = ;
            }

            _objectDefinitions.Add(objectDefinition);

            #endregion 2) Create the new object's ObjectDefinition

            _pickedInfos.Clear();
        }

        #region Fields

        private int _objectTypeIndex;

        private ApplicationArchiVR.EditData EditData => m_application.EditDatas[_objectTypeIndex];

        private List<GameObject> _gameObjects => EditData.GameObjects;

        private List<ObjectDefinition> _objectDefinitions => EditData.ObjectDefinitions;

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