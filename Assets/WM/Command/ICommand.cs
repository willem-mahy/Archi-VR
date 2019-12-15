using WM.Application;

namespace WM.Command
{
    public interface ICommand
    {
        void Execute(UnityApplication application);
    }
}