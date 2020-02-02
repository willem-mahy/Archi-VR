using System;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class ToggleMenuModeCommand : ICommand
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ToggleMenuModeCommand()
        { }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("ToggleMenuModeCommand.Execute()");

            application.ToggleMenuMode();
        }
    }
}
