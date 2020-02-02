using System;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class SetMenuModeCommand : ICommand
    {
        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        public SetMenuModeCommand(UnityApplication.MenuMode mode)
        {
            _mode = mode;
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("SetMenuModeCommand.Execute()");

            application.SetMenuMode(_mode);
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly UnityApplication.MenuMode _mode;
    }
}
