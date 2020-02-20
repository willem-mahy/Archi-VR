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

        /// <summary>
        /// Called when a teleport procedure has started.
        /// </summary>
        public virtual void InitTeleport() { }

        public virtual void UpdateModelLocationAndScale() { }

        public virtual void UpdateTrackingSpacePosition() { }

        public virtual void InitButtonMappingUI() { }
    }
}