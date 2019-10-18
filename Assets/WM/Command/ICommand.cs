using WM.ArchiVR;

namespace WM.ArchiVR.Command
{
    public interface ICommand
    {
        void Execute(ApplicationArchiVR application);
    }
}