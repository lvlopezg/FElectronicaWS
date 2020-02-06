using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public interface ClienteInternacional
    {
        /// <summary>
        /// idContrato que respalda la factura
        /// </summary>
        [JsonProperty("idContrato", NullValueHandling = NullValueHandling.Ignore)]
        public int idContrato { get; set; }

        /// <summary>
        /// Fecha de la Factura
        /// </summary>
        [JsonProperty("FecFactura", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime FecFactura { get; set; }
        /// <summary>
        /// idusuario que elabora la Factura
        /// </summary>
        [JsonProperty("IdUsuarioR", NullValueHandling = NullValueHandling.Ignore)]
        public int IdUsuarioR { get; set; }
        /// <summary>
        /// Nombre de usuario que elabora la Factura
        /// </summary>
        [JsonProperty("nom_usua", NullValueHandling = NullValueHandling.Ignore)]
        public string nom_usua { get; set; }
        /// <summary>
        /// Numero del documento de Identidad del usuario Que elabora la Factura
        /// </summary>
        [JsonProperty("NumDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string NumDocumento { get; set; }
        /// <summary>
        /// Tipo del Documento de identidad del Usuario
        /// </summary>
        [JsonProperty("IdTipoDoc", NullValueHandling = NullValueHandling.Ignore)]
        public byte IdTipoDoc { get; set; }
        /// <summary>
        /// Nombre del Tercero a Quien se factura
        /// </summary>
        [JsonProperty("NomTercero", NullValueHandling = NullValueHandling.Ignore)]
        public string NomTercero { get; set; }
        /// <summary>
        /// ID de identificaciond el Tercero en SAHI
        /// </summary>
        [JsonProperty("IdTercero", NullValueHandling = NullValueHandling.Ignore)]
        public int IdTercero { get; set; }
        /// <summary>
        /// id del Regimen tributario del Cliente
        /// </summary>
        [JsonProperty("idRegimen", NullValueHandling = NullValueHandling.Ignore)]
        public string idRegimen { get; set; }
        /// <summary>
        /// Id de la naturaleza del Cliente
        /// </summary>
        [JsonProperty("IdNaturaleza", NullValueHandling = NullValueHandling.Ignore)]
        public Int16 IdNaturaleza { get; set; }

        /// <summary>
        /// Direccion del Cliente
        /// </summary>
        [JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)]
        public string direccion { get; set; }

        /// <summary>
        /// Nombre de la ciudad del Cliente
        /// </summary>
        [JsonProperty("ciudad", NullValueHandling = NullValueHandling.Ignore)]
        public string ciudad { get; set; }

        /// <summary>
        /// Id de la ciudad
        /// </summary>
        [JsonProperty("idLugar", NullValueHandling = NullValueHandling.Ignore)]
        public int idLugar { get; set; }

        /// <summary>
        /// Codigo del Municipio en Colombia
        /// </summary>
        [JsonProperty("codMunicipio", NullValueHandling = NullValueHandling.Ignore)]
        public string codMunicipio { get; set; }

        /// <summary>
        /// Numero Telefonico
        /// </summary>
        [JsonProperty("telefono", NullValueHandling = NullValueHandling.Ignore)]
        public string telefono { get; set; }

        /// <summary>
        /// Codigo del Departamento
        /// </summary>
        [JsonProperty("Codigo_depto", NullValueHandling = NullValueHandling.Ignore)]
        public string Codigo_depto { get; set; }

        /// <summary>
        /// Codigo del Departamento
        /// </summary>
        [JsonProperty("codigoPais", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoPais { get; set; }

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
        /// Cuenta de Correo del Cliente
        /// </summary>
        [JsonProperty("cuenta_correo", NullValueHandling = NullValueHandling.Ignore)]
        public string cuenta_correo { get; set; }
        /// <summary>
        /// Numero de Documento del Cliente
        /// </summary>
        [JsonProperty("NroDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public string NroDoc_Cliente { get; set; }
        /// <summary>
        /// Tipo de Documento del Cliente
        /// </summary>
        [JsonProperty("TipoDoc_Cliente", NullValueHandling = NullValueHandling.Ignore)]
        public byte TipoDoc_Cliente { get; set; }
    }
}