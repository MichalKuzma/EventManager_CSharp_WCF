using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using EventManager_CSharp_WCF_Lib;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using Microsoft.Samples.XmlRpc;
using Microsoft.ServiceModel.XmlRpc;

namespace EventManager_CSharp_WCF_Lib
{
    public class Listener
    {
        private static Listener instance;
        private ServiceHost selfHost;

        public static Listener Instance
        {
            get
            {
                if (instance == null)
                    instance = new Listener();
                return instance;
            }
        }

        public void Start()
        {
            //Create a URI to serve as the base address
            Uri baseAddress = new UriBuilder(Uri.UriSchemeHttp, Environment.MachineName, -1, "").Uri;

            //Create a ServiceHost instance
            selfHost = new ServiceHost(typeof(EventManager), baseAddress);


            try
            {
                var epXmlRpc = selfHost.AddServiceEndpoint(
                    typeof(IEventManager),
                    new WebHttpBinding(WebHttpSecurityMode.None),
                    new Uri(baseAddress, "./EventManager"));

                epXmlRpc.EndpointBehaviors.Add(new Microsoft.Samples.XmlRpc.XmlRpcEndpointBehavior());

                //Start the service
                selfHost.Open();

                Console.WriteLine("Service up and running at:");
                foreach (var ea in selfHost.Description.Endpoints)
                {
                    Console.WriteLine(ea.Address);
                }

            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                selfHost.Abort();
            }
        }

        public void Stop()
        {
            selfHost.Close();
        }
    }
}
