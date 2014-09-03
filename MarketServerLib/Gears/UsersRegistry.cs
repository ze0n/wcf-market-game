using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace MarketServerLib.Gears
{
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public ulong Score { get; set; }
        public bool IsActive { get; set; }

        public float[] ResourceProbabilityConfiguration = null;

        public string Endpoint { get; set; }

        public User(string name, string password, float[] pResourceProbabilityConfiguration)
        {
            Name = name;
            Password = password;

            Score = 0;
            IsActive = false;

            ResourceProbabilityConfiguration = pResourceProbabilityConfiguration;

            // TODO: delete
            Console.WriteLine(String.Join(", ", ResourceProbabilityConfiguration));
        }

        public override string ToString()
        {
            if (IsActive)
                return String.Format("-{0}- <{1}>", Name, Score);
            else
                return String.Format("-{0}- not active", Name);
        }
    }

    public class UsersRegistry : IDisposable
    {
        #region Singleton
        private static volatile UsersRegistry instance;
        private static readonly object _syncRoot = new Object();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly object _syncData = new Object();
        private Dictionary<string, User> Users = new Dictionary<string, User>();

        public List<string> ResourceTypesList = new List<string>();
        public Dictionary<string, bool> ResourcesGenerated = new Dictionary<string, bool>();

        private static Random rand = new Random((int)DateTime.Now.ToFileTime());



        private UsersRegistry()
        {
            Initialization();
        }

        public static UsersRegistry Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new UsersRegistry();
                            log.Info("Starting UsersRegistry (created singleton object)");
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        public bool CheckLogin(string name, string pass)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (pass == null) throw new ArgumentNullException("pass");

            lock (_syncData)
            {
                if (!Users.ContainsKey(name)) return false;
                return Users[name].Password == pass;
            }
        }

        private void SaveDump()
        {
            lock (_syncData)
            {
                var f = File.CreateText("users_dump.csv");

                foreach (KeyValuePair<string, User> pair in Users)
                {
                    var u = pair.Value;
                    f.WriteLine("{0} {1} {2} {3}", u.Name, u.Password, u.Score, u.Endpoint);
                }

                f.Close();
            }
        }

        private float [] GenerateProbs()
        {
            var rg = ResourcesGenerated;
            var rt = ResourceTypesList;

            float[] probs = new float[rt.Count];

            int ind = 0;
            foreach (string s in rt)
            {
                float p = 0.0f;
                if (rg[s])
                {
                    p = (float)rand.NextDouble();
                }

                probs[ind] = p;
                ind++;
            }

            var ps = probs.Sum();
            for (int i = 0; i < rt.Count; i++)
            {
                probs[i] /= ps;
            }

            return probs;
        }

        private void Initialization()
        {
            var lines2 = File.ReadAllLines("resources.csv");

            foreach (var line in lines2)
            {
                var ls = line.Split(Convert.ToChar(" "));
                var rname = ls[0].Trim().ToUpper();
                var rprice = ulong.Parse(ls[1].Trim());
                var rprob = int.Parse(ls[2].Trim());

                ResourceTypesList.Add(rname);
                ResourcesGenerated[rname] = rprob == 1;

                log.Info("Resource {0} loaded. Proce {1}", rname, rprice);
            }
            
            if (File.Exists("users_dump.csv"))
            {
                var lines = File.ReadAllLines("users_dump.csv");
                log.Info("Loading users from dump 'users_dump.csv'");
                lock (_syncData)
                {
                    foreach (var line in lines)
                    {
                        var ls = line.Split(Convert.ToChar(" "));
                        var uname = ls[0].Trim();
                        var pass = ls[1].Trim();
                        var score = ulong.Parse(ls[2].Trim());
                        var ep = ls[3].Trim();

                        Users[uname] = new User(uname, pass, GenerateProbs());
                        Users[uname].Score = score;
                        Users[uname].Endpoint = ep;
                        Users[uname].IsActive = ep != "";
                        

                        log.Debug("User {0} loaded with score {1}", uname, score);
                    }
                }
            }
            else
            {
                var lines = File.ReadAllLines("users.csv");

                lock (_syncData)
                {
                    foreach (var line in lines)
                    {
                        var ls = line.Split(Convert.ToChar(" "));
                        var uname = ls[0].Trim();
                        var pass = ls[1].Trim();

                        Users[uname] = new User(uname, pass, GenerateProbs());
                        log.Info("User {0} loaded", uname);
                    }
                }
            }
        }

        public IEnumerable<string> GetAllUsersNamesList()
        {
            return Users.Keys.ToList();
        }

        public IEnumerable<User> GetActiveUsersList()
        {
            return Users.Values.Where(user => user.IsActive).ToList();
        }

        public void AddScore(string userName, ulong delta, string reason = "unknown")
        {
            if (userName == null) throw new ArgumentNullException("userName");

            lock (_syncData)
            {
                if (!Users.ContainsKey(userName)) throw new ArgumentOutOfRangeException("No such user");

                Users[userName].Score += delta;

                log.Trace("MONEY '{0}' earned '{1}' score for '{2}'", userName, delta, reason);
            }
        }

        public Dictionary<string, ulong> GetFinalScoreTable()
        {
            Dictionary<string, ulong> ret = new Dictionary<string, ulong>();

            lock (_syncData)
            {
                foreach (var userk in Users.Keys)
                {
                    var u = Users[userk];

                    ulong rmoney = ResourceRegistry.Instance.CalcUserProperty(u.Name);

                    ulong score = u.Score + rmoney;

                    ret.Add(u.Name, score);
                }
            }

            return ret;
        }

        public void AcivateUser(string username, string endpointAddress)
        {
            if (username == null) throw new ArgumentNullException("username");
            if (endpointAddress == null) throw new ArgumentNullException("endpointAddress");

            bool wasAct = false;

            lock (_syncData)
            {
                if (!Users.ContainsKey(username)) throw new ArgumentOutOfRangeException("No such user");

                wasAct = Users[username].IsActive;

                Users[username].Endpoint = endpointAddress;
                Users[username].IsActive = true;

            }
            
            if(!wasAct)
                AddScore(username, 20, "registration");
        }

        public void Dispose()
        {
            SaveDump();
            log.Info("UserRegistry is disposed");
        }

        public void DisacivateUser(string username)
        {
            if (username == null) throw new ArgumentNullException("username");

            lock (_syncData)
            {
                if (!Users.ContainsKey(username)) throw new ArgumentOutOfRangeException("No such user");

                Users[username].Endpoint = null;
                Users[username].IsActive = false;
            }

            log.Info("User '{0}' is disactivated", username);
        }
    }
}
