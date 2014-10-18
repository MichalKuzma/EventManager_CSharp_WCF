using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace EventManager_CSharp_WCF_Lib
{
    public class EventManager : IEventManager
    {
        private static EventManager instance;
        /*
        /// <summary>
        /// Default Constructor.
        /// </summary>
        private EventManager()
        {
        }*/

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
                else
                {
                    Console.Out.WriteLine("Illegal command: " + _input);
                }
            }
        }

        public void _modify(string id, string field, string newValue)
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
                    try
                    {
                        _datetime = new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), 0, 0, 0);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid date given");
                        return;
                    }
                    _fields[1] = _datetime.Day + "." + _datetime.Month + "." + _datetime.Year;
                    break;
                case "time":
                    string[] _time = newValue.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        _datetime = new DateTime(1, 1, 1, Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid time given");
                        return;
                    }
                    _fields[2] = _datetime.ToShortTimeString();
                    break;
                case "duration":
                    try
                    {
                        Int32.Parse(_fields[3]);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid duration given. Should be an integer");
                        return;
                    }
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
                    return;
            }
            _s = String.Join("\t", _fields);
            StreamWriter _sw = new StreamWriter(File.Open(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + id + ".event"), FileMode.Create, FileAccess.Write));
            _sw.WriteLine(_s);
            _sw.Close();
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

            _modify(_id, _field, _newValue);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.eManager._modify(_id, _field, _newValue);
            }
        }

        public void _drop(string ServerAddress)
        {
            Client.clientsMap.Remove(ServerAddress);
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
            //ToDo: Add additional input validity checks
            string _ip = _tokens[1];

            Client.clientsMap[_ip].eManager._drop(getLocalIP());
            _drop(_ip);
        }

        private string getLocalIP()
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

        public Client _register(string serverAddress)
        {
            Client newClient = new Client(serverAddress);
            Client.clientsMap.Add(serverAddress, newClient);
            return newClient;
        }

        private void register(string _input)
        {
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 2)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("register [ip]: Register the host with the given IP address as remote host.\r\n");
                return;
            }
            //ToDo: Add additional input validity checks
            string _ip = _tokens[1];
            Client newClient = _register(_tokens[1]);
            newClient.eManager._register(getLocalIP());
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

        public void _add(DateTime datetime, int duration, string header, string comment)
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            int id = 0;
            foreach (FileInfo fi in di.GetFiles())
            {
                string fileName = fi.Name.Substring(0, fi.Name.Length - ".event".Length);
                int fileNum = Int32.Parse(fileName);
                if (fileNum >= id)
                    id = fileNum + 1;
            }
            string newEventFileContent = id.ToString() + "\t" + datetime.Day + "." + datetime.Month + "." + datetime.Year + "\t" + datetime.ToShortTimeString() + "\t" + duration.ToString() + "\t" + header + "\t" + comment;
            string newEventFilePath = "..\\..\\..\\events\\" + id.ToString() + ".event";
            StreamWriter _sw = new StreamWriter(File.Open(newEventFilePath, FileMode.CreateNew, FileAccess.Write));
            _sw.WriteLine(newEventFileContent);
            _sw.Close();
        }

        /// <summary>
        /// Method to add a new event locally as well as remote
        /// </summary>
        /// <param name="_input">The input string</param>
        private void add(string _input)
        {
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 6)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("add [date] [time] [duration] [header] [comment] --> add a new event with the given arguments; date format: dd.mm.yyyy; time format: hh:mm; duration in minutes;\r\n");
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
                return;
            }

            DateTime _datetime = new DateTime(Int32.Parse(_date[2]), Int32.Parse(_date[1]), Int32.Parse(_date[0]), Int32.Parse(_time[0]), Int32.Parse(_time[1]), 0);
            int _duration = Int32.Parse(_tokens[3]);
            string _header = _tokens[4];
            string _comment = _tokens[5];

            _add(_datetime, _duration, _header, _comment);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.eManager._add(_datetime, _duration, _header, _comment);
            }
        }

        public void _remove(string id)
        {
            File.Delete(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events\\" + id + ".event"));
        }

        /// <summary>
        /// Method to remove the given event locally as well as remote
        /// </summary>
        /// <param name="_input">The input string</param>
        private void remove(string _input)
        {
            string[] _tokens = _input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (_tokens.Count<string>() < 2)
            {
                Console.Out.WriteLine("Illegal arguments");
                Console.Out.WriteLine("remove [id] --> remove the event with the given id;\r\n");
                return;
            }
            try
            {
                int _id = Int32.Parse(_tokens[1]);
            }
            catch
            {
                Console.WriteLine("Invalid event ID given. Should be an integer.");
                return;
            }

            _remove(_tokens[1]);
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.eManager._remove(_tokens[1]);
            }
        }

        public void _clear()
        {
            DirectoryInfo _di = new DirectoryInfo(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "..\\..\\..\\events"));
            foreach (FileInfo _fi in _di.GetFiles())
            {
                File.Delete(_fi.FullName);
            }
        }

        /// <summary>
        /// Method to remove all events locally as well as remote
        /// </summary>
        private void clear()
        {
            _clear();
            foreach (Client _client in Client.clientsMap.Values)
            {
                _client.eManager._clear();
            }
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
    }
}
