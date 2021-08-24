using FElectronicaWS.Clases;

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
    public class facturaXADM : IfacturaXADM
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, string urlPdfFactura, string moneda)
        {
            #region Definicion de Factura
            logFacturas.Info($"****************************************************** Factura Numero: {nroFactura}    ***********************************");
            logFacturas.Info("Se recibe factura con siguientes datos nroFactura: " + nroFactura + "  IdCliente:" + idCliente + " urlPdf:" + urlPdfFactura);
            try
            {
                decimal _Valtotal = 0;
                double TotalGravadoIva = 0;
                double TotalExcentoIva = 0;
                double ValorTotalIva = 0;
                Int32 ValorTasaIva = 0;
                double ValorBruto = 0;
                Decimal _ValPagos = 0;
                decimal _ValCobrar = 0;
                DateTime _FecFactura = DateTime.Now;
                //Decimal _valorIva = 0;
                Int32 _IdUsuarioR = 0;
                Int32 _idTercero = 0;
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                string _numDocCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
                string _RegimenFiscal = string.Empty;
                string _Ciudad = string.Empty;
                Int16 _idNaturaleza = 0;
                //int concepto = 0;
                string _Pais = string.Empty;
                FormaPago formaPagoTmp = new FormaPago();
                string nombrePagos = string.Empty;
                List<cargoDescuento> cargosDescuentos = new List<cargoDescuento>();

                //Fin de Inicializacion
                documentoRoot documentoF2 = new documentoRoot();
                Documento facturaEnviar = new Documento();
                facturaEnviar.identificadorTransaccion = "cb601e82-f1c7-42fe-ba93-6afd6807137f";
                facturaEnviar.URLPDF = urlPdfFactura;
                facturaEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                facturaEnviar.prefijo = Properties.Settings.Default.Prefijo;
                facturaEnviar.numeroDocumento = nroFactura.ToString();
                facturaEnviar.tipoDocumento = 1;
                facturaEnviar.subTipoDocumento = "01";
                facturaEnviar.tipoOperacion = "10";
                facturaEnviar.generaRepresentacionGrafica = false;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    //string qryFacturaEnc = @"SELECT  b.numdocrespaldo,IdTipoDocRespaldo,B.IdTercero,B.IdCliente,nomcliente,IdTipoDocCliente,B.NumDocumento,ValMonto,ValSaldo,FecRegistroC,FecRadicacion,IndEstado,ValFactura,a.IdUsuarioR,t.NomTercero,t.IdNaturaleza from cxcfacmanual a
                    //inner join cxccuenta b on a.idcuenta=b.idcuenta 
                    //inner join genTercero t ON b.IdTercero=t.IdTercero
                    //WHERE b.numdocrespaldo=@nroFactura";
                    string qryFacturaEnc = @"SELECT  b.numdocrespaldo,IdTipoDocRespaldo,B.IdTercero,B.IdCliente,nomcliente,tdoc.TipoDocDian,B.NumDocumento,ValMonto,ValSaldo,FecRegistroC,FecRadicacion,IndEstado,ValFactura,a.IdUsuarioR,t.NomTercero,t.IdNaturaleza from cxcfacmanual a
inner join cxccuenta b on a.idcuenta=b.idcuenta 
inner join genTercero t ON b.IdTercero=t.IdTercero
inner join homologaTipoDocDian tdoc on b.IdTipoDocCliente=tdoc.IdTipoDoc
WHERE b.numdocrespaldo=@nroFactura"; //**************REvisar los campos que se afectan

                    SqlCommand cmdFacturaEnc = new SqlCommand(qryFacturaEnc, conn);
                    cmdFacturaEnc.Parameters.Add("@nroFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdFacturaEnc = cmdFacturaEnc.ExecuteReader();
                    if (rdFacturaEnc.HasRows)
                    {
                        rdFacturaEnc.Read();
                        string valorTf = Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString();
                        _Valtotal = Decimal.Parse(valorTf);
                        _ValPagos = 0;
                        _ValCobrar = Decimal.Parse(Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString());
                        _FecFactura = rdFacturaEnc.GetDateTime(9);
                        _IdUsuarioR = rdFacturaEnc.GetInt32(13);
                        _numDocCliente = rdFacturaEnc.GetString(6);
                        _tipoDocCliente = rdFacturaEnc.GetByte(5);
                        _razonSocial = rdFacturaEnc.GetString(4);
                        _repLegal = rdFacturaEnc.GetString(4);
                        _idTercero = rdFacturaEnc.GetInt32(2);
                        _idNaturaleza = rdFacturaEnc.GetInt16(15);
                    }
                    else
                    {
                        return "No se encontro Informacion General de Encabezado de Factura.";
                    }
                    if (_ValPagos > 0)
                    {
                        //string Consultapagos = "SELECT IdConcepto FROM facFacAtenConcepto WHERE IdFactura=@idFactura";
                        //SqlCommand cmdConsultaPagos = new SqlCommand(Consultapagos, conn);
                        //cmdConsultaPagos.Parameters.Add("@idfactura", SqlDbType.Int).Value = nroFactura;
                        //concepto = int.Parse(cmdConsultaPagos.ExecuteScalar().ToString());
                        string Consultapagos = "SELECT FF.IdConcepto,FF.Valor,F.NomConcepto FROM facFacAtenConcepto FF " +
              "INNER JOIN facConceptoPago F ON f.IdConcepto = FF.IdConcepto " +
              "WHERE IdFactura=@idFactura";
                        SqlCommand cmdConsultaPagos = new SqlCommand(Consultapagos, conn);
                        cmdConsultaPagos.Parameters.Add("@idfactura", SqlDbType.Int).Value = nroFactura;
                        SqlDataReader rdConsultapagos = cmdConsultaPagos.ExecuteReader();
                        if (rdConsultapagos.HasRows)
                        {
                            while (rdConsultapagos.Read())
                            {
                                nombrePagos = rdConsultapagos.GetString(2);
                                List<string> notasWRK = new List<string>();
                                cargoDescuento cuotasWRK = new cargoDescuento();
                                cuotasWRK.esCargo = false;
                                cuotasWRK.codigo = "01";
                                notasWRK.Add(nombrePagos);
                                cuotasWRK.notas = notasWRK;
                                cuotasWRK.valorImporte = double.Parse(rdConsultapagos.GetDouble(1).ToString());
                                cuotasWRK.valorBaseMoneda = "COP";
                                cuotasWRK.valorBase = 0; //double.Parse( _Valtotal.ToString());
                                cuotasWRK.valorBaseMoneda = "COP";
                                cargosDescuentos.Add(cuotasWRK);
                            }
                        }
                        documentoF2.cargosDescuentos = cargosDescuentos;

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
                using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connx.Open();
                    string qryDatosCliente1 = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar,SUBSTRING(B.cod_dipo,5,5) AS 'Ciudad',
       pd.CodigoAlfa2,pd.NombreComun,c.nom_dipo as Depto, substring(c.cod_dipo, 5, 2) as CodDepto
FROM genTerceroLocaliza A
LEFT JOIN GEN_DIVI_POLI B ON A.IdLugar = B.IdLugar
INNER JOIN GEN_PAISES_DIAN AS pd ON cod_dipo_sahi = substring(cod_dipo, 1, 3)
inner join GEN_DIVI_POLI as c ON substring(c.cod_dipo,1,6) = substring(b.cod_dipo, 1, 6) AND c.tip_dipo = 'D'
WHERE IdTercero = @idTercero and IdLocalizaTipo IN(2, 3)
ORDER BY IdLocalizaTipo ";
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
                                _localizacionCliente = rdDatosCliente1.GetInt32(3);
                                _Pais = rdDatosCliente1.GetString(5);
                                _Ciudad = rdDatosCliente1.GetString(2);
                                _departamento = rdDatosCliente1.GetString(7);
                                if (rdDatosCliente1.GetSqlString(5) == "CO")
                                {
                                    _municipioCliente = rdDatosCliente1.GetString(4);
                                }
                                else
                                {
                                    _municipioCliente = "00000"; //Ajustado Para Facturas Administrativas, Clientes Extranjeros, Facturado eb Pessos 2020-08-03
                                }
                            }
                            else if (rdDatosCliente1.GetInt32(0) == 3)
                            {
                                _telefonoCliente = rdDatosCliente1.GetString(1);
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
                    }
                    else
                    {
                        _telefonoCliente = "5716519494";
                    }

                    string qryDatosCliente2 = @"SELECT COD_DEPTO,COD_MPIO,DPTO,NOM_MPIO FROM GEN_DIVI_POLI A
                    INNER JOIN HUSI_Divipola HB ON a.num_ptel=COD_DEPTO
                    WHERE a.IdLugar=@idLugar";
                    SqlCommand cmdDatosCliente2 = new SqlCommand(qryDatosCliente2, connx);
                    cmdDatosCliente2.Parameters.Add("@idLugar", SqlDbType.Int).Value = _localizacionCliente;
                    SqlDataReader rdDatosCliente2 = cmdDatosCliente2.ExecuteReader();
                    if (rdDatosCliente2.HasRows && _Pais == "CO")
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
                        if (rdDatosCliente3.IsDBNull(0))
                        {
                            _correoCliente = "facturaelectronica@husi.org.co";
                        }
                        else
                        {
                            if (rdDatosCliente3.GetString(0).Length == 0)
                            {
                                _correoCliente = "facturaelectronica@husi.org.co";
                            }
                            else
                            {
                                _correoCliente = rdDatosCliente3.GetString(0);
                            }
                        }
                    }
                    else
                    {
                        _correoCliente = "facturaelectronica@husi.org.co";
                    }
                    List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                    NotificacionesItem notificaItem = new NotificacionesItem();
                    notificaItem.tipo = 1;
                    List<string> valorNotificacion = new List<string>();
                    valorNotificacion.Add(_correoCliente.Trim());
                    notificaItem.valor = valorNotificacion;
                    notificaciones.Add(notificaItem);
                    if (_correoCliente.Length > 0)
                    {
                        facturaEnviar.notificaciones = notificaciones;
                    }

                    Adquiriente adquirienteTmp = new Adquiriente();
                    adquirienteTmp.identificacion = _numDocCliente;
                    adquirienteTmp.tipoIdentificacion = _tipoDocCliente;
                    adquirienteTmp.codigoInterno = _idTercero.ToString();
                    adquirienteTmp.razonSocial = _razonSocial;
                    adquirienteTmp.nombreSucursal = _razonSocial;
                    adquirienteTmp.correo = _correoCliente.Trim().Split(';')[0];
                    if (_telefonoCliente.Length == 0)
                    {
                        _telefonoCliente = "5716519494";
                    }
                    adquirienteTmp.telefono = _telefonoCliente;

                    //if (_RegimenFiscal.Equals("C"))
                    //{
                    //    adquirienteTmp.tipoRegimen = "48";
                    //}
                    //else
                    //{
                    //    adquirienteTmp.tipoRegimen = "49";
                    //}
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
                    ubicacionCliente.pais = _Pais;
                    ubicacionCliente.codigoMunicipio = _municipioCliente;
                    ubicacionCliente.ciudad = _Ciudad;
                    ubicacionCliente.direccion = _direccionCliente;
                    adquirienteTmp.ubicacion = ubicacionCliente;
                    ubicacionCliente.departamento = _departamento;
                    documentoF2.adquiriente = adquirienteTmp;
                    documentoF2.documento = facturaEnviar;


                    //List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                    //AnticiposItem anticipoWrk = new AnticiposItem();
                    //anticipoWrk.comprobante = "22";
                    //anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                    //anticiposWrk.Add(anticipoWrk);
                    //documentoF2.anticipos = anticiposWrk;

                    //************************************************************ Detalle de Factura por Administrativa *********************************************
                    using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                    {
                        conexion01.Open();//***este select sigue igual, con 4 columnas
                        string strDetalleFac = @"SELECT top 1000 b.numdocrespaldo as factura,CONVERT(VARCHAR,c.IdCuenta)+'-'+CONVERT(varchar,C.num_regi) AS codProducto,c.Descripcion,c.Cantidad,c.val_unit,
c.val_unit*c.Cantidad as valBruto,ISNULL(c.Vr_IVA,0) AS ValorIva,ISNULL(c.Porcen_IVA,0) as 'PorcentajeIva',c.*
 FROM cxcfacmanual a 
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
                                    List<TibutosDetalle> listaTributos = new List<TibutosDetalle>(); //Tributos para lalinea de producto
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = rdDetalleFac.GetString(1);
                                    lineaProducto.descripcion = rdDetalleFac.GetString(2);
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.unidades = double.Parse(rdDetalleFac.GetInt32(3).ToString());
                                    lineaProducto.unidadMedida = "94";
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetSqlMoney(4).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";
                                    ValorBruto += double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());

                                    TibutosDetalle tributosWRKIva = new TibutosDetalle(); //Detalle de tributos, para el producto
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "IVA";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = double.Parse(rdDetalleFac.GetInt32(16).ToString()).TomarDecimales(2);
                                    tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());

                                    tributosWRKIva.valorImporte = double.Parse(rdDetalleFac.GetSqlDouble(6).ToString()).TomarDecimales(2);

                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);
                                    lineaProducto.tributos = listaTributos;
                                    if (rdDetalleFac.GetInt32(16) > 0)
                                    {
                                        ValorTasaIva = rdDetalleFac.GetInt32(16);
                                        TotalGravadoIva += double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());
                                        ValorTotalIva += double.Parse(rdDetalleFac.GetSqlDouble(6).ToString()).TomarDecimales(2);
                                    }
                                    else
                                    {
                                        TotalExcentoIva += double.Parse(rdDetalleFac.GetSqlMoney(5).ToString());
                                    }
                                    detalleProductos.Add(lineaProducto);
                                    nroLinea++;
                                    // Para moneda extranjera:vr_Total_Inte-vr_IVA_Inte de la tabla
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

                        }
                    }
                    #endregion
                    //***************************** Finalizacion Y Liquidacion **************************************************************************************
                    documentoF2.detalles = detalleProductos;
                    ///<summary>
                    List<TributosItem> tributosTMP = new List<TributosItem>();
                    List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                    DetalleTributos detalleTributosItem = new DetalleTributos();

                    detalleTributosItem.valorImporte = ValorTotalIva;
                    detalleTributosItem.valorBase = TotalGravadoIva;
                    detalleTributosItem.porcentaje = double.Parse(ValorTasaIva.ToString()).TomarDecimales(2);
                    tributosDetalle.Add(detalleTributosItem);
                    TributosItem itemTributo = new TributosItem()
                    {
                        id = "01", //Total de Iva 
                        nombre = "IVA",
                        esImpuesto = true,
                        valorImporteTotal = ValorTotalIva,
                        detalles = tributosDetalle // Detalle de los Ivas
                    };
                    tributosTMP.Add(itemTributo);
                    documentoF2.tributos = tributosTMP;///

                    Totales totalesTmp = new Totales()
                    {
                        valorBruto = ValorBruto,   // double.Parse(ValorBruto.ToString()),
                                                   //valorAnticipos = double.Parse(_ValPagos.ToString()),
                        valorTotalSinImpuestos = ValorBruto,
                        valorTotalConImpuestos = ValorBruto - double.Parse(_ValPagos.ToString()) + double.Parse(ValorTotalIva.ToString()),
                        valorNeto = ValorBruto - double.Parse(_ValPagos.ToString()) + double.Parse(ValorTotalIva.ToString()) //double.Parse(_ValCobrar.ToString())
                    };
                    documentoF2.totales = totalesTmp;
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

                        Byte[ ] data = Encoding.UTF8.GetBytes(facturaJson);

                        Stream st = request.GetRequestStream();
                        st.Write(data, 0, data.Length);
                        st.Close();

                        int loop1, loop2;
                        NameValueCollection valores;
                        valores = request.Headers;

                        // Pone todos los nombres en un Arregle
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
                                            //string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                            //logFacturas.Info("Carpeta de Descarga:" + carpetaDescarga);
                                            //webClient.DownloadFile(respuesta.resultado.URLPDF, carpetaDescarga);
                                            ////System.Threading.Thread.Sleep(1000);
                                            //logFacturas.Info($"Descarga de PDF Factura...Terminada");
                                            //carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".XML";
                                            //webClient.DownloadFile(respuesta.resultado.URLXML, carpetaDescarga);
                                            ////System.Threading.Thread.Sleep(1000);
                                            //logFacturas.Info($"Descarga de XML...Terminada");
                                            //string directorioFactura = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString();
                                            string mes = facturaEnviar.fechaEmision.Substring(5, 2);
                                            string year = facturaEnviar.fechaEmision.Substring(0, 4);
                                            string directorioFactura = Properties.Settings.Default.urlDescargaPdfFACT + year + @"\" + mes;// + @"\" + DateTime.Now.Month.ToString();
                                            DirectoryInfo info = new DirectoryInfo(directorioFactura);
                                            if (!info.Exists)
                                            {
                                                info.Create();
                                            }
                                            string carpetaDescarga = directorioFactura + @"\" + respuesta.resultado.UUID + ".pdf";
                                            logFacturas.Info("Carpeta de Descarga:" + carpetaDescarga);
                                            webClient.DownloadFile(respuesta.resultado.URLPDF, carpetaDescarga);
                                            logFacturas.Info($"Descarga de PDF Factura...Terminada");
                                            carpetaDescarga = directorioFactura + @"\" + respuesta.resultado.UUID + ".XML";
                                            //carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".XML";
                                            webClient.DownloadFile(respuesta.resultado.URLXML, carpetaDescarga);
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
            }
            catch (Exception exp)
            {
                logFacturas.Warn("Se ha presentado una Excepcion:" + exp.Message + "Pila de LLamadas:" + exp.StackTrace);
                return "99";
            }
        }
    }
}
