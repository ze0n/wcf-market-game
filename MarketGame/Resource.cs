using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MarketGame
{

    [DataContract(Namespace = "http://escience.ifmo.ru/study/wcfmarketgame")]
    public class Resource
    {
        [DataMember]
        public Guid Id { get; set; }

        private string _resourceType = null;
        
        [DataMember]
        public string ResourceType
        {
            get { return _resourceType; }
            set { _resourceType = value.ToUpperInvariant(); }
        }

        public string ToString()
        {
            return String.Format("{0} {1}", ResourceType, Id);
        }
    }
}