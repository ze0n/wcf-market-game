using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketServerLib.Gears;

namespace MarketServerLib.Services
{
    public class PublicMarketService : IPublicMarketService
    {
        public bool Ping()
        {
            return true;
        }

        public void ActivateMeAt(string EndpointAddress)
        {
            try
            {
                string username;
                string password;

                if (UsersRegistry.Instance.CheckLogin(username, password))
                {
                    UsersRegistry.Instance.AcivateUser(username, EndpointAddress);
                    UsersRegistry.Instance.AddScore(username, 20, "registration");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
