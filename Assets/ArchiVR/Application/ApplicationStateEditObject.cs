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
    public class ApplicationStateEditObject<D>
        : ApplicationState<ApplicationArchiVR>
         where D : new()
    {
        public ApplicationStateEditObject(
            ApplicationArchiVR application,
            string objectTypeName,
            ref List<GameObject> objects,
            ref List<D> objectDefinitions,
            ApplicationArchiVR.ObjectEditSettings editSettings) : base(application)
        {
            _objectTypeName = objectTypeName;
            _objects = objects;
            _objectDefinitions = objectDefinitions;
            _editSettings = editSettings;
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
            m_application.LocomotionEnabled = true;

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
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Resume"/> implementation.
        /// </summary>
        public override void Resume()
        {
            if (null == _hoverBoxGO)
            {
                _hoverBoxGO = new GameObject("LightEditHoverBox");
                _hoverBox = _hoverBoxGO.AddComponent<LineRendererBox>();
                _hoverBox.Color = m_application.HoverColor;
            }

            m_application.LocomotionEnabled = false;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            m_application.HudInfoText.text = "Edit " + _objectTypeName;
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
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Update"/> implementation.
        /// </summary>
        public override void Update()
        {
            UpdateControllerUI();

            var controllerState = m_application.m_controllerInput.m_controllerState;
                        
            // Delete selected lights.
            if (controllerState.bButtonDown)
            {
                Delete();
            }

            // Pressing 'C' on the keyboard is a shortcut for starting creation of an object.
            if (controllerState.aButtonDown || Input.GetKeyDown(KeyCode.C))
            {
                var applicationState = new ApplicationStateDefineObject<D>(
                    m_application,
                    _objectTypeName,
                    ref _objects,
                    ref _objectDefinitions,
                    _editSettings);

                m_application.PushApplicationState(applicationState);
                return;
            }

            // While hand trigger is pressed, selection is additive.
            _addToSelection = controllerState.rHandTriggerPressed;

            // Pressing 'BackSpace' on the keyboard is a shortcut for returning to the default state.
            var returnToDefaultState = controllerState.lIndexTriggerDown || Input.GetKeyDown(KeyCode.Backspace);

            if (returnToDefaultState)
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
                    return "Selected: " + _selectedObjects.Count + " " + _objectTypeName + "s.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateEditObject.UpdateControllerButtonUI()");

            var isEditor = UnityEngine.Application.isEditor;

            m_application.m_leftControllerText.text = GetSelectionText();
            m_application.m_rightControllerText.text = (null == _hoveredObject) ? "" : _hoveredObject.name;

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
                leftControllerButtonMapping.ThumbLeft.Text = "";
                leftControllerButtonMapping.ThumbRight.Text = "";
            }

            // Right controller
            var rightControllerButtonMapping = m_application.rightControllerButtonMapping;

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.IndexTrigger.Text = "Select";
                rightControllerButtonMapping.HandTrigger.Text = _selectedObjects.Count == 0 ? "" : "Additive selection";

                rightControllerButtonMapping.ButtonOculusStart.Text = "";

                rightControllerButtonMapping.ButtonA.Text = "";
                rightControllerButtonMapping.ButtonB.Text = _selectedObjects.Count == 0 ? "" : "Delete";

                rightControllerButtonMapping.ThumbUp.Text = "";
                rightControllerButtonMapping.ThumbDown.Text = "";
                rightControllerButtonMapping.ThumbLeft.Text = "";
                rightControllerButtonMapping.ThumbRight.Text = "";
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
        private void Delete()
        {
            var selectedObjects = new List<GameObject>(_selectedObjects);

            OnHover(null);
            OnSelect(null);

            foreach (var selectedObject in selectedObjects)
            {
                var objectDefinition = GetObjectDefinition(selectedObject);
                _objectDefinitions.Remove(objectDefinition);

                _objects.Remove(selectedObject);
                UtilUnity.Destroy(selectedObject);
            }   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private D GetObjectDefinition(GameObject gameObject)
        {
            foreach (var objectDefinition in _objectDefinitions)
            {
                var od = objectDefinition as ObjectDefinition;

                if (null != od)
                {
                    if (od.GameObject == gameObject)
                    {
                        return objectDefinition;
                    }
                }
            }

            return default(D);
        }

        #region Fields

        private string _objectTypeName;

        private List<GameObject> _objects;

        private List<D> _objectDefinitions;

        private ApplicationArchiVR.ObjectEditSettings _editSettings;

        private bool _addToSelection = false;

        private GameObject _hoveredObject;
        
        private GameObject _hoverBoxGO;
        private LineRendererBox _hoverBox;

        private List<GameObject> _selectedObjects = new List<GameObject>();

        private List<LineRendererBox> _selectBoxes = new List<LineRendererBox>();

        #endregion Fields
    }
} // namespace ArchiVR.Applicattion