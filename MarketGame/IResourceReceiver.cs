using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MarketGame
{
    [ServiceContract(Namespace = "http://escience.ifmo.ru/study/wcfmarketgame")]
    public interface IResourceReceiver
    {
        [OperationContract]
        bool Ping();

        [OperationContract(Action = "http://escience.ifmo.ru/study/wcfmarketgame/ReceiveResource")]
        void ReceiveResource(Resource resource);
    }
}
