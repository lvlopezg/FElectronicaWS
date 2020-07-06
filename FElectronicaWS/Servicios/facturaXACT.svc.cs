using FElectronicaWS.Clases;
using FElectronicaWS.Contratos;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;


namespace FElectronicaWS.Servicios
{

    public class facturaXACT : IfacturaXACT
    {

        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(Int32 nroFactura, Int32 idCliente, Int32 nroAtencion, string urlPdfFactura)
        {
            logFacturas.Info("Se recibe factura con siguientes datos:Factura x Actividad:" + nroFactura + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfFactura);
            try
            {
                // Inicializacion
                Int32 _idContrato = 0;
                Decimal _Valtotal = 0;
                Decimal _ValDescuento = 0;
                Decimal _ValDescuentoT = 0;
                Decimal _ValPagos = 0;
                Decimal _ValImpuesto = 0;
                Decimal _ValCobrar = 0;
                DateTime _FecFactura = DateTime.Now;
                Decimal _valPos = 0;
                Decimal _valNoPos = 0;
                Int32 _IdUsuarioR = 0;
                Int32 _idTercero = 0;
                Byte _tipoDocClienteDian = 0;
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                Byte _usrIdTipoDoc = 0;
                string _numDocCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
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
                facturaEnviar.tipoOperacion = "05";
                facturaEnviar.generaRepresentacionGrafica = false;

               // ClienteJuridico cliente = new ClienteJuridico();
                string urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteJuridico?idFactura={nroFactura}";
                logFacturas.Info("URL de Request:" + urlClientes);
                HttpWebRequest peticion = WebRequest.Create(urlClientes) as HttpWebRequest;
                peticion.Method = "GET";
                peticion.ContentType = "application/json";
                HttpWebResponse respuestaClientes = peticion.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(respuestaClientes.GetResponseStream());
                string infCliente = sr.ReadToEnd();
                logFacturas.Info("Cliente:" + infCliente);
                ClienteJuridicoConsulta cliente = JsonConvert.DeserializeObject<ClienteJuridicoConsulta>(infCliente);
                
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
                        _Valtotal = Math.Round(rdFacturaEnc.GetDecimal(1), 2);
                        _ValDescuento = Math.Round(rdFacturaEnc.GetDecimal(2), 2);
                        _ValDescuentoT = Math.Round(rdFacturaEnc.GetDecimal(3), 2);
                        _ValPagos = Math.Round(rdFacturaEnc.GetDecimal(4), 2);
                        _ValImpuesto = Math.Round(rdFacturaEnc.GetDecimal(5), 2);
                        _ValCobrar = Math.Round(rdFacturaEnc.GetDecimal(6), 2);
                        _FecFactura = rdFacturaEnc.GetDateTime(7);
                        _valPos = Math.Round(rdFacturaEnc.GetDecimal(8), 2);
                        _valNoPos = Math.Round(rdFacturaEnc.GetDecimal(9), 2);
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
                facturaEnviar.formaPago=formaPagoTmp;
                   
                List<DetallesItem> detalleProductos = new List<DetallesItem>();
                //****************** CLIENTE
                //  Variables Inicializacion
                string _direccionCliente = string.Empty;
                string _telefonoCliente = string.Empty;
                string _municipioCliente = string.Empty;
                string _departamento = string.Empty;
                //int _localizacionCliente = 0;
                string _correoCliente = string.Empty;
                //**** 

                Adquiriente adquirienteTmp = new Adquiriente
                {
                    identificacion = cliente.NroDoc_Cliente,
                    codigoInterno = cliente.IdTercero.ToString(),
                    razonSocial = cliente.NomTercero,
                    nombreSucursal = cliente.NomTercero,
                    correo =cliente.cuenta_correo.Trim().Split(';')[0],
                    telefono = cliente.telefono
                };
                using (SqlConnection connXX = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connXX.Open();
                    string qryTipoDocDian = "SELECT TipoDocDian FROM homologaTipoDocDian WHERE IdTipoDoc=@tipoDoc";
                    SqlCommand cmdTipoDocDian = new SqlCommand(qryTipoDocDian, connXX);
                    cmdTipoDocDian.Parameters.Add("@tipoDoc", SqlDbType.TinyInt).Value = cliente.TipoDoc_Cliente;
                    Int16 tipoDoc = Int16.Parse(cmdTipoDocDian.ExecuteScalar().ToString());
                    _tipoDocClienteDian = byte.Parse(tipoDoc.ToString());
                }
                adquirienteTmp.tipoIdentificacion = _tipoDocClienteDian;
                //if (_tipoDocCliente == 1)//TODO: validar la Homologacion para este campo
                //{
                //    adquirienteTmp.tipoIdentificacion = 31;
                //}
                //else if (_tipoDocCliente == 2)
                //{
                //    adquirienteTmp.tipoIdentificacion = 13;
                //}
                List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                NotificacionesItem notificaItem = new NotificacionesItem();
                notificaItem.tipo = 1;
                List<string> valorNotificacion = new List<string>();
                valorNotificacion.Add(cliente.cuenta_correo.Trim());
                notificaItem.valor= valorNotificacion;
                notificaciones.Add(notificaItem);
                facturaEnviar.notificaciones= notificaciones;

                if (_RegimenFiscal.Equals("C"))
                {
                    adquirienteTmp.tipoRegimen = "48";
                }
                else
                {
                    adquirienteTmp.tipoRegimen = "49";
                }
                //TODO: Aqui insertar lo que se defina de Responsabilidades  RUT documentoF2.adquiriente.responsabilidadesRUT
                if (cliente.IdNaturaleza == 3)
                {
                    adquirienteTmp.tipoPersona = "1";
                }
                else if (cliente.IdNaturaleza == 4)
                {
                    adquirienteTmp.tipoPersona = "2";
                }
                else
                {
                    adquirienteTmp.tipoPersona = "0";
                }
                List<string> responsanbilidadesR = new List<string>();
                responsanbilidadesR.Add("R-12-PJ");
                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = cliente.codigoPais;
                ubicacionCliente.departamento = cliente.Nombre_Depto;
                ubicacionCliente.codigoMunicipio = cliente.codMunicipio;
                ubicacionCliente.direccion = cliente.direccion;
                if (!cliente.codigoPais.Equals("CO"))
                {
                    TRM tasaInf = new TRM();
                    tasaInf.valor = 1;
                    tasaInf.monedaDestino = "COP";
                    tasaInf.fecha = formatoWrk.Split('T')[0];
                    facturaEnviar.TRM = tasaInf;
                }
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;
                #region MyRegion

                //TODO:Definir Los pagos por Copago y Cuota Moderadora, si se envian como Anticipos y Abonos
                //TODO:Definir si aplica para algun caso. Cargos y Descuentos

                //List<TributosItem> tributosTMP = new List<TributosItem>();
                //List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                //DetalleTributos detalleTributos = new DetalleTributos() // Un Objeto por cada Tipo de Iva
                //{
                //    valorImporte = 0,
                //    valorBase = 0,
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
                /////<summary>
                /////Inicio de Totales de la Factura
                ///// </summary> 
                #endregion
                double TotalGravadoIva = 0;
                //double TotalGravadoIca = 0;
                //List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                //AnticiposItem anticipoWrk = new AnticiposItem();
                //anticipoWrk.comprobante = "22";
                //anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                //anticiposWrk.Add(anticipoWrk);
                documentoF2.anticipos = anticiposWrk;

                //************************************************************ Detalle de Factura por Actividad ***********************************************************

                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    string qryFactura = "SELECT IdFactura,NumFactura,IdDestino,IdTransaccion,IdPlan,IdContrato,ValTotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,IndNotaCred,IndTipoFactura,CodEnti,CodEsor,FecFactura,Ruta,IdCausal,IdUsuarioR,IndHabilitado,IdNoFacturado,valPos,valNoPos FROM  facFactura WHERE idFactura=@idFactura AND idDestino=@idAtencion";
                    conexion01.Open();
                    SqlCommand cmdFactura = new SqlCommand(qryFactura, conexion01);
                    cmdFactura.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    cmdFactura.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
                    SqlDataReader rdFactura = cmdFactura.ExecuteReader();
                    if (rdFactura.HasRows)
                    {
                        rdFactura.Read();
                        string strDetalleFac = @"SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, 
isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0)) as Vlr_Total_Serv,
a.IdFactura,F.IdProducto,p.codproducto,p.NomProducto,F.IdMovimiento,F.ValDescuento,F.CantidadFNC,F.IdTasaVenta,F.ValTasa,F.ValCuotaMod,F.indPos,F.Regcum, p.IdProductoTipo,
--dbo.unidadProducto (p.IdProducto,p.IdProductoTipo) as 'Unidad'
'Unidad' as 'Unidad'
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino 
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='ACT' AND  a.idfactura= @idFactura";
                        //logFacturas.Info("consulta de Productos:" + strDetalleFac);
                        SqlCommand cmdDetalleFac = new SqlCommand(strDetalleFac, conexion01);
                        logFacturas.Info("Nro Factura:" + rdFactura.GetInt32(0));
                        cmdDetalleFac.Parameters.Add("@idFactura", SqlDbType.Int).Value = rdFactura.GetInt32(0);
                        SqlDataReader rdDetalleFac = cmdDetalleFac.ExecuteReader();
                        if (rdDetalleFac.HasRows)
                        {

                            Int16 nroLinea = 1;
                            
                            while (rdDetalleFac.Read())
                            {
                                try
                                {
                                    List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = rdDetalleFac.GetString(1);
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    if (rdDetalleFac.GetInt16(18)==5|| rdDetalleFac.GetInt16(18) == 6)
                                    {
                                        lineaProducto.codigoEstandar = "999";
                                    }
                                    else
                                    {
                                        lineaProducto.codigoEstandar = "999";
                                    }
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdDetalleFac.GetString(2); 
                                    lineaProducto.unidades =double.Parse( rdDetalleFac.GetDouble(3).ToString()) ;
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";

                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "Iva";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase =  double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    tributosWRKIva.valorImporte = rdDetalleFac.GetDouble(5) * 0;
                                    TotalGravadoIva = TotalGravadoIva + rdDetalleFac.GetDouble(5);
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);

                                    //TibutosDetalle tributosWRKIca = new TibutosDetalle();
                                    //tributosWRKIca.id = "02";
                                    //tributosWRKIca.nombre = "ICA";
                                    //tributosWRKIca.esImpuesto = true;
                                    //tributosWRKIca.porcentaje = 0;
                                    //tributosWRKIca.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    //tributosWRKIca.valorImporte = rdDetalleFac.GetDouble(5) * 0;
                                    //TotalGravadoIca = TotalGravadoIca + rdDetalleFac.GetDouble(5);
                                    //tributosWRKIca.tributoFijoUnidades = 0;
                                    //tributosWRKIca.tributoFijoValorImporte = 0;

                                    //listaTributos.Add(tributosWRKIca);
                                    lineaProducto.tributos = listaTributos;

                                    detalleProductos.Add(lineaProducto);
                                    nroLinea++;
                                }
                                catch (Exception sqlExp)
                                {
                                    string error = sqlExp.Message + "   " + sqlExp.StackTrace;
                                    throw;
                                }
                            }
                            logFacturas.Info("Numero de productos procesados" + nroLinea);
                        }
                        else // Si No  hay Detalle de Productos
                        {
                            logFacturas.Info("No se encontro Detalle de productos para la factura. " + strDetalleFac);
                        }
                    }
                    else // No se encuentra Informacion de la Factura y Atencion
                    {
                        logFacturas.Info("No se encontro Informacion de Factura y Atencion." + qryFactura);
                    }
                }
                documentoF2.detalles = detalleProductos;
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
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary>
                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(_Valtotal.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = TotalGravadoIva,
                    valorTotalConImpuestos = double.Parse(_Valtotal.ToString()) + double.Parse(_ValImpuesto.ToString()),
                    valorNeto = double.Parse(_ValCobrar.ToString())
                };
                documentoF2.totales = totalesTmp;
                logFacturas.Info("Numero de Productos Procesados, para JSon:" + detalleProductos.Count);
                try
                {
                    //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    request.Timeout = 30 * 1000;
                    documentoF2.documento = facturaEnviar;
                    string facturaJson = JsonConvert.SerializeObject(documentoF2);
                    logFacturas.Info("Json de la Factura:" + facturaJson);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    string Usuario = Properties.Settings.Default.usuario;
                    string Clave = Properties.Settings.Default.clave;
                    string credenciales = Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario + ":" + Clave));
                    request.Headers.Add("Authorization", "Basic " + credenciales);

                    Byte[] data = Encoding.UTF8.GetBytes(facturaJson);

                    Stream st = request.GetRequestStream();
                    st.Write(data, 0, data.Length);
                    st.Close();

                    int loop1, loop2;
                    NameValueCollection valores;
                    valores = request.Headers;

                    // Pone todos los nombres en un Arreglo
                    string[] arr1 = valores.AllKeys;
                    for (loop1 = 0; loop1 < arr1.Length; loop1++)
                    {
                        logFacturas.Info("Key: " + arr1[loop1] + "<br>");
                        // Todos los valores
                        string[] arr2 = valores.GetValues(arr1[loop1]);
                        for (loop2 = 0; loop2 < arr2.Length; loop2++)
                        {
                            logFacturas.Info("Value " + loop2 + ": " + arr2[loop2]);
                        }
                    }
                    
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    logFacturas.Info("Codigo Status:" + response.StatusCode);
                    logFacturas.Info("Descripcion Status:" + response.StatusDescription);
                    StreamReader lectura = new StreamReader(response.GetResponseStream());
                    string strsb = lectura.ReadToEnd();
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
                                                
                                                if (!(respuesta.advertencias is null) )
                                                {
                                                    string qryAdvertencia = @"INSERT INTO dbo.facFacturaTempWSAdvertencias(IdFactura,CodAdvertencia,FecRegistro,DescripcionAdv) 
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
                                                            valorRpta = nroFactura.ToString();

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    logFacturas.Info($"No existen advertencias de DIAN !!! factura{nroFactura}");
                                                    valorRpta = nroFactura.ToString();
                                                }
                                                valorRpta = nroFactura.ToString();
                                            }
                                            else
                                            {
                                                logFacturas.Info("No fue Posible Realizar la Descarga de Archivos de la Factura con Identificadotr:" + respuesta.resultado.UUID);
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
                                        logFacturas.Info("Se ha presentado una Falla durante la descarga de los objetos de la factura:" + webEx1.Message + "     " + webEx1.InnerException.Message);
                                        logFacturas.Warn($"Pila de Mensajes:::::{webEx1.StackTrace}");
                                        valorRpta = "93";
                                    }
                                    catch (Exception exx)
                                    {
                                        logFacturas.Info($"No fue posible descargar los archivos.PDF, XML y QR  !!! Causa:{exx.Message}  Pila:{exx.StackTrace}");
                                        valorRpta = "98";
                                    }
                                }
                            }
                            else
                            {
                                logFacturas.Info("!!!   No fue posible Actualizar la Factura en la Tabla: facFacturaTempWEBService   !!!");
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
                                valorRpta = nroFactura.ToString();
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
                                    consecutivo++;
                                }
                            }
                            else
                            {
                                valorRpta = "99";
                            }
                        }
                    }

                    return valorRpta;
                }
                catch (WebException wExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion WebException:" + wExp01.Message + " Pila de LLamados:" + wExp01.StackTrace);
                    using (SqlConnection connError = new SqlConnection(Properties.Settings.Default.DBConexion))
                    {
                        connError.Open();
                        string qryInsertaError = @"INSERT INTO facFacturaTempWEBServiceError (IdFactura,CodError,FecRegistro,DescripcionError) 
VALUES(@IdFactura,@CodError, @FecRegistro, @DescripcionError)";
                        SqlCommand cmdInsertarError = new SqlCommand(qryInsertaError, connError);
//                        cmdInsertarError.Parameters.AddWithValue("@idTipo", SqlDbType.VarChar).Value = "NC";
                        cmdInsertarError.Parameters.Add("@IdFactura", SqlDbType.Int).Value = nroFactura;
                        cmdInsertarError.Parameters.Add("@CodError", SqlDbType.VarChar).Value = "990201";
                        cmdInsertarError.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = "Error de Comunicaciones. Volver a intentar el envio.";
                        cmdInsertarError.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = DateTime.Now;
                        if (cmdInsertarError.ExecuteNonQuery() > 0)
                        {

                            string qryDetErr = @"INSERT INTO facFacturaTempWSErrorDetalle (IdFactura,CodError,consecutivo,FecRegistro,DescripcionError) 
VALUES(@IdFactura, @CodError, @consecutivo, @FecRegistro, @DescripcionError)";
                            SqlCommand cmdDetErr = new SqlCommand(qryDetErr, connError);
                            cmdDetErr.Parameters.Add("@IdFactura", SqlDbType.Int).Value = nroFactura;
                            cmdDetErr.Parameters.Add("@CodError", SqlDbType.VarChar).Value = "990201";
                            cmdDetErr.Parameters.Add("@consecutivo", SqlDbType.Int).Value = 1;
                            cmdDetErr.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = DateTime.Now;
                            cmdDetErr.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = $"eL SERVIDOR NO ESTA RESPONDIENDO.{wExp01.Message}";
                            if (cmdDetErr.ExecuteNonQuery() > 0)
                            {

                                logFacturas.Info($"Se Inserta Detalle de Errores Factura:Codigo:{wExp01.Message} Mensaje:{wExp01.StackTrace}");
                            }
                            else
                            {
                                logFacturas.Info($"No es Posible Insertar Detalle de Errores Factura: Codigo{wExp01.Message} Mensaje:{wExp01.StackTrace}");
                            }
                        }
                    }
                    return $"990101: Excepcion: WebException: {wExp01.Message}";
                }
                catch (NotSupportedException nsExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion NotSupportedException:" + nsExp01.Message + " Pila de LLamados:" + nsExp01.StackTrace);
                    return "94";
                }
                catch (ProtocolViolationException pexp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion ProtocolViolationException:" + pexp01.Message + " Pila de LLamados:" + pexp01.StackTrace);
                    return "95";
                }
                catch (InvalidOperationException inExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion InvalidOperationException:" + inExp01.Message + " Pila de LLamados:" + inExp01.StackTrace);
                    return "96";
                }
                catch (HttpListenerException httpExp)
                {
                    logFacturas.Warn("Se ha presentado una excepcion httpExp:" + httpExp.Message + " Pila de LLamados:" + httpExp.StackTrace);
                    return "97";
                }

                catch (Exception e)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Exception:" + e.Message + " Pila de LLamados:" + e.StackTrace);
                    return "98";
                }

            }
            catch (Exception exp)
            {
                logFacturas.Warn("Se ha presentado una Excepcion:" + exp.Message + "Pila de LLamadas:" + exp.StackTrace);
                return "99";
            }
        }

    }

}
