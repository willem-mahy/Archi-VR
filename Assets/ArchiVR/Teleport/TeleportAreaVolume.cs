using ArchiVR.Application;
using UnityEngine;
using WM;

namespace ArchiVR
{
    public class TeleportAreaVolume : MonoBehaviour
    {
        private ApplicationArchiVR _applicationArchiVR;

        public bool AllPlayersPresent
        {
            get;
            set;
        } = false;

        private void Start()
        {
            _applicationArchiVR = UtilUnity.FindApplication<ApplicationArchiVR>(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            _applicationArchiVR.m_leftControllerText.text = "OnTriggerEnter";
            AllPlayersPresent = true;
        }

        private void OnTriggerExit(Collider other)
        {
            _applicationArchiVR.m_leftControllerText.text = "OnTriggerExit";
            AllPlayersPresent = false;
        }

        private void OnTriggerStay(Collider other)
        {
            _applicationArchiVR.m_leftControllerText.text = "OnTriggerStay";
        }
    }
}