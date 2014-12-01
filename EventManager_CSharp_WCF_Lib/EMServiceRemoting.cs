using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager_CSharp_WCF_Lib
{
    public class EMServiceRemoting : MarshalByRefObject
    {
        public int Modify(string id, string field, string newValue)
        {
            return EventManager.Instance._modify(id, field, newValue);
        }

        public int Drop(string ServerAddress)
        {
            return EventManager.Instance._drop(ServerAddress);
        }

        public int Register(string serverAddress)
        {
            return EventManager.Instance._register(serverAddress);
        }

        public int Add(string id, string date, string time, string duration, string header, string comment)
        {
            return EventManager.Instance._add(id, date, time, duration, header, comment);
        }

        public int Remove(string id)
        {
            return EventManager.Instance._remove(id);
        }

        public int Clear()
        {
            return EventManager.Instance._clear();
        }

        public int Say(string text)
        {
            return EventManager.Instance._say(text);
        }

        public int SetToken()
        {
            EventManager.Instance.gotToken.Release();
            return 0;
        }

        public int AddNewPeer(string newPeerAddress)
        {
            return EventManager.Instance._addNewPeer(newPeerAddress);
        }

        public int SetNext(string nextAddress)
        {
            return EventManager.Instance.setNext(nextAddress);
        }

        public string GetNext()
        {
            return EventManager.Instance.next.address;
        }

        public int SetPrev(string prevAddress)
        {
            return EventManager.Instance.setPrev(prevAddress);
        }

        public string GetPrev()
        {
            return EventManager.Instance.prev.address;
        }

        public int DropAll()
        {
            return EventManager.Instance._dropAll();
        }
    }
}
