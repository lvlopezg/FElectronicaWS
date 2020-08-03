using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FElectronicaWS.Clases;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace FElectronicaWS.Servicios
{
    public class facturaXREL : IfacturaXREL
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura)
        {
            logFacturas.Info("Se recibe factura con siguietnes datos:Factura por Relacion:" + nroFactura + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfFactura);
            try
            {
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
                        if (rdFacturaEnc.IsDBNull(8)) { _valPos = 0; } else { _valPos = Math.Round(rdFacturaEnc.GetDecimal(8), 0); }
                        if (rdFacturaEnc.IsDBNull(9)) { _valNoPos = 0; } else { _valNoPos = Math.Round(rdFacturaEnc.GetDecimal(9), 0); }
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
                #region MyRegion
                //**** 
                //eFactura facturaEnviar = new eFactura();
                //AdditionalInformation iteminfAdicionalEnc = new AdditionalInformation();
                //List<AdditionalInformation> InformacionAdicionalEn = new List<AdditionalInformation>();
                //if (concepto == 1 && concepto > 0)
                //{
                //    InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "2", Value = _ValPagos.ToString() });
                //    InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "3", Value = "0" });
                //}
                //else
                //{
                //    InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "2", Value = "0" });
                //    InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "3", Value = _ValPagos.ToString() });
                //}

                //Data objData = new Data
                //{
                //    UrlPdf = urlPdfFactura
                //};
                //OriginalRequest peticion = new OriginalRequest
                //{
                //    //Currency = _monedaFactura,
                //    EventName = "FAC-SYNC",
                //    DocumentType = "Invoice",
                //    BroadCastDate = DateTime.Now.ToString("yyyy-MM-dd"),
                //    BroadCastTime = DateTime.Now.ToString("HH:mm:ss"),
                //    IdMotivo = "1",
                //    BillType = "1",
                //    InvoiceId = nroFactura.ToString(),
                //    Prefix = Properties.Settings.Default.Prefijo,
                //    IdBusiness = "860015536",
                //    AdditionalInformation = InformacionAdicionalEn
                //};

                //peticion.DocumentType = "01";
                //TODO:Agregar todos los campos adicionales

                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "3", Value = "Codigo Habilitacion HUSI" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "4", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "5", Value = 0.ToString() });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "6", Value = nroAtencion.ToString() });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "7", Value = "2018-09-16" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "8", Value = "2018-09-23" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "9", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "10", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "11", Value = "false" });


                ////  SellerSupplierParty datosHUSI = new SellerSupplierParty();
                //AccountingCustomerParty Cliente = new AccountingCustomerParty();
                //Address objDireccionHusi = new Address();
                //Address1 objDireccionCliente = new Address1();
                //TaxTotal ivaFactura = new TaxTotal();
                //TaxTotal ipoconsumoFactura = new TaxTotal();
                //TaxTotal icaFactura = new TaxTotal();
                //List<TaxTotal> impuestosFactura = new List<TaxTotal>();
                //LegalMonetaryTotal subtotalesFactura = new LegalMonetaryTotal();

                //LineExtensionAmount lineaExtCant = new LineExtensionAmount();
                //TaxExclusiveAmount totalImpuesto = new TaxExclusiveAmount();
                //PayableAmount totalPagar = new PayableAmount();

                //Contract contrato = new Contract();
                //Party datosHospital = new Party();
                //Party1 datosCliente = new Party1();
                //PhysicalLocation ubicacionFisicaHusi = new PhysicalLocation();
                //PhysicalLocation1 ubicacionFisicaCliente = new PhysicalLocation1();
                //PartyTaxScheme RegimenImpuesto = new PartyTaxScheme();
                //PartyTaxScheme RegimenCliente = new PartyTaxScheme();
                //PartyIdentification idenHusi = new PartyIdentification();
                //PartyIdentification idenCliente = new PartyIdentification();
                //Person repLegalHusi = new Person();
                //Person1 repLegalCliente = new Person1();
                ////********** Definicion Elementos del Detalle de Factura
                //List<DocumentLine> detalleProductos = new List<DocumentLine>();
                //SubDetalle subDetProducto = new SubDetalle();
                //TaxAmount taxIVA = new TaxAmount();
                //TaxAmount taxCONSUMO = new TaxAmount();
                //TaxAmount taxICA = new TaxAmount();
                //TaxableAmount camposAdicionalesICA = new TaxableAmount();
                ////********** Fin Definicion de Detalle de Factura
                //contrato.ID = _idContrato.ToString();
                //contrato.IssueDate = _FecFactura.ToString("yyyy-MM-dd");
                //contrato.ContractType = "1";

                //// datosHUSI.Contract = contrato;
                ////**********
                ////   datosHUSI.AdditionalAccountID = "1";
                //datosHospital.Name = "Hospital Universitario San Ignacio";
                //idenHusi.ID = "860015536";
                //idenHusi.SchemeID = "31";
                //datosHospital.PartyIdentification = idenHusi;


                //objDireccionHusi.Line = "Kra 7 No. 40-62";
                //objDireccionHusi.CityName = "Bogota D.C";
                //objDireccionHusi.CountryCode = "57";
                //objDireccionHusi.Department = "Cundinamarca";

                //ubicacionFisicaHusi.Address = objDireccionHusi;
                //datosHospital.PhysicalLocation = ubicacionFisicaHusi;

                //repLegalHusi.FirstName = "JULIO";
                //repLegalHusi.MiddleName = "CESAR";
                //repLegalHusi.FamilyName = "CASTELLANOS RAMIREZ";

                //datosHospital.Person = repLegalHusi;

                //RegimenImpuesto.TaxLevelCode = "2";
                //datosHospital.PartyTaxScheme = RegimenImpuesto;

                ////  datosHUSI.Party = datosHospital;

                ////****************** CLIENTE
                ////  Variables Inicializacion
                //string _direccionCliente = string.Empty;
                //string _telefonoCliente = string.Empty;
                //string _municipioCliente = string.Empty;
                //string _departamento = string.Empty;
                //int _localizacionCliente = 0;
                //string _correoCliente = string.Empty;
                ////****  
                #endregion
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
                        _correoCliente = "facturaelectronica@husi.org.co";
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
                #endregion
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary>
                double TotalGravadoIva = 0;
                double TotalGravadoIca = 0;
                #region MyRegion
                //Totales totalesTmp = new Totales()
                //{
                //    valorBruto = double.Parse(_Valtotal.ToString()),
                //    valorAnticipos = double.Parse(_ValPagos.ToString()),
                //    valorTotalSinImpuestos = double.Parse(_Valtotal.ToString()),
                //    valorTotalConImpuestos = double.Parse(_Valtotal.ToString()) + double.Parse(_ValImpuesto.ToString()),
                //    valorNeto = double.Parse(_ValCobrar.ToString())
                //};
                //documentoF2.totales = totalesTmp; 
                #endregion

                List<AnticiposItem> anticiposWrk = new List<AnticiposItem>();
                AnticiposItem anticipoWrk = new AnticiposItem();
                anticipoWrk.comprobante = "22";
                anticipoWrk.valorAnticipo = double.Parse(_ValPagos.ToString());
                anticiposWrk.Add(anticipoWrk);
                documentoF2.anticipos = anticiposWrk;

                //************************************************************ Detalle de Factura
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
                        string strDetalleFac = @"SELECT UNI.Cod_Servicio,UNI.Des_Servicio,UNI.Cantidad,UNI.Vlr_Unitario_Serv,UNI.Vlr_Total_Serv,UNI.idProducto,UNI.CodProducto,UNI.nomproducto,O.idOrden FROM (
SELECT upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio,SUM(f.Cantidad) as Cantidad, f.ValTotal as Vlr_Unitario_Serv,
SUM(isnull(AD.ValTotal,round(F.Cantidad*F.ValTotal,0))) as Vlr_Total_Serv, p.idProducto,p.CodProducto,p.nomproducto
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
LEFT JOIN facFacturaDetAjuDec AD ON AD.IdFactura = F.IdFactura and AD.IdProducto = F.IdProducto and AD.IdMovimiento = F.IdMovimiento
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento
INNER JOIN  admatencion c on g.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
LEFT JOIN admatencioncontrato h on h.idatencion=g.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='RAC' AND  a.idfactura=@idFactura
GROUP BY
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )),upper(( isnull(J.NomPRoman,P.NomProducto))),f.ValTotal,
p.idProducto,p.CodProducto,p.nomproducto
) as UNI
INNER JOIN facFacturaDetOrden O on O.idFactura=@idFactura AND UNI.idProducto=O.idProducto
ORDER BY o.Idorden";
                        SqlCommand cmdDetalleFac = new SqlCommand(strDetalleFac, conexion01);
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
                                    string codigoProducto = rdDetalleFac.GetString(0);
                                    lineaProducto.valorCodigoInterno = codigoProducto;
                                    #region MyRegion
                                    //if (rdDetalleFac.GetInt16(18) == 5 || rdDetalleFac.GetInt16(18) == 6)
                                    //{
                                    //    lineaProducto.codigoEstandar = "999";
                                    //}
                                    //else
                                    //{
                                    //    lineaProducto.codigoEstandar = "999";
                                    //} 
                                    #endregion
                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdDetalleFac.GetString(1);
                                    lineaProducto.unidades = double.Parse(rdDetalleFac.GetDouble(2).ToString());
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDouble(3).ToString());
                                    lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    lineaProducto.valorBrutoMoneda = "COP";

                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "Iva";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    tributosWRKIva.valorImporte = rdDetalleFac.GetDouble(4) * 0;
                                    TotalGravadoIva = TotalGravadoIva + rdDetalleFac.GetDouble(4);
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);

                                    #region MyRegion
                                    //TibutosDetalle tributosWRKIca = new TibutosDetalle();
                                    //tributosWRKIca.id = "02";
                                    //tributosWRKIca.nombre = "ICA";
                                    //tributosWRKIca.esImpuesto = true;
                                    //tributosWRKIca.porcentaje = 0;
                                    //tributosWRKIca.valorBase = double.Parse(rdDetalleFac.GetDouble(4).ToString());
                                    //tributosWRKIca.valorImporte = rdDetalleFac.GetDouble(4) * 0;
                                    //TotalGravadoIca = TotalGravadoIca+ rdDetalleFac.GetDouble(4);
                                    //tributosWRKIca.tributoFijoUnidades = 0;
                                    //tributosWRKIca.tributoFijoValorImporte = 0;

                                    //listaTributos.Add(tributosWRKIca); 
                                    #endregion
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
                        }
                        else // Si No  hay Detalle de Productos
                        {
                            logFacturas.Info("No se encontro Detalle de productos para la factura. " + strDetalleFac);
                        }
                    }
                    else // No se encuentra Informacion de la Factura y Atencion
                    {
                        logFacturas.Info("No se encontro Informacion nde Factura y Atencion." + qryFactura);
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
                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(_Valtotal.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = TotalGravadoIva + TotalGravadoIca,
                    valorTotalConImpuestos = double.Parse(_Valtotal.ToString()) + double.Parse(_ValImpuesto.ToString()),
                    valorNeto = double.Parse(_ValCobrar.ToString())
                };
                documentoF2.totales = totalesTmp;
                logFacturas.Info("Numero de Productos procesados, para JSon:" + detalleProductos.Count);
                ////************************************** Adicionar elementos a la Factura **********************************

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
                    //string Usuario = "administrador";
                    //string Clave = "Transfiriendo@2016";
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
                                                            logFacturas.Info($"No es Posible Insertar Detalle de Advertencias: Codigo Advertencia{itemAdv.codigo} Mensaje Advertencia:{itemAdv.mensaje} Estado del Proceso de la Factura:{respuesta.esExitoso}");
                                                            valorRpta = nroFactura.ToString();
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
                catch (Exception e)
                {
                    logFacturas.Warn("Se ha presentado una excepcion:" + e.Message + " Pila de LLamdos:" + e.StackTrace);
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
