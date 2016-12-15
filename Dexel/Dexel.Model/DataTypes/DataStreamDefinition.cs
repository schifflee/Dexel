﻿using System;
using System.Xml.Serialization;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class DataStreamDefinition: IModelWithID
    {
        public Guid ID { get;  set; }
        public string ActionName { get;  set; }
        public bool Connected { get;  set; }
        public string DataNames { get;  set; }
        [XmlIgnore]
        public SoftwareCell Parent { get;  set; }
    }
}
