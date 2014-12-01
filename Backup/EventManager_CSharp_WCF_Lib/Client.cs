using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManager_CSharp_WCF_Lib;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using Microsoft.Samples.XmlRpc;
using Microsoft.ServiceModel.XmlRpc;

namespace EventManager_CSharp_WCF_Lib
{
    [DataContract]
    public class Client
    {
        [DataMember]
        public IEventManager eManager;

        public Client(string serverAddress)
        {
            Uri eventManagerAddress = new Uri("http://" + serverAddress + ":80/EventManager");

            ChannelFactory<IEventManager> eventManagerFactory =
                new ChannelFactory<IEventManager>(
                    new WebHttpBinding(WebHttpSecurityMode.None),
                    new EndpointAddress(eventManagerAddress));
            eventManagerFactory.Endpoint.EndpointBehaviors.Add(new Microsoft.Samples.XmlRpc.XmlRpcEndpointBehavior());

            eManager = eventManagerFactory.CreateChannel();
        }

        public void Close()
        {
            ((IClientChannel)eManager).Close();
        }

        [DataMember]
        public static Dictionary<string, Client> clientsMap = new Dictionary<string, Client>();
    }
}
