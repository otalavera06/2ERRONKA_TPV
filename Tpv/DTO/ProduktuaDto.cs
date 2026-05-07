﻿using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class ProduktuaDto
    {
        public virtual int Id { get; set; }
        public virtual string Izena { get; set; }
        public virtual decimal Prezioa { get; set; }
        [JsonProperty("irudia")]
        public virtual string IrudiaPath { get; set; }
        [JsonProperty("stock")]
        public virtual int Stock { get; set; }
        public virtual int ProduktuenMotakId { get; set; }
        [JsonIgnore]
        public bool IsPlatera { get; set; }
        public override string ToString()
        {
            return $"{Izena} - {Prezioa}€";
        }
    }

    
}
