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

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SetModelLocationCommand()
        { }

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="positionOffset"></param>
        /// <param name="rotationOffset"></param>
        public SetModelLocationCommand(
            float positionOffset,
            float rotationOffset)
        {
            PositionOffset = positionOffset;
            RotationOffset = rotationOffset;
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("SetModelLocationCommand.Execute()");

            var immersionModeMaquette = application.ActiveApplicationState as ImmersionModeMaquette;

            if (null != immersionModeMaquette)
            {
                immersionModeMaquette.SetModelLocation(PositionOffset, RotationOffset);
            }
        }
    }
}
