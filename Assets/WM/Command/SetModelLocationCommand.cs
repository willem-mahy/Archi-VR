using System;
using System.Xml.Serialization;
using UnityEngine;

using WM.ArchiVR;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("SetModelLocationCommand")]
    public class SetModelLocationCommand : ICommand
    {
        [XmlElement("PositionOffset")]
        public float PositionOffset { get; set; }

        [XmlElement("RotationOffset")]
        public float RotationOffset { get; set; }

        public SetModelLocationCommand()
        { }

        public SetModelLocationCommand(
            float positionOffset,
            float rotationOffset)
        {
            PositionOffset = positionOffset;
            RotationOffset = rotationOffset;
        }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("SetModelLocationCommand.Execute()");

            var immersionModeMaquette = application.ImmersionModeMaquette;

            immersionModeMaquette.SetModelLocation(PositionOffset, RotationOffset);
        }
    }
}
