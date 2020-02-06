using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public class ClienteNatural
    {
        /// <summary>
        /// ID del Contrato Que respalda la Factura
        /// </summary>
        [JsonProperty("idContrato", NullValueHandling = NullValueHandling.Ignore)]
        public int idContrato { get; set; }
        /// <summary>
        /// Fecha de la Factura
        /// </summary>
        [JsonProperty("FecFactura", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime FecFactura { get; set; }
        /// <summary>
        /// ID del usuario Que Factura
        /// </summary>
        [JsonProperty("IdUsuarioR", NullValueHandling = NullValueHandling.Ignore)]
        public int IdUsuarioR { get; set; }
        /// <summary>
        /// Nombre del usuario que Factura
        /// </summary>
        [JsonProperty("nom_usua", NullValueHandling = NullValueHandling.Ignore)]
        public string nom_usua { get; set; }
        /// <summary>
        /// Numero de Documento del usuario que Factura
        /// </summary>
        [JsonProperty("Documento_Facturador", NullValueHandling = NullValueHandling.Ignore)]
        public string Documento_Facturador { get; set; }
        /// <summary>
        /// ID o tipo del Documento de la persona Que Factura
        /// </summary>
        [JsonProperty("IdTipoDocFact", NullValueHandling = NullValueHandling.Ignore)]
        public int IdTipoDocFact { get; set; }
        /// <summary>
        /// ID de Tercero del cliente
        /// </summary>
        [JsonProperty("IdTercero", NullValueHandling = NullValueHandling.Ignore)]
        public int IdTercero { get; set; }
        /// <summary>
        /// ID del Regimen del Cliente en SAHI
        /// </summary>
        [JsonProperty("idRegimen", NullValueHandling = NullValueHandling.Ignore)]
        public string idRegimen { get; set; }
        /// <summary>
        /// ID de la Naturaleza del cliente en SAHI
        /// </summary>
        [JsonProperty("IdNaturaleza", NullValueHandling = NullValueHandling.Ignore)]
        public int IdNaturaleza { get; set; }
        /// <summary>
        /// Nombre del Cliente
        /// </summary>
        [JsonProperty("NomCliente", NullValueHandling = NullValueHandling.Ignore)]
        public string NomCliente { get; set; }
        /// <summary>
        /// Apellidos del Cliente
        /// </summary>
        [JsonProperty("ApeCliente", NullValueHandling = NullValueHandling.Ignore)]
        public string ApeCliente { get; set; }
        /// <summary>
        /// Numero de Documento del Cliente
        /// </summary>
        [JsonProperty("NroDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public string NroDoc_Cliente { get; set; }
        /// <summary>
        /// Tipo de Documento del Cliente
        /// </summary>
        [JsonProperty("TipoDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public Byte TipoDoc_Cliente { get; set; }

        /// <summary>
        /// Direccion del Cliente
        /// </summary>
        [JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)]
        public string direccion { get; set; }

        /// <summary>
        /// Codigo Municipio del Cliente
        /// </summary>
        [JsonProperty("cod_municipio", NullValueHandling = NullValueHandling.Ignore)]
        public string cod_municipio { get; set; }

        /// <summary>
        /// Codigo del Pais
        /// </summary>
        [JsonProperty("codigoPais", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoPais { get; set; }

        /// <summary>
        /// Codigo de Lozalizacion del Cliente= idLugar en SAHI
        /// </summary>
        [JsonProperty("idLocalizacion", NullValueHandling = NullValueHandling.Ignore)]
        public Int32 idLocalizacion { get; set; }

        /// <summary>
        /// Cuenta de Correo del Cliente
        /// </summary>
        [JsonProperty("cuenta_correo", NullValueHandling = NullValueHandling.Ignore)]
        public string cuenta_correo { get; set; }
        /// <summary>
        /// Codigo del Departamento
        /// </summary>
        [JsonProperty("Codigo_depto", NullValueHandling = NullValueHandling.Ignore)]
        public string Codigo_depto { get; set; }

        /// <summary>
        /// Nombre del Departamento
        /// </summary>
        [JsonProperty("Nombre_Depto", NullValueHandling = NullValueHandling.Ignore)]
        public string Nombre_Depto { get; set; }
        /// <summary>
        /// Codigo del municipio
        /// </summary>
        [JsonProperty("Codigo_Municipio", NullValueHandling = NullValueHandling.Ignore)]
        public string Codigo_Municipio { get; set; }
        /// <summary>
        /// Nombre del Municipio
        /// </summary>
        [JsonProperty("Nom_Municipio", NullValueHandling = NullValueHandling.Ignore)]
        public string Nom_Municipio { get; set; }

        /// <summary>
        /// Nunmero telefononico del Cliente
        /// </summary>
        [JsonProperty("Nro_Telefono", NullValueHandling = NullValueHandling.Ignore)]
        public string Nro_Telefono { get; set; }

        ///// <summary>
        ///// XXXXXX xxxxx
        ///// </summary>
        //[JsonProperty("XXXXXXXX", NullValueHandling = NullValueHandling.Ignore)]
        //public string xxxxx_XXXXX { get; set; }


    }
}