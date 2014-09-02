using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace MarketGame
{
    class Utils
    {
        public static void StartHost(Type implType)
        {
            ServiceHost host = new ServiceHost(implType);
            host.Open();
            Console.WriteLine("Started...");
            Console.WriteLine("Base address: {0}", host.BaseAddresses[0]);
            foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                Console.WriteLine("Endpoint: \n\t{0}\n\t{1}\n\t{2}",
                                  endpoint.Address,
                                  endpoint.Binding.Name,
                                  endpoint.Contract.ContractType);
            Console.WriteLine("Press <ENTER> to stop...");
            Console.ReadLine();
            host.Close();
        }

    }
}
