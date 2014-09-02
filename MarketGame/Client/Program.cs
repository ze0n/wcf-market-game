using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Client.Market;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(TestTrader));
            host.Open();
            Console.WriteLine("Started...");
            Console.WriteLine("Base address: {0}", host.BaseAddresses[0]);
            foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                Console.WriteLine("Endpoint: \n\t{0}\n\t{1}\n\t{2}",
                                  endpoint.Address,
                                  endpoint.Binding.Name,
                                  endpoint.Contract.ContractType);

            Market.PublicMarketServiceClient cli = new PublicMarketServiceClient();
            cli.ActivateMeAt("goatfeet", "vrBhVQ", host.Description.Endpoints.First().Address.Uri.ToString());

            Console.WriteLine("Press <ENTER> to stop...");
            Console.ReadLine();

            host.Close();
        }
    }
}
