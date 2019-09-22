namespace ArchiVR
{
    public class ApplicationState
    {
        public ApplicationArchiVR m_application = null;

        //! Called once, right after construction.
        public virtual void Init() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Update() { }

        public virtual void UpdateModelLocationAndScale() { }

        public virtual void UpdateTrackingSpacePosition() { }

        public virtual void OnTeleportFadeOutComplete()
        {
        }

        public virtual void OnTeleportFadeInComplete()
        {
        }
    }
}