using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using EventManager_CSharp_WCF_Lib;
using System.ServiceModel.Description;
 
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
            Uri baseAddress = new Uri("http://localhost:8080/");

            //Create a ServiceHost instance
            selfHost = new ServiceHost(typeof(EventManager), baseAddress);

            try
            {
                //Add service endpoint
                selfHost.AddServiceEndpoint(typeof(IEventManager), new WSHttpBinding(), "EventManager");

                //Enable metadata exchange
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);

                //Start the service
                selfHost.Open();
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
