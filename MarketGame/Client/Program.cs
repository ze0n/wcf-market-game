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

            string login = "goatfeet";
            string password = "vrBhVQ";

            Market.PublicMarketServiceClient cli = new PublicMarketServiceClient();

            var cc = new ConsoleCommander();
            cc.AddCmd("activate", strings =>
            {
                var ep = strings[1];
                Console.WriteLine("Requesting activation");
                try
                {
                    cli.ActivateMeAt(login, password, host.Description.Endpoints.First().Address.Uri.ToString());
                    return "Account has been activated";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return "Failed";
            });

            cc.AddCmd("listresources", strings =>
            {
                Console.WriteLine("Requesting list of resources");
                try
                {
                    var rs = cli.GetMyResources(login, password).Select(resource => resource.ToString());
                    return String.Join("\n", rs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return "Failed";                
            });

//registry activate me at "http://192..."
//registry list resources
//registry total score
//registry craft with [guid0, guid1, guid2]
//market list offers
//market buy <offerId>
//chat read
//chat post "yo"

            cc.StartLoop();

            host.Close();
        }
    }
}
