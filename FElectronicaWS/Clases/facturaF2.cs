using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    //public class facturaF2
    //{
    //}

    public class documentoRoot
    {
        [JsonProperty("documento", NullValueHandling = NullValueHandling.Ignore)]
        public Documento documento { get; set; }
        /// <summary>
        /// Informacion del Adquieriente.
        /// </summary>
        [JsonProperty("adquiriente", NullValueHandling = NullValueHandling.Ignore)]
        public Adquiriente adquiriente { get; set; }
        ///<summary>
        ///Totales de la Factura.
        /// </summary>
        [JsonProperty("totales", NullValueHandling = NullValueHandling.Ignore)]
        public Totales totales { get; set; }

        ///<summary>
        ///Relacion de Anticipos
        /// </summary>
        [JsonProperty("anticipos", NullValueHandling = NullValueHandling.Ignore)]
        public List<AnticiposItem> anticipos { get; set; }

        /// <summary>
        /// Relacion de Tributos que se liquidan en el Documento que se esta enviando. Impuestos o Retenciones
        /// </summary>
        [JsonProperty("tributos", NullValueHandling = NullValueHandling.Ignore)]
        public List<TributosItem> tributos { get; set; }

		[JsonProperty("extensionesSalud", NullValueHandling = NullValueHandling.Ignore)]
		public List<extensionSalud> extensionesSalud { get; set; }

        ///<summary>
        /// Detalle de las facturas o documento que se esta enviando
        ///</summary>
        [JsonProperty("detalles", NullValueHandling = NullValueHandling.Ignore)]
        public List<DetallesItem> detalles { get; set; }

        /// <summary>
        /// Documentos referenciados en las Notas Especialmente. En algunas facturas puede aplicar
        /// </summary>
        [JsonProperty("documentosReferencia", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocumentosReferenciaItem> documentosReferencia { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("informacionesAdicionales", NullValueHandling = NullValueHandling.Ignore)]
        public List<InformacionesAdicionalesItem> informacionesAdicionales { get; set; }
    }
    public class Documento
    {
        /// <summary>
        /// ID de Identificador de Transaccion
        /// </summary>
        [JsonProperty("identificadorTransaccion", NullValueHandling = NullValueHandling.Ignore)]
        public string identificadorTransaccion { get; set; }
        /// <summary>
        /// url del PDF de la factura
        /// </summary>
        [JsonProperty("URLPDF", NullValueHandling = NullValueHandling.Ignore)]
        public string URLPDF { get; set; }
        /// <summary>
        /// Nit del Hospital
        /// </summary>
        [JsonProperty("NITFacturador", NullValueHandling = NullValueHandling.Ignore)]
        public string NITFacturador { get; set; }
        /// <summary>
        /// Prefijo de facturacion Resgitrado en la DIAN
        /// </summary>
        [JsonProperty("prefijo", NullValueHandling = NullValueHandling.Ignore)]
        public string prefijo { get; set; }
        /// <summary>
        /// Numero del Documento que se esta enviando
        /// </summary>
        [JsonProperty("numeroDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string numeroDocumento { get; set; }
        /// <summary>
        /// Tipo del documento que se esta enviando: Factura,Nota Debito o Nota Credito
        /// </summary>
        [JsonProperty("tipoDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public int tipoDocumento { get; set; }
        /// <summary>
        /// Tipo de Operacion. Para HUSI:Generica=05
        /// </summary>

        [JsonProperty("tipoOperacion", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoOperacion { get; set; }
        /// <summary>
        /// Tipo de Operacion-Anexo Tecnico DIAN-6.1.3 Para Factura Nacional=01,Exportacion=02,Contingencia Facturador=03 Contingencia DIA=04, ND=92 y NC=91
        /// </summary>
        [JsonProperty("subTipoDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string subTipoDocumento { get; set; }
        /// <summary>
        /// No aplica para HUSI- Nombre de Plantilla, para generar Representacion Grafica
        /// </summary>
        [JsonProperty("plantilla", NullValueHandling = NullValueHandling.Ignore)]
        public string plantilla { get; set; }
        /// <summary>
        /// siempre se envia en TRUE
        /// </summary>
        [JsonProperty("generaRepresentacionGrafica", NullValueHandling = NullValueHandling.Ignore)]
        public bool generaRepresentacionGrafica { get; set; }
        /// <summary>
        /// Unidad de negocio, de donde se origina la Factura. No Obligatorio
        /// </summary>
        [JsonProperty("unidadNegocio", NullValueHandling = NullValueHandling.Ignore)]
        public string unidadNegocio { get; set; }
        /// <summary>
        /// Fecha de Emision de la Factura.
        /// </summary>
        [JsonProperty("fechaEmision", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaEmision { get; set; }
        /// <summary>
        /// Hora de Emision de la Factura
        /// </summary>
        [JsonProperty("horaEmision", NullValueHandling = NullValueHandling.Ignore)]
        public string horaEmision { get; set; }
        /// <summary>
        /// Fecha de Vencimiento de la Factura- No Plica HUSI(No Obligatorio)
        /// </summary>
        [JsonProperty("fechaVencimiento", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaVencimiento { get; set; }
        /// <summary>
        /// Notas asociadas a la emision de la factura
        /// </summary>
        [JsonProperty("notas", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> notas { get; set; }
        /// <summary>
        /// No aplica HUSI. periodos de Facturacion(No Obligatorio)
        /// </summary>
        [JsonProperty("fechaInicioPeriodo", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaInicioPeriodo { get; set; }
        /// <summary>
        /// No aplica HUSI. periodos de Facturacion(No Obligatorio)
        /// </summary>
        [JsonProperty("horaInicioPeriodo", NullValueHandling = NullValueHandling.Ignore)]
        public string horaInicioPeriodo { get; set; }
        /// <summary>
        /// No aplica HUSI. periodos de Facturacion(No Obligatorio)
        /// </summary>
        [JsonProperty("fechaFinPeriodo", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaFinPeriodo { get; set; }
        /// <summary>
        /// No aplica HUSI. periodos de Facturacion(No Obligatorio)
        /// </summary>
        [JsonProperty("horaFinPeriodo", NullValueHandling = NullValueHandling.Ignore)]
        public string horaFinPeriodo { get; set; }
        /// <summary>
        /// Codigo de Moneda general del Documento(Anexo 6.3.3)=COP
        /// </summary>
        [JsonProperty("moneda", NullValueHandling = NullValueHandling.Ignore)]
        public string moneda { get; set; }

        /// <summary>
        /// Lista  de Objetos Documentos afectados por las Notas Debito y Credito
        /// </summary>
        [JsonProperty("documentosAfectados", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocumentosAfectadosItem> documentosAfectados { get; set; }


        /// <summary>
        /// Codigo del Centro de Costo que emite la Factura (No Obligatorio)
        /// </summary>
        [JsonProperty("codigoCentroCostos", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoCentroCostos { get; set; }
        /// <summary>
        /// Nombre del Centro de Costos(No Obligatorio)
        /// </summary>
        [JsonProperty("descripcionCodigoCentroCostos", NullValueHandling = NullValueHandling.Ignore)]
        public string descripcionCodigoCentroCostos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("contactoEntrega", NullValueHandling = NullValueHandling.Ignore)]
        public ContactoEntrega contactoEntrega { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("notificaciones", NullValueHandling = NullValueHandling.Ignore)]
        public List<NotificacionesItem> notificaciones { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("entrega", NullValueHandling = NullValueHandling.Ignore)]
        public Entrega entrega { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigosBarras", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodigosBarrasItem> codigosBarras { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("TRM", NullValueHandling = NullValueHandling.Ignore)]
        public TRM TRM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("TRM2", NullValueHandling = NullValueHandling.Ignore)]
        public TRM2 TRM2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("formaPago", NullValueHandling = NullValueHandling.Ignore)]
        public FormaPago formaPago { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("plazosPago", NullValueHandling = NullValueHandling.Ignore)]
        public List<PlazosPago> plazosPago { get; set; }

    }
    
    public class DocumentosAfectadosItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("numeroDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string numeroDocumento { get; set; }
        /// <summary>
        /// Numero de la Factura o Nota que se Afecta
        /// </summary>
        [JsonProperty("codigoCausal", NullValueHandling = NullValueHandling.Ignore)]

        ///<summary>
        ///Codigo de Causal de la nota crédito o debito 
        /// </summary>
        public int codigoCausal { get; set; }
        /// <summary>
        /// CUFE o CUDE de la Factura o Nota a Afectada
        /// </summary>
        [JsonProperty("UUID", NullValueHandling = NullValueHandling.Ignore)]
        public string UUID { get; set; }

        /// <summary>
        /// fecha de la factura a afectar
        /// </summary>
        [JsonProperty("fecha", NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("observaciones", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> observaciones { get; set; }
    }

    public class DocumentosReferenciaItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipo", NullValueHandling = NullValueHandling.Ignore)]
        public int tipo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipoDocumento", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoDocumento { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("UUID", NullValueHandling = NullValueHandling.Ignore)]
        public string UUID { get; set; }
    }
    public class ContactoEntrega
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("nombre", NullValueHandling = NullValueHandling.Ignore)]
        public string nombre { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("notas", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> notas { get; set; }
    }

    public class NotificacionesItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipo", NullValueHandling = NullValueHandling.Ignore)]
        public int tipo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valor", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> valor { get; set; }
    }

    public class Ubicacion
    {
        /// <summary>
        /// Pais de la Ubicacion
        /// </summary>
        [JsonProperty("pais", NullValueHandling = NullValueHandling.Ignore)]
        public string pais { get; set; }
        /// <summary>
        /// Codigo del Municipio
        /// </summary>
        [JsonProperty("codigoMunicipio", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoMunicipio { get; set; }
        /// <summary>
        /// Nombre de la Ciudad
        /// </summary>
        [JsonProperty("ciudad", NullValueHandling = NullValueHandling.Ignore)]
        public string ciudad { get; set; }
        /// <summary>
        /// Nombre del Departamento
        /// </summary>
        [JsonProperty("departamento", NullValueHandling = NullValueHandling.Ignore)]
        public string departamento { get; set; }
        /// <summary>
        /// Nombre del Barrio
        /// </summary>
        [JsonProperty("barrio", NullValueHandling = NullValueHandling.Ignore)]
        public string barrio { get; set; }
        /// <summary>
        /// Direccion
        /// </summary>
        [JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)]
        public string direccion { get; set; }
        /// <summary>
        /// codigo Postal de la Direccion
        /// </summary>
        [JsonProperty("codigoPostal", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoPostal { get; set; }
    }

    public class Tercero
    {
        /// <summary>
        /// Numero de identificacion
        /// </summary>
        [JsonProperty("identificacion", NullValueHandling = NullValueHandling.Ignore)]
        public string identificacion { get; set; }
        /// <summary>
        /// tipo de Identificacion
        /// </summary>
        [JsonProperty("tipoIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
        public int tipoIdentificacion { get; set; }
        /// <summary>
        /// codigo Interno (ID)
        /// </summary>
        [JsonProperty("codigoInterno", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoInterno { get; set; }
        /// <summary>
        /// matricula mercantil del tercero
        /// </summary>
        [JsonProperty("matriculaMercantil", NullValueHandling = NullValueHandling.Ignore)]
        public int matriculaMercantil { get; set; }
        /// <summary>
        /// Numero de la Matricula mercantil
        /// </summary>
        [JsonProperty("CIIU", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> CIIU { get; set; }
        /// <summary>
        /// Codigo  CIUU de la Actividad en Camara de Comercio
        /// </summary>
        [JsonProperty("razonSocial", NullValueHandling = NullValueHandling.Ignore)]
        public string razonSocial { get; set; }
        /// <summary>
        /// Razon Social del Tercero
        /// </summary>
        [JsonProperty("sitio", NullValueHandling = NullValueHandling.Ignore)]
        public string sitio { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipoRegimen", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoRegimen { get; set; }
        /// <summary>
        /// Codigo del Tipo de Regimen
        /// </summary>
        [JsonProperty("responsabilidadesRUT", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> responsabilidadesRUT { get; set; }
        /// <summary>
        /// Codigo del tipo de Persona
        /// </summary>
        [JsonProperty("tipoPersona", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoPersona { get; set; }
        /// <summary>
        /// Objeto del tipo Ubicacion
        /// </summary>
        [JsonProperty("ubicacion", NullValueHandling = NullValueHandling.Ignore)]
        public Ubicacion ubicacion { get; set; }
        /// <summary>
        /// Listado de los tipos de impuesto aplicacbles
        /// </summary>
        [JsonProperty("tributos", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> tributos { get; set; }
    }

    public class Entrega
    {
        /// <summary>
        /// Fecha de la Entrega. Se utiliza en HUSI, Fecha de Egreso
        /// </summary>
        [JsonProperty("fecha", NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }
        /// <summary>
        /// Hora de la entrega, en HUSI, corresponde a la Hora de Egreso
        /// </summary>
        [JsonProperty("hora", NullValueHandling = NullValueHandling.Ignore)]
        public string hora { get; set; }
        /// <summary>
        /// Objeto de Ubicacion donde se atendio el Paciente
        /// </summary>
        [JsonProperty("ubicacion", NullValueHandling = NullValueHandling.Ignore)]
        public Ubicacion ubicacion { get; set; }
        /// <summary>
        /// Objeto Tipo Tercero
        /// </summary>
        [JsonProperty("tercero", NullValueHandling = NullValueHandling.Ignore)]
        public Tercero tercero { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("terminos", NullValueHandling = NullValueHandling.Ignore)]
        public string terminos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigoTerminos", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoTerminos { get; set; }
    }

    public class CodigosBarrasItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("referencia", NullValueHandling = NullValueHandling.Ignore)]
        public string referencia { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipo", NullValueHandling = NullValueHandling.Ignore)]
        public int tipo { get; set; }
    }

    public class TRM
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valor", NullValueHandling = NullValueHandling.Ignore)]
        public double valor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("monedaOrigen", NullValueHandling = NullValueHandling.Ignore)]
        public string monedaOrigen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("monedaDestino", NullValueHandling = NullValueHandling.Ignore)]
        public string monedaDestino { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fecha", NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }
    }

    public class TRM2
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valor", NullValueHandling = NullValueHandling.Ignore)]
        public double valor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("monedaOrigen", NullValueHandling = NullValueHandling.Ignore)]
        public string monedaOrigen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("monedaDestino", NullValueHandling = NullValueHandling.Ignore)]
        public string monedaDestino { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fecha", NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }
    }

    public class FormaPago
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipoPago", NullValueHandling = NullValueHandling.Ignore)]
        public int tipoPago { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigoMedio", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoMedio { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fechaVencimiento", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaVencimiento { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("notas", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> notas { get; set; }
    }

    public class PlazosPago
    {
        /// <summary>
        /// Codigo del tipo de Plazo de pago Utilizado
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        /// <summary>
        /// Unidades cuando sea el caso.
        /// </summary>
        [JsonProperty("unidades", NullValueHandling = NullValueHandling.Ignore)]
        public string unidades { get; set; }
        /// <summary>
        /// Valor de las Unidades reportadas
        /// </summary>
        [JsonProperty("valorUnidades", NullValueHandling = NullValueHandling.Ignore)]
        public int valorUnidades { get; set; }
        /// <summary>
        /// Notas aclaratorias a que hayya lugar
        /// </summary>
        [JsonProperty("notas", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> notas { get; set; }
    }

    public class InformacionesAdicionalesItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("nombre", NullValueHandling = NullValueHandling.Ignore)]
        public string nombre { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valor", NullValueHandling = NullValueHandling.Ignore)]
        public string valor { get; set; }
    }

    public class Adquiriente
    {
        /// <summary>
        /// identificacion del Adquiriente
        /// </summary>
        [JsonProperty("identificacion", NullValueHandling = NullValueHandling.Ignore)]
        public string identificacion { get; set; }
        /// <summary>
        /// Tipo de la Identificacion del Adquiriente
        /// </summary>
        [JsonProperty("tipoIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
        public int tipoIdentificacion { get; set; }
        /// <summary>
        /// Tipo de la Identificacion del Adquiriente
        /// </summary>
        [JsonProperty("codigoInterno", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoInterno { get; set; }
        /// <summary>
        /// Numero de matricula Mercantil
        /// </summary>
        [JsonProperty("matriculaMercantil", NullValueHandling = NullValueHandling.Ignore)]
        public int matriculaMercantil { get; set; }
        /// <summary>
        /// Codigo CIIU de la Actividad
        /// </summary>
        [JsonProperty("CIIU", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> CIIU { get; set; }
        /// <summary>
        /// Razon social del Adquiriente.
        /// </summary>
        [JsonProperty("razonSocial", NullValueHandling = NullValueHandling.Ignore)]
        public string razonSocial { get; set; }
        /// <summary>
        /// Nombre de la sucursal, si aplica
        /// </summary>
        [JsonProperty("nombreSucursal", NullValueHandling = NullValueHandling.Ignore)]
        public string nombreSucursal { get; set; }
        /// <summary>
        /// Direccion de Correo Electronico
        /// </summary>
        [JsonProperty("correo", NullValueHandling = NullValueHandling.Ignore)]
        public string correo { get; set; }
        /// <summary>
        /// Numero Telefonico de Contacto del Adquiriente
        /// </summary>
        [JsonProperty("telefono", NullValueHandling = NullValueHandling.Ignore)]
        public string telefono { get; set; }
        /// <summary>
        /// Lugar o sitio de Ubicacion del Adquiriente
        /// </summary>
        [JsonProperty("sitio", NullValueHandling = NullValueHandling.Ignore)]
        public string sitio { get; set; }
        /// <summary>
        /// Tipo de Regimen Tributrio del Adquiriente
        /// </summary>
        [JsonProperty("tipoRegimen", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoRegimen { get; set; }
        /// <summary>
        /// Responsabilidades Fiscales del Adquiriente
        /// </summary>
        [JsonProperty("responsabilidadesRUT", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> responsabilidadesRUT { get; set; }
        /// <summary>
        /// Tipo o Naturaleza de la Persona
        /// </summary>
        [JsonProperty("tipoPersona", NullValueHandling = NullValueHandling.Ignore)]
        public string tipoPersona { get; set; }
        /// <summary>
        /// Ubicacion
        /// </summary>
        [JsonProperty("ubicacion", NullValueHandling = NullValueHandling.Ignore)]
        public Ubicacion ubicacion { get; set; }
        /// <summary>
        /// Tributos que aplican al Adquiriente
        /// </summary>
        [JsonProperty("tributos", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> tributos { get; set; }
    }

    public class AnticiposItem
    {
        /// <summary>
        /// Numero de Comprobante del Anticipo
        /// </summary>
        [JsonProperty("comprobante", NullValueHandling = NullValueHandling.Ignore)]
        public string comprobante { get; set; }
        /// <summary>
        /// Valor del Anticipo
        /// </summary>
        [JsonProperty("valorAnticipo", NullValueHandling = NullValueHandling.Ignore)]
        public double valorAnticipo { get; set; }
        /// <summary>
        /// Moneda en que se liquida el valor del Anticipo
        /// </summary>
        [JsonProperty("valorAnticipoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorAnticipoMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fechaPago", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaPago { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("horaPago", NullValueHandling = NullValueHandling.Ignore)]
        public string horaPago { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fechaProcesamientoPago", NullValueHandling = NullValueHandling.Ignore)]
        public string fechaProcesamientoPago { get; set; }
    }

    public class CargosDescuentosItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("esCargo", NullValueHandling = NullValueHandling.Ignore)]
        public string esCargo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigo", NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("notas", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> notas { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorImporte", NullValueHandling = NullValueHandling.Ignore)]
        public double valorImporte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorImporteMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorImporteMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBase", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBaseMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBaseMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("porcentaje", NullValueHandling = NullValueHandling.Ignore)]
        public double porcentaje { get; set; }
    }
    #region Tributos factura

    /// <summary>
    /// Tributos de la Factura
    /// </summary>
    public class TributosItem  /// Tributos de la Factura
    {
        /// <summary>
        /// ID del item que corresponde.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }

        /// <summary>
        /// Nombre del Tributo
        /// </summary>
        [JsonProperty("nombre", NullValueHandling = NullValueHandling.Ignore)]
        public string nombre { get; set; }

        /// <summary>
        /// Indiocador si se trata de un Impuesto o una Retencion
        /// </summary>
        [JsonProperty("esImpuesto", NullValueHandling = NullValueHandling.Ignore)]
        public bool esImpuesto { get; set; }
        /// <summary>
        /// Valor Total que se esta reportando
        /// </summary>
        [JsonProperty("valorImporteTotal", NullValueHandling = NullValueHandling.Ignore)]
        public double valorImporteTotal { get; set; }
        /// <summary>
        /// Moneda en la cual se expresa el valor del importe
        /// </summary>
        [JsonProperty("valorImporteTotalMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorImporteTotalMoneda { get; set; }
        /// <summary>
        /// Detalles que correspondan
        /// </summary>
        [JsonProperty("detalles", NullValueHandling = NullValueHandling.Ignore)]
        public List<DetalleTributos> detalles { get; set; }
    }

    /// <summary>
    /// DEtalle de los Tributos de la Factura
    /// </summary>
    public class DetalleTributos ///Este es el Detalle de Lons Trubutos de la Factura(TributosItem).
    {
        ///// <summary>
        ///// 
        ///// </summary>
        //public string id { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string esImpuesto { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        [JsonProperty("valorImporte", NullValueHandling = NullValueHandling.Ignore)]
        public double valorImporte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorImporteMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorImporteMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBase", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBaseMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBaseMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("porcentaje", NullValueHandling = NullValueHandling.Ignore)]
        public double porcentaje { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoUnidades", NullValueHandling = NullValueHandling.Ignore)]
        public double tributoFijoUnidades { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoUnidadMedida", NullValueHandling = NullValueHandling.Ignore)]
        public string tributoFijoUnidadMedida { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoValorImporte", NullValueHandling = NullValueHandling.Ignore)]
        public double tributoFijoValorImporte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoValorImporteMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string tributoFijoValorImporteMoneda { get; set; }
    }

    #endregion
    public class Totales
    {
        /// <summary>
        /// Balor Bruto de la Factura (suma de los valores brutos de los items)
        /// </summary>
        [JsonProperty("valorBruto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBruto { get; set; }
        /// <summary>
        /// Moneda en que se reporta el Valor Bruto de la Factura
        /// </summary>
        [JsonProperty("valorBrutoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBrutoMoneda { get; set; }
        /// <summary>
        /// Valor delo cargos adicionales que se deban hacer en el valor de la Factura
        /// </summary>
        [JsonProperty("valorCargos", NullValueHandling = NullValueHandling.Ignore)]
        public double valorCargos { get; set; }
        /// <summary>
        /// Moneda en que se expresa el valor de los Cargos
        /// </summary>
        [JsonProperty("valorCargosMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorCargosMoneda { get; set; }
        /// <summary>
        /// valor de los descuentos comerciales aplicados a la factura
        /// </summary>
        [JsonProperty("valorDescuentos", NullValueHandling = NullValueHandling.Ignore)]
        public double valorDescuentos { get; set; }
        /// <summary>
        /// Moneda en que se expresa el valor de los decuentos aplicados
        /// </summary>
        [JsonProperty("valorDescuentosMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorDescuentosMoneda { get; set; }
        /// <summary>
        /// Valor delos Anticipos recibidos y que son aplicables a la Factura
        /// </summary>
        [JsonProperty("valorAnticipos", NullValueHandling = NullValueHandling.Ignore)]
        public double valorAnticipos { get; set; }
        /// <summary>
        /// Moneda en que se expresa el valor de los anticipos
        /// </summary>
        [JsonProperty("valorAnticiposMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorAnticiposMoneda { get; set; }
        /// <summary>
        /// Valor Toal de la Factura, antes de aplicar Impuestos
        /// </summary>
        [JsonProperty("valorTotalSinImpuestos", NullValueHandling = NullValueHandling.Ignore)]
        public double valorTotalSinImpuestos { get; set; }
        /// <summary>
        /// Moneda en al cual se enpresa el Valor sin Impuestos de la Factura
        /// </summary>
        [JsonProperty("valorTotalSinImpuestosMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorTotalSinImpuestosMoneda { get; set; }
        /// <summary>
        /// Valor toal de la Factura, incluyendo Impuestos
        /// </summary>
        [JsonProperty("valorTotalConImpuestos", NullValueHandling = NullValueHandling.Ignore)]
        public double valorTotalConImpuestos { get; set; }
        /// <summary>
        /// Moneda en la cual se expresa el valor total de la Factura con Impuestos
        /// </summary>
        [JsonProperty("valorTotalConImpuestosMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorTotalConImpuestosMoneda { get; set; }
        /// <summary>
        /// Valor Neto de la Factura
        /// </summary>
        [JsonProperty("valorNeto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorNeto { get; set; }
        /// <summary>
        /// Moneda en la cual se expresa el Valor Neto de La Factura
        /// </summary>
        [JsonProperty("valorNetoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorNetoMoneda { get; set; }

        /// <summary>
        /// Diferencia en Redondeos.
        /// </summary>
        [JsonProperty("valorRedondeo", NullValueHandling = NullValueHandling.Ignore)]
        public double valorRedondeo { get; set; }

    }

    public class DetalleTercero
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("identificacion", NullValueHandling = NullValueHandling.Ignore)]
        public string identificacion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipoIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
        public int tipoIdentificacion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("razonSocial", NullValueHandling = NullValueHandling.Ignore)]
        public string razonSocial { get; set; }
    }

    public class SubDetallesItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("objetoContratoAIU", NullValueHandling = NullValueHandling.Ignore)]
        public string objetoContratoAIU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorCodigoInterno", NullValueHandling = NullValueHandling.Ignore)]
        public string valorCodigoInterno { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigoEstandar", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoEstandar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorCodigoEstandar", NullValueHandling = NullValueHandling.Ignore)]
        public string valorCodigoEstandar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("descripcion", NullValueHandling = NullValueHandling.Ignore)]
        public string descripcion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidades", NullValueHandling = NullValueHandling.Ignore)]
        public double unidades { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidadMedida", NullValueHandling = NullValueHandling.Ignore)]
        public string unidadMedida { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidadesPorEmpaque", NullValueHandling = NullValueHandling.Ignore)]
        public int unidadesPorEmpaque { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("marca", NullValueHandling = NullValueHandling.Ignore)]
        public string marca { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("modelo", NullValueHandling = NullValueHandling.Ignore)]
        public string modelo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorUnitarioBruto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorUnitarioBruto { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorUnitarioBrutoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorUnitarioBrutoMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBruto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBruto { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBrutoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBrutoMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferencia", NullValueHandling = NullValueHandling.Ignore)]
        public double valorReferencia { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferenciaMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorReferenciaMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferenciaCodigo", NullValueHandling = NullValueHandling.Ignore)]
        public string valorReferenciaCodigo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("documentosReferencia", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocumentosReferenciaItem> documentosReferencia { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("cargosDescuentos", NullValueHandling = NullValueHandling.Ignore)]
        public List<CargosDescuentosItem> cargosDescuentos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributos", NullValueHandling = NullValueHandling.Ignore)]
        public List<TributosItem> tributos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("detalleTercero", NullValueHandling = NullValueHandling.Ignore)]
        public DetalleTercero detalleTercero { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("informacionesAdicionales", NullValueHandling = NullValueHandling.Ignore)]
        public List<InformacionesAdicionalesItem> informacionesAdicionales { get; set; }
    }

    public class DetallesItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tipoDetalle", NullValueHandling = NullValueHandling.Ignore)]
        public Int16 tipoDetalle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("objetoContratoAIU", NullValueHandling = NullValueHandling.Ignore)]
        public string objetoContratoAIU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string valorCodigoInterno { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("codigoEstandar", NullValueHandling = NullValueHandling.Ignore)]
        public string codigoEstandar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorCodigoEstandar", NullValueHandling = NullValueHandling.Ignore)]
        public string valorCodigoEstandar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("descripcion", NullValueHandling = NullValueHandling.Ignore)]
        public string descripcion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidades", NullValueHandling = NullValueHandling.Ignore)]
        public double unidades { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidadMedida", NullValueHandling = NullValueHandling.Ignore)]
        public string unidadMedida { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unidadesPorEmpaque", NullValueHandling = NullValueHandling.Ignore)]
        public int unidadesPorEmpaque { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("marca", NullValueHandling = NullValueHandling.Ignore)]
        public string marca { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("modelo", NullValueHandling = NullValueHandling.Ignore)]
        public string modelo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorUnitarioBruto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorUnitarioBruto { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorUnitarioBrutoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorUnitarioBrutoMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBruto", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBruto { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBrutoMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBrutoMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferencia", NullValueHandling = NullValueHandling.Ignore)]
        public double valorReferencia { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferenciaMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorReferenciaMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorReferenciaCodigo", NullValueHandling = NullValueHandling.Ignore)]
        public string valorReferenciaCodigo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("documentosReferencia", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocumentosReferenciaItem> documentosReferencia { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("cargosDescuentos", NullValueHandling = NullValueHandling.Ignore)]
        public List<CargosDescuentosItem> cargosDescuentos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributos", NullValueHandling = NullValueHandling.Ignore)]
        public List<TibutosDetalle> tributos { get; set; }                    /// Aqui van los tributos del Detalle. Por cada Producto.
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("detalleTercero", NullValueHandling = NullValueHandling.Ignore)]
        public DetalleTercero detalleTercero { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("informacionesAdicionales", NullValueHandling = NullValueHandling.Ignore)]
        public List<InformacionesAdicionalesItem> informacionesAdicionales { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("subDetalles", NullValueHandling = NullValueHandling.Ignore)]
        public List<SubDetallesItem> subDetalles { get; set; }
    }

    public class TibutosDetalle
    {
        /// <summary>
        /// ID del item que corresponde.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }

        /// <summary>
        /// Nombre del Tributo
        /// </summary>
        [JsonProperty("nombre", NullValueHandling = NullValueHandling.Ignore)]
        public string nombre { get; set; }

        /// <summary>
        /// Indiocador si se trata de un Impuesto o una Retencion
        /// </summary>
        [JsonProperty("esImpuesto", NullValueHandling = NullValueHandling.Ignore)]
        public bool esImpuesto { get; set; }
        /// <summary>
        /// Valor del Impuesto
        /// </summary>
        [JsonProperty("valorImporte", NullValueHandling = NullValueHandling.Ignore)]
        public double valorImporte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorImporteMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorImporteMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBase", NullValueHandling = NullValueHandling.Ignore)]
        public double valorBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("valorBaseMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string valorBaseMoneda { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("porcentaje", NullValueHandling = NullValueHandling.Ignore)]
        public double porcentaje { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoUnidades", NullValueHandling = NullValueHandling.Ignore)]
        public double tributoFijoUnidades { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoUnidadMedida", NullValueHandling = NullValueHandling.Ignore)]
        public string tributoFijoUnidadMedida { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoValorImporte", NullValueHandling = NullValueHandling.Ignore)]
        public double tributoFijoValorImporte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("tributoFijoValorImporteMoneda", NullValueHandling = NullValueHandling.Ignore)]
        public string tributoFijoValorImporteMoneda { get; set; }
    }

	public class extensionSalud
	{
		[JsonProperty("codigoPrestador", NullValueHandling = NullValueHandling.Ignore)]
		public string codigoPrestador{get;set;}

		[JsonProperty("tipoDocumentoIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
		public string tipoDocumentoIdentificacion{get;set;}
		
		[JsonProperty("numeroIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroIdentificacion{get;set;}
		
		[JsonProperty("primerApellido", NullValueHandling = NullValueHandling.Ignore)]
		public string primerApellido{get;set;}

		[JsonProperty("segundoApellido", NullValueHandling = NullValueHandling.Ignore)]
		public string segundoApellido{get;set;}

		[JsonProperty("primerNombre", NullValueHandling = NullValueHandling.Ignore)]
		public string primerNombre{get;set;}

		[JsonProperty("otrosNombres", NullValueHandling = NullValueHandling.Ignore)]
		public string otrosNombres{get;set;}

		[JsonProperty("tipoDeUsuario", NullValueHandling = NullValueHandling.Ignore)]
		public string tipoDeUsuario{get;set;}

		[JsonProperty("modalidadesContratacion", NullValueHandling = NullValueHandling.Ignore)]
		public string modalidadesContratacion{get;set;}

		[JsonProperty("cobertura", NullValueHandling = NullValueHandling.Ignore)]
		public string cobertura{get;set;}
		
		[JsonProperty("numeroAutorizacion", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroAutorizacion{get;set;}

		[JsonProperty("numeroMIPRES", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroMIPRES{get;set;}

		[JsonProperty("numeroIdPrescripcion", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroIdPrescripcion{get;set;}

		[JsonProperty("numeroContrato", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroContrato { get; set; }

		[JsonProperty("numeroPoliza", NullValueHandling = NullValueHandling.Ignore)]
		public string numeroPoliza { get; set; }

		[JsonProperty("fechaInicioFacturacion", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime fechaInicioFacturacion { get; set; }

		[JsonProperty("fechaFinFacturacion", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime fechaFinFacturacion { get; set; }

		[JsonProperty("copago", NullValueHandling = NullValueHandling.Ignore)]
		public Int64 copago { get; set; }

		[JsonProperty("cuotaModeradora", NullValueHandling = NullValueHandling.Ignore)]
		public Int64 cuotaModeradora { get; set; }

		[JsonProperty("cuotaRecuperacion", NullValueHandling = NullValueHandling.Ignore)]
		public Int64 cuotaRecuperacion { get; set; }

		[JsonProperty("pagosCompartidos", NullValueHandling = NullValueHandling.Ignore)]
		public Int64 pagosCompartidos { get; set; }

	}






    //public class Root
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Documento documento { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Adquiriente adquiriente { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<AnticiposItem> anticipos { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<CargosDescuentosItem> cargosDescuentos { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<TributosItem> tributos { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Totales totales { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<DetallesItem> detalles { get; set; }
    //}


}