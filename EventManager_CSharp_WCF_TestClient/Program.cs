using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManager_CSharp_WCF_Lib;

namespace EventManager_CSharp_WCF_TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string _ip;
            Console.Out.WriteLine("Server IP: ");
            _ip = Console.In.ReadLine();
            Client _client = new Client(_ip);

            string input = "";
            while (input != "quit")
            {
                Console.Out.Write(">");
                input = Console.In.ReadLine();
                _client.eManager._say(input);
            }
            _client.Close();
        }
    }
}
