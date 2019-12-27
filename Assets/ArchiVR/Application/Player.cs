using System;

namespace ArchiVR.Application
{
    class Player
    {
        public Guid ID
        {
            get;
        } = Guid.NewGuid();

        public string Name
        {
            get;
            private set;
        } = "Unnamed player";    
    }
}



