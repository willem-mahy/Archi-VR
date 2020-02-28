using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.Application;

namespace ArchiVR.Application
{
    /// <summary>
    /// Application state in which the user defines a light.
    /// </summary>
    public class ApplicationStateDefineLight : ApplicationState<ApplicationArchiVR>
    {
        public class LightType
        {
            public string Name;
            public string PrefabPath;
        };

        public ApplicationStateDefineLight(ApplicationArchiVR application) : base(application)
        {
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
            foreach (var lightType in _lightTypes)
            {
                _lightTypePrefabs.Add(Resources.Load<GameObject>(lightType.PrefabPath));
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

            m_application.HudInfoText.text = "Create light";
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

            if (null != _previewGO)
            {
                _previewGO.SetActive(_hitInfo.HasValue);

                if (_hitInfo.HasValue)
                {
                    _previewGO.transform.position = _hitInfo.Value.point;
                    _previewGO.transform.LookAt(_hitInfo.Value.point + _hitInfo.Value.normal, Vector3.up);
                }
            }

            if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
            {
                if (_hitInfo.HasValue)
                {
                    CreateLight();
                }
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
                rightControllerButtonMapping.IndexTrigger.Text = "Place light";
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
            _activeLightTypeIndex = UtilIterate.MakeCycle(--_activeLightTypeIndex, 0, _lightTypes.Count);

            OnActiveLightTypeChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivateNextLightType()
        {
            _activeLightTypeIndex = UtilIterate.MakeCycle(++_activeLightTypeIndex, 0, _lightTypes.Count);

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

            lightGO.transform.position = _hitInfo.Value.point;
            lightGO.transform.LookAt(_hitInfo.Value.point + _hitInfo.Value.normal, Vector3.up);

            m_application.LightingObjects.Add(lightGO);

            var lightDefinition = new LightDefinition()
            {
                Position = lightGO.transform.position,
                Rotation = lightGO.transform.rotation,
                PrefabPath = ActiveLightType.PrefabPath,
                GameObject = lightGO
            };

            m_application.ProjectData.LightingData.lightDefinitions.Add(lightDefinition);
        }

        #region Fields

        private List<LightType> _lightTypes = new List<LightType>()
        {
            new LightType() { Name = "Ceiling Round", PrefabPath = "ArchiVR/Prefab/Architectural/Lighting/Ceiling Round" },
            new LightType() { Name = "TL", PrefabPath = "ArchiVR/Prefab/Architectural/Lighting/TL/TL Single 120cm" },
            new LightType() { Name = "Spot Round", PrefabPath = "ArchiVR/Prefab/Architectural/Lighting/Spot/Round/SpotBuiltInRound" },
            new LightType() { Name = "Wall Cube", PrefabPath = "ArchiVR/Prefab/Architectural/Lighting/Spot/Wall Cube/Wall Cube" },
            new LightType() { Name = "Pendant Sphere", PrefabPath = "ArchiVR/Prefab/Architectural/Lighting/Pendant/Pendant Sphere" }
        };

        private int _activeLightTypeIndex = 0;

        public LightType ActiveLightType => _lightTypes[_activeLightTypeIndex];

        private RaycastHit? _hitInfo;

        private GameObject ActiveLightTypePrefab => _lightTypePrefabs[_activeLightTypeIndex];

        private GameObject _previewGO;

        private List<GameObject> _lightTypePrefabs = new List<GameObject>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion