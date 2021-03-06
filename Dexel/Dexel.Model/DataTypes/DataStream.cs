﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dexel.Model.Annotations;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class DataStream : BaseModel
    {
        public Guid ID { get; set; }
        public string DataNames { get;  set; }
        public List<DataStreamDefinition> Sources { get;  set; }
        public List<DataStreamDefinition> Destinations { get;  set; }

        public DataStream()
        {
            Sources = new List<DataStreamDefinition>();
            Destinations = new List<DataStreamDefinition>();
        }

    }

}