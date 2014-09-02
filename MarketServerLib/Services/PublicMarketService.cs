using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketGame;
using MarketServerLib.Gears;

namespace MarketServerLib.Services
{
    public class PublicMarketService : IPublicMarketService
    {
        public bool Ping()
        {
            return true;
        }

        public void ActivateMeAt(string username, string password, string EndpointAddress)
        {
            try
            {
                if (UsersRegistry.Instance.CheckLogin(username, password))
                {
                    UsersRegistry.Instance.AcivateUser(username, EndpointAddress);
                }
                else
                {
                    throw new ArgumentException("authentification");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Resource Craft(Guid[] take, string restTypeToGet, string username, string password)
        {
            try
            {
                if (UsersRegistry.Instance.CheckLogin(username, password))
                {
                    return ResourceRegistry.Instance.Craft(take, restTypeToGet, username);
                }
                else
                {
                    throw new ArgumentException("authentification");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
