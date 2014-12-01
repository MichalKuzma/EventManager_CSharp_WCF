using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventManager_CSharp_WCF_Lib;

namespace EventManager_CSharp_WCF_Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener.Instance.Start();
        }
    }
}
