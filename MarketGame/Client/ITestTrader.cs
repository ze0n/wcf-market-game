using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketGame;

namespace Client
{
    [ServiceContract]
    public interface ITestTrader : IResourceReceiver, ITrader
    {
        [OperationContract]
        void DoWork();
    }
}
