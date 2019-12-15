using ArchiVR.Application;

namespace ArchiVR
{
    public class ImmersionMode
    {
        public ApplicationArchiVR Application = null;

        //! Called once, right after construction.
        public virtual void Init() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Update() { }

        public virtual void UpdateModelLocationAndScale() { }

        public virtual void UpdateTrackingSpacePosition() { }

        public virtual void InitButtonMappingUI() { }
    }
}