using ArchiVR.Application;
using System;
using System.Xml.Serialization;
using UnityEngine;
using WM.Application;
using WM.Command;

namespace ArchiVR.Command
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

        public void Execute(UnityApplication application)
        {
            Debug.Log("SetModelLocationCommand.Execute()");

            var immersionModeMaquette = ((ApplicationArchiVR)application).ImmersionModeMaquette;

            immersionModeMaquette.SetModelLocation(PositionOffset, RotationOffset);
        }
    }
}
