using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Omniaudio.Networking
{
    
    [DataContract]
    internal class Server
    {

        [DataMember]
        internal string Title;

        [DataMember]
        internal string Description;
        
        [DataMember]
        internal string InternetProtocol;

        [DataMember]
        internal int Port;

        [DataMember]
        internal int MaxListeners;

        internal int Id;

        internal int Ping;
        
    }

}
