namespace WM.Application
{
    /// <summary>
    /// Base class for application states.
    /// </summary>
    public class ApplicationState
    {
        /// <summary>
        /// The application.
        /// </summary>
        public UnityApplication m_application = null;

        /// <summary>
        /// Called once, right after construction.
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// Called when the application enters the application state.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Called when the application exits the application state.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Called every frame while the application is in the application state.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public virtual void UpdateModelLocationAndScale() { }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public virtual void UpdateTrackingSpacePosition() { }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public virtual void OnTeleportFadeOutComplete()
        {
        }

        /// <summary>
        /// TODO: Comment
        /// </summary>
        public virtual void OnTeleportFadeInComplete()
        {
        }
    }
} // namespace WM.Application