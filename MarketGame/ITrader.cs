using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MarketGame
{
    [ServiceContract(Namespace = "http://escience.ifmo.ru/study/wcfmarketgame")]
    interface ITrader
    {
        [OperationContract(Action = "http://escience.ifmo.ru/study/wcfmarketgame/GetResourcesCatalog")]
        string[] GetResourcesOffer();

        [OperationContract(Action = "http://escience.ifmo.ru/study/wcfmarketgame/GetResourcesCatalog")]
        bool OfferNaturalDeal(resourcesIGiveYou, string )
    }
}
