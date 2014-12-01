using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace EventManager_CSharp_WCF_Lib
{
    [ServiceContract]
    public interface IEventManager
    {
        [OperationContract (Action="EventManager._modify")]
        int _modify(string id, string field, string newValue);

        [OperationContract(Action = "EventManager._drop")]
        int _drop(string ServerAddress);

        [OperationContract(Action = "EventManager._register")]
        int _register(string serverAddress);

        [OperationContract(Action = "EventManager._add")]
        int _add(string id, string date, string time, string duration, string header, string comment);

        [OperationContract(Action = "EventManager._remove")]
        int _remove(string id);

        [OperationContract(Action = "EventManager._clear")]
        int _clear();

        [OperationContract(Action = "EventManager._say")]
        int _say(string text);
    }
}
