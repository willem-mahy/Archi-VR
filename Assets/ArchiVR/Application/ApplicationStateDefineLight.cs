using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.Application;

namespace ArchiVR.Application
{
    /// <summary>
    /// Application state in which the user creates a light.
    /// </summary>
    public class ApplicationStateDefineLight<D>
        : ApplicationState<ApplicationArchiVR>
         where D : new()
    {
        public ApplicationStateDefineLight(
            ApplicationArchiVR application,
            string objectTypeName,
            ref List<GameObject> objects,
            ref List<D> objectDefinitions,
            List<ObjectPrefabDefinition> objectPrefabDefinitions) : base(application)
        {
            _objectTypeName = objectTypeName;
            _objects = objects;
            _objectDefinitions = objectDefinitions;
            _objectPrefabDefinitions = objectPrefabDefinitions;
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
            foreach (var objectPrefabDefinition in _objectPrefabDefinitions)
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
            m_application.LocomotionEnabled = true;

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
            m_application.LocomotionEnabled = false;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            m_application.HudInfoText.text = "Create " + _objectTypeName;
            m_application.HudInfoPanel.SetActive(true);

            InitButtonMappingUI();

            OnActiveLightTypeChanged();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Update"/> implementation.
        /// </summary>
        public override void Update()
        {
            m_application.Fly();
            m_application.UpdateTrackingSpace();
             
            var controllerState = m_application.m_controllerInput.m_controllerState;

            // Pressing 'BackSpace' on the keyboard is a shortcut for returning to the default state.
            var returnToDefaultState = controllerState.lIndexTriggerDown || Input.GetKeyDown(KeyCode.Backspace);

            if (returnToDefaultState)
            {
                m_application.PopApplicationState();
            }

            if (controllerState.lThumbstickDirectionLeftDown)
            {
                ActivatePreviousLightType();
            }

            if (controllerState.lThumbstickDirectionRightDown)
            {
                ActivateNextLightType();
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

            if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
            {
                if (_hitInfo.HasValue)
                {
                    _pickedInfos.Add(_hitInfo.Value);

                    if (null != _previewGO)
                    {
                        var pi = _previewGO.GetComponent<IPickInitializable>();

                        if (null == pi)
                        {
                            CreateLight();
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
                    var pi = _previewGO.GetComponent<IPickInitializable>();

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

            if (m_application.m_controllerInput.m_controllerState.xButtonDown)
            {
                CreateLight();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitButtonMappingUI()
        {
            m_application.Logger.Debug("ApplicationStateDefineLight.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.IndexTrigger.Text = "";
                leftControllerButtonMapping.HandTrigger.Text = "";

                leftControllerButtonMapping.ButtonStart.Text = "";

                leftControllerButtonMapping.ButtonX.Text = "Place";
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
                rightControllerButtonMapping.HandTrigger.Text = "";

                rightControllerButtonMapping.ButtonOculusStart.Text = "";

                rightControllerButtonMapping.ButtonA.Text = "";
                rightControllerButtonMapping.ButtonB.Text = "";

                rightControllerButtonMapping.ThumbUp.Text = "Beweeg vooruit" + (isEditor ? " (Up)" : "");
                rightControllerButtonMapping.ThumbDown.Text = "Beweeg achteruit" + (isEditor ? " (Down)" : "");
                rightControllerButtonMapping.ThumbLeft.Text = "Beweeg links" + (isEditor ? "(Left)" : "");
                rightControllerButtonMapping.ThumbRight.Text = "Beweeg rechts" + (isEditor ? " (Right)" : "");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivatePreviousLightType()
        {
            _activeLightTypeIndex = UtilIterate.MakeCycle(--_activeLightTypeIndex, 0, _objectPrefabDefinitions.Count);

            OnActiveLightTypeChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivateNextLightType()
        {
            _activeLightTypeIndex = UtilIterate.MakeCycle(++_activeLightTypeIndex, 0, _objectPrefabDefinitions.Count);

            OnActiveLightTypeChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnActiveLightTypeChanged()
        {
            if (null != _previewGO)
            {
                UtilUnity.Destroy(_previewGO);

                _previewGO = null;
            }

            _previewGO = GameObject.Instantiate(
                ActiveLightTypePrefab,
                Vector3.zero,
                Quaternion.identity);

            m_application.m_leftControllerText.text = ActiveLightType.Name;
            m_application.m_leftControllerText.gameObject.SetActive(true);
        }

        private void CreateLight()
        {
            var lightGO = GameObject.Instantiate(
                ActiveLightTypePrefab,
                Vector3.zero,
                Quaternion.identity);

            var pi = lightGO.GetComponent<IPickInitializable>();

            if (null != pi)
            {
                pi.Initialize(_pickedInfos);
            }
            else
            {
                lightGO.transform.position = _hitInfo.Value.point;
                lightGO.transform.LookAt(_hitInfo.Value.point + _hitInfo.Value.normal, Vector3.up);
            }

            m_application.LightingObjects.Add(lightGO);

            var d = new D();

            if (d is ObjectDefinition objectDefinition)
            {
                objectDefinition.Name = lightGO.name;
                objectDefinition.Position = lightGO.transform.position;
                objectDefinition.Rotation = lightGO.transform.rotation;
                objectDefinition.PrefabPath = ActiveLightType.PrefabPath;
                objectDefinition.GameObject = lightGO;
            }

            if (d is LightDefinition lightDefinition)
            {
                //lightDefinition.LayerName = ;
                //lightDefinition.LightColor = ;
                //lightDefinition.BodyColor1 = ;
                //lightDefinition.BodyColor2 = ;
            }

            if (d is PropDefinition propDefinition)
            {   
                //propDefinition.LayerName = ;
            }

            if (d is POIDefinition poiDefinition)
            {
                //poiDefinition.LayerName = ;
            }

            _objectDefinitions.Add(d);

            _pickedInfos.Clear();
        }

        #region Fields

        string _objectTypeName;

        List<GameObject> _objects;

        List<D> _objectDefinitions;

        List<ObjectPrefabDefinition> _objectPrefabDefinitions;

        private int _activeLightTypeIndex = 0;

        public ObjectPrefabDefinition ActiveLightType => _objectPrefabDefinitions[_activeLightTypeIndex];

        /// <summary>
        /// The position where the pick ray is currently picking.
        /// </summary>
        private List<RaycastHit> _pickedInfos = new List<RaycastHit>();

        /// <summary>
        /// The position where the pick ray is currently picking.
        /// </summary>
        private RaycastHit? _hitInfo;

        private GameObject ActiveLightTypePrefab => _objectPrefabs[_activeLightTypeIndex];

        private GameObject _previewGO;

        private List<GameObject> _objectPrefabs = new List<GameObject>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion