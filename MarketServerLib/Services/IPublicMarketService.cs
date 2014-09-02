﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MarketGame;

namespace MarketServerLib.Services
{
    [ServiceContract]
    public interface IPublicMarketService
    {
        [OperationContract]
        bool Ping();

        [OperationContract]
        void ActivateMeAt(string username, string password, string EndpointAddress);

//        [OperationContract]
//        Resource[] GetMyResources();
//
//        [OperationContract]
//        void IWannaTradeWith
    }
}
