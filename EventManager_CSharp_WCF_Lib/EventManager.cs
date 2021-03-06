﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EventManager_CSharp_WCF_Lib
{
    public class EventManager
    {
        //For testing on single machine
        public static int servNum = 1;

        private static EventManager instance;

        public int Protocol;
        public const int XML_RPC = 0;
        public const int REMOTING = 1;

        public Semaphore token;
        public bool waitsForToken = false;

        public Client next = null, prev = null;

        public Semaphore gotToken;
        public Thread tokenAdmin;
        public bool shouldStop = false;

        public static bool singleMachineDebug = false;

        /// <summary>
        /// Checks if possible to add given event. Returns true, if no existing event overlaps the new one.
        /// </summary>
        /// <param name="_date">Date of the new event</param>
        /// <param name="_duration">Duration of the new event</param>
        /// <param name="exclude">Optional parameter. If set, method skips check of the event with given id. Used when checking modification.</param>
        /// <returns>Bool determing if new event is valid (doesn't overlap existing ones)</returns>
        private bool confirmTime (DateTime _date, int _duration, string exclude = "")
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            foreach (FileInfo fi in di.GetFiles())
            {
                if (exclude != "" && fi.Name == exclude + ".event")
                    continue;
                StreamReader _sr = new StreamReader(File.Open(fi.FullName, FileMode.Open, FileAccess.Read));
                string _s = _sr.ReadLine();
                _sr.Close();
                string[] _tokens = _s.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string[] _tempDate = _tokens[1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string[] _tempTime = _tokens[2].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int dur = Int32.Parse(_tokens[3]);
                DateTime date = new DateTime(Int32.Parse(_tempDate[2]), Int32.Parse(_tempDate[1]), Int32.Parse(_tempDate[0]), Int32.Parse(_tempTime[0]), Int32.Parse(_tempTime[1]), 0);
                if ((date.CompareTo(_date) <= 0 && date.Add(new TimeSpan(0, dur, 0)).CompareTo(_date) >= 0) || (date.CompareTo(_date.Add(new TimeSpan(0, _duration, 0))) <= 0 && date.Add(new TimeSpan(0, dur, 0)).CompareTo(_date.Add(new TimeSpan(0, _duration, 0))) >= 0))
                    return false;
                if ((_date.CompareTo(date) <= 0 && _date.Add(new TimeSpan(0, _duration, 0)).CompareTo(date) >= 0) || (_date.CompareTo(date.Add(new TimeSpan(0, dur, 0))) <= 0 && _date.Add(new TimeSpan(0, _duration, 0)).CompareTo(date.Add(new TimeSpan(0, dur, 0))) >= 0))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// No-parameter constructor. Sets the semaphores and starts "Semaphore administrator" thread.
        /// </summary>
        private EventManager()
        {
            token = new Semaphore(1, 1);
            gotToken = new Semaphore(0, 1);
            tokenAdmin = new Thread(new ThreadStart(administrateToken));
            tokenAdmin.Start();
        }
        /// <summary>
        /// Sets next peer in the ring
        /// </summary>
        /// <param name="nextAddress">Address of the next peer to add</param>
        /// <returns>0 if no error occured</returns>
        public int setNext(string nextAddress)
        {
            if (nextAddress != "")
                next = new Client(nextAddress);
            else
                next = null;
            return 0;
        }

        /// <summary>
        /// Sets previous peer in the ring
        /// </summary>
        /// <param name="prevAddress">Address of the previous peer to add</param>
        /// <returns>0 if no error occured</returns>
        public int setPrev(string prevAddress)
        {
            if (prevAddress != "")
                prev = new Client(prevAddress);
            else
                prev = null;
            return 0;
        }

        /// <summary>
        /// Used to set the token on the current peer
        /// </summary>
        public void setToken()
        {
            if (waitsForToken)
                token.Release();
            else if (next != null)
                next.makeFunction("SetToken", null);
            else
                token.Release();
        }

        /// <summary>
        /// Forwards the token to next peer
        /// </summary>
        private void forwardToken()
        {
            if (next != null)
                next.makeFunction("SetToken", null);
            else
                token.Release();
        }

        /// <summary>
        /// Method run in separate thread used to administrate the token. It tecides what to do once the peer gets the token.
        /// </summary>
        private void administrateToken()
        {
            while (!shouldStop)
            {
                gotToken.WaitOne();
                if (!shouldStop)
                    setToken();
            }
        }

        /// <summary>
        /// Singleton pattern instantiation of the EventManager object.
        /// Returns the unique instance of the EventManager object.
        /// </summary>
        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new EventManager();
                return instance;
            }
        }

        /// <summary>
        /// Method waits for input and takes actions according to that input.
        /// </summary>
        public void Start()
        {
            Console.WriteLine("EventManager started");
            string _input = "";
            while (_input != "quit")
            {
                Console.Write(">");
                _input = Console.ReadLine();
                if (_input == "le")
                {
                    le();
                }
                else if (_input.StartsWith("add"))
                {
                    add(_input);
                }
                else if (_input.StartsWith("remove"))
                {
                    remove(_input);
                }
                else if (_input == "clear")
                {
                    clear();
                }
                else if (_input == "help")
                {
                    help();
                }
                else if (_input.StartsWith("register"))
                {
                    register(_input);
                }
                else if (_input.StartsWith("drop"))
                {
                    drop(_input);
                }
                else if (_input.StartsWith("modify"))
                {
                    modify(_input);
                }
                else if (_input == "quit")
                {
                    Console.Out.WriteLine("EventManager is terminating...");
                }
                else if (_input.StartsWith("say"))
                {
                    letThemTalk(_input);
                }
                else if (_input == "listPeers")
                    listPeers();
                else
                {
                    Console.Out.WriteLine("Illegal command: " + _input);
                }
            }

            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            if (next != null)
            {
                if (next.address == prev.address)
                {
                    prev.makeFunction("SetPrev", new string[] { "" });
                    prev.makeFunction("SetNext", new string[] { "" });
                }
                else
                {
                    prev.makeFunction("SetNext", new string[] { next.address });
                    next.makeFunction("SetPrev", new string[] { prev.address });
                }
            }
            foreach (Client c in Client.clientsMap.Values)
            {
                if (EventManager.singleMachineDebug)
                    //For debugging on a single machine
                    c.makeFunction("Drop", new string[] { getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() });
                else
                    c.makeFunction("Drop", new string[] { getLocalIP() });
            }
            forwardToken();
            Listener.Instance.Stop();
        }

        public int _modify(string id, string field, string newValue)
        {
            StreamReader _sr = new StreamReader(File.Open(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + id + ".event"), FileMode.Open, FileAccess.Read));
            string _s = _sr.ReadLine();
            _sr.Close();
            string[] _fields = _s.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            DateTime _datetime;
            switch (field)
            {
                case "date":
                    string[] _date = newValue.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    _datetime = new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), 0, 0, 0);
                    _fields[1] = _datetime.Day + "." + _datetime.Month + "." + _datetime.Year;
                    break;
                case "time":
                    string[] _time = newValue.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    _datetime = new DateTime(1, 1, 1, Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
                    _fields[2] = _datetime.ToShortTimeString();
                    break;
                case "duration":
                    Int32.Parse(_fields[3]);
                    _fields[3] = newValue;
                    break;
                case "header":
                    _fields[4] = newValue;
                    break;
                case "comment":
                    _fields[5] = newValue;
                    break;
                default:
                    Console.WriteLine("Wrong field name given");
                    return 1;
            }
            _s = String.Join("\t", _fields);
            StreamWriter _sw = new StreamWriter(File.Open(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + id + ".event"), FileMode.Create, FileAccess.Write));
            _sw.WriteLine(_s);
            _sw.Close();
            return 0;
        }

        /// <summary>
        /// The method modifies the given event locally as well as remote
        /// </summary>
        /// <param name="_input">The input string</param>
        private void modify(string _input)
        {
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 4)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("modify [id] [field] [newValue] --> Modify the event with the given id and set the value of the specified field to [newValue].\r\n");
                return;
            }
            string _id = _tokens[1];
            string _field = _tokens[2];
            string _newValue = _tokens[3];

            DateTime _datetime;
            switch (_field)
            {
                case "date":
                    string[] _date = _newValue.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), 0, 0, 0);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid date given");
                        return;
                    }
                    break;
                case "time":
                    string[] _time = _newValue.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        _datetime = new DateTime(1, 1, 1, Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid time given");
                        return;
                    }
                    break;
                case "duration":
                    try
                    {
                        Int32.Parse(_newValue);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid duration given. Should be an integer");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Wrong field name given");
                    return;
            }

            if (_field == "date" || _field == "time")
            {
                StreamReader _sr = new StreamReader(File.Open(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + _id + ".event"), FileMode.Open, FileAccess.Read));
                string _s = _sr.ReadLine();
                _sr.Close();
                string[] _tempFields = _s.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string[] _tempDate = _tempFields[1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string[] _tempTime = _tempFields[2].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                int dur = Int32.Parse(_tempFields[3]);
                DateTime _date = new DateTime(Int32.Parse(_tempDate[2]), Int32.Parse(_tempDate[1]), Int32.Parse(_tempDate[0]), Int32.Parse(_tempTime[0]), Int32.Parse(_tempTime[1]), 0);
                string[] temp;
                if (_field == "date")
                {
                    temp = _newValue.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    _date = new DateTime(Int32.Parse(temp[2]), Int32.Parse(temp[1]), Int32.Parse(temp[0]), _date.Hour, _date.Minute, _date.Second);
                }
                else if (_field == "time")
                {
                    temp = _newValue.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    _date = new DateTime(_date.Year, _date.Month, _date.Day, Int32.Parse(temp[0]), Int32.Parse(temp[1]), _date.Second);
                }
                else if (_field == "duration")
                    dur = Int32.Parse(_newValue);
                if (!confirmTime(_date, dur, _id))
                {
                    Console.Out.WriteLine("New event overlaps with existing ones. Cannot be added");
                    return;
                }
            }

            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            _modify(_id, _field, _newValue);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.makeFunction("Modify", new string[] { _id, _field, _newValue });
            }
            forwardToken();
        }

        public int _drop(string ServerAddress)
        {
            if (next != null && next.address == ServerAddress)
                if (next.makeFunction("GetNext", null) == next.makeFunction("GetPrev", null))
                {
                    next = null;
                }
                else
                    next = new Client(Client.clientsMap[ServerAddress].makeFunction("GetNext", null));
            if (prev != null && prev.address == ServerAddress)
                if (prev.makeFunction("GetNext", null) == prev.makeFunction("GetPrev", null))
                {
                    prev = null;
                }
                else
                    prev = new Client(Client.clientsMap[ServerAddress].makeFunction("GetPrev", null));
            Client.clientsMap.Remove(ServerAddress);
            return 0;
        }

        public int _dropAll()
        {
            Client.clientsMap.Clear();
            return 0;
        }

        /// <summary>
        /// Method delete the given node from the list of remote hosts
        /// </summary>
        /// <param name="_input">The input string</param>
        private void drop(string _input)
        {
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 2)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("drop [ip]: Remove the host with the given IP address from the list of remote hosts. \r\n");
                return;
            }
            string _ip = _tokens[1];
            if (!Client.clientsMap.Keys.Contains(_ip))
            {
                Console.Out.WriteLine("Peer under given address is not connected with you.");
                return;
            }

            Client.clientsMap[_ip].makeFunction("DropAll", null);
            Client tempClient = Client.clientsMap[_ip];
            _drop(_ip);
            tempClient.makeFunction("SetNext", new string[] { "" });
            tempClient.makeFunction("SetPrev", new string[] { "" });
            tempClient.makeFunction("SetToken", null);
            foreach (Client c in Client.clientsMap.Values)
            {
                if (c.address != _ip)
                    c.makeFunction("Drop", new string[] { _ip });
            }

        }

        public string getLocalIP()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    localIP = ip.ToString();
            }
            return localIP;
        }

        public int _register(string serverAddress)
        {
            Client newClient = new Client(serverAddress);
            Client.clientsMap.Add(serverAddress, newClient);
            return 0;
        }

        public int _addNewPeer(string newPeerAddress)
        {
            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            
            Client newPeer = new Client(newPeerAddress);
            foreach (string address in Client.clientsMap.Keys)
            {
                newPeer.makeFunction("Register", new string[] { address });
                Client.clientsMap[address].makeFunction("Register", new string[] { newPeerAddress });
            }
            Client.clientsMap.Add(newPeerAddress, newPeer);
            if (EventManager.singleMachineDebug)
            {
                //For testing on a single machine
                newPeer.makeFunction("Register", new string[] { getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() });
                newPeer.makeFunction("SetPrev", new string[] { getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() });
            }
            else
            {
                newPeer.makeFunction("Register", new string[] { getLocalIP() });
                newPeer.makeFunction("SetPrev", new string[] { getLocalIP() });
            }

            if (next != null)
            {
                next.makeFunction("SetPrev", new string[] { newPeerAddress });
                newPeer.makeFunction("SetNext", new string[] { next.address });
            }
            else
            {
                if (EventManager.singleMachineDebug)
                    //For debugging on a single machine
                    newPeer.makeFunction("SetNext", new string[] { getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() });
                else
                    newPeer.makeFunction("SetNext", new string[] { getLocalIP() });
                prev = newPeer;
            }
            next = newPeer;

            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            foreach (FileInfo fi in di.GetFiles())
            {
                StreamReader _sr = new StreamReader(File.Open(fi.FullName, FileMode.Open, FileAccess.Read));
                string _s = _sr.ReadLine();
                _sr.Close();
                string[] _tokens = _s.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                newPeer.makeFunction("Add", _tokens);
            }

            forwardToken();
            return 0;
        }

        private void register(string _input)
        {
            token.WaitOne();
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 2)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("register [ip]: Register the host with the given IP address as remote host.\r\n");
                return;
            }
            string _ip = _tokens[1];

            Client newClient = new Client(_ip);

            try
            {
                newClient.makeFunction("Say", new string[] { "Peer of address: " + getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() + " registered you" });
            }
            catch
            {
                Console.Out.WriteLine("Wrong address given. Couldn't connect.");
                return;
            }

            if (EventManager.singleMachineDebug)
                //For debugging on a single machine
                newClient.makeFunction("AddNewPeer", new string[] { getLocalIP() + ":8000/xmlrpc" + EventManager.servNum.ToString() });
            else
                newClient.makeFunction("AddNewPeer", new string[] { getLocalIP() });
        }

        /// <summary>
        /// Method to lists all events
        /// </summary>
        private void le()
        {

            DirectoryInfo _di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            foreach (FileInfo _fi in _di.GetFiles())
            {
                StreamReader _sr = new StreamReader(File.Open(_fi.FullName, FileMode.Open, FileAccess.Read));
                String _s = _sr.ReadLine();
                _sr.Close();
                Console.WriteLine(_s);
            }
        }

        public int _add(string id, string date, string time, string duration, string header, string comment)
        {
            string newEventFileContent = id.ToString() + "\t" + date + "\t" + time + "\t" + duration.ToString() + "\t" + header + "\t" + comment;
            string newEventFilePath = "..\\..\\..\\events\\" + id.ToString() + ".event";
            StreamWriter _sw = new StreamWriter(File.Open(newEventFilePath, FileMode.CreateNew, FileAccess.Write));
            _sw.WriteLine(newEventFileContent);
            _sw.Close();
            return 0;
        }

        /// <summary>
        /// Method to add a new event locally as well as remote
        /// </summary>
        /// <param name="_input">The input string</param>
        private void add(string _input)
        {
            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 6)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("add [date] [time] [duration] [header] [comment] --> add a new event with the given arguments; date format: dd.mm.yyyy; time format: hh:mm; duration in minutes;\r\n");
                forwardToken();
                return;
            }

            string[] _date = _tokens[1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            string[] _time = _tokens[2].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

            //Date validity check
            try
            {
                new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), 0, 0, 0);
            }
            catch
            {
                Console.WriteLine("Invalid date given");
                forwardToken();
                return;
            }

            //Time validity check
            try
            {
                new DateTime(1, 1, 1, Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
            }
            catch
            {
                Console.WriteLine("Invalid time given");
                forwardToken();
                return;
            }

            //Duration validity check
            try
            {
                Int32.Parse(_tokens[3]);
            }
            catch
            {
                Console.WriteLine("Invalid duration given. Should be an integer");
                forwardToken();
                return;
            }

            DateTime _datetime = new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
            string _header = _tokens[4];
            string _comment = _tokens[5];

            if (!confirmTime(_datetime, Int32.Parse(_tokens[3])))
            {
                Console.Out.WriteLine("New event overlaps with existing ones. Cannot be added");
                forwardToken();
                return;
            }

            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            int id = 0;
            foreach (FileInfo fi in di.GetFiles())
            {
                string fileName = fi.Name.Substring(0, fi.Name.Length - ".event".Length);
                int fileNum = Int32.Parse(fileName);
                if (fileNum >= id)
                    id = fileNum + 1;
            }

            string date = _datetime.Day + "." + _datetime.Month + "." + _datetime.Year;

            _add(id.ToString(), date, _datetime.ToShortTimeString(), _tokens[3], _header, _comment);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.makeFunction("Add", new string[] { id.ToString(), date, _datetime.ToShortTimeString(), _tokens[3], _header, _comment });
            }
            forwardToken();
        }

        public int _remove(string id)
        {
            File.Delete(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + id + ".event"));
            return 0;
        }

        /// <summary>
        /// Method to remove the given event locally as well as remote
        /// </summary>
        /// <param name="_input">The input string</param>
        private void remove(string _input)
        {
            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 2)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("remove [id] --> remove the event with the given id;\r\n");
                forwardToken();
                return;
            }
            try
            {
                int _id = Int32.Parse(_tokens[1]);
            }
            catch
            {
                Console.WriteLine("Invalid event ID given. Should be an integer.");
                forwardToken();
                return;
            }

            _remove(_tokens[1]);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.makeFunction("Remove", new string[] { _tokens[1] });
            }
            forwardToken();
        }

        public int _clear()
        {
            DirectoryInfo _di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            foreach (FileInfo _fi in _di.GetFiles())
            {
                File.Delete(_fi.FullName);
            }
            return 0;
        }

        /// <summary>
        /// Method to remove all events locally as well as remote
        /// </summary>
        private void clear()
        {
            waitsForToken = true;
            token.WaitOne();
            waitsForToken = false;
            _clear();
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.makeFunction("Clear", null);
            }
            forwardToken();
        }

        /// <summary>
        /// Method to print all valid commands and arguments to the screen 
        /// </summary>
        private void help()
        {
            Console.Out.WriteLine("le: List all events.\r\n");
            Console.Out.WriteLine("add [date] [time] [duration] [header] [comment]: Add a new event with the given arguments. date format: dd.mm.yyyy; time format: hh:mm; duration in minutes;\r\n");
            Console.Out.WriteLine("clear: Remove all events.\r\n");
            Console.Out.WriteLine("drop [ip]: Remove the host with the given IP address from the list of remote hosts. \r\n");
            Console.Out.WriteLine("modify [id] [field] [value]: Modify the event with the given id and set the value of the specified field to [value].\r\n");
            Console.Out.WriteLine("register [ip]: Register the host with the given IP address as remote host.\r\n");
            Console.Out.WriteLine("remove [id]: Remove the event with the given id.\r\n");
            Console.Out.WriteLine("quit: Quit the application.\r\n");
        }

        public int _say(string text)
        {
            Console.Out.WriteLine(text);
            Console.Out.Write(">");
            return 0;
        }

        private void letThemTalk(string text)
        {
            text = text.Substring(3);
            foreach (Client _client in Client.clientsMap.Values)
                _client.makeFunction("Say", new string[] { text });
        }

        private void listPeers()
        {
            foreach (string addr in Client.clientsMap.Keys)
                Console.Out.WriteLine(addr);
        }
    }
}
