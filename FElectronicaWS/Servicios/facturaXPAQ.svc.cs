using FElectronicaWS.Clases;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "facturaXPAQ" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione facturaXPAQ.svc o facturaXPAQ.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class facturaXPAQ : IfacturaXPAQ
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        public string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura)
        {
            logFacturas.Info("Se recibe factura con siguietnes datos:nroFactura:" + nroFactura + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfFactura);
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
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                Byte _usrIdTipoDoc = 0;
                string _numDocCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
                Int32 _idTercero = 0;

                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryFacturaEnc = @"SELECT fact.idContrato,Valtotal,ValDescuento,ValDescuentoT,ValPagos,ValImpuesto,ValCobrar,FecFactura,valPos,valNoPos,fact.IdUsuarioR,
usr.nom_usua,usr.NumDocumento,usr.IdTipoDoc,ter.NumDocumento,ter.IdTipoDoc,ter.NomTercero,ter.CodTercero,con.NomRepComercial,ter.IdTercero
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
                    }
                }

                eFactura facturaEnviar = new eFactura();
                AdditionalInformation iteminfAdicionalEnc = new AdditionalInformation();
                List<AdditionalInformation> InformacionAdicionalEn = new List<AdditionalInformation>();
                Data objData = new Data();
                objData.UrlPdf = urlPdfFactura;
                OriginalRequest peticion = new OriginalRequest();
                peticion.Currency = "COP";
                peticion.EventName = "FAC";
                //peticion.DocumentType = "01";
                peticion.Currency = "COP";
                peticion.DocumentType = "Invoice";
                peticion.BroadCastDate = DateTime.Now.ToString("yyyy-MM-dd");
                peticion.BroadCastTime = DateTime.Now.ToString("hh:mm:ss");
                peticion.IdMotivo = "1";
                peticion.InvoiceId = nroFactura.ToString();
                peticion.Prefix = "PRUE";
                peticion.IdBusiness = "860015536";
                //TODO:Agregar todos los campos adicionales
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "1", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "2", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "3", Value = "Codigo Habilitacion HUSI" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "4", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "5", Value = 0.ToString() });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "6", Value = nroAtencion.ToString() });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "7", Value = "2018-09-16" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "8", Value = "2018-09-23" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "9", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "10", Value = "0" });
                //InformacionAdicionalEn.Add(new AdditionalInformation() { Position = "11", Value = "false" });

                peticion.AdditionalInformation = InformacionAdicionalEn;

                AccountingCustomerParty Cliente = new AccountingCustomerParty();
                Address objDireccionHusi = new Address();
                Address1 objDireccionCliente = new Address1();
                TaxTotal ivaFactura = new TaxTotal();
                TaxTotal ipoconsumoFactura = new TaxTotal();
                TaxTotal icaFactura = new TaxTotal();
                List<TaxTotal> impuestosFactura = new List<TaxTotal>();
                LegalMonetaryTotal subtotalesFactura = new LegalMonetaryTotal();

                LineExtensionAmount lineaExtCant = new LineExtensionAmount();
                TaxExclusiveAmount totalImpuesto = new TaxExclusiveAmount();
                PayableAmount totalPagar = new PayableAmount();

                Contract contrato = new Contract();
                Party datosHospital = new Party();
                Party1 datosCliente = new Party1();
                PhysicalLocation ubicacionFisicaHusi = new PhysicalLocation();
                PhysicalLocation1 ubicacionFisicaCliente = new PhysicalLocation1();
                PartyTaxScheme RegimenImpuesto = new PartyTaxScheme();
                PartyTaxScheme RegimenCliente = new PartyTaxScheme();
                PartyIdentification idenHusi = new PartyIdentification();
                PartyIdentification idenCliente = new PartyIdentification();
                Person repLegalHusi = new Person();
                Person1 repLegalCliente = new Person1();
                //********** Definicion Elementos del Detalle de Factura
                List<DocumentLine> detalleProductos = new List<DocumentLine>();

                SubDetalle subDetProducto = new SubDetalle();

                TaxAmount taxIVA = new TaxAmount();
                TaxAmount taxCONSUMO = new TaxAmount();
                TaxAmount taxICA = new TaxAmount();

                //********** Fin Definicion de Detalle de Factura
                contrato.ID = _idContrato.ToString();
                contrato.IssueDate = _FecFactura.ToString("yyyy-MM-dd");
                contrato.ContractType = "1";

                // datosHUSI.Contract = contrato;
                //**********
                //   datosHUSI.AdditionalAccountID = "1";
                datosHospital.Name = "Hospital Universitario San Ignacio";
                idenHusi.ID = "860015536";
                idenHusi.SchemeID = "31";
                datosHospital.PartyIdentification = idenHusi;


                objDireccionHusi.Line = "Kra 7 No. 40-62";
                objDireccionHusi.CityName = "Bogota D.C";
                objDireccionHusi.CountryCode = "57";
                objDireccionHusi.Department = "Cundinamarca";

                ubicacionFisicaHusi.Address = objDireccionHusi;
                datosHospital.PhysicalLocation = ubicacionFisicaHusi;

                repLegalHusi.FirstName = "JULIO";
                repLegalHusi.MiddleName = "CESAR";
                repLegalHusi.FamilyName = "CASTELLANOS RAMIREZ";

                datosHospital.Person = repLegalHusi;

                RegimenImpuesto.TaxLevelCode = "2";
                datosHospital.PartyTaxScheme = RegimenImpuesto;

                //  datosHUSI.Party = datosHospital;

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
                    string qryDatosCliente1 = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar FROM genTerceroLocaliza A
INNER JOIN GEN_DIVI_POLI B ON A.IdLugar=B.IdLugar
WHERE IdTerceroLocaliza=@idTercero and IdLocalizaTipo IN (2,3)
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
                            }
                            else if (rdDatosCliente1.GetInt32(0) == 3)
                            {
                                _telefonoCliente = rdDatosCliente1.GetString(1);
                            }
                            _municipioCliente = rdDatosCliente1.GetString(2);
                            _localizacionCliente = rdDatosCliente1.GetInt32(3);
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

                }

                idenCliente.SchemeID = "31";
                idenCliente.ID = _numDocCliente;

                Cliente.AdditionalAccountID = "1";

                datosCliente.PartyIdentification = idenCliente;
                datosCliente.Name = _razonSocial;
                objDireccionCliente.Line = _direccionCliente;
                objDireccionCliente.CityName = _municipioCliente;
                objDireccionCliente.CountryCode = "CO";
                objDireccionCliente.CitySubdivisionName = "";
                objDireccionCliente.Department = _departamento;
                ubicacionFisicaCliente.Address = objDireccionCliente;
                datosCliente.PhysicalLocation = ubicacionFisicaCliente;

                RegimenCliente.TaxLevelCode = "2";
                datosCliente.PartyTaxScheme = RegimenCliente;
                string primerNombre = string.Empty;
                string segundoNombre = string.Empty;
                string Apellidos = string.Empty;
                if (_repLegal.Length > 1)
                {
                    string[] nombreRepresentante = _repLegal.Split(' ');
                    primerNombre = nombreRepresentante[0];
                    segundoNombre = nombreRepresentante[1];
                    Apellidos = nombreRepresentante[2];
                    if (nombreRepresentante.Length > 3)
                    {
                        Apellidos = Apellidos + nombreRepresentante[3];
                    }
                }
                repLegalCliente.FirstName = primerNombre;
                repLegalCliente.MiddleName = segundoNombre;
                repLegalCliente.FamilyName = Apellidos;
                repLegalCliente.Telephone = _telefonoCliente;
                repLegalCliente.Email = _correoCliente;

                datosCliente.Person = repLegalCliente;

                Cliente.Party = datosCliente;

                ivaFactura.ID = "01";
                taxIVA.Amount = 0;
                taxIVA.Currency = "COP";
                ivaFactura.TaxAmount = taxIVA;
                ivaFactura.TaxEvidenceIndicator = "true";

                ipoconsumoFactura.ID = "02";
                taxCONSUMO.Amount = 0;
                taxCONSUMO.Currency = "COP";
                ipoconsumoFactura.TaxAmount = taxCONSUMO;
                ipoconsumoFactura.TaxEvidenceIndicator = "true";

                icaFactura.ID = "03";
                taxICA.Amount = 0;
                taxICA.Currency = "COP";
                icaFactura.TaxAmount = taxICA;
                icaFactura.TaxEvidenceIndicator = "true";

                impuestosFactura.Add(ivaFactura);
                impuestosFactura.Add(ipoconsumoFactura);
                impuestosFactura.Add(icaFactura);
                // Queda pendiente Definir el porque de los campos adicionales en la documentacion del Servicio.
                lineaExtCant.Amount = Int32.Parse(_ValCobrar.ToString());
                lineaExtCant.Currency = "COP";
                subtotalesFactura.LineExtensionAmount = lineaExtCant;
                // aqui va el TaxExclusiveAmount

                totalImpuesto.Amount = _Valtotal; //Total Base Imponible (Importe Bruto + Cargos- Descuentos): Base imponible para el cálculo de los impuestos. Todos los impuestos los calculan sobre una misma base???
                totalImpuesto.Currency = "COP";
                subtotalesFactura.TaxExclusiveAmount = totalImpuesto;
                Decimal totalFacturapago = _Valtotal + _ValImpuesto - _ValPagos;
                totalPagar.Amount = totalFacturapago; //Total de Factura: Total importe bruto + Total Impuestos-Total Impuesto Retenidos
                totalPagar.Currency = "COP";
                subtotalesFactura.PayableAmount = totalPagar;
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
                        string strDetalleFac = @"SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, F.Cantidad * F.ValTotal as Vlr_Total_Serv
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='PAQ' AND  a.IdFactura=@idFactura
 UNION ALL
SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, F.Cantidad * F.ValTotal as Vlr_Total_Serv
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino and g.IdProcPrincipal=2513
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE a.IndTipoFactura='PAQ' AND  a.idfactura=@idFactura
UNION ALL
SELECT isnull(h.NumAutorizacionInicial,'0')   AS Nro_Autorizacion,
upper(isnull(J.CodProMan,CASE ISNULL(f.REGCUM,'0') WHEN '0' THEN P.CodProducto ELSE F.REGCUM END )) as Cod_Servicio,
upper(( isnull(J.NomPRoman,P.NomProducto)) ) as Des_Servicio, f.Cantidad as Cantidad, f.ValTotal as Vlr_Unitario_Serv, F.Cantidad * F.ValTotal as Vlr_Total_Serv
FROM facfactura a
INNER JOIN  concontrato b on a.idcontrato=b.idcontrato
INNER JOIN  admatencion c on a.iddestino=c.idatencion
INNER JOIN  admcliente d on d.idcliente=c.idcliente
INNER JOIN  gentipodoc e on e.idtipodoc=d.idtipodoc
INNER JOIN  facfacturadet f on f.idfactura=a.idfactura
INNER JOIN  proproducto p on p.idproducto=f.idproducto AND p.IdProductoTipo not IN (8,12)
INNER JOIN  facmovimiento g on   g.idmovimiento=f.idmovimiento and g.iddestino=a.iddestino and g.IdProcPrincipal<>2513
LEFT JOIN vwFacProcPrincAsocPaq PQ on PQ.idfactura = a.idfactura and g.IdProcPrincipal=PQ.IdProcPrincipal 
LEFT JOIN admatencioncontrato h on h.idatencion=a.iddestino and a.idcontrato=h.idcontrato and a.idplan=h.idplan and h.indhabilitado=1
LEFT JOIN contarifa i on i.idtarifa=b.idtarifa
LEFT JOIN conManualAltDet J ON J.IdProducto = F.IdProducto AND J.IndHabilitado = 1 AND J.IdManual = i.IdManual
WHERE PQ.idfactura is null and a.IndTipoFactura='PAQ' AND   a.idfactura=@idFactura ORDER BY 4 ";
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
                                    DocumentLine lineaProducto = new DocumentLine();
                                    LineExtensionAmount extencionLinea = new LineExtensionAmount();
                                    Item itemLinea = new Item();
                                    Price precioProd = new Price();
                                    List<TaxLine> impuestoProducto = new List<TaxLine>();
                                    TaxLine ivaProducto = new TaxLine();
                                    TaxLine ipoconsumoProducto = new TaxLine();
                                    TaxAmount ivaTax = new TaxAmount();
                                    TaxAmount ipoConsumoTax = new TaxAmount();
                                    TaxLine lineaImpuestos = new TaxLine();

                                    lineaProducto.TypeLine = 1; // Lineaq Normal
                                    lineaProducto.ID = nroLinea;
                                    lineaProducto.InvoicedQuantity = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(3), 0).ToString());
                                    Int32 cantidad = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(3)).ToString());
                                    Int32 valor = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(4)).ToString());
                                    Int32 totalProducto = cantidad * valor;
                                    extencionLinea.Amount = totalProducto;
                                    lineaProducto.LineExtensionAmount = extencionLinea;
                                    itemLinea.Description = rdDetalleFac.GetString(2);
                                    lineaProducto.Item = itemLinea;
                                    precioProd.Amount = valor;
                                    lineaProducto.Price = precioProd;

                                    ivaProducto.ID = "01";
                                    //ivaProducto.Percent = 0;
                                    ivaTax.Amount = 0;
                                    ivaTax.Currency = "COP";
                                    ivaProducto.TaxAmount = ivaTax;
                                    impuestoProducto.Add(ivaProducto);
                                    //                                lineaProducto.TaxLine.Add(ivaProducto);
                                    ipoconsumoProducto.ID = "02";
                                    //ipoconsumoProducto.Percent = 0;
                                    ipoConsumoTax.Amount = 0;
                                    ipoConsumoTax.Currency = "COP";
                                    ipoconsumoProducto.TaxAmount = ipoConsumoTax;
                                    impuestoProducto.Add(ipoconsumoProducto);

                                    lineaProducto.TaxLine = impuestoProducto;

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

                        }
                    }
                    else // No se encuentra Informacion de la Factura y Atencion
                    {

                    }
                }

                //************************************** Adicionar elementos a la Factura **********************************
                //List<SellerSupplierParty> listaProveedores = new List<SellerSupplierParty>();
                //listaProveedores.Add(datosHUSI);
                //peticion.SellerSupplierParty = listaProveedores;
                peticion.AccountingCustomerParty = Cliente;
                //peticion.TaxTotal = impuestosFactura;
                peticion.LegalMonetaryTotal = subtotalesFactura;
                peticion.DocumentLines = detalleProductos;
                objData.OriginalRequest = peticion;
                facturaEnviar.Data = objData;

                //facturaEnviar.Data.OriginalRequest.SellerSupplierParty.Add(datosHUSI);
                facturaEnviar.Data.OriginalRequest.AccountingCustomerParty.Party = Cliente.Party;
                facturaEnviar.Data.OriginalRequest.TaxTotal = impuestosFactura;
                facturaEnviar.Data.OriginalRequest.LegalMonetaryTotal = subtotalesFactura;
                facturaEnviar.Data.OriginalRequest.DocumentLines = detalleProductos;

                //****************************************
                MemoryStream ms = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(eFactura));

                ser.WriteObject(ms, facturaEnviar);
                byte[] jsonEnviar = ms.ToArray();
                ms.Close();
                string resultado = Encoding.UTF8.GetString(jsonEnviar, 0, jsonEnviar.Length);
                //***************
                try
                {
                    string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
                    logFacturas.Info("URL de Request:" + urlConsumo);
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    //WebRequest WR = WebRequest.Create(requestUrl);   
                    string sb = JsonConvert.SerializeObject(facturaEnviar);
                    logFacturas.Info("Json de la Factura:" + sb);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    string Usuario = "admin";
                    string Clave = "super";
                    string codificado = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(Usuario + ":" + Clave));

                    request.Headers.Add("Authorization", "Basic " + codificado);
                    // "POST";request.ContentType = JSONContentType; // "application/json";   
                    Byte[] bt = Encoding.UTF8.GetBytes(sb);
                    Stream st = request.GetRequestStream();
                    st.Write(bt, 0, bt.Length);
                    st.Close();
                    //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    //{
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(String.Format("Server error (HTTP {0}: {1}).", response.StatusCode, response.StatusDescription));
                    Stream stream1 = response.GetResponseStream();
                    // DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));// object objResponse = JsonConvert.DeserializeObject();Stream stream1 = response.GetResponseStream();   
                    logFacturas.Info("Codigo Status:" + response.StatusCode);
                    logFacturas.Info("Descripcion Status:" + response.StatusDescription);
                    StreamReader sr = new StreamReader(stream1);
                    string strsb = sr.ReadToEnd();
                    object objResponse = JsonConvert.DeserializeObject(strsb);//, JSONResponseType);
                    logFacturas.Info("Respuesta:" + strsb);
                    return nroFactura.ToString(); ;
                    //}
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
