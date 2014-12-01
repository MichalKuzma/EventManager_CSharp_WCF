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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

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
            Console.Out.WriteLine("Select communication protocol:");
            Console.Out.WriteLine("0: XML-RPC");
            Console.Out.WriteLine("1: .Net Remoting");

            string _in = Console.In.ReadLine();

            HttpChannel _httpchannel = null;

            do
            {
                if (_in == "0")
                {
                    Uri _baseAddress;
                    if (EventManager.singleMachineDebug)
                    {
                        // For testing purposes only
                        bool lookForAddress = true;
                        do
                        {
                            Uri tempAddress = new Uri("http://localhost:8000/xmlrpc" + EventManager.servNum.ToString());
                            ServiceHost tempHost = new ServiceHost(typeof(EMServiceWCF_XML_RPC), tempAddress);
                            try
                            {
                                tempHost.Open();
                                lookForAddress = false;
                                tempHost.Close();
                            }
                            catch
                            {
                                EventManager.servNum++;
                            }
                        } while (lookForAddress);
                        _baseAddress = new Uri("http://localhost:8000/xmlrpc" + EventManager.servNum.ToString());
                    }
                    else
                        _baseAddress = new Uri("http://localhost:8000/xmlrpc");
                    selfHost = new ServiceHost(typeof(EMServiceWCF_XML_RPC), _baseAddress);

                    try
                    {
                        ServiceEndpoint epXmlRpc = selfHost.AddServiceEndpoint(
                            typeof(IEMServiceWCF_XML_RPC),
                            new WebHttpBinding(WebHttpSecurityMode.None),
                            "EventManager");
                        epXmlRpc.Behaviors.Add(new XmlRpcEndpointBehavior());

                        ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                        smb.HttpGetEnabled = true;
                        selfHost.Description.Behaviors.Add(smb);
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
                else if (_in == "1")
                {
                    //Start .Net Remoting Server                    
                    _httpchannel = new HttpChannel(8080);
                    ChannelServices.RegisterChannel(_httpchannel, false);
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                    RemotingConfiguration.ApplicationName = "EventManager";
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(EMServiceRemoting), "RemotingService", WellKnownObjectMode.Singleton);
                    Console.Out.WriteLine("EventManager Service running");
                }
            }
            while (_in != "0" && _in != "1");

            EventManager.Instance.Protocol = Int32.Parse(_in);

            EventManager.Instance.Start();

            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                selfHost.Close();
            else
                if (_httpchannel != null)
                    ChannelServices.UnregisterChannel(_httpchannel);

            Console.WriteLine("EventManagerService stopped");   
        }

        public void Stop()
        {
            EventManager.Instance.gotToken.Release();
            EventManager.Instance.shouldStop = true;
            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                selfHost.Close();
        }
    }
}
