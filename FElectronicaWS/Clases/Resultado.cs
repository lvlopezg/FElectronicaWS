using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public class Resultado
    {
        /// <summary>
        /// Identificador Unico Para Transaccion Factrura electronica
        /// </summary>
        [JsonProperty("identificadorTransaccion", NullValueHandling = NullValueHandling.Ignore)]
        public string identificadorTransaccion { get; set; }
        /// <summary>
        /// Numero de la Factura o Nota
        /// </summary>
        [JsonProperty("numeroDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string numeroDocumento { get; set; }
        /// <summary>
        /// Fecha de Validacion Realizada por la DIAN
        /// </summary>
        [JsonProperty("fechaValidacionDian", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaValidacionDian { get; set; }
        /// <summary>
        /// Identificador UUID Establecido por la DIAN
        /// </summary>
        [JsonProperty("UUID", NullValueHandling = NullValueHandling.Ignore)]
        public string UUID { get; set; }
        /// <summary>
        /// Direccion para Descarga del PDF
        /// </summary>
        [JsonProperty("URLPDF", NullValueHandling = NullValueHandling.Ignore)]
        public string URLPDF { get; set; }
        /// <summary>
        /// Direccion para Descarga del archivo XML de la Factura
        /// </summary>
        [JsonProperty("URLXML", NullValueHandling = NullValueHandling.Ignore)]
        public string URLXML { get; set; }
        /// <summary>
        /// contenido del Codigo QR de la Factura
        /// </summary>
        [JsonProperty("QR", NullValueHandling = NullValueHandling.Ignore)]
        public string QR { get; set; }
    }

    public class ErroresItem
    {
        /// <summary>
        /// Codigo de Error
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        /// <summary>
        /// Mensaje del Error
        /// </summary>
        [JsonProperty("mensaje", NullValueHandling = NullValueHandling.Ignore)]
        public string mensaje { get; set; }
    }

    public class AdvertenciasItem
    {
        /// <summary>
        /// Codigo de la Advertencia
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        /// <summary>
        /// Mensaje Descriptivo de la Advertencia
        /// </summary>
        [JsonProperty("mensaje", NullValueHandling = NullValueHandling.Ignore)]
        public string mensaje { get; set; }
    }

    public class RespuestaTransfiriendo
    {
        /// <summary>
        /// Resultado de Exito de la Transaccion
        /// </summary>
        /// 
        [JsonProperty("esExitoso",NullValueHandling = NullValueHandling.Ignore)]
        public bool esExitoso { get; set; }
        /// <summary>
        /// Mensaje que corresponde al Resultado de la Transaccion
        /// </summary>
        [JsonProperty("mensaje", NullValueHandling = NullValueHandling.Ignore)]
        public string mensaje { get; set; }
        /// <summary>
        /// FEcha del Mensaje de Respuesta
        /// </summary>
        [JsonProperty("fecha", NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }
        /// <summary>
        /// Codigo de la Respuesta de la Transaccion
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public int codigo { get; set; }
        /// <summary>
        /// Contenido del Resultado
        /// </summary>
        [JsonProperty("resultado", NullValueHandling = NullValueHandling.Ignore)]
        public Resultado resultado { get; set; }
        /// <summary>
        /// Listado de Errores Cuando se Presentan
        /// </summary>
        [JsonProperty("errores", NullValueHandling = NullValueHandling.Ignore)]
        public List<ErroresItem> errores { get; set; }
        /// <summary>
        /// Listado de Advertencias
        /// </summary>
        [JsonProperty("advertencias", NullValueHandling = NullValueHandling.Ignore)]
        public List<AdvertenciasItem> advertencias { get; set; }
    }

}