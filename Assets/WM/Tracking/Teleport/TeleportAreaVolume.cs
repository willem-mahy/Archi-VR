using TMPro;
using UnityEngine;
using WM.Application;

namespace WM
{
    public class TeleportAreaVolume : MonoBehaviour
    {
        private UnityApplication _application;

        private Material _volumeMaterial;

        private TextMeshPro _text;

        public bool AllPlayersPresent
        {
            get;
            set;
        } = false;

        private void Start()
        {
            _application = UtilUnity.FindApplication<UnityApplication>(gameObject);

            _text = gameObject.transform.parent.Find("Text").gameObject.GetComponent<TextMeshPro>();

            _volumeMaterial = GetComponent<Renderer>().material;
        }

        private void OnTriggerEnter(Collider other)
        {
            _application.m_leftControllerText.text = "OnTriggerEnter";
            AllPlayersPresent = true;
        }

        private void OnTriggerExit(Collider other)
        {
            _application.m_leftControllerText.text = "OnTriggerExit";
            AllPlayersPresent = false;
        }

        private void OnTriggerStay(Collider other)
        {
            _application.m_leftControllerText.text = "OnTriggerStay";
        }

        private void Update()
        {
            var myPos = gameObject.transform.position;
            var headPos = _application.m_centerEyeAnchor.transform.position;

            // Project headPos on the same horizontal plane as myPos.
            headPos.y = myPos.y;

            var distance = (headPos - myPos).magnitude;

            var fade = 1.0f;

            if (distance < 1.0f)
            {
                fade = 0.0f;
            }
            if (distance < 2.0f)
            {
                fade = (distance - 1.0f);
            }

            var volumeColor = _volumeMaterial.color;
            volumeColor.a = fade * 0.5f;
            _volumeMaterial.color = volumeColor;

            var textColor = _text.color;
            textColor.a = fade;
            _text.color = textColor;
        }
    }
}