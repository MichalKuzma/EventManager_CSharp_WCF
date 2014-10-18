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
        [OperationContract]
        void _modify(string id, string field, string newValue);

        [OperationContract]
        void _drop(string ServerAddress);

        [OperationContract]
        Client _register(string serverAddress);

        [OperationContract]
        void _add(DateTime datetime, int duration, string header, string comment);

        [OperationContract]
        void _remove(string id);

        [OperationContract]
        void _clear();
    }
}
