using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{


    public class respuestaEntregaExitosa
    {
        [JsonProperty("Identificador")]
        public string Identificador { get; set; }
        [JsonProperty("Resultado")]
        public Resultado Resultado { get; set; }
        [JsonProperty("EsExitoso")]
        public bool EsExitoso { get; set; }
    }
    public class Resultado
    {
        [JsonProperty("PDF")]
        public string PDF { get; set; }
        [JsonProperty("ZIP")]
        public string ZIP { get; set; }
        [JsonProperty("CUFE")]
        public string CUFE { get; set; }
        [JsonProperty("QR")]
        public string QR { get; set; }
    }


}