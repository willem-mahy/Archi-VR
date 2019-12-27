using WM.Application;

namespace WM.Command
{
    public interface ICommand
    {
        /// <summary>
        /// Executes the command on the given application instance.
        /// </summary>
        /// <param name="application"></param>
        void Execute(UnityApplication application);
    }
}