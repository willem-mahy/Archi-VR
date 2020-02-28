namespace WM.Application
{
    /// <summary>
    /// Base class for application states.
    /// T is the application type, which must be a subclass of UnityApplication.
    /// </summary>
    public interface IApplicationState
    {
        /// <summary>
        /// Called once, right after construction.
        /// </summary>
        void Init();

        /// <summary>
        /// Called when the application enters the application state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when the application exits the application state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Called when the application pushes another application state on top of this application state.
        /// </summary>
        void Pause();

        /// <summary>
        /// Called when the application pops an application state and this becomes again the active application state.
        /// </summary>
        void Resume();

        /// <summary>
        /// Called every frame on the active application state.
        /// </summary>
        void Update();

        /// <summary>
        /// Called when a teleport procedure has started.
        /// </summary>
        void InitTeleport();

        /// <summary>
        /// TODO: Comment
        /// </summary>
        void UpdateModelLocationAndScale();

        /// <summary>
        /// TODO: Comment
        /// </summary>
        void UpdateTrackingSpacePosition();

        /// <summary>
        /// TODO: Comment
        /// </summary>
        void OnTeleportFadeOutComplete();

        /// <summary>
        /// TODO: Comment
        /// </summary>
        void OnTeleportFadeInComplete();
    }

    /// <summary>
    /// Base class for application states.
    /// T is the application type, which must be a subclass of UnityApplication.
    /// </summary>
    public class ApplicationState<T> : IApplicationState
    {
        /// <summary>
        /// The application.
        /// </summary>
        protected T m_application;

        public ApplicationState(T application)
        {
            m_application = application;
        }

        public virtual void Init() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Pause() { }

        public virtual void Resume() { }

        public virtual void Update() { }

        public virtual void InitTeleport() { }

        public virtual void UpdateModelLocationAndScale() { }

        public virtual void UpdateTrackingSpacePosition() { }

        public virtual void OnTeleportFadeOutComplete() { }

        public virtual void OnTeleportFadeInComplete() { }
    }
} // namespace WM.Application