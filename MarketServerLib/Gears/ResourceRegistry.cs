using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MarketGame;
using NLog;

namespace MarketServerLib.Gears
{
    class OwnedResource
    {
        public Resource Resource { get; set; }
        public string Owner { get; set; }

        public OwnedResource(Resource resource, string owner)
        {
            Resource = resource;
            Owner = owner;
        }
    }

    class ResourceRegistry
    {
        #region Singleton
        private static volatile ResourceRegistry instance;
        private static readonly object _syncRoot = new Object();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        private static readonly object _syncData = new Object();
        private Dictionary<Guid, OwnedResource> Resources = new Dictionary<Guid, OwnedResource>();

        private Dictionary<string, ulong> ResourcesPrices = new Dictionary<string, ulong>();

        private IEnumerable<string> possibleOwners;

        private ResourceRegistry()
        {
            Initialization();
        }

        public static ResourceRegistry Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ResourceRegistry();
                            log.Info("Starting ResourceRegistry (created singleton object)");
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        private void Initialization()
        {
            possibleOwners = UsersRegistry.Instance.GetUsersList();

            var lines = File.ReadAllLines("resources_prices.csv");

            foreach (var line in lines)
            {
                var ls = line.Split(Convert.ToChar(" "));
                var rname = ls[0].Trim();
                var rprice = ulong.Parse(ls[1].Trim());

                ResourcesPrices[rname] = rprice;

                log.Info("Resource {0} loaded. Proce {1}", rname, rprice);
            }
        }

        public void AddResource(Resource resource, string owner)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (owner == null) throw new ArgumentNullException("owner");
            if (!possibleOwners.Contains(owner)) throw new ArgumentOutOfRangeException("No such user");

            lock (_syncData)
            {
                if (Resources.ContainsKey(resource.Id)) throw new ArgumentOutOfRangeException("Resource id already in registry");

                Resources[resource.Id] = new OwnedResource(resource, owner);
            }
        }

        public void TransferResource(string oldOwner, string newOwner, Resource resource)
        {
            if (oldOwner == null) throw new ArgumentNullException("oldOwner");
            if (newOwner == null) throw new ArgumentNullException("newOwner");
            if (resource == null) throw new ArgumentNullException("resource");
            if (!possibleOwners.Contains(oldOwner)) throw new ArgumentOutOfRangeException("No such user");
            if (!possibleOwners.Contains(newOwner)) throw new ArgumentOutOfRangeException("No such user");

            lock (_syncData)
            {
                if (!Resources.ContainsKey(resource.Id)) throw new ArgumentException("No such resource");

                Resources[resource.Id].Owner = newOwner;
            }
        }

        public ulong CalcUserProperty(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            lock (_syncData)
            {
                var resmoney = Resources.Values.Where(resource => resource.Owner == name).Select(resource => ResourcesPrices[resource.Resource.ResourceType]);
                ulong S = 0;
                foreach (var rm in resmoney)
                {
                    S += rm;
                }

                return S;
            }
        }
    }
}
