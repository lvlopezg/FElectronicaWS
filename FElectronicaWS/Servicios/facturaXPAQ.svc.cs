using FElectronicaWS.Clases;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FElectronicaWS.Servicios
{
    public class facturaXPAQ : IfacturaXPAQ
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura)
        {
            logFacturas.Info($"Se recibe factura con siguientes datos:Factura por Paquete:{ nroFactura}   IdCliente:{ idCliente}  nroAtencion:{ nroAtencion} urlPdf:{ urlPdfFactura}");
            try
            {
                Int32 _idContrato = 0;
                Decimal _Valtotal = 0;
                Decimal _ValDescuento = 0;
                Decimal _ValDescuentoT = 0;
                Decimal _ValPagos = 0;
                Decimal _ValImpuesto = 0;
                //Int32 totalIvaFactura = 0;
                Decimal _ValCobrar = 0;
                DateTime _FecFactura = DateTime.Now;
                Decimal _valPos = 0;
                Decimal _valNoPos = 0;
                Int32 _IdUsuarioR = 0;
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                Byte _usrIdTipoDoc = 0;
                string _numDocCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
                Int32 _idTercero = 0;
                string _RegimenFiscal = string.Empty;
                Int16 _idNaturaleza = 0;
                int concepto = 0;
                FormaPago formaPagoTmp = new FormaPago();
                List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();

                //Fin de Inicializacion

                documentoRoot documentoF2 = new documentoRoot();
                Documento facturaEnviar = new Documento();
                facturaEnviar.identificadorTransaccion = "D7F719C2 - 75F4 - 4F06 - B7CB - F583FC28DBEE";
                facturaEnviar.URLPDF = urlPdfFactura;
                facturaEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                facturaEnviar.prefijo = Properties.Settings.Default.Prefijo;
                facturaEnviar.numeroDocumento = nroFactura.ToString();
                facturaEnviar.tipoDocumento = 1;
                facturaEnviar.subTipoDocumento = "01";
                facturaEnviar.tipoOperacion = "10";
                facturaEnviar.generaRepresentacionGrafica = false;

                //ClienteJuridico cliente = new ClienteJuridico();
                string urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteJuridico?idFactura={nroFactura}";
                logFacturas.Info("URL de Request:" + urlClientes);
                HttpWebRequest peticion = WebRequest.Create(urlClientes) as HttpWebRequest;
                peticion.Method = "GET";
                peticion.ContentType = "application/json";
                HttpWebResponse respuestaClientes = peticion.GetResponse() as HttpWebResponse;
                StreamReader lectorDatos = new StreamReader(respuestaClientes.GetResponseStream());
                string infCliente = lectorDatos.ReadToEnd();
                logFacturas.Info("Cliente:" + infCliente);
                Cliente cliente = JsonConvert.DeserializeObject<Cliente>(infCliente);

                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryFacturaEnc = @"SELECT fact.idContrato,Valtotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,FecFactura,valPos,valNoPos,fact.IdUsuarioR,
usr.nom_usua,usr.NumDocumento,usr.IdTipoDoc,ter.NumDocumento,ter.IdTipoDoc,ter.NomTercero,ter.CodTercero,con.NomRepComercial,ter.IdTercero,ter.idRegimen,ter.IdNaturaleza  
FROM facFactura fact
INNER JOIN ASI_USUA usr ON fact.IdUsuarioR = usr.IdUsuario
INNER JOIN conContrato con ON fact.IdContrato=con.IdContrato
INNER JOIN genTercero ter ON con.IdTercero=ter.IdTercero
WHERE IdFactura =  @nroFactura";
                    SqlCommand cmdFacturaEnc = new SqlCommand(qryFacturaEnc, conn);
                    cmdFacturaEnc.Parameters.Add("@nroFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdFacturaEnc = cmdFacturaEnc.ExecuteReader();
                    if (rdFacturaEnc.HasRows)
                    {
                        rdFacturaEnc.Read();
                        _idContrato = rdFacturaEnc.GetInt32(0);
                        _Valtotal = Math.Round(rdFacturaEnc.GetDecimal(1), 0);
                        _ValDescuento = Math.Round(rdFacturaEnc.GetDecimal(2), 0);
                        _ValDescuentoT = Math.Round(rdFacturaEnc.GetDecimal(3), 0);
                        _ValPagos = Math.Round(rdFacturaEnc.GetDecimal(4), 0);
                        _ValImpuesto = Math.Round(rdFacturaEnc.GetDecimal(5), 0);
                        _ValCobrar = Math.Round(rdFacturaEnc.GetDecimal(6), 0);
                        _FecFactura = rdFacturaEnc.GetDateTime(7);
                        _valPos = Math.Round(rdFacturaEnc.GetDecimal(8), 0);
                        _valNoPos = Math.Round(rdFacturaEnc.GetDecimal(9), 0);
                        _IdUsuarioR = rdFacturaEnc.GetInt32(10);
                        _usrNombre = rdFacturaEnc.GetString(11);
                        _usrNumDocumento = rdFacturaEnc.GetString(12);
                        _usrIdTipoDoc = rdFacturaEnc.GetByte(13);
                        _numDocCliente = rdFacturaEnc.GetString(14);
                        _tipoDocCliente = rdFacturaEnc.GetByte(15);
                        _razonSocial = rdFacturaEnc.GetString(16);
                        _repLegal = rdFacturaEnc.GetString(18);
                        _idTercero = rdFacturaEnc.GetInt32(19);
                        _RegimenFiscal = rdFacturaEnc.GetString(20);
                        _idNaturaleza = rdFacturaEnc.GetInt16(21);
                    }

                    if (_ValPagos > 0)
                    {
                        string Consultapagos = "SELECT IdConcepto,Valor FROM facFacAtenConcepto WHERE IdFactura=@idFactura";
                        SqlCommand cmdConsultaPagos = new SqlCommand(Consultapagos, conn);
                        cmdConsultaPagos.Parameters.Add("@idfactura", SqlDbType.Int).Value = nroFactura;
                        SqlDataReader rdConsultapagos = cmdConsultaPagos.ExecuteReader();
                        if (rdConsultapagos.HasRows)
                        {
                            while (rdConsultapagos.Read())
                            {
                                AnticiposItem anticipoWrk = new AnticiposItem();
                                anticipoWrk.comprobante = $"{rdConsultapagos.GetInt32(0)}-{nroFactura}";
                                anticipoWrk.valorAnticipo = double.Parse(rdConsultapagos.GetDouble(1).ToString());
                                anticiposWrk.Add(anticipoWrk);
                            }
                        }
                    }
                }

                string formatoWrk = formatosFecha.formatofecha(_FecFactura);
                facturaEnviar.fechaEmision = formatoWrk.Split('T')[0];
                facturaEnviar.horaEmision = formatoWrk.Split('T')[1];
                facturaEnviar.moneda = "COP";
                formaPagoTmp.tipoPago = 1;
                formaPagoTmp.codigoMedio = "10";
                facturaEnviar.formaPago = formaPagoTmp;

                List<DetallesItem> detalleProductos = new List<DetallesItem>();
                //****************** CLIENTE
                //  Variables Inicializacion
                string _direccionCliente = string.Empty;
                string _telefonoCliente = string.Empty;
                string _municipioCliente = string.Empty;
                string _departamento = string.Empty;
                int _localizacionCliente = 0;
                string _correoCliente = string.Empty;
                facturaEnviar.fechaEmision = formatoWrk.Split('T')[0];
                facturaEnviar.horaEmision = formatoWrk.Split('T')[1];
                facturaEnviar.moneda = "COP";
                formaPagoTmp.tipoPago = 1;
                formaPagoTmp.codigoMedio = "10";
                facturaEnviar.formaPago = formaPagoTmp;
                using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connx.Open();
                    string qryDatosCliente1 = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar,RIGHT(B.cod_dipo,5) FROM genTerceroLocaliza A
LEFT JOIN GEN_DIVI_POLI B ON A.IdLugar=B.IdLugar
WHERE IdTercero=@idTercero and IdLocalizaTipo IN (2,3)
ORDER BY IdLocalizaTipo";
                    SqlCommand cmdDatosCliente1 = new SqlCommand(qryDatosCliente1, connx);
                    cmdDatosCliente1.Parameters.Add("@idTercero", SqlDbType.Int).Value = _idTercero;
                    SqlDataReader rdDatosCliente1 = cmdDatosCliente1.ExecuteReader();
                    if (rdDatosCliente1.HasRows)
                    {
                        while (rdDatosCliente1.Read())
                        {
                            if (rdDatosCliente1.GetInt32(0) == 2)
                            {
                                _direccionCliente = rdDatosCliente1.GetString(1);
                                _municipioCliente = rdDatosCliente1.GetString(4);
                                _localizacionCliente = rdDatosCliente1.GetInt32(3);
                            }
                            else if (rdDatosCliente1.GetInt32(0) == 3)
                            {
                                _telefonoCliente = rdDatosCliente1.GetString(1);
                                if (_telefonoCliente.Length > 10)
                                {
                                    _telefonoCliente = _telefonoCliente.Substring(0, 10);
                                }
                            }
                        }
                    }
                    string qryDatosCliente2 = @"SELECT COD_DEPTO,COD_MPIO,DPTO,NOM_MPIO FROM GEN_DIVI_POLI A
                    INNER JOIN HUSI_Divipola HB ON a.num_ptel=COD_DEPTO
                    WHERE a.IdLugar=@idLugar";
                    SqlCommand cmdDatosCliente2 = new SqlCommand(qryDatosCliente2, connx);
                    cmdDatosCliente2.Parameters.Add("@idLugar", SqlDbType.Int).Value = _localizacionCliente;
                    SqlDataReader rdDatosCliente2 = cmdDatosCliente2.ExecuteReader();
                    if (rdDatosCliente2.HasRows)
                    {
                        rdDatosCliente2.Read();
                        _departamento = rdDatosCliente2.GetString(2);
                    }
                    string qryDatosCliente3 = @"SELECT A.Correo FROM concontratocorreo A
INNER JOIN facFactura B ON A.IdContrato=B.IdContrato
WHERE A.indhabilitado=1 AND B.idFactura=@idFactura
UNION ALL
SELECT A.Deslocalizacion As Correo FROM gentercerolocaliza A
INNER JOIN conContrato C ON C.IdTercero=A.IdTercero
INNER JOIN  facFactura D ON D.IdContrato=C.IdContrato
LEFT JOIN concontratocorreo B ON  B.indhabilitado=1 and B.idcontrato=D.IdContrato  
WHERE B.idcontrato is null and A.IdLocalizaTipo=1 and A.indhabilitado=1 and D.IdFactura=@idFactura";
                    SqlCommand cmdDatosCliente3 = new SqlCommand(qryDatosCliente3, connx);
                    cmdDatosCliente3.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdDatosCliente3 = cmdDatosCliente3.ExecuteReader();
                    if (rdDatosCliente3.HasRows)
                    {
                        rdDatosCliente3.Read();
                        _correoCliente = rdDatosCliente3.GetString(0);
                    }
                    else
                    {
                        _correoCliente = "";
                    }
                }

                List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                NotificacionesItem notificaItem = new NotificacionesItem();
                notificaItem.tipo = 1;
                List<string> valorNotificacion = new List<string>();
                valorNotificacion.Add(_correoCliente.Trim());
                notificaItem.valor = valorNotificacion;
                notificaciones.Add(notificaItem);
                facturaEnviar.notificaciones = notificaciones;

                Adquiriente adquirienteTmp = new Adquiriente();
                adquirienteTmp.identificacion = _numDocCliente;

                if (_tipoDocCliente == 1)//TODO: validar la Homologacion para este campo
                {
                    adquirienteTmp.tipoIdentificacion = 31;
                }
                else if (_tipoDocCliente == 2)
                {
                    adquirienteTmp.tipoIdentificacion = 13;
                }
                adquirienteTmp.codigoInterno = _idTercero.ToString();
                adquirienteTmp.razonSocial = _razonSocial;
                adquirienteTmp.nombreSucursal = _razonSocial;
                adquirienteTmp.correo = _correoCliente.Trim().Split(';')[0];
                adquirienteTmp.telefono = _telefonoCliente;

                if (_RegimenFiscal.Equals("C"))
                {
                    adquirienteTmp.tipoRegimen = "48";
                }
                else
                {
                    adquirienteTmp.tipoRegimen = "49";
                }
                //TODO: Aqui insertar lo que se defina de Responsabilidades  RUT documentoF2.adquiriente.responsabilidadesRUT
                if (_idNaturaleza == 3)
                {
                    adquirienteTmp.tipoPersona = "1";
                }
                else if (_idNaturaleza == 4)
                {
                    adquirienteTmp.tipoPersona = "2";
                }
                else
                {
                    adquirienteTmp.tipoPersona = "0";
                }
                List<string> responsanbilidadesR = new List<string>();
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conexion01.Open();
                    SqlCommand sqlValidaDet = new SqlCommand("spTerceroResponsabilidadRut", conexion01);
                    sqlValidaDet.CommandType = CommandType.StoredProcedure;
                    sqlValidaDet.Parameters.Add("@idtercero", SqlDbType.Int).Value = _idTercero;
                    SqlDataReader rdValidaDet = sqlValidaDet.ExecuteReader();
                    if (rdValidaDet.HasRows)
                    {
                        rdValidaDet.Read();
                        responsanbilidadesR.Add(rdValidaDet.GetString(0));
                    }
                    else
                    {
                        responsanbilidadesR.Add("R-99-PN");
                    }
                }
                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = "CO";
                ubicacionCliente.codigoMunicipio = _municipioCliente;
                ubicacionCliente.direccion = _direccionCliente;
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;

                //TODO:Definir Los pagos por Copago y Cuota Moderadora, si se envian como Anticipos y Abonos
                //TODO:Definir si aplica para algun caso. Cargos y Descuentos
                //List<TributosItem> tributosTMP = new List<TributosItem>();
                //List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                //DetalleTributos detalleTributos = new DetalleTributos() // Un Objeto por cada Tipo de Iva
                //{
                //    valorImporte = 0,
                //    valorBase = double.Parse(_Valtotal.ToString()),
                //    porcentaje = 0
                //};

                //tributosDetalle.Add(detalleTributos);

                //TributosItem itemTributo = new TributosItem()
                //{
                //    id = "01", //Total de Iva
                //    nombre = "Iva",
                //    esImpuesto = true,
                //    valorImporteTotal = 0,
                //    detalles = tributosDetalle // DEtalle de los Ivas
                //};
                //tributosTMP.Add(itemTributo);
                //documentoF2.tributos = tributosTMP;
                double TotalGravadoIva = 0;
                double TotalGravadoIca = 0;
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary>

                ////List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                ////AnticiposItem anticipoWrk = new AnticiposItem();
                ////anticipoWrk.comprobante = "22";
                ////anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                ////anticiposWrk.Add(anticipoWrk);
                documentoF2.anticipos = anticiposWrk;

        #region Extension Salud
        //********************* Extension Sector Salud ****************************************************************
        extensionSalud itemExtensionSalud = new extensionSalud();  //TODO: Implementacion de Campos de Salud
        List<extensionSalud> extencionesSalud = new List<extensionSalud>();

        using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
        {

          //****Prestador
          conn.Open();
          string qryPrestador = "SELECT ValConstanteT FROM genConstante WHERE CodConstante =@CodConstante";
          SqlCommand cmdPrestador = new SqlCommand(qryPrestador, conn);
          cmdPrestador.Parameters.Add("", SqlDbType.VarChar).Value = "CPFE";
          SqlDataReader rdPrestador = cmdPrestador.ExecuteReader();
          if (rdPrestador.HasRows)
          {
            rdPrestador.Read();
            itemExtensionSalud.codigoPrestador = rdPrestador.GetString(0);
          }
          else
          {
            logFacturas.Warn("!! No fue posible obtener el codigo de Prestador !!");
          }

          //*********Numero y Tipo de Documento y Nombres y Apellidos Campos 2,3,4,5,6,7
          string qryDatosPac = "SELECT d.CodTipoDoc , c.NumDocumento, c.ApeCliente, c.NomCliente FROM facFactura a " +
            "INNER JOIN admAtencion b on b.IdAtencion = a.IdDestino" +
            "INNER JOIN admCliente c on c.IdCliente = b.IdCliente" +
            "INNER JOIN genTipoDoc d on d.IdTipoDoc = c.IdTipoDoc" +
            "WHERE a.IdFactura =@idFactura";

          SqlCommand cmdPaciente = new SqlCommand(qryDatosPac, conn);
          cmdPaciente.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdPaciente = cmdPaciente.ExecuteReader();
          if (rdPaciente.HasRows)
          {
            rdPaciente.Read();
            itemExtensionSalud.tipoDocumentoIdentificacion = rdPaciente.GetString(0).Equals("NV") ? "CN" : rdPaciente.GetString(0);
            itemExtensionSalud.numeroIdentificacion = rdPaciente.GetString(1);
            string[ ] apellidos = UtilidadesFactura.separarApellidos(rdPaciente.GetString(2));
            string[ ] nombres = UtilidadesFactura.separarNombres(rdPaciente.GetString(3));
          }
          //********* Tipo de Usuario y Poliza: Campo 8 y 15
          string qryTipoUsua = "SELECT c.idTipoUsuario,C.numPoliza,*  ----esta sin definir en la Tabla  admAtencionContrato" +
            "FROM facFactura a" +
            "INNER JOIN admAtencion b on b.IdAtencion = a.IdDestino" +
            "INNER JOIN admAtencionContrato c on c.IdAtencion = b.IdAtencion and c.OrdPrioridad = 1" +
            "INNER JOIN genLista d on d.IdLista = c.idTipoUsuario" +
            "WHERE a.IdFactura = @idFactura ";
          SqlCommand cmdTipoUsua = new SqlCommand(qryTipoUsua, conn);
          cmdTipoUsua.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdTipoUsua = cmdTipoUsua.ExecuteReader();
          if (rdTipoUsua.HasRows)
          {
            rdTipoUsua.Read();
            itemExtensionSalud.tipoDeUsuario = rdTipoUsua.GetInt16(0).ToString();
            itemExtensionSalud.numeroPoliza = rdTipoUsua.GetInt16(1).ToString();
          }

          //********* Campo 9
          string qryContyPago = "SELECT a.idContratoYpago" +
            "FROM facFactura a" +
            "INNER JOIN genLista d on d.IdLista = a.idContratoYpago" +
            "WHERE a.idFactura = @idFactura";//TODO:Origen de NOmbre de Modalidades de Contratacion
          SqlCommand cmdContyPaso = new SqlCommand(qryContyPago, conn);
          cmdContyPaso.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdContyPaso = cmdContyPaso.ExecuteReader();
          if (rdContyPaso.HasRows)
          {
            rdContyPaso.Read();
            itemExtensionSalud.modalidadesContratacion = rdContyPaso.GetString(0).ToString();
          }
          //*************** Campo 10 - Cobertura 
          string qryCobertura = "";//TODO: DEfinir Campo Cobertura
          SqlCommand cmdCobertura = new SqlCommand(qryCobertura, conn);
          cmdCobertura.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdCobertura = cmdCobertura.ExecuteReader();
          if (rdCobertura.HasRows)
          {
            rdCobertura.Read();
            itemExtensionSalud.cobertura = rdCobertura.GetString(0);
          }

          //*************** Campo 11 - Autorizaciones
          string qryAutorizaciones = "";
          SqlCommand cmdAutorizaciones = new SqlCommand(qryAutorizaciones, conn);
          cmdAutorizaciones.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdAutorizaciones = cmdAutorizaciones.ExecuteReader();
          if (rdAutorizaciones.HasRows)
          {
            List<string> Autorizaciones = new List<string>();
            while (rdAutorizaciones.Read())
            {
              Autorizaciones.Add(rdAutorizaciones.GetString(0));
            }
            itemExtensionSalud.numeroAutorizacion = Autorizaciones;
          }

          // Campo 12 - Nro mi Pres
          string qryNroMiPres = "";
          SqlCommand cmdNroMiPres = new SqlCommand(qryNroMiPres, conn);
          cmdNroMiPres.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdNroMiPres = cmdNroMiPres.ExecuteReader();
          if (rdNroMiPres.HasRows)
          {
            List<string> NumerosMiPres = new List<string>();
            while (rdNroMiPres.Read())
            {
              NumerosMiPres.Add(rdNroMiPres.GetString(0));
            }
            itemExtensionSalud.numeroMIPRES = NumerosMiPres;
          }
          //**************** Campo Nro 13 Numero id Suministro
          string qryNroSuministro = "";
          SqlCommand cmdNroSuministro = new SqlCommand(qryNroSuministro, conn);
          cmdNroSuministro.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdNroSuministro = cmdNroSuministro.ExecuteReader();
          if (rdNroSuministro.HasRows)
          {
            List<string> NumerosSuministro = new List<string>();
            while (rdNroSuministro.Read())
            {
              NumerosSuministro.Add(rdNroMiPres.GetString(0));
            }
            itemExtensionSalud.numeroIdPrescripcion = NumerosSuministro;
          }

          //****************** Campo Numero 14 Numero de Contrato
          string qryContrato = "SELECT NumeroContrato FROM conContrato";
          SqlCommand cmdContrato = new SqlCommand(qryContrato, conn);
          cmdContrato.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
          SqlDataReader rdContrato = cmdContrato.ExecuteReader();
          if (rdContrato.HasRows)
          {
            rdContrato.Read();
            itemExtensionSalud.numeroContrato = rdContrato.GetString(0);
          }

          //**************** Campos Numero 15 y 16 Fecha de Inicio y Fecha Final de Atencion
          string qryAtencion = "SELECT FecIngreso,FecEgreso FROM admAtencion WHERE IdAtencion=@idAtencion";
          SqlCommand cmdAtencion = new SqlCommand(qryAtencion, conn);
          cmdAtencion.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
          SqlDataReader rdAtencion = cmdAtencion.ExecuteReader();
          if (rdAtencion.HasRows)
          {
            rdAtencion.Read();
            itemExtensionSalud.fechaInicioFacturacion = rdContrato.GetDateTime(0);
            itemExtensionSalud.fechaFinFacturacion = rdContrato.GetDateTime(1);
          }

          //***************** Campo 17  ----Copago



          //***************** Campo 18  ---- Moderadora




          //**************** Campo 19  ------Recuperacion



          //**************** Campo 20 ------- Pagos Compartidos



          //*************** Campo 21



          extencionesSalud.Add(itemExtensionSalud);

          documentoF2.extensionesSalud = extencionesSalud;
        }
        #endregion

        //************************************************************ Detalle de Factura
        using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conexion01.Open();
                    string qryFactura = "SELECT IdFactura,NumFactura,IdDestino,IdTransaccion,IdPlan,IdContrato,ValTotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,IndNotaCred,IndTipoFactura,CodEnti,CodEsor,FecFactura,Ruta,IdCausal,IdUsuarioR,IndHabilitado,IdNoFacturado,valPos,valNoPos FROM  facFactura WHERE idFactura=@idFactura AND idDestino=@idAtencion";
                    SqlCommand cmdFactura = new SqlCommand(qryFactura, conexion01);
                    cmdFactura.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    cmdFactura.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
                    SqlDataReader rdFactura = cmdFactura.ExecuteReader();
                    if (rdFactura.HasRows)
                    {
                        rdFactura.Read();
                        string strDetalleFac = @"SELECT   isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, 
isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0)) as Vlr_Total_Serv,O.idOrden
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN facFacturaDetOrden O on f.idFactura=O.idFactura AND f.idProducto=O.idProducto AND f.idMovimiento=O.idMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='PAQ' AND  a.IdFactura=@idFactura
UNION ALL
SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, 
isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0)) as Vlr_Total_Serv,O.idOrden
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN facFacturaDetOrden O on f.idFactura=O.idFactura AND f.idProducto=O.idProducto AND f.idMovimiento=O.idMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12,5)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino and g.IdProcPrincipal=2513
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='PAQ' AND  a.idfactura=@idFactura
UNION ALL
SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, 
isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0)) as Vlr_Total_Serv,O.idOrden
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN facFacturaDetOrden O on f.idFactura=O.idFactura AND f.idProducto=O.idProducto AND f.idMovimiento=O.idMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12,5)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino and g.IdProcPrincipal<>2513
LEFT JOIN vwFacProcPrincAsocPaq PQ on PQ.idfactura = a.idfactura and g.IdProcPrincipal=PQ.IdProcPrincipal 
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE PQ.idfactura is null and a.IndTipoFactura='PAQ' AND a.idfactura=@idFactura 
UNION ALL
SELECT  isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, 
isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0)) as Vlr_Total_Serv,O.idOrden
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN facFacturaDetOrden O on f.idFactura=O.idFactura AND f.idProducto=O.idProducto AND f.idMovimiento=O.idMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino
INNER JOIN  (   select distinct IdMovimiento,IdProductoCx,IdProcPrincipal from  facMovimientoDetCx  )  GX ON GX.IdMovimiento=g.IdMovimiento AND GX.IdProductoCx=f.IdProducto
LEFT JOIN vwFacProcPrincAsocPaq PQ on PQ.idfactura = a.idfactura and GX.IdProcPrincipal=PQ.IdProcPrincipal 
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE PQ.idfactura is null and a.IndTipoFactura='PAQ' AND   a.idfactura=@idFactura
ORDER BY o.Idorden";

                        SqlCommand cmdDetalleFac = new SqlCommand(strDetalleFac, conexion01);

                        cmdDetalleFac.Parameters.Add("@idFactura", SqlDbType.Int).Value = rdFactura.GetInt32(0);
                        SqlDataReader rdDetalleFac = cmdDetalleFac.ExecuteReader();
                        if (rdDetalleFac.HasRows)
                        {
                            Int16 nroLinea = 1;

                            while (rdDetalleFac.Read()) // Armar Detalle Fase II
                            {
                                try
                                {
                                    List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = rdDetalleFac.GetString(1);
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    //if (rdDetalleFac.GetInt16(18) == 5 || rdDetalleFac.GetInt16(18) == 6)
                                    //{
                                    //    lineaProducto.codigoEstandar = "999";
                                    //}
                                    //else
                                    //{
                                    //    lineaProducto.codigoEstandar = "999";
                                    //}
                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdDetalleFac.GetString(2);
                                    lineaProducto.unidades = double.Parse(rdDetalleFac.GetDouble(3).ToString());
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";
                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "Iva";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    tributosWRKIva.valorImporte = rdDetalleFac.GetDouble(5) * 0;
                                    TotalGravadoIva = TotalGravadoIva + rdDetalleFac.GetDouble(5);
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);


                                    TibutosDetalle tributosWRKIca = new TibutosDetalle();
                                    tributosWRKIca.id = "02";
                                    tributosWRKIca.nombre = "ICA";
                                    tributosWRKIca.esImpuesto = true;
                                    tributosWRKIca.porcentaje = 0;
                                    tributosWRKIca.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    tributosWRKIca.valorImporte = rdDetalleFac.GetDouble(5) * 0;
                                    TotalGravadoIca = TotalGravadoIca + rdDetalleFac.GetDouble(5);
                                    tributosWRKIca.tributoFijoUnidades = 0;
                                    tributosWRKIca.tributoFijoValorImporte = 0;

                                    //listaTributos.Add(tributosWRKIca);
                                    lineaProducto.tributos = listaTributos;
                                    detalleProductos.Add(lineaProducto);
                                    nroLinea++;
                                }
                                catch (Exception sqlExp)
                                {
                                    logFacturas.Warn($"Se ha presentado Excepcion: {sqlExp.Message }    Pilade llamados:{sqlExp.StackTrace}");
                                    throw;
                                }
                            }
                        }
                        else // Si No  hay Detalle de Productos
                        {
                            logFacturas.Info("No se encontro Detalle de productos para la factura. " + strDetalleFac);
                        }
                    }
                    else // No se encuentra Informacion de la Factura y Atencion
                    {
                        logFacturas.Info("No se encontro Informacion de Factura y Atencion En  facFactura." + qryFactura + "      Parametros::::  Numero Factura:" + nroFactura + "     Numero de Atencion:" + nroAtencion);
                    }
                }
                List<TributosItem> tributosTMP = new List<TributosItem>();
                List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                DetalleTributos detalleTributos = new DetalleTributos() // Un Objeto por cada Tipo de Iva
                {
                    valorImporte = 0,
                    valorBase = TotalGravadoIva,
                    porcentaje = 0
                };

                tributosDetalle.Add(detalleTributos);

                TributosItem itemTributo = new TributosItem()
                {
                    id = "01", //Total de Iva
                    nombre = "Iva",
                    esImpuesto = true,
                    valorImporteTotal = 0,
                    detalles = tributosDetalle // DEtalle de los Ivas
                };
                tributosTMP.Add(itemTributo);
                documentoF2.tributos = tributosTMP;

                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(_Valtotal.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = double.Parse(_Valtotal.ToString()),
                    valorTotalConImpuestos = TotalGravadoIva,
                    valorNeto = double.Parse(_ValCobrar.ToString())
                };
                documentoF2.totales = totalesTmp;
                documentoF2.detalles = detalleProductos;
                try
                {
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;

                    documentoF2.documento = facturaEnviar;
                    string facturaJson = JsonConvert.SerializeObject(documentoF2);
                    logFacturas.Info("Json de la Factura:" + facturaJson);
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    //string Usuario = "administrador";
                    //string Clave = "Transfiriendo@2016";

                    string Usuario = Properties.Settings.Default.usuario;
                    string Clave = Properties.Settings.Default.clave;

                    string credenciales = Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario + ":" + Clave));
                    request.Headers.Add("Authorization", "Basic " + credenciales);

                    Byte[ ] data = Encoding.UTF8.GetBytes(facturaJson);

                    Stream st = request.GetRequestStream();
                    st.Write(data, 0, data.Length);
                    st.Close();

                    int loop1, loop2;
                    NameValueCollection valores;
                    valores = request.Headers;

                    // Pone todos los nombres en un Arreglo
                    string[ ] arr1 = valores.AllKeys;
                    for (loop1 = 0; loop1 < arr1.Length; loop1++)
                    {
                        logFacturas.Info("Key: " + arr1[loop1] + "<br>");
                        // Todos los valores
                        string[ ] arr2 = valores.GetValues(arr1[loop1]);
                        for (loop2 = 0; loop2 < arr2.Length; loop2++)
                        {
                            logFacturas.Info("Value " + loop2 + ": " + arr2[loop2]);
                        }
                    }
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    logFacturas.Info("Codigo Status:" + response.StatusCode);
                    logFacturas.Info("Descripcion Status:" + response.StatusDescription);
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string strsb = sr.ReadToEnd();
                    logFacturas.Info("Respuesta:" + strsb);
                    string valorRpta = "00";
                    RespuestaTransfiriendo respuesta = JsonConvert.DeserializeObject<RespuestaTransfiriendo>(strsb);
                    if (respuesta.esExitoso)
                    {
                        logFacturas.Info($"PDF:{respuesta.resultado.URLPDF}");
                        logFacturas.Info($"XML:{respuesta.resultado.URLXML}");
                        logFacturas.Info($"UUID:{ respuesta.resultado.UUID}");
                        logFacturas.Info($"QR:{respuesta.resultado.QR}");
                        using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                        {
                            conn.Open();
                            string strActualiza = @"UPDATE dbo.facFacturaTempWEBService SET identificador=@identificador WHERE IdFactura=@nrofactura";
                            SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
                            cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                            cmdActualiza.Parameters.Add("@nrofactura", SqlDbType.Int).Value = nroFactura;
                            if (cmdActualiza.ExecuteNonQuery() > 0)
                            {
                                logFacturas.Info("Factura Actualizada con UUID en facFacturaTempWEBService");
                                using (WebClient webClient = new WebClient())
                                {
                                    try
                                    {
                                        string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                        logFacturas.Info("Carpeta de Descarga:" + carpetaDescarga);
                                        webClient.DownloadFile(respuesta.resultado.URLPDF, carpetaDescarga);
                                        //System.Threading.Thread.Sleep(1000);
                                        logFacturas.Info($"Descarga de PDF Factura...Terminada");
                                        carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".XML";
                                        webClient.DownloadFile(respuesta.resultado.URLXML, carpetaDescarga);
                                        //System.Threading.Thread.Sleep(1000);
                                        logFacturas.Info($"Descarga de XML...Terminada");
                                        using (SqlConnection conn3 = new SqlConnection(Properties.Settings.Default.DBConexion))
                                        {
                                            conn3.Open();
                                            string qryActualizaTempWEBService = @"UPDATE dbo.facFacturaTempWEBService SET CodCUFE=@cufe,cadenaQR=@cadenaQR WHERE identificador=@identificador";
                                            SqlCommand cmdActualizaTempWEBService = new SqlCommand(qryActualizaTempWEBService, conn);
                                            cmdActualizaTempWEBService.Parameters.Add("@cufe", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                                            cmdActualizaTempWEBService.Parameters.Add("@cadenaQR", SqlDbType.NVarChar).Value = respuesta.resultado.QR;
                                            cmdActualizaTempWEBService.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                                            if (cmdActualizaTempWEBService.ExecuteNonQuery() > 0)
                                            {
                                                logFacturas.Info("Descarga Existosa de Archivos de la Factura con Identificadotr:" + respuesta.resultado.UUID + " Destino:" + carpetaDescarga);
                                                if (!(respuesta.advertencias is null))
                                                {
                                                    string qryAdvertencia = @"INSERT INTO dbo.facFacturaTempWSAdvertencias (IdFactura,CodAdvertencia,FecRegistro,DescripcionAdv) 
VALUES(@IdFactura, @CodAdvertencia, @FecRegistro, @DescripcionAdv)";
                                                    SqlCommand cmdInsertarAdvertencia = new SqlCommand(qryAdvertencia, conn);
                                                    cmdInsertarAdvertencia.Parameters.Add("@IdFactura", SqlDbType.Int);
                                                    cmdInsertarAdvertencia.Parameters.Add("@CodAdvertencia", SqlDbType.VarChar);
                                                    cmdInsertarAdvertencia.Parameters.Add("@DescripcionAdv", SqlDbType.NVarChar);
                                                    cmdInsertarAdvertencia.Parameters.Add("@FecRegistro", SqlDbType.DateTime);
                                                    foreach (AdvertenciasItem itemAdv in respuesta.advertencias)
                                                    {
                                                        cmdInsertarAdvertencia.Parameters["@IdFactura"].Value = nroFactura;
                                                        cmdInsertarAdvertencia.Parameters["@CodAdvertencia"].Value = itemAdv.codigo;
                                                        //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                        cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                        cmdInsertarAdvertencia.Parameters["@DescripcionAdv"].Value = itemAdv.mensaje;
                                                        if (cmdInsertarAdvertencia.ExecuteNonQuery() > 0)
                                                        {
                                                            logFacturas.Info($"Se Inserta Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                            valorRpta = nroFactura.ToString();
                                                        }
                                                        else
                                                        {
                                                            logFacturas.Info($"No es Posible Insertar Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                        }
                                                    }
                                                }
                                                valorRpta = nroFactura.ToString();
                                            }
                                            else
                                            {
                                                logFacturas.Info($"No fue Actualizar la Factura en facFacturaTempWEBService. con Identificadotr:{respuesta.resultado.UUID}   El proceso de la Factura fue:{ respuesta.esExitoso}");
                                            }
                                        }
                                    }
                                    catch (NotSupportedException nSuppExp)
                                    {
                                        logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de los objetos de la Factura:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
                                        valorRpta = "9X";
                                    }
                                    catch (ArgumentNullException argNull)
                                    {
                                        logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de los objetos de la Factura:" + argNull.Message + "     " + argNull.InnerException.Message);
                                        valorRpta = "9X";
                                    }
                                    catch (WebException webEx1)
                                    {
                                        logFacturas.Info($"Se ha presentado una Falla durante la descarga de los objetos de la factura:{webEx1.Message} Mensaje Interno:{webEx1.InnerException.Message}  El proceso de la Factura fue:{ respuesta.esExitoso}");
                                        logFacturas.Warn($"Pila de Mensajes:::::{webEx1.StackTrace}");
                                        valorRpta = "93";
                                    }
                                    catch (Exception exx)
                                    {
                                        logFacturas.Info($"No fue posible descargar los archivos.PDF, XML y QR  !!! Causa:{exx.Message}  Pila:{exx.StackTrace}... El proceso de la Factura fue:{respuesta.esExitoso} ");
                                        valorRpta = "98";
                                    }
                                }
                            }
                            else
                            {
                                logFacturas.Info($"!!!   No fue posible Actualizar la Factura en la Tabla: facFacturaTempWEBService   !!! El proceso de la Factura fue:{respuesta.esExitoso}");
                            }
                        }
                    }
                    else
                    {

                        using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                        {
                            conn.Open();
                            string qryInsertaError = @"INSERT INTO facFacturaTempWEBServiceError (IdFactura,CodError,DescripcionError,FecRegistro) 
VALUES(@IdFactura, @CodError, @DescripcionError, @FecRegistro)";
                            SqlCommand cmdInsertarError = new SqlCommand(qryInsertaError, conn);
                            cmdInsertarError.Parameters.Add("@IdFactura", SqlDbType.Int).Value = nroFactura;
                            cmdInsertarError.Parameters.Add("@CodError", SqlDbType.VarChar).Value = respuesta.codigo;
                            cmdInsertarError.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = respuesta.mensaje;
                            cmdInsertarError.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = DateTime.Parse(respuesta.fecha);
                            if (cmdInsertarError.ExecuteNonQuery() > 0)
                            {
                                valorRpta = "99";
                                string qryDetErr = @"INSERT INTO facFacturaTempWSErrorDetalle (IdFactura,CodError,consecutivo,FecRegistro,DescripcionError) 
VALUES(@IdFactura, @CodError, @consecutivo, @FecRegistro, @DescripcionError)";
                                SqlCommand cmdDetErr = new SqlCommand(qryDetErr, conn);
                                cmdDetErr.Parameters.Add("@IdFactura", SqlDbType.Int);
                                cmdDetErr.Parameters.Add("@CodError", SqlDbType.VarChar);
                                cmdDetErr.Parameters.Add("@consecutivo", SqlDbType.Int);
                                cmdDetErr.Parameters.Add("@FecRegistro", SqlDbType.DateTime);
                                cmdDetErr.Parameters.Add("@DescripcionError", SqlDbType.NVarChar);
                                List<ErroresItem> listaErrores = new List<ErroresItem>();
                                int consecutivo = 1;
                                foreach (ErroresItem itemErr in respuesta.errores)
                                {
                                    cmdDetErr.Parameters["@IdFactura"].Value = nroFactura;
                                    cmdDetErr.Parameters["@CodError"].Value = itemErr.codigo;
                                    cmdDetErr.Parameters["@consecutivo"].Value = consecutivo;
                                    cmdDetErr.Parameters["@FecRegistro"].Value = DateTime.Parse(respuesta.fecha);
                                    cmdDetErr.Parameters["@DescripcionError"].Value = itemErr.mensaje;
                                    if (cmdDetErr.ExecuteNonQuery() > 0)
                                    {
                                        logFacturas.Info($"Se Inserta Detalle de Errores:codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                                    }
                                    else
                                    {
                                        logFacturas.Info($"No es Posible Insertar Detalle de Errores: Codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                                    }
                                }
                            }
                            else
                            {
                                valorRpta = "99";
                                logFacturas.Info($"No es Posible Insertar En Tabla de Errores Para Factura:{nroFactura}");
                            }
                        }
                    }

                    return valorRpta;
                }
                catch (WebException wExp)
                {
                    logFacturas.Warn("Se ha presentado una Excepcion:" + wExp.Message + "Pila de LLamadas:" + wExp.StackTrace);
                    return "99";
                }
                catch (NotSupportedException nsExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Http:" + nsExp01.Message + " Pila de LLamados:" + nsExp01.StackTrace);
                    return "94";
                }
                catch (ProtocolViolationException pexp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Http:" + pexp01.Message + " Pila de LLamados:" + pexp01.StackTrace);
                    return "95";
                }
                catch (InvalidOperationException inExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Http:" + inExp01.Message + " Pila de LLamados:" + inExp01.StackTrace);
                    return "96";

                }
                catch (HttpListenerException httpExp)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Http:" + httpExp.Message + " Pila de LLamados:" + httpExp.StackTrace);
                    return "97";
                }

                catch (Exception e)
                {
                    logFacturas.Warn("Se ha presentado una excepcion:" + e.Message + " Pila de LLamados:" + e.StackTrace);
                    return "98";
                }
            }
            catch (WebException wExp)
            {
                logFacturas.Warn("Se ha presentado una Excepcion:" + wExp.Message + "Pila de LLamadas:" + wExp.StackTrace);
                return "99";
            }
            catch (NotSupportedException nsExp01)
            {
                logFacturas.Warn("Se ha presentado una excepcion Http:" + nsExp01.Message + " Pila de LLamados:" + nsExp01.StackTrace);
                return "94";
            }
            catch (ProtocolViolationException pexp01)
            {
                logFacturas.Warn("Se ha presentado una excepcion Http:" + pexp01.Message + " Pila de LLamados:" + pexp01.StackTrace);
                return "95";
            }
            catch (InvalidOperationException inExp01)
            {
                logFacturas.Warn("Se ha presentado una excepcion Http:" + inExp01.Message + " Pila de LLamados:" + inExp01.StackTrace);
                return "96";

            }
            catch (HttpListenerException httpExp)
            {
                logFacturas.Warn("Se ha presentado una excepcion Http:" + httpExp.Message + " Pila de LLamados:" + httpExp.StackTrace);
                return "97";
            }

            catch (Exception e)
            {
                logFacturas.Warn("Se ha presentado una excepcion:" + e.Message + " Pila de LLamados:" + e.StackTrace);
                return "98";
            }
        }
    }
}
