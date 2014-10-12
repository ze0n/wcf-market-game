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

    public class ResourceRegistry : IDisposable
    {
        #region Singleton
        private static volatile ResourceRegistry instance;
        private static readonly object _syncRoot = new Object();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        private static readonly object _syncData = new Object();
        private Dictionary<Guid, OwnedResource> Resources = new Dictionary<Guid, OwnedResource>();
        public List<string> ResourceTypesList = new List<string>();
        private Dictionary<string, ulong> ResourcesPrices = new Dictionary<string, ulong>();
        public Dictionary<string, bool> ResourcesGenerated = new Dictionary<string, bool>();

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
            possibleOwners = UsersRegistry.Instance.GetAllUsersNamesList();

            var lines = File.ReadAllLines("resources.csv");

            foreach (var line in lines)
            {
                var ls = line.Split(Convert.ToChar(" "));
                var rname = ls[0].Trim().ToUpper();
                var rprice = ulong.Parse(ls[1].Trim());
                var rprob = int.Parse(ls[1].Trim());

                ResourceTypesList.Add(rname);
                ResourcesPrices[rname] = rprice;
                ResourcesGenerated[rname] = rprob == 1;

                log.Info("Resource {0} loaded. Proce {1}", rname, rprice);
            }

            var lines5 = File.ReadAllLines("crafting_rules.txt");

            foreach (var line in lines5)
            {
                Rules.Add(line.Trim());
                log.Info("Crafting rule loaded {0}", line);
            }

            if (File.Exists("resources_dump.csv"))
            {
                log.Info("Loading resources from dump");
                var lines0 = File.ReadAllLines("resources_dump.csv");

                lock (_syncData)
                {
                    foreach (var line in lines0)
                    {
                        var ls = line.Split(Convert.ToChar(" "));
                        var owner = ls[0].Trim();
                        var resId = Guid.Parse(ls[1].Trim());
                        var resType = ls[2].Trim().ToUpper();

                        var r = new Resource() { Id = resId, ResourceType = resType };
                        var or = new OwnedResource(r, owner);

                        Resources[r.Id] = or;
                    }
                }
            }
        }

        private void SaveDump()
        {
            lock (_syncData)
            {
                var f = File.CreateText("resources_dump.csv");

                foreach (var pair in Resources)
                {
                    var or = pair.Value;
                    f.WriteLine("{0} {1} {2}", or.Owner, or.Resource.Id, or.Resource.ResourceType);
                }

                f.Close();
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

        public void Dispose()
        {
            SaveDump();
            log.Info("ResourceRegistry is disposed");
        }

        private List<string> Rules = new List<string>(); 

        public Resource Craft(Guid[] take, string restTypeToGet, string username)
        {
            if (take == null) throw new ArgumentNullException("take");
            if (restTypeToGet == null) throw new ArgumentNullException("restTypeToGet");

            lock (_syncData)
            {
                foreach (var guid in take)
                {
                    if (!Resources.ContainsKey(guid))
                        throw new ArgumentOutOfRangeException("No such resource");

                    if(Resources[guid].Owner != username)
                        throw new ArgumentOutOfRangeException("Resource is not yours");
                }

                // search for rule
                List<string> rtypes = new List<string>();
                foreach (var guid in take)
                {
                    rtypes.Add(Resources[guid].Resource.ResourceType);
                }
                rtypes.Sort();

                var left = String.Join("+", rtypes);
                var rule = left + "=" + restTypeToGet;

                if(Rules.Contains(rule))
                {
                    foreach (var guid in take)
                    {
                        Resources.Remove(guid);
                        log.Trace("Resource '{0}' removed", guid);
                    }

                    var or = new OwnedResource(new Resource() {Id = Guid.NewGuid(), ResourceType = restTypeToGet},
                                               username);
                    Resources.Add(or.Resource.Id, or);
                    log.Trace("Resource '{0}' created", or.Resource);

                    UsersRegistry.Instance.AddScore(username, 1, "craft");

                    return or.Resource;
                }
                else
                {
                    throw new InvalidOperationException("No such rule");
                }

            }

            
        }

        public Resource[] GetUserResources(string username)
        {
            lock (_syncData)
            {
                return Resources.Values.Where(resource => resource.Owner == username).Select(resource => resource.Resource).ToArray();
            }
        }
    }
}
