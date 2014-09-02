using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using MarketGame;
using NLog;

namespace MarketServerLib.Generator
{
    class ResourceGenerator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public const string TargetNamespace = "http://escience.ifmo.ru/study/wcfmarketgame/";

        private static void SendResource(Resource R, string pEndpointAddress)
        {
            try
            {
                // endpoint
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.Namespace = TargetNamespace;
                EndpointAddress endpointAddress = new EndpointAddress(pEndpointAddress);
                IResourceReceiver notifee = ChannelFactory<IResourceReceiver>.CreateChannel(binding, endpointAddress);

                log.Debug("Sending resource '{0}' to user '{1}'", R, );
                using (notifee as IDisposable)
                    notifee.ReceiveResource(n);

                log.Debug("Notification was successfully sent.");

            }
            catch (Exception ex)
            {
                log.Error(ex);
                // TODO: check
                SubscriptionManager.Instance.CancelSubscription(anotification.sid, Eventing.EventBodyMappings.ReasonEnum.SinkConnectionError);
            }
        }


    }
}
