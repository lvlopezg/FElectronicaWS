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
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "facturaXAdmParticular" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione facturaXAdmParticular.svc o facturaXAdmParticular.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class facturaXAdmParticular : IfacturaXAdmParticular
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, string urlPdfFactura, string moneda)
        {
            logFacturas.Info($"Se recibe factura con siguientes datos:Factura x Actividad:{nroFactura} IdCliente:{idCliente} urlPdf:{urlPdfFactura}");
            try
            {
                // Inicializacion
                Int32 _idContrato = 0;
                Decimal _Valtotal = 0;
                Decimal _ValDescuento = 0;
                Decimal _ValDescuentoT = 0;
                Decimal _ValPagos = 0;
                Decimal _ValImpuesto = 0;
                Decimal _valorIva = 0;
                Decimal _ValCobrar = 0;
                DateTime _FecFactura = DateTime.Now;
                Decimal _valPos = 0;
                Decimal _valNoPos = 0;
                Int32 _IdUsuarioR = 0;
                Int32 _idTercero = 0;
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

                //Fin de Inicializacion
                documentoRoot documentoF2 = new documentoRoot();
                Documento facturaEnviar = new Documento();
                facturaEnviar.identificadorTransaccion = "78ed919a-6382-48a8-acdc-0ae8c3e3761f";
                facturaEnviar.URLPDF = urlPdfFactura;
                facturaEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                facturaEnviar.prefijo = Properties.Settings.Default.Prefijo;
                facturaEnviar.numeroDocumento = nroFactura.ToString();
                facturaEnviar.tipoDocumento = 1;
                facturaEnviar.subTipoDocumento = "01";
                facturaEnviar.tipoOperacion = "05";
                facturaEnviar.generaRepresentacionGrafica = false;

                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryFacturaEnc = @"SELECT  b.numdocrespaldo,IdTipoDocRespaldo,B.IdTercero,B.IdCliente,nomcliente,IdTipoDocCliente,B.NumDocumento,ValMonto,ValSaldo,FecRegistroC,FecRadicacion,IndEstado,ValFactura,a.IdUsuarioR,t.NomTercero,t.IdNaturaleza from cxcfacmanual a
inner join cxccuenta b on a.idcuenta=b.idcuenta 
inner join genTercero t ON b.IdTercero=t.IdTercero
WHERE b.numdocrespaldo=@nroFactura";
                    SqlCommand cmdFacturaEnc = new SqlCommand(qryFacturaEnc, conn);
                    cmdFacturaEnc.Parameters.Add("@nroFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdFacturaEnc = cmdFacturaEnc.ExecuteReader();
                    if (rdFacturaEnc.HasRows)
                    {
                        rdFacturaEnc.Read();
                        //_idContrato = rdFacturaEnc.GetInt32(0);
                        string valorTf = Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString();
                        _Valtotal = Decimal.Parse(valorTf);
                        //_ValDescuento = 0;
                        //_ValDescuentoT = 0;
                        _ValPagos = 0;
                        _ValImpuesto = 0;
                        _ValCobrar = Decimal.Parse(Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString());
                        _FecFactura = rdFacturaEnc.GetDateTime(9);
                        //_valPos = 0;
                        //_valNoPos = 0;

                        _IdUsuarioR = rdFacturaEnc.GetInt32(13);
                        //_usrNombre = rdFacturaEnc.GetString(11);
                        //_usrNumDocumento = rdFacturaEnc.GetString(12);
                        //_usrIdTipoDoc = rdFacturaEnc.GetByte(13);
                        _numDocCliente = rdFacturaEnc.GetString(6);
                        _tipoDocCliente = rdFacturaEnc.GetByte(5);
                        _razonSocial = rdFacturaEnc.GetString(4);
                        _repLegal = rdFacturaEnc.GetString(4);
                        _idTercero = rdFacturaEnc.GetInt32(2);
                        _idNaturaleza = rdFacturaEnc.GetInt16(15);
                    }
                    if (_ValPagos > 0)
                    {
                        string Consultapagos = "SELECT IdConcepto FROM facFacAtenConcepto WHERE IdFactura=@idFactura";
                        SqlCommand cmdConsultaPagos = new SqlCommand(Consultapagos, conn);
                        cmdConsultaPagos.Parameters.Add("@idfactura", SqlDbType.Int).Value = nroFactura;
                        concepto = int.Parse(cmdConsultaPagos.ExecuteScalar().ToString());
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
                //**** 
                using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connx.Open();
                    //                    string qryDatosCliente1 = @"SELECT C.DirCasa,C.DirOficina,IdLugarCliente,C.TelCasa,C.TelCelular,C.TelOficina,C.CorreoE,pp.nom_dipo,RIGHT(PP.cod_dipo,5) AS 'Codigo' FROM facfactura F
                    //INNER JOIN admAtencion A ON F.IdDestino=A.IdAtencion
                    //INNER JOIN admCliente C ON A.IdCliente=C.IdCliente
                    //INNER JOIN GEN_DIVI_POLI PP ON c.IdLugarCliente=PP.IdLugar
                    //WHERE IdFactura=@idFactura";
                    string qryDatosCliente = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar,RIGHT(B.cod_dipo,5) FROM genTerceroLocaliza A
LEFT JOIN GEN_DIVI_POLI B ON A.IdLugar=B.IdLugar
WHERE IdTercero=@idTercero and IdLocalizaTipo IN (2,3)
ORDER BY IdLocalizaTipo";
                    SqlCommand cmdDatosCliente = new SqlCommand(qryDatosCliente, connx);
                    //logFacturas.Info($"_idTercero, para la busqueda del Correo, Administrativa, particular PN:{_idTercero}");
                    cmdDatosCliente.Parameters.Add("@idTercero", SqlDbType.Int).Value = _idTercero;
                    SqlDataReader rdDatosCliente = cmdDatosCliente.ExecuteReader();
                    if (rdDatosCliente.HasRows)
                    {
                        while (rdDatosCliente.Read())
                        {
                            if (rdDatosCliente.GetInt32(0) == 2)
                            {
                                _direccionCliente = rdDatosCliente.GetString(1);
                            }
                            else if (rdDatosCliente.GetInt32(0) == 3)
                            {
                                _localizacionCliente = rdDatosCliente.GetInt32(3);
                                _municipioCliente = rdDatosCliente.GetString(4);
                                _telefonoCliente = rdDatosCliente.GetString(1);
                                if (_telefonoCliente.Length > 10)
                                {
                                    _telefonoCliente = _telefonoCliente.Substring(0, 10);
                                }
                                else if (_telefonoCliente.Length == 0)
                                {
                                    _telefonoCliente = "5716519494";
                                }
                            }
                        }
                        //**
                        qryDatosCliente = @"SELECT C.DirCasa,C.DirOficina,IdLugarCliente,C.TelCasa,C.TelCelular,C.TelOficina,C.CorreoE,pp.nom_dipo,RIGHT(PP.cod_dipo,5) AS 'Codigo'  FROM admCliente C
                    INNER JOIN genTercero T ON C.idtercero = T.IdTercero
                    INNER JOIN GEN_DIVI_POLI PP ON c.IdLugarCliente = PP.IdLugar
                    WHERE T.IdTercero = @idTercero ";
                        using (SqlCommand cmdCorreo = new SqlCommand(qryDatosCliente, connx))
                        {
                            logFacturas.Info($"_idTercero, para la busqueda del Correo, Administrativa, particular PN:{_idTercero}");
                            cmdCorreo.Parameters.Add("@idTercero", SqlDbType.Int).Value = _idTercero;
                            SqlDataReader rdCorreo = cmdCorreo.ExecuteReader();
                            if (rdCorreo.HasRows)
                            {
                                rdCorreo.Read();
                                if (rdCorreo.IsDBNull(6))
                                {
                                    _correoCliente = "facturaelectronica@husi.org.co";
                                }
                                else
                                {
                                    if (rdCorreo.GetString(6).Length == 0)
                                    {
                                        _correoCliente = "facturaelectronica@husi.org.co";
                                    }
                                    else
                                    {
                                        _correoCliente = rdCorreo.GetString(6);
                                    }
                                }
                            }
                            else
                            {
                                _correoCliente = "facturaElectronica@husi.org.co";
                            }
                        }
                        //**
                    }
                    else
                    {
                        qryDatosCliente = @"SELECT C.DirCasa,C.DirOficina,IdLugarCliente,C.TelCasa,C.TelCelular,C.TelOficina,C.CorreoE,pp.nom_dipo,RIGHT(PP.cod_dipo,5) AS 'Codigo'  FROM admCliente C
                    INNER JOIN genTercero T ON C.idtercero = T.IdTercero
                    INNER JOIN GEN_DIVI_POLI PP ON c.IdLugarCliente = PP.IdLugar
                    WHERE T.IdTercero = @idTercero ";
                        using (SqlCommand cmdCorreo = new SqlCommand(qryDatosCliente, connx))
                        {
                            logFacturas.Info($"_idTercero, para la busqueda del Correo, Administrativa, particular PN:{_idTercero}");
                            cmdCorreo.Parameters.Add("@idTercero", SqlDbType.Int).Value = _idTercero;
                            SqlDataReader rdCorreo = cmdCorreo.ExecuteReader();
                            if (rdCorreo.HasRows)
                            {
                                rdCorreo.Read();
                                if (rdCorreo.IsDBNull(6))
                                {
                                    _correoCliente = "facturaelectronica@husi.org.co";
                                }
                                else
                                {
                                    if (rdCorreo.GetString(6).Length == 0)
                                    {
                                        _correoCliente = "facturaelectronica@husi.org.co";
                                    }
                                    else
                                    {
                                        _correoCliente = rdCorreo.GetString(6);
                                        _localizacionCliente = rdCorreo.GetInt32(2);
                                        _municipioCliente = rdCorreo.GetString(8);
                                        _direccionCliente = rdCorreo.GetString(0);
                                    }
                                }
                            }
                            else
                            {
                                _correoCliente = "facturaElectronica@husi.org.co";
                            }
                        }
                    }
                    //                        string qryuCorreo = @"SELECT CorreoE FROM genTercero T
                    //INNER JOIN admCliente C ON T.IdCliente = C.IdCliente
                    //WHERE T.IdTercero = @idTercero";

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
                    string qryIvaFact = @"SELECT sum(a.val_cuen) as totalIva
FROM cxcfacmanualdet a 
inner join cxccuenta b on a.idcuenta=b.idcuenta and  b.numdocrespaldo=@factura
WHERE a.cod_cuen in (  select replace (val_camp, ';','') from gen_enti_dato where  nom_camp='CXC_FIVA')";
                    SqlCommand cmdIvaFact = new SqlCommand(qryIvaFact, connx);
                    cmdIvaFact.Parameters.Add("@factura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdIvaFact = cmdIvaFact.ExecuteReader();
                    if (rdIvaFact.HasRows)
                    {
                        rdIvaFact.Read();
                        if (!rdIvaFact.IsDBNull(0))
                        {
                            _valorIva = Math.Round(rdIvaFact.GetDecimal(0), 0);
                        }
                    }
                }
                if (_telefonoCliente.Length == 0)
                {
                    _telefonoCliente = "5716519494";
                }
                Adquiriente adquirienteTmp = new Adquiriente
                {
                    identificacion = _numDocCliente,
                    codigoInterno = _idTercero.ToString(),
                    razonSocial = _razonSocial,
                    nombreSucursal = _razonSocial,
                    correo = _correoCliente.Trim(),
                    telefono = _telefonoCliente,
                };

                using (SqlConnection connXX = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connXX.Open();
                    string qryTipoDocDian = "SELECT TipoDocDian FROM homologaTipoDocDian WHERE IdTipoDoc=@tipoDoc";
                    SqlCommand cmdTipoDocDian = new SqlCommand(qryTipoDocDian, connXX);
                    cmdTipoDocDian.Parameters.Add("@tipoDoc", SqlDbType.TinyInt).Value = _tipoDocCliente;
                    Int16 tipoDoc = Int16.Parse(cmdTipoDocDian.ExecuteScalar().ToString());
                    _tipoDocCliente = byte.Parse(tipoDoc.ToString());
                }
                adquirienteTmp.tipoIdentificacion = _tipoDocCliente;
                List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                NotificacionesItem notificaItem = new NotificacionesItem();
                notificaItem.tipo = 1;
                List<string> valorNotificacion = new List<string>();
                valorNotificacion.Add(_correoCliente.Trim());
                notificaItem.valor = valorNotificacion;
                notificaciones.Add(notificaItem);
                facturaEnviar.notificaciones = notificaciones;

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
                responsanbilidadesR.Add("R-12-PJ");
                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = "CO";
                ubicacionCliente.codigoMunicipio = _municipioCliente;
                ubicacionCliente.direccion = _direccionCliente;
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;

                double TotalGravadoIva = 0;
                //double TotalGravadoIca = 0;

                List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                AnticiposItem anticipoWrk = new AnticiposItem();
                anticipoWrk.comprobante = "22";
                anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                anticiposWrk.Add(anticipoWrk);
                documentoF2.anticipos = anticiposWrk;

                List<TributosItem> tributosTMP = new List<TributosItem>();
                List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                DetalleTributos detalleTributos = new DetalleTributos(); // Un Objeto por cada Tipo de Iva

                double valorImporte = double.Parse(_valorIva.ToString());
                double valorBase = double.Parse((_Valtotal - _valorIva).ToString());
                double porcentaje = Math.Round(((valorImporte / valorBase) * 100).TomarDecimales(2), 0);
                detalleTributos.valorImporte = valorImporte;
                detalleTributos.valorBase = valorBase;
                detalleTributos.porcentaje = porcentaje;

                tributosDetalle.Add(detalleTributos);
                TributosItem itemTributo = new TributosItem()
                {
                    id = "01", //Total de Iva 
                    nombre = "Iva",
                    esImpuesto = true,
                    valorImporteTotal = valorImporte,
                    detalles = tributosDetalle // Detalle de los Ivas
                };
                //tributosTMP.Add(itemTributo);
                //************************************************************ Detalle de Factura por Administrativa ***********************************************************
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conexion01.Open();
                    string strDetalleFac = @"SELECT top 10 b.numdocrespaldo, c.* FROM cxcfacmanual a
INNER JOIN cxccuenta b on a.idcuenta = b.idcuenta
INNER JOIN cxcfacmanualconc c on c.IdCuenta = a.idcuenta
WHERE c.val_unit_inte is null and NumDocRespaldo = @idFactura ORDER BY c.num_regi";
                    SqlCommand cmdDetalleFac = new SqlCommand(strDetalleFac, conexion01);
                    cmdDetalleFac.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
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
                                string codigoProducto = rdDetalleFac.GetInt32(1).ToString();
                                lineaProducto.valorCodigoInterno = codigoProducto;
                                lineaProducto.codigoEstandar = "999";
                                lineaProducto.valorCodigoEstandar = codigoProducto;
                                lineaProducto.descripcion = rdDetalleFac.GetString(4);
                                lineaProducto.unidades = double.Parse(rdDetalleFac.GetInt32(3).ToString());
                                lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());
                                double valorBrutoW = double.Parse(rdDetalleFac.GetSqlMoney(6).ToString());
                                lineaProducto.valorBruto = valorBrutoW;// double.Parse(rdDetalleFac.GetSqlMoney(6).ToString());
                                lineaProducto.valorBrutoMoneda = "COP";
                                //detalleProductos.Add(lineaProducto);

                                TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                tributosWRKIva.id = "01";
                                tributosWRKIva.nombre = "Iva";
                                tributosWRKIva.esImpuesto = true;
                                tributosWRKIva.porcentaje = porcentaje;
                                tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetSqlMoney(5).ToString()) * double.Parse(rdDetalleFac.GetInt32(3).ToString()); //Unidades * precio
                                tributosWRKIva.valorImporte = valorImporte;
                                TotalGravadoIva = TotalGravadoIva + valorBrutoW;
                                tributosWRKIva.tributoFijoUnidades = 0;
                                tributosWRKIva.tributoFijoValorImporte = 0;
                                listaTributos.Add(tributosWRKIva);
                                lineaProducto.tributos = listaTributos;
                                detalleProductos.Add(lineaProducto);
                                nroLinea++;
                            }
                            catch (Exception sqlExp)
                            {
                                string error = $"Mensaje de Error:{sqlExp.Message}   Traza de la Pila:{sqlExp.StackTrace}";
                                logFacturas.Warn($"Se ha presentado una Excepcion:{error}");
                                throw;
                            }
                        }
                    }
                    else // Si No  hay Detalle de Productos
                    {
                        //todo: Insertar la funcion para el cargue de errores
                    }
                }
                //***************************** Finalizacion Y Liquidacion
                documentoF2.detalles = detalleProductos;
                tributosTMP.Add(itemTributo);
                documentoF2.tributos = tributosTMP;
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary>
                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(valorBase.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = valorBase,
                    valorTotalConImpuestos = double.Parse(valorBase.ToString()) + double.Parse(valorImporte.ToString()),
                    valorNeto = double.Parse(_ValCobrar.ToString())
                };
                documentoF2.totales = totalesTmp;
                documentoF2.documento = facturaEnviar;
                logFacturas.Info("Numero de Productos procesados, para JSon:" + detalleProductos.Count);

                try
                {
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica; //+ Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    request.Timeout = 60 * 1000;
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

                    // Pone todos los nombres en un Arregle
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
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string strsb = sr.ReadToEnd();
                    logFacturas.Info("Respuesta Recibida Transfiriendo:" + strsb);
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
                                                        cmdInsertarAdvertencia.Parameters["@CodError"].Value = itemAdv.codigo;
                                                        //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                        cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                        cmdInsertarAdvertencia.Parameters["@DescripcionError"].Value = itemAdv.mensaje;
                                                        if (cmdInsertarAdvertencia.ExecuteNonQuery() > 0)
                                                        {
                                                            logFacturas.Info($"Se Inserta Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
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
                                                valorRpta = "99";
                                                logFacturas.Info($"No fue Posible Realizar la Descarga de Archivos de la Factura con Identificadotr:{respuesta.resultado.UUID} Estado Proceso de Factura:{respuesta.esExitoso}");
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
                                valorRpta = "98";
                                logFacturas.Info("!!!   No fue posible Actualizar la Factura en la Tabla: facFacturaTempWEBService   !!!");
                            }
                        }
                        return valorRpta;
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
                                }
                            }
                            else
                            {
                                valorRpta = "99";
                            }
                        }
                    }

                    return valorRpta;
                    //               }
                    //else
                    //{
                    //	logFacturas.Info("!!!  Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
                    //	logFacturas.Warn("Respuesta recibida:" + strsb);
                    //	//*Aqui se debe insertar en la tabla de fallas
                    //	return "Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService";
                    //}

                }
                catch (WebException wExp01)
                {
                    logFacturas.Warn("Se ha presentado una excepcion Http:" + wExp01.Message + " Pila de LLamados:" + wExp01.StackTrace);
                    return "93";
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
            catch (Exception exp)
            {
                logFacturas.Warn("Se ha presentado una Excepcion:" + exp.Message + "Pila de LLamadas:" + exp.StackTrace);
                return "99";
            }
        }
    }
}