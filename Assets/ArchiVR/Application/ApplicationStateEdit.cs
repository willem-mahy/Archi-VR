using ArchiVR.Application.Editable;
using ArchiVR.Application.Properties;
using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.Application;
using WM.Unity;

namespace ArchiVR.Application
{
    /// <summary>
    /// Application state in which the user edits lights (select, delete, create).
    /// </summary>
    public class ApplicationStateEdit
        : ApplicationState<ApplicationArchiVR>
    {
        public ApplicationStateEdit(ApplicationArchiVR application) : base(application)
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
            Resume();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Exit"/> implementation.
        /// </summary>
        public override void Exit()
        {
            Pause();
            
            //m_application.LocomotionEnabled = true;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(false);

            m_application.HudInfoPanel.SetActive(false);
            m_application.HudInfoText.text = "";

            foreach (var box in _selectBoxes)
            {
                UtilUnity.Destroy(box.gameObject);
            }
            _selectBoxes.Clear();

            if (null != _hoverBoxGO)
            {
                UtilUnity.Destroy(_hoverBoxGO);
                _hoverBoxGO = null;
                _hoverBox = null;
            }

            m_application.SaveProjectData();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Resume"/> implementation.
        /// </summary>
        public override void Resume()
        {
            m_application.LocomotionEnabled = true;// false;

            // Make all editable objects visible
            m_application.PoiEditData.ContainerGameObject.SetActive(true);
            m_application.PropEditData.ContainerGameObject.SetActive(true);
            m_application.LightEditData.ContainerGameObject.SetActive(true);

            // Show the Edit menu.
            m_application.EditMenuPanel.gameObject.SetActive(true);
            m_application.EditMenuPanel.ApplicationState = this;
            m_application.AddPickRaySelectionTarget(m_application.EditMenuPanel.gameObject);

            if (null == _hoverBoxGO)
            {
                _hoverBoxGO = new GameObject("LightEditHoverBox");
                _hoverBox = _hoverBoxGO.AddComponent<LineRendererBox>();
                _hoverBox.Color = m_application.HoverColor;
            }

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            m_application.HudInfoPanel.SetActive(true);

            OnHover(null);
            OnSelect(null);
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Pause"/> implementation.
        /// </summary>
        public override void Pause()
        {
            // Make all POI editable objects invisible
            m_application.PoiEditData.ContainerGameObject.SetActive(false);

            _hoverBoxGO.SetActive(false);

            // Hide the Edit menu.
            m_application.EditMenuPanel.gameObject.SetActive(false);
            m_application.AddPickRaySelectionTarget(m_application.EditMenuPanel.gameObject);

            m_application.PropertiesMenuPanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Update"/> implementation.
        /// </summary>
        public override void Update()
        {
            UpdateControllerUI();

            m_application.Fly();
            m_application.UpdateTrackingSpace();

            var controllerState = m_application.m_controllerInput.m_controllerState;

            // Setting lights as the active object type is done by:
            // - Pressing 'L' on the keyboard.
            if (Input.GetKeyDown(KeyCode.L))
            {
                StartCreateLight();
            }

            // Setting props as the active object type is done by:
            // - Pressing 'F' on the keyboard.
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCreateProp();
            }

            // Setting POI as the active object type is done by:
            // - Pressing 'P' on the keyboard.
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCreatePOI();
            }

            // Delete selected lights is performed by:
            //  - Pressing the controller 'B' button.
            var delete = controllerState.bButtonDown;

            if (delete)
            {
                Delete();
            }

            // Additive selection is enabled while:
            // - keeping R controller hand trigger pressed.
            _addToSelection = controllerState.rHandTriggerPressed;

            // Exiting edit mode is done by:
            // - Pressing left controller index trigger.
            var returnToParentState = controllerState.lIndexTriggerDown;

            if (returnToParentState)
            {
                if (!m_application.PropertiesMenuPanel.gameObject.activeSelf)
                {
                    m_application.PopApplicationState();
                }
                else
                {
                    CloseProperties();
                }
                return;
            }

            #region Update hovered object

            GameObject hoveredObject = null;

            var pickRay = m_application.RPickRay.GetRay();

            foreach (var editData in m_application.EditDatas)
            {
                foreach (var gameObject in editData.GameObjects)
                {
                    var hitInfo = new RaycastHit();
                    hitInfo.distance = float.NaN;

                    GameObject pickedGO = null;

                    UtilUnity.PickRecursively(
                        gameObject,
                        pickRay,
                        gameObject,
                        ref pickedGO,
                        ref hitInfo);

                    if (null != pickedGO)
                    {
                        hoveredObject = gameObject;
                        break;
                    }
                }
            }

            OnHover(hoveredObject);

            #endregion Update hovered object

            if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
            {
                if (!m_application.PropertiesMenuPanel.gameObject.activeSelf) // Do not select/unselect objects while properties menu is open.
                {
                    OnSelect(_hoveredObject);
                }
            }
        }

        /// <summary>
        /// Gets a descriptive text for the current selection.
        /// In case of nothing selected => the empty string.
        /// In case of single object selected => the object name.
        /// In case of multiple object selected => "Selected 2 Props", "Selected 3 POIs", Selected 1000 Lights".
        /// </summary>
        /// <returns></returns>
        private string GetSelectionText()
        {
            switch (_selectedObjects.Count)
            {
                case 0:
                    return "";
                case 1:
                    return "Selected: " + _selectedObjects[0].name;
                default:
                    return "Selected " + _selectedObjects.Count + " objects";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateEditObject.UpdateControllerButtonUI()");

            var isEditor = UnityEngine.Application.isEditor;

            m_application.HudInfoText.text = "Edit";

            m_application.m_leftControllerText.text = GetSelectionText();
            m_application.m_rightControllerText.text = (null == _hoveredObject) ? "" : _hoveredObject.name;

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
                leftControllerButtonMapping.ThumbLeft.Text = "";
                leftControllerButtonMapping.ThumbRight.Text = "";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.IndexTrigger.Text = "Select";
                rightControllerButtonMapping.HandTrigger.Text = "Additive selection";

                rightControllerButtonMapping.ButtonOculusStart.Text = "";

                rightControllerButtonMapping.ButtonA.Text = "Create";
                rightControllerButtonMapping.ButtonB.Text = _selectedObjects.Count == 0 ? "" : "Delete";

                m_application.DisplayFlyControls();
            }
        }

        private void OnHover(GameObject gameObject)
        {
            _hoveredObject = gameObject;

            if (null == gameObject)
            {
                _hoverBoxGO.gameObject.SetActive(false);
            }
            else
            {
                _hoverBoxGO.SetActive(true);

                var boxCollider = _hoveredObject.gameObject.GetComponent<Collider>() as BoxCollider;

                if (null == boxCollider)
                {
                    // The top-level go does not have a box collider => compute bounds recursively.
                    var bounds = UtilUnity.CalculateBounds(_hoveredObject);

                    if (bounds.HasValue)
                    {
                        _hoverBoxGO.transform.position = bounds.Value.center;
                        _hoverBoxGO.transform.rotation = Quaternion.identity;
                        _hoverBox.Size = bounds.Value.size;
                    }
                }
                else
                {
                    // The top-level has a box collider => use that as input for our hoverbox definition.
                    _hoverBoxGO.transform.position = _hoveredObject.gameObject.transform.TransformPoint(boxCollider.center);
                    _hoverBoxGO.transform.rotation = _hoveredObject.gameObject.transform.rotation;
                    _hoverBox.Size = boxCollider.size;
                }
            }
        }

        private void OnSelect(GameObject gameObject)
        {
            // Remove bounding boxes from current selection.
            foreach (var box in _selectBoxes)
            {
                UtilUnity.Destroy(box.gameObject);
            }
            _selectBoxes.Clear();


            // Update selection.
            if (_addToSelection)
            {
                if (null != gameObject)
                {
                    if (_selectedObjects.Contains(gameObject))
                    {
                        _selectedObjects.Remove(gameObject);
                    }
                    else
                    {
                        _selectedObjects.Add(gameObject);
                    }
                }
            }
            else
            {
                _selectedObjects.Clear();

                if (null != gameObject)
                {
                    _selectedObjects.Add(gameObject);
                }
            }


            // Add bounding boxes for current selection.
            foreach (var selectedObject in _selectedObjects)
            {
                var selectedObjectBoundingBoxGO = new GameObject("SelectedObjectBoundingBox");
                var selectedObjectBoundingBox = selectedObjectBoundingBoxGO.AddComponent<LineRendererBox>();
                selectedObjectBoundingBox.Color = m_application.SelectionColor;

                selectedObjectBoundingBoxGO.SetActive(true);
                selectedObjectBoundingBoxGO.transform.SetParent(selectedObject.transform, false);

                var bounds = UtilUnity.CalculateBounds(selectedObject);

                if (bounds.HasValue)
                {
                    selectedObjectBoundingBoxGO.transform.position = bounds.Value.center;
                    selectedObjectBoundingBox.Size = selectedObject.transform.InverseTransformVector(bounds.Value.size);
                }

                _selectBoxes.Add(selectedObjectBoundingBox);
            }

            ShowProperties();
        }

        /// <summary>
        /// Deletes the selected objects.
        /// </summary>
        public void Delete()
        {
            var selectedObjects = new List<GameObject>(_selectedObjects);

            OnHover(null);
            OnSelect(null);

            foreach (var selectedObject in selectedObjects)
            {
                foreach (var editData in m_application.EditDatas)
                {
                    if (editData.GameObjects.Contains(selectedObject))
                    {
                        editData.GameObjects.Remove(selectedObject);
                    }
                    UtilUnity.Destroy(selectedObject);
                }
            }   
        }
        
        /// <summary>
        /// Closes the properties menu.
        /// </summary>
        public void CloseProperties()
        {
            m_application.PropertiesMenuPanel.gameObject.SetActive(false);
            m_application.EditMenuPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the properties menu (if having a single object selected).
        /// </summary>
        public void ShowProperties()
        {
            if (_selectedObjects.Count != 1)
            {
                return;
            }
            var selectedObject = _selectedObjects[0];
            var properties = selectedObject.GetComponent<PropertiesBase>();

            if (null != properties)
            {
                m_application.PropertiesMenuPanel.Properties = properties;
                m_application.PropertiesMenuPanel.gameObject.SetActive(true);
                m_application.EditMenuPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnActiveObjectTypeChanged()
        {
            _addToSelection = false;
            OnSelect(null);
        }

        public void StartCreateLight()
        {
            var applicationState = new ApplicationStateCreate<ArchiVRLight, LightDefinition>(
                m_application,
                m_application.LightEditData);

            m_application.PushApplicationState(applicationState);
        }

        public void StartCreatePOI()
        {
            var applicationState = new ApplicationStateCreate<ArchiVRPOI, POIDefinition>(
                m_application,
                m_application.PoiEditData);

            m_application.PushApplicationState(applicationState);
        }

        public void StartCreateProp()
        {
            var applicationState = new ApplicationStateCreate<ArchiVRProp, PropDefinition>(
                m_application,
                m_application.PropEditData);

            m_application.PushApplicationState(applicationState);
        }

        #region Fields

        /// <summary>
        /// Flags whether additive selection is enabled.
        /// </summary>
        private bool _addToSelection = false;

        /// <summary>
        /// The game object of the editable object we are currently hovering with the pick ray.
        /// </summary>
        private GameObject _hoveredObject;
        
        /// <summary>
        /// The bounding box shown around the currently hovered editable object.
        /// </summary>
        private GameObject _hoverBoxGO;

        private LineRendererBox _hoverBox;

        private List<GameObject> _selectedObjects = new List<GameObject>();

        private List<LineRendererBox> _selectBoxes = new List<LineRendererBox>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion