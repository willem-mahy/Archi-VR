using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WM.Application;
using WM.Unity;

namespace WM
{
    public class TeleportAreaVolume : MonoBehaviour
    {
        private UnityApplication _application;

        private Material _volumeMaterial;

        private TextMeshPro _text;

        public HashSet<Guid> Players = new HashSet<Guid>();

        public bool AllPlayersPresent
        {
            get
            {
                return Players.Count == Math.Max(_application.Players.Count, 1); // WM:TODO: investigate: in standalone, Players is empty?!?
            }
        }

        private void Start()
        {
            _application = UtilUnity.FindApplication<UnityApplication>(gameObject);

            _text = gameObject.transform.parent.Find("Text").gameObject.GetComponent<TextMeshPro>();

            _volumeMaterial = GetComponent<Renderer>().material;
        }

        private void OnTriggerEnter(Collider collider)
        {
            _application.Logger.Debug("TeleportAreaVolume.OnTriggerEnter");

            var playerHeadCollider = collider.gameObject.GetComponent<PlayerHeadCollider>();

            if (null != playerHeadCollider)
            {
                Players.Add(playerHeadCollider.PlayerID);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            _application.Logger.Debug("TeleportAreaVolume.OnTriggerExit");

            var playerHeadCollider = collider.gameObject.GetComponent<PlayerHeadCollider>();

            if (null != playerHeadCollider)
            {
                Players.Remove(playerHeadCollider.PlayerID);
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            //_application.Logger.Debug("TeleportAreaVolume.OnTriggerStay");

            var playerHeadCollider = collider.gameObject.GetComponent<PlayerHeadCollider>();

            if (null != playerHeadCollider)
            {
                Players.Add(playerHeadCollider.PlayerID);
            }
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