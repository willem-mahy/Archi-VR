﻿using UnityEngine;

namespace ArchiVR
{
    public class ImmersionModeMaquette : ImmersionMode
    {
        #region variables

        // The surroundings in which the maquette is previewed
        GameObject m_maquettePreviewContext = null;

        // The translational (up) distance.
        float m_maquetteOffset = 0;

        // The maquette rotational offset.
        float m_maquetteRotation = 0;

        // Represents pick hit position.
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Represents pick ray.
        GameObject pickRayGO = null;

        // The layer currently being picked.
        GameObject pickedLayer;

        enum MaquetteManipulationMode
        {
            None = 0,
            Translate,
            Rotate
        };

        private MaquetteManipulationMode maquetteManipulationMode
        {
            get;
            set;
        } = MaquetteManipulationMode.None;

        #endregion

        public override void Init()
        {
            Logger.Debug("ImmersionModeMaquette.Init()");

            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = GameObject.Find("MaquettePreviewContext");                
            }

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(false);

            sphere.transform.parent = m_maquettePreviewContext.transform;
            sphere.transform.localScale = 0.025f * Vector3.one;
            sphere.SetActive(false);

            pickRayGO = GameObject.Find("PickRay");
            pickRayGO.SetActive(false);
        }

        public override void Enter()
        {
            Logger.Debug("ImmersionModeMaquette.Enter()");

            InitButtonMappingUI();

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            m_application.m_flySpeedUpDown = 0.0f;

            pickRayGO.SetActive(true);

            maquetteManipulationMode = MaquetteManipulationMode.None;
        }

        public override void Exit()
        {
            Logger.Debug("ImmersionModeMaquette.Exit()");

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(false);

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;

            sphere.SetActive(false);
            pickRayGO.SetActive(false);
        }

        public override void Update()
        {
            //Logger.Debug("ImmersionModeMaquette.Update()");

            if (m_application.ToggleActiveProject())
            {
                return;
            }

            if (m_application.ToggleImmersionMode2())
            {
                return;
            }

            // Toggle model layer visibility using picking.
            if (m_application.m_controllerInput.m_controllerState.button5Down)
            {
                if (pickedLayer != null)
                {
                    pickedLayer.SetActive(!pickedLayer.activeSelf);
                }
                else
                {
                    m_application.UnhideAllModelLayers();
                }
            }

            // Show name of picked model layer in right control text.
            m_application.m_rightControllerText.text = (pickedLayer == null) ? "" : pickedLayer.name;

            m_application.Fly();

            #region Maquette manipulation.

            var cs = m_application.m_controllerInput.m_controllerState;

            float magnitudeRotateMaquette = cs.lThumbStick.x;
            float magnitudeTranslateMaquette = cs.lThumbStick.y;

            // Update maquette manipulationMode
            bool manipulating = (Mathf.Abs(magnitudeRotateMaquette) > 0.1f) || (Mathf.Abs(magnitudeTranslateMaquette) > 0.1f);
            if (maquetteManipulationMode == MaquetteManipulationMode.None)
            {
                if (manipulating)
                {
                    maquetteManipulationMode = (Mathf.Abs(magnitudeRotateMaquette) > Mathf.Abs(magnitudeTranslateMaquette))
                        ? MaquetteManipulationMode.Rotate
                        : MaquetteManipulationMode.Translate;
                }
                else
                    maquetteManipulationMode = MaquetteManipulationMode.None;
            }
            else
            {
                if (!manipulating)
                    maquetteManipulationMode = MaquetteManipulationMode.None;
            }

            if (maquetteManipulationMode == MaquetteManipulationMode.Translate)
            {
                // Translate Up/Down
                var maquetteMoveSpeed = 1.0f;

                m_maquetteOffset = Mathf.Min(1.0f, m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime);
            }

            if (maquetteManipulationMode == MaquetteManipulationMode.Rotate)
            {
                // Rotate around 'up' vector.
                var maquetteRotateSpeed = 60.0f;

                m_maquetteRotation += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;
            }
            UpdateModelLocationAndScale();

            #endregion

            Ray pickRay = new Ray(
                m_application.m_leftHandAnchor.transform.position,
                m_application.m_leftHandAnchor.transform.forward);
            
            float minHitDistance = -1;

            pickedLayer = null;

            foreach (var layer in m_application.m_layers)
            {
                foreach (Transform geometryTransform in layer.transform)
                {
                    var geometryCollider = geometryTransform.GetComponent<Collider>();

                    if (geometryCollider)
                    {
                        float hitDistance = -1;

                        if (geometryCollider.bounds.IntersectRay(pickRay, out hitDistance))
                        {
                            if (minHitDistance == -1)
                            {
                                minHitDistance = hitDistance;
                                pickedLayer = layer;
                            }
                            else if (hitDistance < minHitDistance)
                            {
                                minHitDistance = hitDistance;
                                pickedLayer = layer;
                            }

                            sphere.transform.position =
                                m_application.m_leftHandAnchor.transform.position
                                + hitDistance * m_application.m_leftHandAnchor.transform.forward;                                                          
                        }
                    }
                }
            }

            sphere.SetActive(minHitDistance >= 0);

            Debug.DrawRay(
                pickRay.origin,
                pickRay.direction * System.Math.Max(200, minHitDistance),
                Color.white,
                0.0f, // duration
                true); // depthTest
        }

        public override void UpdateModelLocationAndScale()
        {
            Logger.Debug("ImmersionModeMaquette.UpdateModelLocationAndScale()");

            var activeProject = m_application.ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            var position = Vector3.zero;
            position.y = 1 + m_maquetteOffset;

            var rotation = Quaternion.AngleAxis(m_maquetteRotation, Vector3.up);

            var scale = 0.04f * Vector3.one;

            activeProject.transform.position = position;
            activeProject.transform.rotation = rotation;
            activeProject.transform.localScale = scale;
        }

        public override void UpdateTrackingSpacePosition()
        {
            Logger.Debug("ImmersionModeMaquette.UpdateTrackingSpacePosition()");

            m_application.ResetTrackingSpacePosition(); // Center around model.

            if (Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        public override void InitButtonMappingUI()
        {
            Logger.Debug("ImmersionModeMaquette.InitButtonMappingUI()");

            var isEditor = Application.isEditor;
            
            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.textLeftHandTrigger.text = "";

                m_application.leftControllerButtonMapping.textLeftIndexTrigger.text = "Verander schaal" + (isEditor ? " (?)" : "");

                m_application.leftControllerButtonMapping.textButtonStart.text = "Toggle menu" + (isEditor ? " (F11)" : "");

                m_application.leftControllerButtonMapping.textButtonX.text = "Vorig project" + (isEditor ? " (F1)" : "");
                m_application.leftControllerButtonMapping.textButtonY.text = "Volgend project" + (isEditor ? " (F2)" : "");

                m_application.leftControllerButtonMapping.textLeftThumbUp.text = "Model omhoog" + (isEditor ? " (Z)" : "");
                m_application.leftControllerButtonMapping.textLeftThumbDown.text = "Model omlaag" + (isEditor ? " (S)" : "");
                m_application.leftControllerButtonMapping.textLeftThumbLeft.text = "Model links" + (isEditor ? " (Q)" : "");
                m_application.leftControllerButtonMapping.textLeftThumbRight.text = "Model rechts" + (isEditor ? " (D)" : "");
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.textRightIndexTrigger.text = "";
                m_application.rightControllerButtonMapping.textRightHandTrigger.text = "";

                m_application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                m_application.rightControllerButtonMapping.textButtonA.text = "";
                m_application.rightControllerButtonMapping.textButtonB.text = "";

                m_application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit" + (isEditor ? "(ArrowUp)" : "");
                m_application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                m_application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                m_application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
            }
        }
    }
}