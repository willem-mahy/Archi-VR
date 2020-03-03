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

            if (ActiveObjectTypeIndex == 2)
            {
                m_application.PoiContainerGameObject.SetActive(true);
            }
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
            }

            if (ActiveObjectTypeIndex == 2)
            {
                m_application.PoiContainerGameObject.SetActive(false);
            }

            m_application.SaveProjectData();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Resume"/> implementation.
        /// </summary>
        public override void Resume()
        {
            m_application.LocomotionEnabled = true;// false;

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
            _hoverBoxGO.SetActive(false);

            // Hide the Edit menu.
            m_application.EditMenuPanel.gameObject.SetActive(false);
            m_application.AddPickRaySelectionTarget(m_application.EditMenuPanel.gameObject);
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
                ActiveObjectTypeIndex = 0;
            }

            // Setting props as the active object type is done by:
            // - Pressing 'F' on the keyboard.
            if (Input.GetKeyDown(KeyCode.F))
            {
                ActiveObjectTypeIndex = 1;
            }

            // Setting POI as the active object type is done by:
            // - Pressing 'P' on the keyboard.
            if (Input.GetKeyDown(KeyCode.P))
            {
                ActiveObjectTypeIndex = 2;
            }

            // Starting creation of an object is done by:
            //  - Pressing the controller 'A' button.
            var startCreateMode = controllerState.aButtonDown;
            
            if (startCreateMode)
            {
                var applicationState = new ApplicationStateCreate(
                    m_application,
                    ActiveObjectTypeIndex);

                m_application.PushApplicationState(applicationState);
                return;
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
                m_application.PopApplicationState();
                return;
            }

            #region Update hovered object

            GameObject hoveredObject = null;

            var pickRay = m_application.RPickRay.GetRay();

            foreach (var gameObject in _objects)
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

            OnHover(hoveredObject);

            #endregion Update hovered object

            if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
            {
                OnSelect(_hoveredObject);
            }
        }

        private string GetSelectionText()
        {
            switch (_selectedObjects.Count)
            {
                case 0:
                    return "";
                case 1:
                    return "Selected: " + _selectedObjects[0].name;
                default:
                    return "Selected: " + _selectedObjects.Count + " " + EditData.Settings.ObjectTypeName + "s.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateEditObject.UpdateControllerButtonUI()");

            var isEditor = UnityEngine.Application.isEditor;

            m_application.HudInfoText.text = "Edit " + EditData.Settings.ObjectTypeName;

            m_application.m_leftControllerText.text = GetSelectionText();
            m_application.m_rightControllerText.text = (null == _hoveredObject) ? "" : _hoveredObject.name;

            // Left controller
            var leftControllerButtonMapping = m_application.leftControllerButtonMapping;

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.IndexTrigger.Text = "Back" + (isEditor ? " (R)" : "");
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
                rightControllerButtonMapping.IndexTrigger.Text = "Select" + (isEditor ? " (LMB)" : "");
                rightControllerButtonMapping.HandTrigger.Text = _selectedObjects.Count == 0 ? "" : "Additive selection";

                rightControllerButtonMapping.ButtonOculusStart.Text = "";

                rightControllerButtonMapping.ButtonA.Text = "Create" + (isEditor ? " (F3)" : "");
                rightControllerButtonMapping.ButtonB.Text = _selectedObjects.Count == 0 ? "" : "Delete" + (isEditor ? " (F4)" : "");

                rightControllerButtonMapping.ThumbUp.Text = "Beweeg vooruit" + (isEditor ? " (ArrowUp)" : "");
                rightControllerButtonMapping.ThumbDown.Text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                rightControllerButtonMapping.ThumbLeft.Text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                rightControllerButtonMapping.ThumbRight.Text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
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

                var bounds = UtilUnity.CalculateBounds(_hoveredObject);

                if (bounds.HasValue)
                {
                    _hoverBoxGO.transform.position = bounds.Value.center;
                    _hoverBox.Size = gameObject.transform.InverseTransformVector(bounds.Value.size);
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
                var objectDefinition = EditData.GetObjectDefinition(selectedObject);
                _objectDefinitions.Remove(objectDefinition);

                _objects.Remove(selectedObject);
                UtilUnity.Destroy(selectedObject);
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

        #region Fields

        private int _activeObjectTypeIndex = 0;

        public int ActiveObjectTypeIndex
        {
            get
            {
                return _activeObjectTypeIndex;
            }
            set
            {
                if (_activeObjectTypeIndex == 2)
                {
                    m_application.PoiContainerGameObject.SetActive(false);
                }

                _activeObjectTypeIndex = value;

                if (_activeObjectTypeIndex == 2)
                {
                    m_application.PoiContainerGameObject.SetActive(true);
                }

                OnActiveObjectTypeChanged();
            }
        }

        private ApplicationArchiVR.EditData EditData => m_application.EditDatas[_activeObjectTypeIndex];

        private List<GameObject> _objects => EditData.GameObjects;

        private List<ObjectDefinition> _objectDefinitions => EditData.ObjectDefinitions;

        private ApplicationArchiVR.ObjectEditSettings _editSettings => EditData.Settings;


        private bool _addToSelection = false;

        private GameObject _hoveredObject;
        
        private GameObject _hoverBoxGO;

        private LineRendererBox _hoverBox;

        private List<GameObject> _selectedObjects = new List<GameObject>();

        private List<LineRendererBox> _selectBoxes = new List<LineRendererBox>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion