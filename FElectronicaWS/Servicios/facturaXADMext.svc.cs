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
using System.Threading;

namespace FElectronicaWS.Servicios
{
    public class facturaXADMext : IfacturaXADMext
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, string urlPdfFactura, string moneda)
        {
            logFacturas.Info("***************************************************************************************************************************");
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
                //Decimal _valPos = 0;
                //Decimal _valNoPos = 0;
                Decimal _valorIvaPesos = 0;
                //double tasaIva = 0;
                //Int32 _IdUsuarioR = 0;
                Int32 _idTercero = 0;
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                //Byte _usrIdTipoDoc = 0;
                string _numDocCliente = string.Empty;
                //Byte _tipoDocCliente = 0;
                Byte _tipoDocClienteDian = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
                string _RegimenFiscal = string.Empty;
                //Int16 _idNaturaleza = 0;
                int concepto = 0;

                FormaPago formaPagoTmp = new FormaPago();
                documentoRoot documentoF2 = new documentoRoot();
                Documento facturaEnviar = new Documento();
                facturaEnviar.identificadorTransaccion = "3464d2c5-c24b-464e-ad46-da9c6cd94de3";
                facturaEnviar.URLPDF = urlPdfFactura;
                facturaEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                facturaEnviar.prefijo = Properties.Settings.Default.Prefijo;
                facturaEnviar.numeroDocumento = nroFactura.ToString();
                facturaEnviar.tipoDocumento = 1;
                facturaEnviar.subTipoDocumento = "01";
                facturaEnviar.tipoOperacion = "10";
                facturaEnviar.moneda = moneda;
                facturaEnviar.generaRepresentacionGrafica = false;
                //ClienteInternacional cliente = new ClienteInternacional();
                string urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteInternacional?idFactura={nroFactura}";
                logFacturas.Info("URL de Request:" + urlClientes);
                HttpWebRequest peticion = WebRequest.Create(urlClientes) as HttpWebRequest;
                peticion.Method = "GET";
                peticion.ContentType = "application/json";
                HttpWebResponse respuestaClientes = peticion.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(respuestaClientes.GetResponseStream());
                string infCliente = sr.ReadToEnd();
                logFacturas.Info("Cliente:" + infCliente);
                Cliente cliente = JsonConvert.DeserializeObject<Cliente>(infCliente);
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryFacturaEnc = @"SELECT  b.numdocrespaldo,IdTipoDocRespaldo,B.IdTercero,B.IdCliente,nomcliente,tdoc.TipoDocDian,B.NumDocumento,ValMonto,ValSaldo,FecRegistroC,FecRadicacion,IndEstado,ValFactura,a.IdUsuarioR,t.NomTercero,t.IdNaturaleza from cxcfacmanual a
inner join cxccuenta b on a.idcuenta=b.idcuenta 
inner join genTercero t ON b.IdTercero=t.IdTercero
inner join homologaTipoDocDian tdoc on b.IdTipoDocCliente=tdoc.IdTipoDoc
WHERE b.numdocrespaldo=@nroFactura";
                    SqlCommand cmdFacturaEnc = new SqlCommand(qryFacturaEnc, conn);
                    cmdFacturaEnc.Parameters.Add("@nroFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdFacturaEnc = cmdFacturaEnc.ExecuteReader();
                    if (rdFacturaEnc.HasRows)
                    {
                        rdFacturaEnc.Read();
                        string valorTf = Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString(); // Valor Toal de la factura Incluido el IVA
                        _Valtotal = Decimal.Parse(valorTf);
                        _ValPagos = 0;
                        //_ValImpuesto = 0;
                        _ValCobrar = Decimal.Parse(Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString()); // Valor a Cobrar
                        cliente.FecFactura = rdFacturaEnc.GetDateTime(9);
                        _tipoDocClienteDian = rdFacturaEnc.GetByte(5);
                    }
                    else
                    {
                        return "No se encontro Informacion General de Encabezado de Factura.";
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
                facturaEnviar.moneda = moneda;
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
                //int _localizacionCliente = 0;
                string _correoCliente = string.Empty;
                string codigoPais = string.Empty;

                using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connx.Open();
                    List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                    NotificacionesItem notificaItem = new NotificacionesItem();
                    notificaItem.tipo = 1;
                    List<string> valorNotificacion = new List<string>();
                    valorNotificacion.Add(cliente.cuenta_correo.Trim());
                    notificaItem.valor = valorNotificacion;
                    notificaciones.Add(notificaItem);
                    facturaEnviar.notificaciones = notificaciones;
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
                            _valorIvaPesos = rdIvaFact.GetDecimal(0); /// valor del IVA de la Factura
                        }
                    }
                    string qryPais = @"SELECT CodigoAlfa2 FROM GEN_DIVI_POLI a 
LEFT JOIN GEN_DIVI_POLI b on substring(a.cod_dipo,1,3) = b.cod_dipo
LEFT JOIN GEN_PAISES_DIAN c on c.idLugarSAHI = b.IdLugar
WHERE a.IdLugar = @idLugar";
                    SqlCommand cmdPais = new SqlCommand(qryPais, connx);
                    cmdPais.Parameters.Add("@idLugar", SqlDbType.VarChar).Value = cliente.idLugar;
                    SqlDataReader rdCodigoPais = cmdPais.ExecuteReader();
                    if (rdCodigoPais.HasRows)
                    {
                        rdCodigoPais.Read();
                        if (rdCodigoPais.IsDBNull(0))
                        { codigoPais = "CO"; }
                        else
                        {
                            codigoPais = rdCodigoPais.GetString(0);
                        }
                    }
                    else
                    {
                        codigoPais = "CO";
                    }
                }
                Adquiriente adquirienteTmp = new Adquiriente();
                adquirienteTmp.identificacion = cliente.NroDoc_Cliente;
                adquirienteTmp.tipoIdentificacion = _tipoDocClienteDian;
                adquirienteTmp.codigoInterno =cliente.IdTercero.ToString();
                adquirienteTmp.razonSocial = cliente.NomTercero;
                adquirienteTmp.nombreSucursal = cliente.NomTercero;
                adquirienteTmp.correo = cliente.cuenta_correo.Trim().Split(';')[0];
                adquirienteTmp.telefono = cliente.telefono;
                if (cliente.idRegimen.Equals("C"))
                {
                    adquirienteTmp.tipoRegimen = "48";
                }
                else
                {
                    adquirienteTmp.tipoRegimen = "49";
                }

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
                }

                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = codigoPais;

                if (cliente.codigoPais=="CO")
                {
                    ubicacionCliente.codigoMunicipio = cliente.codMunicipio;// "00000"; // se ajusta para factura 6180330 Cliente Internacional
                }
                else
                {
                    ubicacionCliente.codigoMunicipio = "00000";
                }
                ubicacionCliente.departamento = cliente.Nombre_Depto;
                ubicacionCliente.ciudad = cliente.Nom_Municipio;
                ubicacionCliente.direccion = cliente.direccion;
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;
                List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                AnticiposItem anticipoWrk = new AnticiposItem();
                anticipoWrk.comprobante = "22";
                anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                anticiposWrk.Add(anticipoWrk);
                documentoF2.anticipos = anticiposWrk;
                //************************************************************ Detalle de Factura por Administrativa Moneda Extranjera ***********************************************************
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conexion01.Open();
                    string strDetalleFac = @"SELECT top 1000 b.numdocrespaldo as factura,CONVERT(VARCHAR,c.IdCuenta)+'-'+CONVERT(varchar,C.num_regi) AS codProducto,c.Descripcion,c.Cantidad,c.val_unit,
c.val_unit* c.Cantidad as valBruto,ISNULL(c.Vr_IVA, 0) AS ValorIva, ISNULL(c.Porcen_IVA, 0) as 'PorcentajeIva',c.*
     FROM cxcfacmanual a
INNER JOIN cxccuenta b on a.idcuenta = b.idcuenta
INNER JOIN cxcfacmanualconc c on c.IdCuenta = a.idcuenta
WHERE c.val_unit_inte is NOT null and NumDocRespaldo = @idFactura ORDER BY c.num_regi";
                    SqlCommand cmdDetalleFac = new SqlCommand(strDetalleFac, conexion01);
                    cmdDetalleFac.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdDetalleFac = cmdDetalleFac.ExecuteReader();
                    if (rdDetalleFac.HasRows)
                    {
                        #region MyRegion
                        //Int16 nroLinea = 1;
                        //while (rdDetalleFac.Read())
                        //{
                        //    tasaIva = ((double.Parse(_valorIvaPesos.ToString())) / (double.Parse(rdDetalleFac.GetDecimal(6).ToString())) * 100).TomarDecimales(2); ;
                        //    tasaIva = Math.Round(tasaIva, 0);
                        //    try
                        //    {
                        //        List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                        //        DetallesItem lineaProducto = new DetallesItem();
                        //        lineaProducto.tipoDetalle = 1; // Linea Normal
                        //        string codigoProducto = rdDetalleFac.GetInt32(1).ToString();
                        //        lineaProducto.valorCodigoInterno = codigoProducto;
                        //        lineaProducto.codigoEstandar = "999";
                        //        lineaProducto.valorCodigoEstandar = codigoProducto;
                        //        lineaProducto.descripcion = rdDetalleFac.GetString(4);
                        //        lineaProducto.unidades = double.Parse(rdDetalleFac.GetInt32(3).ToString());
                        //        lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                        //        lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDecimal(7).ToString());
                        //        ValBrutoProd = ValBrutoProd + (double.Parse(rdDetalleFac.GetDecimal(7).ToString()) * double.Parse(rdDetalleFac.GetInt32(3).ToString()));
                        //        lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetInt32(3).ToString()) * double.Parse(rdDetalleFac.GetDecimal(7).ToString());
                        //        lineaProducto.valorBrutoMoneda = moneda;
                        //        //detalleProductos.Add(lineaProducto);

                        //        TibutosDetalle tributosWRKIva = new TibutosDetalle();
                        //        tributosWRKIva.id = "01";
                        //        tributosWRKIva.nombre = "Iva";
                        //        tributosWRKIva.esImpuesto = true;
                        //        tributosWRKIva.porcentaje = double.Parse(tasaIva.ToString());

                        //        tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDecimal(7).ToString());
                        //        _totalIvaProd = _totalIvaProd+((double.Parse(rdDetalleFac.GetDecimal(7).ToString()) * double.Parse(tasaIva.ToString())) / 100).TomarDecimales(2);

                        //        tributosWRKIva.valorImporte = ((double.Parse(rdDetalleFac.GetDecimal(7).ToString()) * double.Parse(tasaIva.ToString()))/100).TomarDecimales(2);
                        //        TotalGravadoIva = TotalGravadoIva + double.Parse(rdDetalleFac.GetDecimal(7).ToString());
                        //        tributosWRKIva.tributoFijoUnidades = 0;
                        //        tributosWRKIva.tributoFijoValorImporte = 0;
                        //        listaTributos.Add(tributosWRKIva);

                        //        lineaProducto.tributos = listaTributos;
                        //        detalleProductos.Add(lineaProducto);
                        //        nroLinea++;
                        //    }
                        //    catch (Exception sqlExp)
                        //    {
                        //        string error = $"Mensaje de Error:{sqlExp.Message}   Traza de la Pila:{sqlExp.StackTrace}";
                        //        logFacturas.Warn($"Se ha presentado una Excepcion:{error}");
                        //        throw;
                        //    }
                        //} 
                        #endregion
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
                                lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetSqlMoney(14).ToString()).TomarDecimales(2);
                                lineaProducto.valorBrutoMoneda = moneda;// "COP";
                                double totalUnitario = lineaProducto.valorUnitarioBruto * lineaProducto.unidades;
                                lineaProducto.valorBruto = totalUnitario.TomarDecimales(2);// double.Parse(rdDetalleFac.GetSqlDouble(18).ToString());
                                ValorBruto += totalUnitario.TomarDecimales(2);// double.Parse(rdDetalleFac.GetSqlDouble(18).ToString());
                                TibutosDetalle tributosWRKIva = new TibutosDetalle(); //Detalle de tributos, para el producto
                                tributosWRKIva.id = "01";
                                tributosWRKIva.nombre = "Iva";
                                tributosWRKIva.esImpuesto = true;
                                tributosWRKIva.porcentaje = double.Parse(rdDetalleFac.GetInt32(16).ToString()).TomarDecimales(2);
                                tributosWRKIva.valorBase = totalUnitario;// double.Parse(rdDetalleFac.GetSqlDouble(18).ToString());

                                tributosWRKIva.valorImporte = double.Parse(rdDetalleFac.GetSqlDouble(19).ToString()).TomarDecimales(2);

                                tributosWRKIva.tributoFijoUnidades = 0;
                                tributosWRKIva.tributoFijoValorImporte = 0;
                                listaTributos.Add(tributosWRKIva);
                                lineaProducto.tributos = listaTributos;
                                if (rdDetalleFac.GetInt32(16) > 0)
                                {
                                    ValorTasaIva = rdDetalleFac.GetInt32(16);
                                    TotalGravadoIva += totalUnitario;// double.Parse(rdDetalleFac.GetSqlMoney(14).ToString());
                                    ValorTotalIva += double.Parse(rdDetalleFac.GetSqlDouble(19).ToString()).TomarDecimales(2);
                                }
                                else
                                {
                                    TotalExcentoIva += totalUnitario;// double.Parse(rdDetalleFac.GetSqlDouble(18).ToString());
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

                documentoF2.detalles = detalleProductos;

                List<TributosItem> tributosTMP = new List<TributosItem>(); //Segmento de Iva  pata Totales de Factura
                List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                DetalleTributos detalleTributosItem = new DetalleTributos(); // Un Objeto por cada Tipo de Iva
                //***estos valores se deben tomar del select o Tabla
                //double valorImporte = double.Parse(_valorIva.ToString());
                //double valorBase = double.Parse((_Valtotal - _valorIva).ToString());
                //double porcentaje = Math.Round(((valorImporte / valorBase) * 100).TomarDecimales(2), 0);
                //detalleTributosItem.valorImporte = valorImporte;
                //detalleTributosItem.valorBase = valorBase;
                //detalleTributosItem.porcentaje = porcentaje;
                detalleTributosItem.valorImporte = ValorTotalIva;
                detalleTributosItem.valorBase = TotalGravadoIva;
                detalleTributosItem.porcentaje = double.Parse(ValorTasaIva.ToString()).TomarDecimales(2);
                tributosDetalle.Add(detalleTributosItem);
                TributosItem itemTributo = new TributosItem()
                {
                    id = "01", //Total de Iva 
                    nombre = "Iva",
                    esImpuesto = true,
                    valorImporteTotal = ValorTotalIva,
                    detalles = tributosDetalle // Detalle de los Ivas
                };
                tributosTMP.Add(itemTributo);
                documentoF2.tributos = tributosTMP;///
                Totales totalesTmp = new Totales()
                {
                    valorBruto = ValorBruto,   // double.Parse(ValorBruto.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = ValorBruto,
                    valorTotalConImpuestos = ValorBruto - double.Parse(_ValPagos.ToString()) + double.Parse(ValorTotalIva.ToString()),
                    valorNeto = ValorBruto - double.Parse(_ValPagos.ToString()) + double.Parse(ValorTotalIva.ToString()) //double.Parse(_ValCobrar.ToString())
                };
                documentoF2.totales = totalesTmp;
                logFacturas.Info("Numero de Productos procesados, para JSon:" + detalleProductos.Count);

                double tasa = 0;
                string monedaDestino = String.Empty;
                DateTime fechaTrm = DateTime.Now;
                using (SqlConnection connX1 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connX1.Open();
                    string qryTasa = "SELECT Trm,FechaTrm,MonedaOrigen FROM facFacturaTasaTrm WHERE idFactura=@idFactura";
                    SqlCommand cmdTasa = new SqlCommand(qryTasa, connX1);
                    cmdTasa.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdTasa = cmdTasa.ExecuteReader();
                    if (rdTasa.HasRows)
                    {
                        rdTasa.Read();
                        tasa = rdTasa.GetDouble(0);
                        fechaTrm = rdTasa.GetDateTime(1);
                        monedaDestino = rdTasa.GetString(2);
                    }
                    else
                    {
                        logFacturas.Info($"No se encontro Tasa de Cambio para la Factura Numero:{nroFactura}");
                    }
                }
                formatoWrk = formatosFecha.formatofecha(fechaTrm);
                TRM tasaInf = new TRM();
                tasaInf.valor = tasa;
                tasaInf.monedaDestino = monedaDestino;
                tasaInf.fecha = formatoWrk.Split('T')[0];
                facturaEnviar.TRM = tasaInf;

                logFacturas.Info("Numero de Productos procesados, para JSon:" + detalleProductos.Count);
                try
                {
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    //request.Timeout = 60 * 1000;
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
                    StreamReader lectorDatos = new StreamReader(response.GetResponseStream());
                    string datosRespuesta = lectorDatos.ReadToEnd();
                    logFacturas.Info("Respuesta Recibida Transfiriendo:" + datosRespuesta);
                    string valorRpta = "00";

                    RespuestaTransfiriendo respuesta = JsonConvert.DeserializeObject<RespuestaTransfiriendo>(datosRespuesta);
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
                                                        cmdInsertarAdvertencia.Parameters["@CodAdvertencia"].Value = itemAdv.codigo;
                                                        //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                        cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                        cmdInsertarAdvertencia.Parameters["@DescripcionAdv"].Value = itemAdv.mensaje;
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
                    //}
                    //else
                    //{
                    //    logFacturas.Info("!!!  Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
                    //    logFacturas.Warn("Respuesta recibida:" + strsb);
                    //    //*Aqui se debe insertar en la tabla de fallas
                    //    return "Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService";
                    //}

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
