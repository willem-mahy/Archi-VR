using UnityEngine;
using WM.ArchiVR.Command;

namespace WM
{
    namespace ArchiVR
    {
        public class ImmersionModeMaquette : ImmersionMode
        {
            #region variables

            // The surroundings in which the maquette is previewed
            private GameObject m_maquettePreviewContext = null;

            // The maquette translational manipulation speed.
            float maquetteMoveSpeed = 1.0f;

            // The maquette rotational manipulation speed.
            float maquetteRotateSpeed = 60.0f;

            // The translational offset distance along the up vector.
            private float m_maquetteOffset = 0;

            // The rotational offset angle around the up vector.
            private float m_maquetteRotation = 0;

            // The layer currently being picked.
            private GameObject pickedLayer;

            enum MaquetteManipulationMode
            {
                None = 0,
                Translate,
                Rotate
            };

            private MaquetteManipulationMode maquetteManipulationMode = MaquetteManipulationMode.None;

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
            }

            public override void Enter()
            {
                Logger.Debug("ImmersionModeMaquette.Enter()");

                InitButtonMappingUI();

                if (m_maquettePreviewContext)
                    m_maquettePreviewContext.SetActive(true);

                // Disable moving up/down.
                m_application.m_flySpeedUpDown = 0.0f;

                // Enable only R pickray.
                m_application.RPickRay.gameObject.SetActive(true);

                maquetteManipulationMode = MaquetteManipulationMode.None;
            }

            public override void Exit()
            {
                Logger.Debug("ImmersionModeMaquette.Exit()");

                if (m_maquettePreviewContext)
                    m_maquettePreviewContext.SetActive(false);

                // Restore default moving up/down.
                m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;

                m_application.RPickRay.gameObject.SetActive(false);
            }

            public override void Update()
            {
                //Logger.Debug("ImmersionModeMaquette.Update()");

                if (m_application.ToggleActiveProject())
                {
                    return;
                }

                if (m_application.ToggleImmersionModeIfInputAndNetworkModeAllows())
                {
                    return;
                }

                // Toggle model layer visibility using picking.
                if (m_application.m_controllerInput.m_controllerState.button8Down)
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

                // Clients cannot manipulate model!
                if (m_application.NetworkMode != Net.NetworkMode.Client)
                {
                    var cs = m_application.m_controllerInput.m_controllerState;

                    float magnitudeRotateMaquette = cs.lThumbStick.x;
                    float magnitudeTranslateMaquette = cs.lThumbStick.y;

                    #region Update MaquetteManipulationMode

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

                    #endregion

                    float positionOffset = m_maquetteOffset;
                    float rotationOffset = m_maquetteRotation;

                    switch (maquetteManipulationMode)
                    {
                        case MaquetteManipulationMode.Translate:
                            {
                                positionOffset = Mathf.Clamp(m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime, -1.0f, 0.6f);
                            }
                            break;
                        case MaquetteManipulationMode.Rotate:
                            {
                                rotationOffset += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;
                            }
                            break;
                    }

                    if (maquetteManipulationMode != MaquetteManipulationMode.None)
                    {
                        var command = new SetModelLocationCommand(positionOffset, rotationOffset);

                        if (m_application.NetworkMode == Net.NetworkMode.Server)
                        {
                            m_application.Server.BroadcastCommand(command);
                        }
                        else
                        {
                            command.Execute(m_application);
                        }
                    }
                }

                #endregion

                #region Updated picked model layer

                // Clients cannot pick model layers!
                if (m_application.NetworkMode != Net.NetworkMode.Client)
                {
                    var pickRay = m_application.RPickRay.GetRay();

                    float minHitDistance = float.NaN;

                    pickedLayer = null;

                    foreach (var layer in m_application.GetModelLayers())
                    {
                        PickRecursively(
                            layer,
                            pickRay,
                            layer,
                            ref pickedLayer,
                            ref minHitDistance);
                    }

                    m_application.RPickRay.HitDistance = minHitDistance;
                }

                #endregion
            }

            public void SetModelLocation(
                float positionOffset,
                float rotationOffset)
            {
                m_maquetteOffset = positionOffset;
                m_maquetteRotation = rotationOffset;
                UpdateModelLocationAndScale();
            }

            private void PickRecursively(
                GameObject layer,
                Ray pickRay,
                GameObject gameObject,
                ref GameObject pickedLayer,
                ref float minHitDistance)
            {
                // Pick-test on self.
                var geometryCollider = gameObject.GetComponent<Collider>();

                if (geometryCollider)
                {
                    float hitDistance = float.NaN;

                    // Raycast
                    RaycastHit hitInfo = new RaycastHit();

                    if (geometryCollider.Raycast(pickRay, out hitInfo, 9000))
                    {
                        hitDistance = hitInfo.distance;
                    }

                    // Bounds-check.
                    /*
                    if (geometryCollider.bounds.IntersectRay(pickRay, out hitDistance))
                    {
                    }
                    else
                    {
                        hitDistance = float.Nan;
                    }
                    */

                    if (!float.IsNaN(hitDistance))
                    {
                        if (float.IsNaN(minHitDistance))
                        {
                            minHitDistance = hitDistance;
                            pickedLayer = layer;
                        }
                        else if (hitDistance < minHitDistance)
                        {
                            minHitDistance = hitDistance;
                            pickedLayer = layer;
                        }
                    }
                }

                // Recurse.
                foreach (Transform childTransform in gameObject.transform)
                {
                    PickRecursively(
                            layer,
                            pickRay,
                            childTransform.gameObject,
                            ref pickedLayer,
                            ref minHitDistance);
                }
            }

            public override void UpdateModelLocationAndScale()
            {
                //Logger.Debug("ImmersionModeMaquette.UpdateModelLocationAndScale()");

                var activeProject = m_application.ActiveProject;

                if (activeProject == null)
                {
                    return;
                }

                var scale = 0.04f;
                activeProject.transform.position = Vector3.zero;
                activeProject.transform.rotation = Quaternion.identity;
                activeProject.transform.localScale = scale * Vector3.one;

                // Locate around anchor.
                var modelAnchor = GameObject.Find("ModelAnchor");

                if (modelAnchor != null)
                {
                    activeProject.transform.localPosition = -scale * modelAnchor.transform.localPosition;
                }

                // Add height offset.
                var pos = activeProject.transform.position;
                pos.y = 1 + m_maquetteOffset;
                activeProject.transform.position = pos;

                activeProject.transform.RotateAround(Vector3.zero, Vector3.up, m_maquetteRotation);
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
                    m_application.leftControllerButtonMapping.textLeftHandTrigger.text = "GFX Quality";

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
    } // namespace ArchiVR
} // namespace WM