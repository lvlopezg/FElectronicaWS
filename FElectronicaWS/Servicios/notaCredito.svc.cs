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
    public class notaCredito : InotaCredito
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string getData(int nroNotaCredito, int idCliente, int nroAtencion, string monedaNota, int nroFactura, string urlPdfNotaCredito)
        {
            logFacturas.Info("Se recibe Nota Credito con siguientes datos:nroNota:" + nroNotaCredito + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfNotaCredito);
            try
            {
                #region inicializar
                Decimal _Valtotal = 0;
                Decimal _ValPagos = 0;
                Decimal _ValCobrar = 0;
                DateTime _FecNotaCredito = DateTime.Now;
                Int32 _idMovimiento = 0;
                Int32 _idTercero = 0;
                Int32 _facturaRelacionada = 0;
                string _repLegal = string.Empty;
                string DescNota = string.Empty;
                Int16 _codigoCausal = 0;
                Int32 _tipoDocClienteDian = 0;
                #endregion

                //Fin de Inicializacion
                documentoRoot documentoF2 = new documentoRoot();
                Documento NotaCreditoEnviar = new Documento();
                NotaCreditoEnviar.identificadorTransaccion = "D7F719C2 - 75F4 - 4F06 - B7CB - F583FC28DBEE";
                NotaCreditoEnviar.URLPDF = urlPdfNotaCredito;
                NotaCreditoEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                NotaCreditoEnviar.prefijo = Properties.Settings.Default.PrefijoNotasNC;
                NotaCreditoEnviar.numeroDocumento = nroNotaCredito.ToString();
                NotaCreditoEnviar.tipoDocumento = 2;
                NotaCreditoEnviar.subTipoDocumento = "91";

                NotaCreditoEnviar.generaRepresentacionGrafica = false;

                bool facXRel = false;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryEspeciales = "SELECT IdDestino,IndTipoFactura FROM facFactura where IdFactura=@idFactura";
                    SqlCommand cmdEspeciales = new SqlCommand(qryEspeciales, conn);
                    cmdEspeciales.Parameters.Add("@idFactura",SqlDbType.Int).Value=nroFactura;
                    SqlDataReader rdEspeciales = cmdEspeciales.ExecuteReader();
                    if (rdEspeciales.HasRows)
                    {
                        if (rdEspeciales.Read())
                        {
                            if (rdEspeciales.GetInt32(0) == 0 && rdEspeciales.GetString(1) == "RAC")
                            {
                                facXRel = true;
                            }
                        }
                    }
                }
                string urlClientes = string.Empty;
                if (idCliente == 0 && nroAtencion == 0 && !facXRel)
                {
                    urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteInternacional?idFactura={nroFactura}";
                }
                else
                {
                    urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteJuridico?idFactura={nroFactura}";
                }
                    logFacturas.Info("URL de Request:" + urlClientes);
                    HttpWebRequest peticion = WebRequest.Create(urlClientes) as HttpWebRequest;
                    peticion.Method = "GET";
                    peticion.ContentType = "application/json";
                    HttpWebResponse respuestaClientes = peticion.GetResponse() as HttpWebResponse;
                    StreamReader sr = new StreamReader(respuestaClientes.GetResponseStream());
                    string infCliente = sr.ReadToEnd();
                    logFacturas.Info("Cliente:" + infCliente);
                     Cliente cliente = JsonConvert.DeserializeObject<Cliente>(infCliente);


                //****************** CLIENTE
                //  Variables Inicializacion
                string _direccionCliente = string.Empty;
                string _telefonoCliente = string.Empty;
                string _municipioCliente = string.Empty;
                string _departamento = string.Empty;
                //int _localizacionCliente = 0;
                string _correoCliente = string.Empty;
                //**** 
                string codCufeFactura = string.Empty;
                List<DetallesItem> detalleProductos = new List<DetallesItem>();
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    //string codCufeFact = "SELECT CodCUFE FROM facFacturaTempWEBService WHERE IdFactura=@idFactura";
                    string qrycodCufeFact = @"SELECT CodCufe FROM facFacturaTempWEBService
WHERE idFactura = @idFactura
UNION
SELECT CodCufe FROM facFacturaTempWEBServiceHist
WHERE idFactura = @idFactura";
                    SqlCommand cmdCodCufe = new SqlCommand(qrycodCufeFact, conn);
                    cmdCodCufe.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdCufe = cmdCodCufe.ExecuteReader();
                    if (rdCufe.HasRows)
                    {
                        rdCufe.Read();
                        codCufeFactura = rdCufe.GetString(0);
                    }
                    else
                    {
                        List<ErroresItem> detalle = new List<ErroresItem>();
                        ErroresItem item = new ErroresItem();
                        item.codigo = "990102";
                        item.mensaje = "No se ha encontrado el CUFE, para la Factura afectada con esta  Nota Credito. CUFE de Factura NO existe. Revisar SAHI";
                        detalle.Add(item);
                        return UtilidadRespuestas.insertarErrorND("NC", nroNotaCredito, "9901", "CUFE de la FacturaRelacionada No Encontado", DateTime.Now, detalle);
                    }
                    string qryNotaCredito = @"SELECT TOP 1 B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,(T.ValTotal + ISNULL(L.ValTotalIVA,0))   AS ValMonto,
cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,(T.ValTotal + ISNULL(L.ValTotalIVA,0)) AS ValFactura,B.IdMovimiento,2 as 'Causal'  
FROM cxcTipoMovimientoEfectoNotas A
INNER JOIN cxcCarteraMovi B ON A.IdTipoMovimiento=B.IdTipoMovimiento
INNER JOIN cxcCuenta cxcCta ON cxcCta.IdCuenta=B.IdCuenta
LEFT JOIN cxcCarteraMoviNoNota C ON B.IdMovimiento=C.IdMovimiento
INNER JOIN facnotacredito F ON F.IdNotaCredito = @nroNotaCredito AND F.INDTOTAL=1
INNER JOIN FACFACTURA T ON T.IDFACTURA = F.IdFactura
LEFT JOIN FACFACTURAIVA L ON L.IDFACTURA=T.IDFACTURA
where A.Indhabilitado=1 and C.IdMovimiento IS NULL
and B.NumDocumento=@nroNotaCredito and A.IndTipoNota='C'
UNION
SELECT B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,B.ValMonto,
cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,cxcCta.ValFactura,B.IdMovimiento,6 as 'Causal' 
FROM cxcTipoMovimientoEfectoNotas A
INNER JOIN cxcCarteraMovi B ON A.IdTipoMovimiento=B.IdTipoMovimiento
INNER JOIN cxcCuenta cxcCta ON cxcCta.IdCuenta=B.IdCuenta
LEFT JOIN cxcCarteraMoviNoNota C ON B.IdMovimiento=C.IdMovimiento
INNER JOIN facnotacredito F ON F.IdNotaCredito = @nroNotaCredito AND F.INDTOTAL=0  
where A.Indhabilitado=1 and C.IdMovimiento IS NULL
and B.NumDocumento=@nroNotaCredito and A.IndTipoNota='C'
UNION
SELECT B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,B.ValMonto,
cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,cxcCta.ValFactura,B.IdMovimiento,6 as 'Causal' FROM cxcTipoMovimientoEfectoNotas A
INNER JOIN cxcCarteraMovi B ON A.IdTipoMovimiento=B.IdTipoMovimiento
INNER JOIN cxcCuenta cxcCta ON cxcCta.IdCuenta=B.IdCuenta
INNER JOIN cxcCarteraMoviNoNota C ON B.IdMovimiento=C.IdMovimiento
where A.Indhabilitado=1 and C.IdNumeroNota=@nroNotaCredito2 and A.IndTipoNota='C'";
                    SqlCommand cmdNotaCredito = new SqlCommand(qryNotaCredito, conn);
                    cmdNotaCredito.Parameters.Add("@nroNotaCredito", SqlDbType.VarChar).Value = nroNotaCredito;
                    cmdNotaCredito.Parameters.Add("@nroNotaCredito2", SqlDbType.Int).Value = nroNotaCredito;
                    SqlDataReader rdNotaCredito = cmdNotaCredito.ExecuteReader();
                    if (rdNotaCredito.HasRows)
                    {
                        rdNotaCredito.Read();
                        _idMovimiento = rdNotaCredito.GetInt32(14);
                        var valorTNota = rdNotaCredito["ValMonto"];
                        _Valtotal = Decimal.Parse(valorTNota.ToString());
                        _ValCobrar = Decimal.Parse(valorTNota.ToString());
                        _FecNotaCredito = rdNotaCredito.GetDateTime(4);
                        _facturaRelacionada = nroFactura;
                        _idTercero = cliente.IdTercero;
                        _codigoCausal = short.Parse(rdNotaCredito.GetInt32(15).ToString());
                    }
                    else
                    {
                        List<ErroresItem> detalle = new List<ErroresItem>();
                        ErroresItem item = new ErroresItem();
                        item.codigo = "990101";
                        item.mensaje = "No hay informacion disponible de la Nota Credito, para obtener informacion necesaria. La Nota NO existe. Revisar SAHI";
                        detalle.Add(item);
                        return UtilidadRespuestas.insertarErrorND("NC", nroNotaCredito, "9901", "Informacion de la Nota Credito No Encontada", DateTime.Now, detalle);
                    }
                    FormaPago formaPagoTmp = new FormaPago();
                    string formatoWrk = formatosFecha.formatofecha(_FecNotaCredito);
                    NotaCreditoEnviar.fechaEmision = formatoWrk.Split('T')[0];
                    NotaCreditoEnviar.horaEmision = formatoWrk.Split('T')[1];
                    NotaCreditoEnviar.moneda = monedaNota;
                    formaPagoTmp.tipoPago = 1;
                    formaPagoTmp.codigoMedio = "10";
                    NotaCreditoEnviar.formaPago = formaPagoTmp;

                    Adquiriente adquirienteTmp = new Adquiriente();

                    using (SqlConnection connXX = new SqlConnection(Properties.Settings.Default.DBConexion))
                    {
                        connXX.Open();
                        string qryTipoDocDian = "SELECT TipoDocDian FROM homologaTipoDocDian WHERE IdTipoDoc=@tipoDoc";
                        SqlCommand cmdTipoDocDian = new SqlCommand(qryTipoDocDian, connXX);
                        cmdTipoDocDian.Parameters.Add("@tipoDoc", SqlDbType.TinyInt).Value = cliente.TipoDoc_Cliente;
                        logFacturas.Info($" cliente.TipoDoc_Cliente:{ cliente.TipoDoc_Cliente}"); 
                         Int16 tipoDoc = Int16.Parse(cmdTipoDocDian.ExecuteScalar().ToString());
                        _tipoDocClienteDian = Int32.Parse(tipoDoc.ToString());
                        logFacturas.Info($"_tipoDocClienteDian:{_tipoDocClienteDian}");
                    }
                    adquirienteTmp.tipoIdentificacion = _tipoDocClienteDian;
                    adquirienteTmp.identificacion = cliente.NroDoc_Cliente;
                    adquirienteTmp.codigoInterno = cliente.IdTercero.ToString();
                    adquirienteTmp.razonSocial = cliente.NomTercero;
                    adquirienteTmp.nombreSucursal = cliente.NomTercero;
                    adquirienteTmp.correo = cliente.cuenta_correo.Trim().Split(';')[0];
                    if (cliente.telefono.Length > 10)
                    {
                        cliente.telefono = cliente.telefono.Substring(0, 10);
                    }
                    adquirienteTmp.telefono = cliente.telefono;
                    if (cliente.idRegimen.Equals("C"))
                    {
                        adquirienteTmp.tipoRegimen = "48";
                    }
                    else
                    {
                        adquirienteTmp.tipoRegimen = "49";
                    }
                    List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                    NotificacionesItem notificaItem = new NotificacionesItem();
                    notificaItem.tipo = 1;
                    List<string> valorNotificacion = new List<string>();
                    valorNotificacion.Add(cliente.cuenta_correo.Trim());
                    notificaItem.valor = valorNotificacion;
                    notificaciones.Add(notificaItem);
                    NotaCreditoEnviar.notificaciones = notificaciones;
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

                        SqlCommand sqlValidaFactura = new SqlCommand("spFACEIdentificadorTipoOperaNota", conexion01);
                        sqlValidaFactura.CommandType = CommandType.StoredProcedure;
                        sqlValidaFactura.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                        SqlDataReader rdValidaFactura = sqlValidaFactura.ExecuteReader();
                        if (rdValidaFactura.HasRows)
                        {
                            rdValidaFactura.Read();
                            if (rdValidaFactura.GetInt32(0) == 0)
                            {
                                NotaCreditoEnviar.tipoOperacion = "10"; //Standar
                            }
                            else
                            {
                                NotaCreditoEnviar.tipoOperacion = "22"; //Standar
                            }
                        }
                        else
                        {
                            logFacturas.Error($"No hay Datos de Factura, para la Factura:{nroFactura}");
                        }
                    }
                    adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                    Ubicacion ubicacionCliente = new Ubicacion();
                    ubicacionCliente.pais = cliente.codigoPais;
                    ubicacionCliente.departamento = cliente.Nombre_Depto;
                    ubicacionCliente.codigoMunicipio = cliente.codMunicipio;
                    ubicacionCliente.direccion = cliente.direccion;
                    adquirienteTmp.ubicacion = ubicacionCliente;
                    documentoF2.adquiriente = adquirienteTmp;
                    double TotalGravadoIva = 0;
                    //double TotalGravadoIca = 0;

                    //************************************************************ Detalle de Nota Credito   ***********************************************************
                    using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                    {
                        conexion01.Open();
                        try
                        {
                            SqlCommand sqlValidaDet = new SqlCommand("spTraeDetalleNotas", conexion01);
                            sqlValidaDet.CommandType = CommandType.StoredProcedure;
                            sqlValidaDet.Parameters.Add("@idNota", SqlDbType.Int).Value = nroNotaCredito;
                            SqlDataReader rdValidaDet = sqlValidaDet.ExecuteReader();
                            if (rdValidaDet.HasRows)
                            {
                                while (rdValidaDet.Read())
                                {
                                    List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = rdValidaDet.GetString(0);
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdValidaDet.GetString(1);
                                    lineaProducto.unidades = double.Parse(rdValidaDet.GetInt32(2).ToString());
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdValidaDet.GetDouble(3).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdValidaDet.GetDouble(4).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";

                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "Iva";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase = double.Parse(_Valtotal.ToString());
                                    tributosWRKIva.valorImporte = double.Parse(_Valtotal.ToString()) * 0;
                                    TotalGravadoIva = TotalGravadoIva + double.Parse(_Valtotal.ToString());
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);
                                    lineaProducto.tributos = listaTributos;
                                    detalleProductos.Add(lineaProducto);
                                }
                            }
                            else
                            {
                                string qryTipoNota = @"SELECT IdMovimiento,NumDocumento,DesMovimiento,b.IdTipoHomologoDian from cxccarteramovi a
inner join CXCTIPOMOVIMIENTOEFECTONOTAS b on a.idtipomovimiento=b.idtipomovimiento and b.indhabilitado=1
WHERE a.IdMovimiento=@idMovimiento";
                                SqlCommand cmdTipoNota = new SqlCommand(qryTipoNota, conexion01);
                                cmdTipoNota.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = _idMovimiento;
                                SqlDataReader rdTipoNota = cmdTipoNota.ExecuteReader();
                                if (rdTipoNota.HasRows)
                                {
                                    rdTipoNota.Read();
                                    DescNota = rdTipoNota.GetString(2);
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = "NC1";
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = DescNota;
                                    lineaProducto.unidades = 1;// double.Parse(rdValidaDet.GetInt32(2).ToString());
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(_Valtotal.ToString());
                                    lineaProducto.valorBruto = double.Parse(_Valtotal.ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";
                                    detalleProductos.Add(lineaProducto);
                                }
                            }
                        }
                        catch (Exception sqlExp)
                        {
                            string errorSQL = sqlExp.Message + "     " + sqlExp.StackTrace;
                            logFacturas.Warn("Se ha presentado una excepcion de SQL" + errorSQL);
                            throw;
                        }
                    }
                    string ObservacionesNota = string.Empty;
                    string qryObservaciones = @"SELECT Top 1 isnull(b.NomCausal, A.DesMovimiento) Observacion FROM cxcCarteraMovi A
LEFT JOIN genCausal B ON A.IdCausal = B.IdCausal
WHERE IdMovimiento = @idMovimiento";
                    using (SqlCommand cmdObservaciones = new SqlCommand(qryObservaciones, conn))
                    {
                        cmdObservaciones.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = _idMovimiento;
                        SqlDataReader rdObservaciones = cmdObservaciones.ExecuteReader();
                        if (rdObservaciones.HasRows)
                        {
                            rdObservaciones.Read();
                            ObservacionesNota = rdObservaciones.GetString(0);
                        }
                        else
                        {
                            if (_codigoCausal == 6)
                            {
                                ObservacionesNota = $"Nota Credito por Concepto de Otros Motivos No codificados Factura{nroFactura} ";
                            }
                            else if (_codigoCausal == 2)
                            {
                                ObservacionesNota = $"Anulacion de Factura:{nroFactura}";
                            }
                        }
                    }
                    DocumentosAfectadosItem itemAfectado = new DocumentosAfectadosItem();
                    itemAfectado.numeroDocumento = $"{Properties.Settings.Default.Prefijo}-{_facturaRelacionada.ToString()}";//todo: Ingresar el nro de Factura o Nota;
                    itemAfectado.UUID = codCufeFactura; //todo:Ingresar el UUID del Docuemtno afectado
                    itemAfectado.codigoCausal = _codigoCausal;
                    //itemAfectado.fecha = formatosFecha.formatofecha(DateTime.Now); //todo: registrar fechz de la factura o Nota afectada
                    List<string> observaciones = new List<string>();
                    observaciones.Add(ObservacionesNota);
                    itemAfectado.observaciones = observaciones;
                    List<DocumentosAfectadosItem> DocumentosAfectados = new List<DocumentosAfectadosItem>();
                    DocumentosAfectados.Add(itemAfectado);
                    NotaCreditoEnviar.documentosAfectados = DocumentosAfectados;
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
                        valorTotalConImpuestos = double.Parse(_Valtotal.ToString()),// + double.Parse(_ValImpuesto.ToString()),
                        valorNeto = double.Parse(_Valtotal.ToString())
                    };
                    documentoF2.totales = totalesTmp;
                    logFacturas.Info("Numero de Productos Procesados, para JSon:" + detalleProductos.Count);
                    try
                    {

                        //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                        string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                        logFacturas.Info("URL de Request:" + urlConsumo);
                        HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                        //request.Timeout = 60 * 1000;
                        documentoF2.documento = NotaCreditoEnviar;
                        string NotaCreditoJson = JsonConvert.SerializeObject(documentoF2);
                        logFacturas.Info("Json de la Nota Credito::" + NotaCreditoJson);
                        request.Method = "POST";
                        request.ContentType = "application/json";
                        string Usuario = Properties.Settings.Default.usuario;
                        string Clave = Properties.Settings.Default.clave;
                        string credenciales = Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario + ":" + Clave));
                        request.Headers.Add("Authorization", "Basic " + credenciales);
                        Byte[] data = Encoding.UTF8.GetBytes(NotaCreditoJson);
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
                        StreamReader lecturaDatos = new StreamReader(response.GetResponseStream());
                        string datosRespuesta = lecturaDatos.ReadToEnd();
                        logFacturas.Info("Respuesta:" + datosRespuesta);
                        string valorRpta = "00";
                        RespuestaTransfiriendo respuesta = JsonConvert.DeserializeObject<RespuestaTransfiriendo>(datosRespuesta);
                        if (respuesta.esExitoso)
                        {
                            logFacturas.Info($"PDF:{respuesta.resultado.URLPDF}");
                            logFacturas.Info($"XML:{respuesta.resultado.URLXML}");
                            logFacturas.Info($"UUID:{ respuesta.resultado.UUID}");
                            logFacturas.Info($"QR:{respuesta.resultado.QR}");
                            using (SqlConnection connUpdate = new SqlConnection(Properties.Settings.Default.DBConexion))
                            {
                                connUpdate.Open();
                                string strActualiza = @"UPDATE dbo.facNotaTempWEBService SET identificador=@identificador WHERE IdNota=@nroNota AND IdTipoNota=@idTipoNota";
                                SqlCommand cmdActualiza = new SqlCommand(strActualiza, connUpdate);
                                cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                                cmdActualiza.Parameters.Add("@nroNota", SqlDbType.Int).Value = nroNotaCredito;
                                cmdActualiza.Parameters.Add("@idTipoNota", SqlDbType.VarChar).Value = "NC";

                                if (cmdActualiza.ExecuteNonQuery() > 0)
                                {
                                    logFacturas.Info("Nota Credito Actualizada con UUID en facNotaTempWEBService");
                                    using (WebClient webClient = new WebClient())
                                    {
                                        try
                                        {
                                            //string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                            string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfNC + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                            logFacturas.Info("Carpeta de Descarga:" + carpetaDescarga);
                                            webClient.DownloadFile(respuesta.resultado.URLPDF, carpetaDescarga);
                                            //System.Threading.Thread.Sleep(1000);
                                            logFacturas.Info($"Descarga de PDF Nota Credito...Terminada en: {carpetaDescarga}");
                                            carpetaDescarga = Properties.Settings.Default.urlDescargaPdfNC + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".XML";
                                            webClient.DownloadFile(respuesta.resultado.URLXML, carpetaDescarga);
                                            //System.Threading.Thread.Sleep(1000);
                                            logFacturas.Info($"Descarga de XML(ZIP) Nota Credito...Terminada en {carpetaDescarga}");
                                            using (SqlConnection conn3 = new SqlConnection(Properties.Settings.Default.DBConexion))
                                            {
                                                conn3.Open();
                                                string qryActualizaTempWEBService = @"UPDATE dbo.facNotaTempWEBService SET CodCUFE=@cufe,cadenaQR=@cadenaQR WHERE identificador=@identificador";
                                                SqlCommand cmdActualizaTempWEBService = new SqlCommand(qryActualizaTempWEBService, conn3);
                                                cmdActualizaTempWEBService.Parameters.Add("@cufe", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                                                cmdActualizaTempWEBService.Parameters.Add("@cadenaQR", SqlDbType.NVarChar).Value = respuesta.resultado.QR;
                                                cmdActualizaTempWEBService.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                                                if (cmdActualizaTempWEBService.ExecuteNonQuery() > 0)
                                                {
                                                    logFacturas.Info("Descarga Existosa de Archivos de la Nota Credito con Identificadotr:" + respuesta.resultado.UUID + " Destino:" + carpetaDescarga);
                                                    if (!(respuesta.advertencias is null))
                                                    {
                                                        string qryAdvertencia = @"INSERT INTO dbo.facNotaTempWSAdvertencias(IdNota,CodAdvertencia,FecRegistro,DescripcionAdv,idTipoNota) 
VALUES(@IdNota, @CodAdvertencia, @FecRegistro, @DescripcionAdv,@idTipoNota)";
                                                        SqlCommand cmdInsertarAdvertencia = new SqlCommand(qryAdvertencia, conn3);
                                                        cmdInsertarAdvertencia.Parameters.Add("@IdNota", SqlDbType.Int);
                                                        cmdInsertarAdvertencia.Parameters.Add("@CodAdvertencia", SqlDbType.VarChar);
                                                        cmdInsertarAdvertencia.Parameters.Add("@DescripcionAdv", SqlDbType.NVarChar);
                                                        cmdInsertarAdvertencia.Parameters.Add("@FecRegistro", SqlDbType.DateTime);
                                                        cmdInsertarAdvertencia.Parameters.Add("@idTipoNota", SqlDbType.VarChar);
                                                        foreach (AdvertenciasItem itemAdv in respuesta.advertencias)
                                                        {
                                                            cmdInsertarAdvertencia.Parameters["@IdNota"].Value = nroNotaCredito;
                                                            cmdInsertarAdvertencia.Parameters["@CodAdvertencia"].Value = itemAdv.codigo;
                                                            //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                            cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                            cmdInsertarAdvertencia.Parameters["@DescripcionAdv"].Value = itemAdv.mensaje;
                                                            cmdInsertarAdvertencia.Parameters["@idTipoNota"].Value = "NC";
                                                            if (cmdInsertarAdvertencia.ExecuteNonQuery() > 0)
                                                            {
                                                                logFacturas.Info($"Se Inserta Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                                valorRpta = nroNotaCredito.ToString();
                                                            }
                                                            else
                                                            {
                                                                logFacturas.Info($"No es Posible Insertar Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                                valorRpta = nroNotaCredito.ToString();
                                                            }
                                                        }
                                                    }
                                                    valorRpta = nroNotaCredito.ToString();
                                                }
                                                else
                                                {
                                                    logFacturas.Info("No fue Posible Actualizar la Tabla facNotaTempWEBService de la Nota Credito con Identificador:" + respuesta.resultado.UUID);
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
                                            logFacturas.Info("No fue posible descargar los archivos.PDF, XML y QR  !!! Causa:" + exx.Message);
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

                            using (SqlConnection connError = new SqlConnection(Properties.Settings.Default.DBConexion))
                            {
                                connError.Open();
                                string qryInsertaError = @"INSERT INTO facNotaTempWEBServiceError (IdTipoNota,IdNota,CodError,DescripcionError,FecRegistro) 
VALUES(@idTipo,@IdNota, @CodError, @DescripcionError, @FecRegistro)";
                                SqlCommand cmdInsertarError = new SqlCommand(qryInsertaError, connError);
                                cmdInsertarError.Parameters.AddWithValue("@idTipo", SqlDbType.VarChar).Value = "NC";
                                cmdInsertarError.Parameters.Add("@IdNota", SqlDbType.Int).Value = nroNotaCredito;
                                cmdInsertarError.Parameters.Add("@CodError", SqlDbType.VarChar).Value = respuesta.codigo;
                                cmdInsertarError.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = respuesta.mensaje;
                                cmdInsertarError.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = DateTime.Parse(respuesta.fecha);
                                if (cmdInsertarError.ExecuteNonQuery() > 0)
                                {
                                    valorRpta = nroNotaCredito.ToString();
                                    string qryDetErr = @"INSERT INTO facNotaTempWSErrorDetalle (IdNota,CodError,consecutivo,FecRegistro,DescripcionError) 
VALUES(@IdNota, @CodError, @consecutivo, @FecRegistro, @DescripcionError)";
                                    SqlCommand cmdDetErr = new SqlCommand(qryDetErr, conn);
                                    cmdDetErr.Parameters.Add("@IdNota", SqlDbType.Int);
                                    cmdDetErr.Parameters.Add("@CodError", SqlDbType.VarChar);
                                    cmdDetErr.Parameters.Add("@consecutivo", SqlDbType.Int);
                                    cmdDetErr.Parameters.Add("@FecRegistro", SqlDbType.DateTime);
                                    cmdDetErr.Parameters.Add("@DescripcionError", SqlDbType.NVarChar);
                                    List<ErroresItem> listaErrores = new List<ErroresItem>();
                                    int consecutivo = 1;
                                    foreach (ErroresItem itemErr in respuesta.errores)
                                    {
                                        cmdDetErr.Parameters["@IdNota"].Value = nroNotaCredito;
                                        cmdDetErr.Parameters["@CodError"].Value = itemErr.codigo;
                                        cmdDetErr.Parameters["@consecutivo"].Value = consecutivo;
                                        cmdDetErr.Parameters["@FecRegistro"].Value = DateTime.Parse(respuesta.fecha);
                                        cmdDetErr.Parameters["@DescripcionError"].Value = itemErr.mensaje;
                                        if (cmdDetErr.ExecuteNonQuery() > 0)
                                        {
                                            logFacturas.Info($"Se Inserta Detalle de Errores Nota Credito:codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                                            consecutivo++;
                                        }
                                        else
                                        {
                                            logFacturas.Info($"No es Posible Insertar Detalle de Errores Nota Credito: Codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
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
            catch (Exception ex1)
            {
                logFacturas.Warn("Se ha presentado una excepcion:" + ex1.Message + " Pila de LLamados:" + ex1.StackTrace);
                return "98";
            }


        }
    }
}
