using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace MarketServerLib.Gears
{
    class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public ulong Score { get; set; }
        public bool IsActive { get; set; }

        public float[] ResourceProbabilityConfiguration = null;

        public string Endpoint { get; set; }

        public User(string name, string password)
        {
            Name = name;
            Password = password;

            Score = 0;
            IsActive = false;
        }

        public override string ToString()
        {
            if (IsActive)
                return String.Format("-{0}- <{1}>", Name, Score);
            else
                return String.Format("-{0}- not active", Name);
        }
    }

    class UsersRegistry
    {
        #region Singleton
        private static volatile UsersRegistry instance;
        private static readonly object _syncRoot = new Object();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        private static readonly object _syncData = new Object();
        private Dictionary<string, User> Users = new Dictionary<string, User>();

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

        public 

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

        private void Initialization()
        {
            var lines = File.ReadAllLines("users.csv");

            lock (_syncData)
            {
                foreach (var line in lines)
                {
                    var ls = line.Split(Convert.ToChar(" "));
                    var uname = ls[0].Trim();
                    var pass = ls[1].Trim();

                    Users[uname] = new User(uname, pass);
                    log.Info("User {0} loaded", uname);
                }
            }
        }

        public IEnumerable<string> GetUsersList()
        {
            return Users.Keys.ToList();
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
        }
    }
}
