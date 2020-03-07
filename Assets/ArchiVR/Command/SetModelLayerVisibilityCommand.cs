using ArchiVR.Application;
using System;
using System.Xml.Serialization;
using UnityEngine;
using WM.Application;
using WM.Command;

namespace ArchiVR.Command
{
    [Serializable]
    public class SetModelLayerVisibilityCommand : ICommand
    {
        #region Fields

        /// <summary>
        /// The index of the Model layer to hide/unhide.
        /// </summary>
        public int ModelLayerIndex { get; }

        /// <summary>
        /// The visibility state to apply to the model layer.
        /// </summary>
        public bool Visible { get; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="positionOffset"></param>
        /// <param name="rotationOffset"></param>
        public SetModelLayerVisibilityCommand(
            int modelLayerIndex,
            bool visible)
        {
            ModelLayerIndex = modelLayerIndex;
            Visible = visible;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("SetModelLayerVisibilityCommand.Execute()");

            var applicationArchiVR = application as ApplicationArchiVR;

            //var immersionModeMaquette = applicationArchiVR.ImmersionModeMaquette;

            //immersionModeMaquette.SetModelLayerVisibility(ModelLayerIndex, Visible);

            var layer = applicationArchiVR.GetLayers()[ModelLayerIndex];

            layer.SetActive(Visible);
        }
    }
}
