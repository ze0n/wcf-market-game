using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using MarketGame;
using MarketServerLib.Gears;
using NLog;

namespace MarketServerLib.Generator
{
    public class ResourceGenerator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public const string TargetNamespace = "http://escience.ifmo.ru/study/wcfmarketgame/";

        private Thread thread;

        public void StartThread()
        {
            thread = new Thread(ThreadFunc);
            thread.Start();
        }

        static Random rand = new Random((int)DateTime.Now.ToFileTime());

        private static bool StopRequest = false;

        private static int GetIndex(float[] probs)
        {
            var d = rand.NextDouble();

            for (int i = 0; i < probs.Length; i++)
            {
                d = d - probs[i];

                if (d <= 0.0)
                {
                    return i;
                }
            }

            throw new InvalidOperationException();
        }

        private static void ThreadFunc()
        {
            log.Info("Resource generator started in separate thread");
            int day = 0;

            while (true)
            {
                var users = UsersRegistry.Instance.GetActiveUsersList();
                foreach (User user in users)
                {
                    var i = GetIndex(user.ResourceProbabilityConfiguration);

                    string rt = ResourceRegistry.Instance.ResourceTypesList[i];
                    var r = new Resource(){Id = Guid.NewGuid(), ResourceType = rt};

                    try
                    {
                        SendResource(r, user.Endpoint);
                        ResourceRegistry.Instance.AddResource(r, user.Name);
                        log.Info("User '{0}' got a piece of '{1}'", user.Name, r.ResourceType);
                    }
                    catch(Exception ex)
                    {
                        UsersRegistry.Instance.DisacivateUser(user.Name);
                    }
                }


                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Day #{0}", day++);
                Console.WriteLine("-------------------------------------------------");
                var tab = UsersRegistry.Instance.GetFinalScoreTable();
                foreach (var f in tab.OrderByDescending(pair => pair.Value))
                {
                    Console.WriteLine("{0}\t\t{1}", f.Key, f.Value);
                }
                Console.WriteLine("-------------------------------------------------");


                Thread.Sleep(20000);

                if (StopRequest)
                {
                    log.Info("Resource generator stopped");
                    break;
                }
            }
        }

        private static void SendResource(Resource R, string pEndpointAddress)
        {
            try
            {
                // endpoint
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.Namespace = TargetNamespace;
                EndpointAddress endpointAddress = new EndpointAddress(pEndpointAddress);
                IResourceReceiver notifee = ChannelFactory<IResourceReceiver>.CreateChannel(binding, endpointAddress);

                log.Debug("Sending resource '{0}' to user '{1}'", R, pEndpointAddress);
                using (notifee as IDisposable)
                    notifee.ReceiveResource(R);

                log.Debug("Resource was successfully sent.");
            }
            catch (Exception ex)
            {
                log.Error("Unable to transfer resource");
                throw ex;
            }
        }

        public void StopThread()
        {
            StopRequest = true;
        }
    }
}
