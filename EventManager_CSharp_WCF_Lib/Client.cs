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
        public IEMServiceWCF_XML_RPC eManagerWCF_XML_RPC;

        public EMServiceRemoting eManagerREMOTING;

        public string address;

        public Client(string serverAddress)
        {
            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
            {
                address = serverAddress;
                Uri eventManagerAddress;
                if (EventManager.singleMachineDebug)
                    //For test on a single machine
                    eventManagerAddress = new Uri("http://" + serverAddress + "/EventManager");
                else
                    eventManagerAddress = new Uri("http://" + serverAddress + ":8000/xmlrpc/EventManager");

                ChannelFactory<IEMServiceWCF_XML_RPC> eventManagerFactory =
                    new ChannelFactory<IEMServiceWCF_XML_RPC>(
                        new WebHttpBinding(WebHttpSecurityMode.None),
                        new EndpointAddress(eventManagerAddress));
                eventManagerFactory.Endpoint.EndpointBehaviors.Add(new Microsoft.Samples.XmlRpc.XmlRpcEndpointBehavior());

                eManagerWCF_XML_RPC = eventManagerFactory.CreateChannel();
            }
            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
            {
                eManagerREMOTING = (EMServiceRemoting)Activator.GetObject(typeof(EMServiceRemoting), "http://" + serverAddress + ":8080/EventManager/RemotingService");
            }
        }

        public string makeFunction (string funcName, string[] args)
        {
            bool tryAgain = true;
            int failCounter = 0;
            while (tryAgain)
            {
                tryAgain = false;
                try
                {
                    switch (funcName)
                    {
                        case "Modify":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Modify(args[0], args[1], args[2]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Modify(args[0], args[1], args[2]).ToString();
                            break;
                        case "Drop":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Drop(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Drop(args[0]).ToString();
                            break;
                        case "Register":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Register(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Register(args[0]).ToString();
                            break;
                        case "Add":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Add(args[0], args[1], args[2], args[3], args[4], args[5]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Add(args[0], args[1], args[2], args[3], args[4], args[5]).ToString();
                            break;
                        case "Remove":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Remove(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Remove(args[0]).ToString();
                            break;
                        case "Clear":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Clear().ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Clear().ToString();
                            break;
                        case "Say":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.Say(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.Say(args[0]).ToString();
                            break;
                        case "SetToken":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.SetToken().ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.SetToken().ToString();
                            break;
                        case "AddNewPeer":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.AddNewPeer(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.AddNewPeer(args[0]).ToString();
                            break;
                        case "SetNext":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.SetNext(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.SetNext(args[0]).ToString();
                            break;
                        case "GetNext":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.GetNext();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.GetNext();
                            break;
                        case "SetPrev":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.SetPrev(args[0]).ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.SetPrev(args[0]).ToString();
                            break;
                        case "GetPrev":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.GetPrev();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.GetPrev();
                            break;
                        case "DropAll":
                            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                                return eManagerWCF_XML_RPC.DropAll().ToString();
                            else if (EventManager.Instance.Protocol == EventManager.REMOTING)
                                return eManagerREMOTING.DropAll().ToString();
                            break;
                        default:
                            Console.Out.WriteLine("Wrong function name");
                            break;
                    }
                    return "";
                }
                catch
                {
                    if (failCounter < 3)
                    {
                        failCounter++;
                        Console.Out.WriteLine("Error occured while performing " + funcName + " on " + address + " for " + failCounter.ToString() + " time.");
                        tryAgain = true;
                    }
                    else
                    {
                        Console.Out.WriteLine("Dead peer found");
                        return "Dead peer found";
                    }
                }
            }
            return "";
        }

        public void Close()
        {
            if (EventManager.Instance.Protocol == EventManager.XML_RPC)
                ((IClientChannel)eManagerWCF_XML_RPC).Close();
        }

        [DataMember]
        public static Dictionary<string, Client> clientsMap = new Dictionary<string, Client>();
    }
}
