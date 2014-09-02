using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using MarketGame;
using MarketServerLib.Gears;
using MarketServerLib.Generator;
using MarketServerLib.Services;

namespace MarketServerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(PublicMarketService));
            host.Open();
            Console.WriteLine("Started...");
            Console.WriteLine("Base address: {0}", host.BaseAddresses[0]);
            foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                Console.WriteLine("Endpoint: \n\t{0}\n\t{1}\n\t{2}",
                                  endpoint.Address,
                                  endpoint.Binding.Name,
                                  endpoint.Contract.ContractType);

            var u = UsersRegistry.Instance;
            var r = ResourceRegistry.Instance;

            ResourceGenerator RG = new ResourceGenerator();
            RG.StartThread();

            Console.WriteLine("Press <ENTER> to stop...");
            Console.ReadLine();

            RG.StopThread();
            host.Close();

            u.Dispose();
            r.Dispose();
        }
    }
}
