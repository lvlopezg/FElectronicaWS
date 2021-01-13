using FElectronicaWS.Contratos;
using System;
using FElectronicaWS.Clases;
using System.Collections.Generic;
using System.Net;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;
using NLog;
using System.Data;
using System.Text;

namespace FElectronicaWS.Servicios
{
    public class notaDebito : InotaDebito
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        
        public string getData(int nroNotaDebito, int idCliente, int nroAtencion, string monedaNota, int nroFactura, string urlPdfNotaDebito)
        {
            logFacturas.Info($"Se recibe Nota Debito con siguientes datos:Nro Nota:{ nroNotaDebito}  IdCliente: {idCliente} nroAtencion:{nroAtencion} urlPdf:{urlPdfNotaDebito}");
            try
            {
                #region inicializar
                // Inicializacion
                //Int32 _idContrato = 0;
                Decimal _Valtotal = 0;
                //Decimal _ValDescuento = 0;
                //Decimal _ValDescuentoT = 0;
                Decimal _ValPagos = 0;
                //Decimal _ValImpuesto = 0;
                Decimal _ValCobrar = 0;
                DateTime _FecNotaDebito = DateTime.Now;
                Int32 _facturaRelacionada = 0;
                Int32 _idMovimiento = 0;
                //Decimal _valPos = 0;
                //Decimal _valNoPos = 0;
                //Int32 _IdUsuarioR = 0;
                Int32 _idTercero = 0;
                //string _usrNombre = string.Empty;
                //string _usrNumDocumento = string.Empty;
                //Byte _usrIdTipoDoc = 0;
                //string _numDocCliente = string.Empty;
                //Byte _tipoDocCliente = 0;
                //string _razonSocial = string.Empty;
                //string _repLegal = string.Empty;
                //string _RegimenFiscal = string.Empty;
                //Int16 _idNaturaleza = 0;
                //int concepto = 0;
                string DescNota = string.Empty;
                FormaPago formaPagoTmp = new FormaPago();

                #endregion
                //Fin de Inicializacion
                documentoRoot documentoF2 = new documentoRoot();
                Documento NotaDebitoEnviar = new Documento();
                NotaDebitoEnviar.identificadorTransaccion = "D7F719C2 - 75F4 - 4F06 - B7CB - F583FC28DBEE";
                NotaDebitoEnviar.URLPDF = urlPdfNotaDebito;
                NotaDebitoEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                NotaDebitoEnviar.prefijo = Properties.Settings.Default.PrefijoNotaND;
                NotaDebitoEnviar.numeroDocumento = nroNotaDebito.ToString();
                NotaDebitoEnviar.tipoDocumento = 3;
                NotaDebitoEnviar.subTipoDocumento = "92";
                NotaDebitoEnviar.tipoOperacion = "10";
                NotaDebitoEnviar.generaRepresentacionGrafica = false;

                //ClienteJuridico cliente = new ClienteJuridico();
                //string urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteJuridico?idFactura={nroFactura}";
                //logFacturas.Info("URL de Request:" + urlClientes);
                //HttpWebRequest peticion = WebRequest.Create(urlClientes) as HttpWebRequest;
                //peticion.Method = "GET";
                //peticion.ContentType = "application/json";
                //HttpWebResponse respuestaClientes = peticion.GetResponse() as HttpWebResponse;
                //StreamReader sr = new StreamReader(respuestaClientes.GetResponseStream());
                //string infCliente = sr.ReadToEnd();
                //logFacturas.Info("Cliente:" + infCliente);
                //cliente = JsonConvert.DeserializeObject<ClienteJuridico>(infCliente);
                bool facXRel = false;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryEspeciales = "SELECT IdDestino,IndTipoFactura FROM facFactura where IdFactura=@idFactura";
                    SqlCommand cmdEspeciales = new SqlCommand(qryEspeciales, conn);
                    cmdEspeciales.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
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
                //ClienteInternacional cliente = new ClienteInternacional();
                if (idCliente == 0 && nroAtencion == 0 && !facXRel)
                {
                    urlClientes = $"{Properties.Settings.Default.urlServicioClientes}ClienteInternacional?idFactura={nroFactura}";
                }
                else
                {
                    //ClienteJuridico cliente = new ClienteJuridico();
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
                int _localizacionCliente = 0;
                string _correoCliente = string.Empty;
                //**** 
                string codCufeFactura = string.Empty;

                List<DetallesItem> detalleProductos = new List<DetallesItem>();

                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();

                    //string qrycodCufeFact = "SELECT CodCUFE FROM facFacturaTempWEBService WHERE IdFactura=@idFactura"; // TODO:Acoplar Historioco
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
                        // Retora mensaje de error. Porque no hay CUFE de Factyura
                    }
                    //FormaPago formaPagoTmp = new FormaPago();
                    string formatoWrk = formatosFecha.formatofecha(_FecNotaDebito);
                    NotaDebitoEnviar.fechaEmision = formatoWrk.Split('T')[0];
                    NotaDebitoEnviar.horaEmision = formatoWrk.Split('T')[1];
                    NotaDebitoEnviar.moneda = monedaNota;
                    formaPagoTmp.tipoPago = 1;
                    formaPagoTmp.codigoMedio = "10";
                    NotaDebitoEnviar.formaPago = formaPagoTmp;

                    string qryNotaDebito1 = @"SELECT B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,B.ValMonto,cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,cxcCta.ValFactura,B.IdMovimiento FROM cxcTipoMovimientoEfectoNotas A
INNER JOIN cxcCarteraMovi B ON A.IdTipoMovimiento=B.IdTipoMovimiento
INNER JOIN cxcCuenta cxcCta ON cxcCta.IdCuenta=B.IdCuenta
LEFT JOIN cxcCarteraMoviNoNota C ON B.IdMovimiento=C.IdMovimiento
where A.Indhabilitado=1 and C.IdMovimiento IS NULL
and B.NumDocumento=@nroNotaDebito and A.IndTipoNota='D'
union
SELECT B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,B.ValMonto,
cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,cxcCta.ValFactura,B.IdMovimiento FROM cxcTipoMovimientoEfectoNotas A
INNER JOIN cxcCarteraMovi B ON A.IdTipoMovimiento=B.IdTipoMovimiento
INNER JOIN cxcCuenta cxcCta ON cxcCta.IdCuenta=B.IdCuenta
INNER JOIN cxcCarteraMoviNoNota C ON B.IdMovimiento=C.IdMovimiento
where A.Indhabilitado=1 
and C.IdNumeroNota=@nroNotaDebito2 and A.IndTipoNota='D'";
                    SqlCommand cmdNotaDebito = new SqlCommand(qryNotaDebito1, conn);
                    cmdNotaDebito.Parameters.Add("@nroNotaDebito", SqlDbType.VarChar).Value = nroNotaDebito.ToString();
                    cmdNotaDebito.Parameters.Add("@nroNotaDebito2", SqlDbType.Int).Value = nroNotaDebito;
                    SqlDataReader rdNotaDebito = cmdNotaDebito.ExecuteReader();
                    if (rdNotaDebito.HasRows)
                    {
                        rdNotaDebito.Read();
                        _idTercero = rdNotaDebito.GetInt32(7);
                        _idMovimiento = rdNotaDebito.GetInt32(14);
                        var valorTNota = rdNotaDebito["ValMonto"];
                        _Valtotal = Decimal.Parse(valorTNota.ToString());
                        _ValCobrar = Decimal.Parse(valorTNota.ToString());
                        _FecNotaDebito = rdNotaDebito.GetDateTime(4);
                        _facturaRelacionada = Int32.Parse(rdNotaDebito.GetString(12));
                        //_idTercero = rdNotaDebito.GetInt32(7);
                    }
                    else
                    {
                        List<ErroresItem> detalle = new List<ErroresItem>();
                        ErroresItem item = new ErroresItem();
                        item.codigo = "990101"; //codigo de Error Especifico en HUSIpara adherirse al Modelo de Errores Transfiriendo
                        item.mensaje = "No hay informacion disponible de la Nota, para obtener informacion necesaria. La Nota no existe";
                        detalle.Add(item);
                        return UtilidadRespuestas.insertarErrorND("ND", nroNotaDebito, "9901", "Informacion de la Nota No Encontada", DateTime.Now, detalle);
                    }

                    #region MyRegion
                    //string strTercero = "SELECT IdTipoDoc,NumDocumento,NomTercero,idnaturaleza FROM genTercero WHERE IdTercero=@tercero";
                    //SqlCommand cmdTercero = new SqlCommand(strTercero, conn);
                    //cmdTercero.Parameters.Add("@tercero", SqlDbType.Int).Value = _idTercero;
                    //SqlDataReader rdtercero = cmdTercero.ExecuteReader();
                    //if (rdtercero.HasRows)
                    //{
                    //    rdtercero.Read();
                    //    _tipoDocCliente = rdtercero.GetByte(0);
                    //    _numDocCliente = rdtercero.GetString(1);
                    //    _razonSocial = rdtercero.GetString(2);
                    //    _idNaturaleza = rdtercero.GetInt16(3);
                    //}
                    //                    string qryDatosCliente1 = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar,RIGHT(B.cod_dipo,5) FROM genTerceroLocaliza A
                    //LEFT JOIN GEN_DIVI_POLI B ON A.IdLugar=B.IdLugar
                    //WHERE IdTercero=@idTercero and IdLocalizaTipo IN (2,3)
                    //ORDER BY IdLocalizaTipo";
                    //                    SqlCommand cmdDatosCliente1 = new SqlCommand(qryDatosCliente1, conn);
                    //                    cmdDatosCliente1.Parameters.Add("@idTercero", SqlDbType.Int).Value = _idTercero;
                    //                    SqlDataReader rdDatosCliente1 = cmdDatosCliente1.ExecuteReader();
                    //                    if (rdDatosCliente1.HasRows)
                    //                    {
                    //                        while (rdDatosCliente1.Read())
                    //                        {
                    //                            if (rdDatosCliente1.GetInt32(0) == 2)
                    //                            {
                    //                                _direccionCliente = rdDatosCliente1.GetString(1);
                    //                                _municipioCliente = rdDatosCliente1.GetString(4);
                    //                                _localizacionCliente = rdDatosCliente1.GetInt32(3);
                    //                            }
                    //                            else if (rdDatosCliente1.GetInt32(0) == 3)
                    //                            {
                    //                                if (rdDatosCliente1.GetString(1).Length>10)
                    //                                {
                    //                                    _telefonoCliente = rdDatosCliente1.GetString(1).Substring(0,10);
                    //                                }
                    //                                else
                    //                                {
                    //                                    _telefonoCliente = rdDatosCliente1.GetString(1);
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                    string qryDatosCliente2 = @"SELECT COD_DEPTO,COD_MPIO,DPTO,NOM_MPIO FROM GEN_DIVI_POLI A
                    //                    INNER JOIN HUSI_Divipola HB ON a.num_ptel=COD_DEPTO
                    //                    WHERE a.IdLugar=@idLugar";
                    //                    SqlCommand cmdDatosCliente2 = new SqlCommand(qryDatosCliente2, conn);
                    //                    cmdDatosCliente2.Parameters.Add("@idLugar", SqlDbType.Int).Value = _localizacionCliente;
                    //                    SqlDataReader rdDatosCliente2 = cmdDatosCliente2.ExecuteReader();
                    //                    if (rdDatosCliente2.HasRows)
                    //                    {
                    //                        rdDatosCliente2.Read();
                    //                        _departamento = rdDatosCliente2.GetString(2);
                    //                    }
                    //                    string qryDatosCliente3 = @"SELECT A.Correo FROM concontratocorreo A
                    //INNER JOIN facFactura B ON A.IdContrato=B.IdContrato
                    //WHERE A.indhabilitado=1 AND B.idFactura=@idFactura
                    //UNION ALL
                    //SELECT A.Deslocalizacion As Correo FROM gentercerolocaliza A
                    //INNER JOIN conContrato C ON C.IdTercero=A.IdTercero
                    //INNER JOIN  facFactura D ON D.IdContrato=C.IdContrato
                    //LEFT JOIN concontratocorreo B ON  B.indhabilitado=1 and B.idcontrato=D.IdContrato  
                    //WHERE B.idcontrato is null and A.IdLocalizaTipo=1 and A.indhabilitado=1 and D.IdFactura=@idFactura";
                    //                    SqlCommand cmdDatosCliente3 = new SqlCommand(qryDatosCliente3, conn);
                    //                    cmdDatosCliente3.Parameters.Add("@idFactura", SqlDbType.Int).Value = _facturaRelacionada;
                    //                    SqlDataReader rdDatosCliente3 = cmdDatosCliente3.ExecuteReader();
                    //                    if (rdDatosCliente3.HasRows)
                    //                    {
                    //                        rdDatosCliente3.Read();
                    //                        _correoCliente = rdDatosCliente3.GetString(0);
                    //                    }
                    //                    else
                    //                    {
                    //                        _correoCliente = "";
                    //                    } 
                    #endregion
                }
                Adquiriente adquirienteTmp = new Adquiriente();
                adquirienteTmp.identificacion = cliente.NroDoc_Cliente;
                if (cliente.TipoDoc_Cliente == 1)//TODO: validar la Homologacion para este campo
                {
                    adquirienteTmp.tipoIdentificacion = 31;
                }
                else if (cliente.TipoDoc_Cliente == 2)
                {
                    adquirienteTmp.tipoIdentificacion = 13;
                }
                adquirienteTmp.codigoInterno =cliente.IdTercero.ToString();
                adquirienteTmp.razonSocial = cliente.NomTercero;
                adquirienteTmp.nombreSucursal = cliente.NomTercero;
                adquirienteTmp.correo = cliente.cuenta_correo.Trim().Split(';')[0];
                adquirienteTmp.telefono = cliente.telefono;

                List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                NotificacionesItem notificaItem = new NotificacionesItem();
                notificaItem.tipo = 1;
                List<string> valorNotificacion = new List<string>();
                valorNotificacion.Add(cliente.cuenta_correo.Trim());
                notificaItem.valor = valorNotificacion;
                notificaciones.Add(notificaItem);
                NotaDebitoEnviar.notificaciones = notificaciones;

                using (SqlConnection connXX = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connXX.Open();
                    string qryTipoDocDian = "SELECT TipoDocDian FROM homologaTipoDocDian WHERE IdTipoDoc=@tipoDoc";
                    SqlCommand cmdTipoDocDian = new SqlCommand(qryTipoDocDian, connXX);
                    cmdTipoDocDian.Parameters.Add("@tipoDoc", SqlDbType.TinyInt).Value = cliente.TipoDoc_Cliente;
                    Int16 tipoDoc = Int16.Parse(cmdTipoDocDian.ExecuteScalar().ToString());
                    adquirienteTmp.tipoIdentificacion = byte.Parse(tipoDoc.ToString());
                }

                if (cliente.idRegimen.Equals("C"))
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
                            NotaDebitoEnviar.tipoOperacion = "10"; //Standar
                        }
                        else
                        {
                            NotaDebitoEnviar.tipoOperacion = "32"; //Standar nota debito sin referencia a facturas
                        }
                    }
                    else
                    {
                        logFacturas.Error($"No hay Datos de Factura, para la Factura:{nroFactura}");
                    }
                }
                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = cliente.codigoPais;//"CO";

                if (cliente.codigoPais == "CO")
                {
                    ubicacionCliente.codigoMunicipio = cliente.codMunicipio;// "00000"; // se ajusta para factura 6180330 Cliente Internacional
                }
                else
                {
                    ubicacionCliente.codigoMunicipio = "00000";
                }
                //ubicacionCliente.codigoMunicipio = cliente.codMunicipio;
                ubicacionCliente.direccion = cliente.direccion;
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;
                #region MyRegion

                #endregion
                double TotalGravadoIva = 0;
                //double TotalGravadoIca = 0;
                //************************************************************ Detalle de Nota Debito   ***********************************************************
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conexion01.Open();
                    try
                    {
                        SqlCommand sqlValidaDet = new SqlCommand("spTraeDetalleNotas", conexion01);
                        sqlValidaDet.CommandType = CommandType.StoredProcedure;
                        sqlValidaDet.Parameters.Add("@idNota", SqlDbType.Int).Value = nroNotaDebito;
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
                                lineaProducto.valorUnitarioBruto = rdValidaDet.GetDouble(3);
                                lineaProducto.valorBruto = rdValidaDet.GetDouble(4);
                                lineaProducto.valorBrutoMoneda = monedaNota;

                                TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                tributosWRKIva.id = "01";
                                tributosWRKIva.nombre = "Iva";
                                tributosWRKIva.esImpuesto = true;
                                tributosWRKIva.porcentaje = 0;
                                tributosWRKIva.valorBase =rdValidaDet.GetDouble(4);
                                tributosWRKIva.valorImporte = double.Parse(_Valtotal.ToString()) * 0;
                                TotalGravadoIva = TotalGravadoIva +rdValidaDet.GetDouble(4);
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
                            SqlDataReader rdDetalleFac = cmdTipoNota.ExecuteReader();
                            if (rdDetalleFac.HasRows)
                            {
                                rdDetalleFac.Read();
                                DescNota = rdDetalleFac.GetString(2);
                                DetallesItem lineaProducto = new DetallesItem();
                                lineaProducto.tipoDetalle = 1; // Linea Normal
                                string codigoProducto = "ND1";
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
                        #region MyRegion_01
                        //                        string qryTipoNota = @"SELECT IdMovimiento,NumDocumento,DesMovimiento,b.IdTipoHomologoDian from cxccarteramovi a
                        //inner join CXCTIPOMOVIMIENTOEFECTONOTAS b on a.idtipomovimiento=b.idtipomovimiento and b.indhabilitado=1
                        //WHERE a.IdMovimiento=@idMovimiento";
                        //                        SqlCommand cmdTipoNota = new SqlCommand(qryTipoNota, conexion01);
                        //                        cmdTipoNota.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = _idMovimiento;
                        //                        SqlDataReader rdDetalleFac = cmdTipoNota.ExecuteReader();
                        //                        rdDetalleFac.Read();
                        //                        string DescNota = rdDetalleFac.GetString(2);
                        //                        try
                        //                        {
                        //                            List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                        //                            DetallesItem lineaProducto = new DetallesItem();
                        //                            lineaProducto.tipoDetalle = 1; // Linea Normal
                        //                            string codigoProducto = rdDetalleFac.GetString(1);
                        //                            lineaProducto.valorCodigoInterno = codigoProducto;
                        //                            if (rdDetalleFac.GetInt16(18) == 5 || rdDetalleFac.GetInt16(18) == 6)
                        //                            {
                        //                                lineaProducto.codigoEstandar = "999";
                        //                            }
                        //                            else
                        //                            {
                        //                                lineaProducto.codigoEstandar = "999";
                        //                            }
                        //                            lineaProducto.valorCodigoEstandar = codigoProducto;
                        //                            lineaProducto.descripcion = rdDetalleFac.GetString(2);

                        //                            lineaProducto.unidades = 1;
                        //                            lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                        //                            lineaProducto.valorUnitarioBruto = double.Parse(_Valtotal.ToString());
                        //                            lineaProducto.valorBruto = double.Parse(_Valtotal.ToString());
                        //                            lineaProducto.valorBrutoMoneda = "COP";

                        //                            TibutosDetalle tributosWRKIva = new TibutosDetalle();
                        //                            tributosWRKIva.id = "01";
                        //                            tributosWRKIva.nombre = "Iva";
                        //                            tributosWRKIva.esImpuesto = true;
                        //                            tributosWRKIva.porcentaje = 0;
                        //                            tributosWRKIva.valorBase = double.Parse(_Valtotal.ToString());
                        //                            tributosWRKIva.valorImporte = double.Parse(_Valtotal.ToString()) * 0;
                        //                            TotalGravadoIva = TotalGravadoIva + double.Parse(_Valtotal.ToString());
                        //                            tributosWRKIva.tributoFijoUnidades = 0;
                        //                            tributosWRKIva.tributoFijoValorImporte = 0;
                        //                            listaTributos.Add(tributosWRKIva);

                        //                            lineaProducto.tributos = listaTributos;

                        //                            detalleProductos.Add(lineaProducto);

                        //                        }
                        //                        catch (Exception sqlExp)
                        //                        {
                        //                            string error = sqlExp.Message + "   " + sqlExp.StackTrace;
                        //                            throw;
                        //                        }
                        //                        //**** fin de Modificacion 
                        #endregion
                    }
                    catch (Exception sqlExp)
                    {
                        string error = sqlExp.Message + "   " + sqlExp.StackTrace;
                        throw;
                    }
                }
                string ObservacionesNota = string.Empty;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
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
                            ObservacionesNota = $"Nota Debito Por Concepto de Cambio de Valor Factura{nroFactura} ";
                        }
                    }
                }
                DocumentosAfectadosItem itemAfectado = new DocumentosAfectadosItem();
                itemAfectado.numeroDocumento = $"{Properties.Settings.Default.Prefijo}-{_facturaRelacionada.ToString()}";//todo: Ingresar el nro de Factura o Nota;
                itemAfectado.UUID = codCufeFactura; //todo:Ingresar el UUID del Docuemtno afectado
                itemAfectado.codigoCausal = 3;
                //itemAfectado.fecha = formatosFecha.formatofecha(DateTime.Now); //todo: registrar fechz de la factura o Nota afectada
                List<string> observaciones = new List<string>();
                observaciones.Add(ObservacionesNota);
                itemAfectado.observaciones = observaciones;
                List<DocumentosAfectadosItem> DocumentosAfectados = new List<DocumentosAfectadosItem>();
                DocumentosAfectados.Add(itemAfectado);
                NotaDebitoEnviar.documentosAfectados = DocumentosAfectados;

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
                    #region MyRegionAnterior
                    //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                    //logFacturas.Info("URL de Request:" + urlConsumo);
                    //HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    //request.Timeout = 60 * 1000;
                    //string facturaJson = JsonConvert.SerializeObject(notaEnviar);
                    //logFacturas.Info("Json de la Nota Debito:" + facturaJson);
                    //request.Method = "POST";
                    //request.ContentType = "application/json";
                    //string Usuario = "admin";
                    //string Clave = "super";
                    //string credenciales = Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario + ":" + Clave));
                    //request.Headers.Add("Authorization", "Basic " + credenciales);

                    //Byte[] data = Encoding.UTF8.GetBytes(facturaJson);

                    //Stream st = request.GetRequestStream();
                    //st.Write(data, 0, data.Length);
                    //st.Close();

                    //int loop1, loop2;
                    //NameValueCollection valores;
                    //valores = request.Headers;

                    //// Pone todos los nombres en un Arregle
                    //string[] arr1 = valores.AllKeys;
                    //for (loop1 = 0; loop1 < arr1.Length; loop1++)
                    //{
                    //    logFacturas.Info("Key: " + arr1[loop1] + "<br>");
                    //    // Todos los valores
                    //    string[] arr2 = valores.GetValues(arr1[loop1]);
                    //    for (loop2 = 0; loop2 < arr2.Length; loop2++)
                    //    {
                    //        logFacturas.Info("Value " + loop2 + ": " + arr2[loop2]);
                    //    }
                    //}
                    //HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    //StreamReader sr = new StreamReader(response.GetResponseStream());
                    //string strsb = sr.ReadToEnd();
                    //object objResponse = JsonConvert.DeserializeObject(strsb);//, JSONResponseType);
                    //logFacturas.Info("Respuesta Transfiriendo:" + strsb);
                    //string valorRpta = "00";
                    //string validacion = "\"PDF\":";
                    //string validacionError = "\"Errors\":";
                    //if (strsb.Contains(validacion))
                    //{
                    //    respuestaEntregaExitosa rptaEntrega = JsonConvert.DeserializeObject<respuestaEntregaExitosa>(strsb);
                    //    logFacturas.Info("PDF:" + rptaEntrega.Resultado.URLPDF);
                    //    logFacturas.Info("XML:" + rptaEntrega.Resultado.URLXML);
                    //    logFacturas.Info("CUFE:" + rptaEntrega.Resultado.UUID);
                    //    if (rptaEntrega.EsExitoso)
                    //    {
                    //        using (SqlConnection conn2 = new SqlConnection(Properties.Settings.Default.DBConexion))
                    //        {
                    //            conn2.Open();
                    //            string strActualiza = @"UPDATE dbo.facNotaTempWEBService SET identificador=@identificador WHERE IdNota=@nroNota AND IdTipoNota=@idTipoNota";
                    //            SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
                    //            cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
                    //            cmdActualiza.Parameters.Add("@nroNota", SqlDbType.Int).Value = nroNotaDebito;
                    //            cmdActualiza.Parameters.Add("@idTipoNota", SqlDbType.VarChar).Value = "ND";
                    //            if (cmdActualiza.ExecuteNonQuery() > 0)
                    //            {
                    //                logFacturas.Info("Nota Debito Actualizada en facNotaTempWEBService");
                    //                using (WebClient webClient = new WebClient())
                    //                {
                    //                    try
                    //                    {

                    //                        System.Threading.Thread.Sleep(1000);
                    //                        string[] arregloPDF = rptaEntrega.Resultado.URLPDF.Split('/');
                    //                        string nombreArchivo = arregloPDF[arregloPDF.Length - 1];
                    //****************************************************************
                    //                        string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfND + DateTime.Now.Year + @"\" + nombreArchivo + ".pdf";
                    //                        webClient.DownloadFile(rptaEntrega.Resultado.URLPDF, carpetaDescarga);
                    //                        //System.Threading.Thread.Sleep(1000);
                    //                        carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.UUID + ".zip";
                    //                        webClient.DownloadFile(rptaEntrega.Resultado.URLXML, carpetaDescarga);
                    //                        //webClient.DownloadFile(rptaEntrega.Resultado.ZIP, @"D:\NotaDebitoQR\" + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.CUFE + ".zip");
                    //                        using (SqlConnection conn3 = new SqlConnection(Properties.Settings.Default.DBConexion))
                    //                        {
                    //                            conn3.Open();
                    //                            string qryActualizaTempWEBService = @"UPDATE dbo.facNotaTempWEBService SET CodCUFE=@cufe,cadenaQR=@cadenaQR WHERE identificador=@identificador";
                    //                            SqlCommand cmdActualizaTempWEBService = new SqlCommand(qryActualizaTempWEBService, conn);
                    //                            cmdActualizaTempWEBService.Parameters.Add("@cufe", SqlDbType.VarChar).Value = nombreArchivo;
                    //                            cmdActualizaTempWEBService.Parameters.Add("@cadenaQR", SqlDbType.NVarChar).Value = rptaEntrega.Resultado.QR;
                    //                            cmdActualizaTempWEBService.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
                    //                            if (cmdActualizaTempWEBService.ExecuteNonQuery() > 0)
                    //                            {
                    //                                logFacturas.Info("Descarga Existosa de Archivos de la Nota Debito con Identificadotr:" + rptaEntrega.Identificador + "    Destino:" + carpetaDescarga);
                    //                                valorRpta = nroNotaDebito.ToString();
                    //                            }
                    //                            else
                    //                            {
                    //                                logFacturas.Info("No fue Posible realizar la Actualizacion de la Tabla facNotaTempWEBService de la Factura con Identificadotr:" + rptaEntrega.Identificador);
                    //                            }
                    //                        }
                    //                    }
                    //                    catch (NotSupportedException nSuppExp)
                    //                    {
                    //                        logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de los objetos de la Nota Debito:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
                    //                        valorRpta = "9X";
                    //                    }
                    //                    catch (ArgumentNullException argNull)
                    //                    {
                    //                        logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de los objetos de la Nota Debito:" + argNull.Message + "     " + argNull.InnerException.Message);
                    //                        valorRpta = "9X";
                    //                    }
                    //                    catch (WebException webEx1)
                    //                    {
                    //                        logFacturas.Info("Se ha presentado una Falla durante la descarga de los objetos de la factura:" + webEx1.Message + "     " + webEx1.InnerException.Message);
                    //                        valorRpta = "93";
                    //                    }
                    //                    catch (Exception exx)
                    //                    {
                    //                        logFacturas.Info("No fue posible descargar los archivos.PDF, ZIP,CUFE y QR  !!! Causa:" + exx.Message);
                    //                        valorRpta = "98";
                    //                    }

                    //                }

                    //            }
                    //            else
                    //            {
                    //                logFacturas.Info("!!!   No fue posible Actualizar la Nota Debito en la Tabla: facNotaTempWEBService   !!!");
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        logFacturas.Info("!!!  La Recuperacion de Documentos de la Nota Debito, No fue posible. No se Actualiza la Factura en facNotaTempWEBService   !!!");
                    //        logFacturas.Warn("la respuesta recibida:" + strsb);
                    //        //**** aqui se debe insertar en la tabla de fallos

                    //    }
                    //}
                    //else if (strsb.Contains(validacionError))
                    //{
                    //    respuestaEntregaError rptaEntregaErrores = JsonConvert.DeserializeObject<respuestaEntregaError>(strsb);
                    //    logFacturas.Info("!!!  La Entrega de la factura No fue Existosa. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
                    //    logFacturas.Info("Errores:  Resultado:" + rptaEntregaErrores.Resultado);
                    //    logFacturas.Info("Respuesta del Servicio" + strsb);
                    //    valorRpta = "99";
                    //}
                    //return valorRpta;

                    ////} 
                    #endregion
                    //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    //request.Timeout = 60 * 1000;

                    documentoF2.documento = NotaDebitoEnviar;
                    string NotaDebitoJson = JsonConvert.SerializeObject(documentoF2);
                    logFacturas.Info("Json de la Nota Debito::" + NotaDebitoJson);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    string Usuario = Properties.Settings.Default.usuario;
                    string Clave = Properties.Settings.Default.clave;
                    string credenciales = Convert.ToBase64String(Encoding.ASCII.GetBytes(Usuario + ":" + Clave));
                    request.Headers.Add("Authorization", "Basic " + credenciales);

                    Byte[] data = Encoding.UTF8.GetBytes(NotaDebitoJson);

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
                    StreamReader lectorDatos = new StreamReader(response.GetResponseStream());
                    string datosRespuesta = lectorDatos.ReadToEnd();
                    logFacturas.Info("Respuesta:" + datosRespuesta);
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
                            string strActualiza = @"UPDATE dbo.facNotaTempWEBService SET identificador=@identificador WHERE IdNota=@nroNota AND IdTipoNota=@idTipoNota";
                            SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
                            cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                            cmdActualiza.Parameters.Add("@nroNota", SqlDbType.Int).Value = nroNotaDebito;
                            cmdActualiza.Parameters.Add("@idTipoNota", SqlDbType.VarChar).Value = "ND";
                            if (cmdActualiza.ExecuteNonQuery() > 0)
                            {
                                logFacturas.Info("Nota Debito Actualizada con UUID en facNotaTempWEBService");
                                using (WebClient webClient = new WebClient())
                                {
                                    try
                                    {
                                        //string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                        string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfND + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".pdf";
                                        logFacturas.Info("Carpeta de Descarga:" + carpetaDescarga);
                                        try
                                        {
                                            webClient.DownloadFile(respuesta.resultado.URLPDF, carpetaDescarga);
                                            System.Threading.Thread.Sleep(500);
                                            logFacturas.Info($"Descarga de PDF Nota Debito...Terminada En:{carpetaDescarga}");
                                        }
                                        catch (NotSupportedException nSuppExp)
                                        {
                                            logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de Archivo PDF de la Nota Debito:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
                                            valorRpta = "9X";
                                        }
                                        catch (ArgumentNullException argNull)
                                        {
                                            logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de Archivo PDF de la Nota Debito:" + argNull.Message + "     " + argNull.InnerException.Message);
                                            valorRpta = "9X";
                                        }
                                        catch (WebException webEx1)
                                        {
                                            logFacturas.Info("Se ha presentado una Falla durante la descarga de Archivo PDF de la Nota Debito:" + webEx1.Message + "     " + webEx1.InnerException.Message);
                                            logFacturas.Warn($"Pila de Mensajes:::::{webEx1.StackTrace}");
                                            valorRpta = "93";
                                        }
                                        catch (Exception exx)
                                        {
                                            logFacturas.Info("Se ha presentado una Excepcion en la descarga de Archivo PDF de la Nota Debito  !!! Causa:" + exx.Message);
                                            valorRpta = "98";
                                        }

                                        try
                                        {
                                            carpetaDescarga = Properties.Settings.Default.urlDescargaPdfND + DateTime.Now.Year + @"\" + respuesta.resultado.UUID + ".XML";
                                            webClient.DownloadFile(respuesta.resultado.URLXML, carpetaDescarga);
                                            System.Threading.Thread.Sleep(500);
                                            logFacturas.Info($"Descarga de XML...Terminada En:{carpetaDescarga}");
                                        }
                                        catch (NotSupportedException nSuppExp)
                                        {
                                            logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de Archivo XML de la Nota Debito:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
                                            valorRpta = "9X";
                                        }
                                        catch (ArgumentNullException argNull)
                                        {
                                            logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de Archivo XML de la Nota Debito:" + argNull.Message + "     " + argNull.InnerException.Message);
                                            valorRpta = "9X";
                                        }
                                        catch (WebException webEx1)
                                        {
                                            logFacturas.Info("Se ha presentado una Falla durante la descarga de Archivo XML de la Nota Debito:" + webEx1.Message + "     " + webEx1.InnerException.Message);
                                            logFacturas.Warn($"Pila de Mensajes:::::{webEx1.StackTrace}");
                                            valorRpta = "93";
                                        }
                                        catch (Exception exx)
                                        {
                                            logFacturas.Info("Se ha presentado una Excepcion en la descarga de Archivo XML de la Nota Debito  !!! Causa:" + exx.Message);
                                            valorRpta = "98";
                                        }

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
                                                logFacturas.Info("Descarga Existosa de Archivos de la Nota Debito con Identificadotr:" + respuesta.resultado.UUID + " Destino:" + carpetaDescarga);
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
                                                        cmdInsertarAdvertencia.Parameters["@IdNota"].Value = nroNotaDebito;
                                                        cmdInsertarAdvertencia.Parameters["@CodAdvertencia"].Value = itemAdv.codigo;
                                                        //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                        cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                        cmdInsertarAdvertencia.Parameters["@DescripcionAdv"].Value = itemAdv.mensaje;
                                                        cmdInsertarAdvertencia.Parameters["@idTipoNota"].Value = "ND";
                                                        if (cmdInsertarAdvertencia.ExecuteNonQuery() > 0)
                                                        {
                                                            logFacturas.Info($"Se Inserta Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                            valorRpta = nroNotaDebito.ToString();
                                                        }
                                                        else
                                                        {
                                                            logFacturas.Info($"No es Posible Insertar Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje}");
                                                            valorRpta = nroNotaDebito.ToString();
                                                        }
                                                    }
                                                }
                                                valorRpta = nroNotaDebito.ToString();
                                            }
                                            else
                                            {
                                                logFacturas.Info("No fue Posible Actualizar la tabla facNotaTempWEBService de la Nota Debito con UUID:" + respuesta.resultado.UUID);
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
                            string qryInsertaError = @"INSERT INTO facNotaTempWEBServiceError (IdTipoNota,IdNota,CodError,DescripcionError,FecRegistro) 
VALUES(@idTipo,@IdNota, @CodError, @DescripcionError, @FecRegistro)";
                            SqlCommand cmdInsertarError = new SqlCommand(qryInsertaError, conn);
                            cmdInsertarError.Parameters.AddWithValue("@idTipo", SqlDbType.VarChar).Value = "ND";
                            cmdInsertarError.Parameters.Add("@IdNota", SqlDbType.Int).Value = nroNotaDebito;
                            cmdInsertarError.Parameters.Add("@CodError", SqlDbType.VarChar).Value = respuesta.codigo;
                            cmdInsertarError.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = respuesta.mensaje;
                            cmdInsertarError.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = DateTime.Parse(respuesta.fecha);
                            if (cmdInsertarError.ExecuteNonQuery() > 0)
                            {
                                valorRpta = nroNotaDebito.ToString();
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
                                    cmdDetErr.Parameters["@IdNota"].Value = nroNotaDebito;
                                    cmdDetErr.Parameters["@CodError"].Value = itemErr.codigo;
                                    cmdDetErr.Parameters["@consecutivo"].Value = consecutivo;
                                    cmdDetErr.Parameters["@FecRegistro"].Value = DateTime.Parse(respuesta.fecha);
                                    cmdDetErr.Parameters["@DescripcionError"].Value = itemErr.mensaje;
                                    if (cmdDetErr.ExecuteNonQuery() > 0)
                                    {
                                        logFacturas.Info($"Se Inserta Detalle de Errores Nota Debito:codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                                        consecutivo++;
                                    }
                                    else
                                    {
                                        logFacturas.Info($"No es Posible Insertar Detalle de Errores Nota Debito: Codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
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
                //}
            }
            catch (Exception ex1)
            {
                logFacturas.Warn("Se ha presentado una excepcion:" + ex1.Message + " Pila de LLamados:" + ex1.StackTrace);
                return "98";
            }


        }
    }

}
