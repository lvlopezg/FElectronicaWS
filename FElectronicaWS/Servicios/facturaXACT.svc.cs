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
            SqlConnectionStringBuilder stringConn = new SqlConnectionStringBuilder();
            //stringConn.InitialCatalog = "HSI_PRI";
            //stringConn.DataSource = "TYCHO";
            //stringConn.UserID = "interface";
            //stringConn.Password = "interface";
            //stringConn.MultipleActiveResultSets = true;

            logFacturas.Info($"****************   Se recibe Factura x Actividad:{ nroFactura }   IdCliente:{ idCliente }  nroAtencion:{ nroAtencion }  urlPdf:{ urlPdfFactura} *******************");
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
                string nombrePagos = string.Empty;
                List<cargoDescuento> cargosDescuentos = new List<cargoDescuento>();
                FormaPago formaPagoTmp = new FormaPago();

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
                string _correoCliente = string.Empty;

                Adquiriente adquirienteTmp = new Adquiriente
                {
                    identificacion = cliente.NroDoc_Cliente,
                    codigoInterno = cliente.IdTercero.ToString(),
                    razonSocial = cliente.NomTercero,
                    nombreSucursal = cliente.NomTercero,
                    correo = cliente.cuenta_correo.Trim().Split(';')[0],
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
                List<NotificacionesItem> notificaciones = new List<NotificacionesItem>();
                NotificacionesItem notificaItem = new NotificacionesItem();
                notificaItem.tipo = 1;
                List<string> valorNotificacion = new List<string>();
                valorNotificacion.Add(cliente.cuenta_correo.Trim());
                notificaItem.valor = valorNotificacion;
                notificaciones.Add(notificaItem);
                facturaEnviar.notificaciones = notificaciones;

                #region MyRegion
                //if (_RegimenFiscal.Equals("C"))
                //{
                //  adquirienteTmp.tipoRegimen = "48";
                //}
                //else
                //{
                //  adquirienteTmp.tipoRegimen = "49";
                //}
                //TODO: Aqui insertar lo que se defina de Responsabilidades  RUT documentoF2.adquiriente.responsabilidadesRUT 
                #endregion

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

                double TotalGravadoIva = 0;

                #region Extension Salud
                //********************* Extension Sector Salud ****************************************************************
                /*
                  1. Código del prestador de servicios de salud 
                  2. Tipo de documento de identificación del usuario 
                  3. Número de documento de identificación del usuario 
                  4. Primer apellido del usuario 
                  5. Segundo apellido del usuario 
                  6. Primer nombre del usuario 
                  7. Segundo nombre del usuario 
                  8. Tipo de usuario 
                  9. Modalidades de contratación y de pago 
                  10. Cobertura o plan de beneficios 
                  11. Número de autorización 
                  12. Número de mi prescripción (MIPRES) 
                  13. Número de ID entrega de mi prescripción (MIPRES) 
                  14. Número de contrato 
                  15. Número de póliza 
                  16. Copago 
                  17. Cuota moderadora 
                  18. Cuota de recuperación 
                  19. Pagos compartidos en planes voluntarios de salud 
                  20. Fecha de inicio del periodo de facturación 
                  21. Fecha final del periodo de facturación 
                */
                extensionSalud itemExtensionSalud = new extensionSalud();  //TODO: Implementacion de Campos de Salud
                List<extensionSalud> extencionesSalud = new List<extensionSalud>();
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    //****Prestador Campo 1
                    conn.Open();
                    string qryPrestador = "SELECT ValConstanteT FROM genConstante WHERE CodConstante =@CodConstante";
                    SqlCommand cmdPrestador = new SqlCommand(qryPrestador, conn);
                    cmdPrestador.Parameters.Add("@CodConstante", SqlDbType.VarChar).Value = "CPFE";
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
                      "INNER JOIN admAtencion b on b.IdAtencion = a.IdDestino " +
                      "INNER JOIN admCliente c on c.IdCliente = b.IdCliente " +
                      "INNER JOIN genTipoDoc d on d.IdTipoDoc = c.IdTipoDoc " +
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
                        itemExtensionSalud.primerApellido = apellidos[0].Length > 0 ? apellidos[0] : "    ";
                        itemExtensionSalud.segundoApellido = apellidos[1].Length > 0 ? apellidos[1] : "    ";
                        itemExtensionSalud.primerNombre = nombres[0].Length > 0 ? nombres[0] : "    ";
                        itemExtensionSalud.otrosNombres = nombres[1].Length > 0 ? nombres[1] : "    ";
                    }
                    //********* Tipo de Usuario y Poliza: Campo 8 y 15 ,*  ----esta sin definir en la Tabla  admAtencionContrato ???
                    string qryTipoUsua = "SELECT d.desItem,FFXML.NumPoliza " +
                      "FROM facFactura a " +
                      "INNER JOIN facFacturaCamposSaludXML FFXML ON a.IdFactura = FFXML.IdFactura " +
                      "INNER JOIN genLista d on d.IdLista = FFXML.idTipoUsuario " +
                      "WHERE a.IdFactura = @idFactura ";
                    SqlCommand cmdTipoUsua = new SqlCommand(qryTipoUsua, conn);
                    cmdTipoUsua.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdTipoUsua = cmdTipoUsua.ExecuteReader();
                    if (rdTipoUsua.HasRows)
                    {
                        rdTipoUsua.Read();
                        if (!rdTipoUsua.IsDBNull(0))
                        {
                            itemExtensionSalud.tipoDeUsuario = rdTipoUsua.GetString(0).ToString();
                        }
                        else
                        {
                            itemExtensionSalud.tipoDeUsuario = "  ";
                        }
                        if (!rdTipoUsua.IsDBNull(1))
                        {
                            itemExtensionSalud.numeroPoliza = rdTipoUsua.GetString(1).ToString();
                        }
                        else
                        {
                            itemExtensionSalud.numeroPoliza = "    ";
                        }
                    }
                    else
                    {
                        itemExtensionSalud.tipoDeUsuario ="   ";
                        itemExtensionSalud.numeroPoliza = "    ";
                    }
                    //********* Campo 9  Modalidad de Contratacion y de Pago Se incluye en el siguiente Bloque


                    //*************** Campos:9-10 - Cobertura 
                    SqlCommand sqlCamposVarios = new SqlCommand("spFaceDatosSaludXML", conn);
                    sqlCamposVarios.CommandType = CommandType.StoredProcedure;
                    sqlCamposVarios.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdCamposVarios = sqlCamposVarios.ExecuteReader();
                    if (rdCamposVarios.HasRows)
                    {
                        rdCamposVarios.Read();
                        itemExtensionSalud.cobertura = rdCamposVarios.GetString(0);
                        Double copagoWRK = rdCamposVarios.GetDouble(1);
                        Double cuotaModeradoraWRK = rdCamposVarios.GetDouble(2);
                        Double cuotaRecuperacionWRK = rdCamposVarios.GetDouble(3);
                        Double pagosCompartidosWRK = rdCamposVarios.GetDouble(4);
                        itemExtensionSalud.copago = copagoWRK;
                        itemExtensionSalud.cuotaModeradora = cuotaModeradoraWRK;
                        itemExtensionSalud.cuotaRecuperacion = cuotaRecuperacionWRK;
                        itemExtensionSalud.pagosCompartidos = pagosCompartidosWRK;
                        itemExtensionSalud.modalidadesContratacion = rdCamposVarios.GetString(5).ToString();
                    }
                    SqlCommand sqlAutorizaciones = new SqlCommand("spFaceBuscaAutorizaciones", conn);
                    sqlAutorizaciones.CommandType = CommandType.StoredProcedure;
                    sqlAutorizaciones.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdAutorizaciones = sqlAutorizaciones.ExecuteReader();
                    List<string> Autorizaciones = new List<string>();
                    if (rdAutorizaciones.HasRows)
                    {
                        while (rdAutorizaciones.Read())
                        {
                            Autorizaciones.Add(rdAutorizaciones.GetString(0));
                        }
                    }
                    if (Autorizaciones.Count > 0)
                    {
                        itemExtensionSalud.numeroAutorizacion = Autorizaciones;
                    }

                    // Campo 12 Y 13 - NromiPres Y idSuministro
                    string qryNroMiPres = "SELECT RCTA.miPres,RCTA.idSuministro FROM facElNoPosResumenCta RCTA " +
                      "INNER JOIN facPreFactura FFAC ON FFAC.IdFactura = RCTA.idResumen " +
                      "WHERE IdFacturaAsociada = @idFactura AND (LEN(RCTA.miPres)>0 AND LEN(RCTA.idSuministro)>0)";
                    SqlCommand cmdNroMiPres = new SqlCommand(qryNroMiPres, conn);
                    cmdNroMiPres.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdNroMiPres = cmdNroMiPres.ExecuteReader();
                    if (rdNroMiPres.HasRows)
                    {
                        List<string> NumerosMiPres = new List<string>();
                        List<string> NumerosSuministro = new List<string>();
                        while (rdNroMiPres.Read())
                        {
                            NumerosMiPres.Add(rdNroMiPres.GetString(0));
                            NumerosSuministro.Add(rdNroMiPres.GetString(0));
                        }
                        itemExtensionSalud.numeroMIPRES = NumerosMiPres;
                        itemExtensionSalud.numeroIdPrescripcion = NumerosSuministro;
                    }


                    //****************** Campo Numero 14 Numero de Contrato
                    string qryContrato = "SELECT NumeroContrato FROM conContrato CCO " +
                      "INNER JOIN facFactura FFT ON CCO.IdContrato=FFT.IdContrato " +
                      "WHERE FFT.IdFactura=@idFactura";
                    SqlCommand cmdContrato = new SqlCommand(qryContrato, conn);
                    cmdContrato.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdContrato = cmdContrato.ExecuteReader();
                    if (rdContrato.HasRows)
                    {
                        rdContrato.Read();
                        if (rdContrato.IsDBNull(0))
                        {
                            itemExtensionSalud.numeroContrato = "    ";
                        }
                        else
                        {
                            itemExtensionSalud.numeroContrato = rdContrato.GetString(0);
                        }
                    }

                    //***************** Campo 16  ----Copago

                    //***************** Campo 17  ---- Moderadora

                    //**************** Campo 18  ------Recuperacion

                    //**************** Campo 19 ------- Pagos Compartidos

                    //*************** Campos 20 y 21 FEcha de Inicio y fin de Atencion

                    string qryAtencion = "SELECT FecIngreso, FecEgreso FROM facFacturaCamposSaludXML " +
                      "WHERE IdFactura = @idFactura";
                    SqlCommand cmdAtencion = new SqlCommand(qryAtencion, conn);
                    cmdAtencion.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdAtencion = cmdAtencion.ExecuteReader();
                    if (rdAtencion.HasRows)
                    {
                        rdAtencion.Read();
                        itemExtensionSalud.fechaInicioFacturacion = rdAtencion.GetDateTime(0).ToString("yyyy-MM-dd");
                        itemExtensionSalud.fechaFinFacturacion = rdAtencion.GetDateTime(1).ToString("yyyy-MM-dd");
                    }
                    else//todo:Verificar Cobertura, cuando facFacturaCamposSaludXML No hay Datos
                    {
                        string qryAtencion2 = "SELECT FecIngreso,FecEgreso FROM admAtencion WHERE idAtencion=@idAtencion";
                        SqlCommand cmdAtencion2 = new SqlCommand(qryAtencion2, conn);
                        cmdAtencion2.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
                        SqlDataReader rdAtencion2 = cmdAtencion2.ExecuteReader();
                        if (rdAtencion2.HasRows)
                        {
                            rdAtencion2.Read();
                            itemExtensionSalud.fechaInicioFacturacion = rdAtencion2.GetDateTime(0).ToString("yyyy-MM-dd");
                            itemExtensionSalud.fechaFinFacturacion = rdAtencion2.GetDateTime(1).ToString("yyyy-MM-dd");
                        }
                    }
                    extencionesSalud.Add(itemExtensionSalud);
                    facturaEnviar.extensionesSalud = extencionesSalud;
                }
                #endregion
                //************************************************************ Detalle de Factura por Actividad ***********************************************************
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    string qryFactura = "SELECT IdFactura,NumFactura,IdDestino,IdTransaccion,IdPlan,IdContrato,ValTotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,IndNotaCred,IndTipoFactura,CodEnti,CodEsor,FecFactura,Ruta,IdCausal,IdUsuarioR,IndHabilitado,IdNoFacturado,valPos,valNoPos " +
                      "FROM  facFactura WHERE idFactura=@idFactura AND idDestino=@idAtencion";
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
'Unidad' as 'Unidad',O.idOrden
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
INNER JOIN facFacturaDetOrden O on f.idFactura=O.idFactura AND f.idProducto=O.idProducto AND f.idMovimiento=O.idMovimiento
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino 
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='ACT' AND  a.idfactura= @idFactura ORDER BY A.IdFactura,o.Idorden";
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
                                    if (rdDetalleFac.GetInt16(18) == 5 || rdDetalleFac.GetInt16(18) == 6)
                                    {
                                        lineaProducto.codigoEstandar = "999";
                                    }
                                    else
                                    {
                                        lineaProducto.codigoEstandar = "999";
                                    }
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdDetalleFac.GetString(2);
                                    lineaProducto.unidades = double.Parse(rdDetalleFac.GetDouble(3).ToString());
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";
                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "IVA";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    tributosWRKIva.valorImporte = rdDetalleFac.GetDouble(5) * 0;
                                    TotalGravadoIva = TotalGravadoIva + rdDetalleFac.GetDouble(5);
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);

                                    lineaProducto.tributos = listaTributos;

                                    //////List<cargoDescuento> cargosDtosProd = new List<cargoDescuento>();
                                    //////cargoDescuento cdtoProducto = new cargoDescuento();
                                    //////if (((double.Parse(rdDetalleFac.GetDouble(5).ToString()))>double.Parse(_ValPagos.ToString())) && !procesadoPago )
                                    //////{
                                    //////    //lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDouble(4).ToString()) - double.Parse(_ValPagos.ToString());
                                    //////    //lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDouble(5).ToString()) - double.Parse(_ValPagos.ToString());
                                    //////    //tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString())- double.Parse(_ValPagos.ToString());
                                    //////    procesadoPago = true;
                                    //////    List<string> notasWRK = new List<string>();
                                    //////    cdtoProducto.esCargo = false;
                                    //////    cdtoProducto.codigo = "00";
                                    //////    notasWRK.Add(nombrePagos);
                                    //////    cdtoProducto.notas = notasWRK;
                                    //////    cdtoProducto.valorImporte =  double.Parse(_ValPagos.ToString());
                                    //////    cdtoProducto.valorBaseMoneda = "COP";
                                    //////    cdtoProducto.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString());
                                    //////    cdtoProducto.valorBaseMoneda = "COP";
                                    //////    cargosDtosProd.Add(cdtoProducto);
                                    //////}
                                    //////else
                                    //////{
                                    //////    List<string> notasWRK = new List<string>();
                                    //////    cdtoProducto.esCargo = false;
                                    //////    cdtoProducto.codigo = "00";
                                    //////    notasWRK.Add(nombrePagos);
                                    //////    cdtoProducto.notas = notasWRK;
                                    //////    cdtoProducto.valorImporte =0;
                                    //////    cdtoProducto.valorBaseMoneda = "COP";
                                    //////    cdtoProducto.valorBase = double.Parse(rdDetalleFac.GetDouble(5).ToString()); //double.Parse( _Valtotal.ToString());
                                    //////    cdtoProducto.valorBaseMoneda = "COP";
                                    //////    cargosDtosProd.Add(cdtoProducto);
                                    //////}
                                    //////lineaProducto.cargosDescuentos= cargosDtosProd;
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
                        logFacturas.Info($"No se encontro Informacion de Factura y Atencion: {qryFactura}");
                    }
                }
                documentoF2.detalles = detalleProductos;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryFacturaEnc = @"SELECT fact.idContrato,Valtotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,FecFactura,valPos,valNoPos,fact.IdUsuarioR,
usr.nom_usua,usr.NumDocumento,usr.IdTipoDoc,ter.NumDocumento,ter.IdTipoDoc,ter.NomTercero,'' as CodTercero,con.NomRepComercial,ter.IdTercero,ter.idRegimen,ter.IdNaturaleza  
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

                        if (rdFacturaEnc.IsDBNull(8))
                        { _valPos = 0; }
                        else
                        { _valPos = Math.Round(rdFacturaEnc.GetDecimal(8), 0); }

                        if (rdFacturaEnc.IsDBNull(9))
                        { _valNoPos = 0; }
                        else
                        { _valNoPos = Math.Round(rdFacturaEnc.GetDecimal(9), 0); }

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
                List<TributosItem> tributosTMP = new List<TributosItem>();
                List<DetalleTributos> tributosDetalle = new List<DetalleTributos>();
                DetalleTributos detalleTributos = new DetalleTributos() // Un Objeto por cada Tipo de Iva
                {
                    valorImporte = 0,
                    valorBase = TotalGravadoIva - double.Parse(_ValPagos.ToString()),
                    porcentaje = 0
                };
                tributosDetalle.Add(detalleTributos);
                TributosItem itemTributo = new TributosItem()
                {
                    id = "01", //Total de Iva 
                    nombre = "IVA",
                    esImpuesto = true,
                    valorImporteTotal = 0,
                    detalles = tributosDetalle // Detalle de los Ivas
                };
                tributosTMP.Add(itemTributo);
                documentoF2.tributos = tributosTMP;
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary>
                /// 
                var Valor_Neto = _Valtotal - _ValPagos;
                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(_Valtotal.ToString()),
                    valorDescuentos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = TotalGravadoIva,
                    valorTotalConImpuestos = double.Parse(_Valtotal.ToString()) + double.Parse(_ValImpuesto.ToString()),
                    valorNeto = double.Parse(Valor_Neto.ToString()) // Valor Factura sin descontar los anticipos Res:1.8
                };
                documentoF2.totales = totalesTmp;
                logFacturas.Info("Numero de Productos Procesados, para JSon:" + detalleProductos.Count);
                try
                {
                    //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica;// + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    request.Timeout = 20 * 1000;
                    documentoF2.documento = facturaEnviar;
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
                    logFacturas.Info($"Codigo Status:{response.StatusCode}  Descripcion Status:{response.StatusDescription}");
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
                            string strActualiza = @"UPDATE dbo.facFacturaTempWEBService SET identificador=@identificador,CadenaJSON=@facturaJson,CadenaJSONRespuesta=@JSONRespuesta WHERE IdFactura=@nrofactura";
                            SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
                            cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = respuesta.resultado.UUID;
                            cmdActualiza.Parameters.Add("@nrofactura", SqlDbType.Int).Value = nroFactura;
                            cmdActualiza.Parameters.Add("@facturaJson", SqlDbType.VarChar).Value = facturaJson;

                            cmdActualiza.Parameters.Add("@JSONRespuesta", SqlDbType.VarChar).Value = strsb;
                            if (cmdActualiza.ExecuteNonQuery() > 0)
                            {
                                logFacturas.Info("Factura Actualizada con UUID en facFacturaTempWEBService");
                                using (WebClient webClient = new WebClient())
                                {
                                    try
                                    {
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
                            //cmdInsertarError.Parameters.Add("@facturaJson", SqlDbType.VarChar).Value = facturaJson;
                            //cmdInsertarError.Parameters.Add("@JSONRespuesta", SqlDbType.VarChar).Value = strsb;
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
                    logFacturas.Info($"******************************** Se Termina de Procesar la Factura {nroFactura}  de la Atencion:{nroAtencion}.........................");
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
