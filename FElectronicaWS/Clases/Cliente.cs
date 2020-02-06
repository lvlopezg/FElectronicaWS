using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public class Cliente : ClienteInternacional,ClienteJuridico
    {
        [JsonProperty("idContrato", NullValueHandling = NullValueHandling.Ignore)]
        public int idContrato { get ; set; }

        [JsonProperty("FecFactura", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime FecFactura { get; set; }

        [JsonProperty("IdUsuarioR", NullValueHandling = NullValueHandling.Ignore)]
        public int IdUsuarioR { get; set; }

        [JsonProperty("nom_usua", NullValueHandling = NullValueHandling.Ignore)]
        public string nom_usua { get; set; }

        [JsonProperty("NumDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string NumDocumento { get; set; }

        [JsonProperty("IdTipoDoc", NullValueHandling = NullValueHandling.Ignore)]
        public byte IdTipoDoc { get; set; }

        [JsonProperty("NomTercero", NullValueHandling = NullValueHandling.Ignore)]
        public string NomTercero { get; set; }

        [JsonProperty("IdTercero", NullValueHandling = NullValueHandling.Ignore)]
        public int IdTercero { get; set; }

        [JsonProperty("idRegimen", NullValueHandling = NullValueHandling.Ignore)]
        public string idRegimen { get; set; }

        [JsonProperty("IdNaturaleza", NullValueHandling = NullValueHandling.Ignore)]
        public short IdNaturaleza { get; set; }

        [JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)]
        public string direccion { get; set; }

        [JsonProperty("ciudad", NullValueHandling = NullValueHandling.Ignore)]
        public string ciudad { get; set; }

        [JsonProperty("idLugar", NullValueHandling = NullValueHandling.Ignore)]
        public int idLugar { get; set; }

        [JsonProperty("codMunicipio", NullValueHandling = NullValueHandling.Ignore)]
        public string codMunicipio { get; set; }

        [JsonProperty("telefono", NullValueHandling = NullValueHandling.Ignore)]
        public string telefono { get; set; }

        [JsonProperty("Codigo_depto", NullValueHandling = NullValueHandling.Ignore)]
        public string Codigo_depto { get; set; }

        [JsonProperty("Nombre_Depto", NullValueHandling = NullValueHandling.Ignore)]
        public string Nombre_Depto { get; set; }

        [JsonProperty("Codigo_Municipio", NullValueHandling = NullValueHandling.Ignore)]
        public string Codigo_Municipio { get; set; }

        [JsonProperty("Nom_Municipio", NullValueHandling = NullValueHandling.Ignore)]
        public string Nom_Municipio { get; set; }

        [JsonProperty("codigoPais", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoPais { get; set; }

        [JsonProperty("cuenta_correo", NullValueHandling = NullValueHandling.Ignore)]
        public string cuenta_correo { get; set; }

        [JsonProperty("NroDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public string NroDoc_Cliente { get; set; }

        [JsonProperty("TipoDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public byte TipoDoc_Cliente { get; set; }
    }
}