using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketGame;

namespace Client
{
    public class TestTrader : ITestTrader
    {
        public void DoWork()
        {
        }

        public bool Ping()
        {
            return true;
        }

        public void ReceiveResource(Resource resource)
        {
            Console.WriteLine();
            Console.WriteLine("Received resource {0}", resource.ToString());
            Console.WriteLine();
        }

        public string[] GetResourcesOffer()
        {
            return new string[0];
        }
    }
}
