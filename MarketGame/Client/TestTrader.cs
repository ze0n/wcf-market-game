using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketGame;

namespace Client
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TestTrader" in both code and config file together.
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
            Console.WriteLine(resource);
        }

        public string[] GetResourcesOffer()
        {
            return new string[0];
        }
    }
}
