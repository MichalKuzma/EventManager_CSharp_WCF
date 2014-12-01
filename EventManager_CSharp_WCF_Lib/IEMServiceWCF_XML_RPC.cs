using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace EventManager_CSharp_WCF_Lib
{
    [ServiceContract]
    public interface IEMServiceWCF_XML_RPC
    {
        [OperationContract (Action="EventManager.Modify")]
        int Modify(string id, string field, string newValue);

        [OperationContract(Action = "EventManager.Drop")]
        int Drop(string ServerAddress);

        [OperationContract(Action = "EventManager.Register")]
        int Register(string serverAddress);

        [OperationContract(Action = "EventManager.Add")]
        int Add(string id, string date, string time, string duration, string header, string comment);

        [OperationContract(Action = "EventManager.Remove")]
        int Remove(string id);

        [OperationContract(Action = "EventManager.Clear")]
        int Clear();

        [OperationContract(Action = "EventManager.Say")]
        int Say(string text);

        [OperationContract(Action = "EventManager.SetToken")]
        int SetToken();

        [OperationContract(Action = "EventManager.AddNewPeer")]
        int AddNewPeer(string newPeerAddress);

        [OperationContract(Action = "EventManager.SetNext")]
        int SetNext(string nextAddress);

        [OperationContract(Action = "EventManager.GetNext")]
        string GetNext();

        [OperationContract(Action = "EventManager.SetPrev")]
        int SetPrev(string prevAddress);

        [OperationContract(Action = "EventManager.GetPrev")]
        string GetPrev();

        [OperationContract(Action = "EventManager.DropAll")]
        int DropAll();
    }
}
