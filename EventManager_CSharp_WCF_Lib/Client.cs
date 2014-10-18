using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManager_CSharp_WCF_Lib;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace EventManager_CSharp_WCF_Lib
{
    [DataContract]
    public class Client
    {
        [DataMember]
        public IEventManager eManager;

        public Client(string serverAddress)
        {
            eManager = ChannelFactory<IEventManager>.CreateChannel(
                new WSHttpBinding(),
                new EndpointAddress(
                    "http://" + serverAddress + ":8080/EventManager"
                ));
        }

        [DataMember]
        public static Dictionary<string, Client> clientsMap = new Dictionary<string, Client>();
    }
}
