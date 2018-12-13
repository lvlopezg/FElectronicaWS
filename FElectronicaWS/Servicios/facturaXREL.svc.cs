﻿using NLog;
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

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "facturaXREL" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione facturaXREL.svc o facturaXREL.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class facturaXREL : IfacturaXREL
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
                Int32 _idTercero = 0;
                string _usrNombre = string.Empty;
                string _usrNumDocumento = string.Empty;
                Byte _usrIdTipoDoc = 0;
                string _numDocCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
                string _repLegal = string.Empty;
                string _monedaFactura = "COP";
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
                    }
                }

                eFactura facturaEnviar = new eFactura();
                AdditionalInformation iteminfAdicionalEnc = new AdditionalInformation();
                List<AdditionalInformation> InformacionAdicionalEn = new List<AdditionalInformation>();
                Data objData = new Data();
                objData.UrlPdf = urlPdfFactura;
                OriginalRequest peticion = new OriginalRequest();
                peticion.Currency = _monedaFactura;
                peticion.EventName = "FAC";
                //peticion.DocumentType = "01";
                peticion.Currency = _monedaFactura;
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

                //  SellerSupplierParty datosHUSI = new SellerSupplierParty();
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
                idenCliente.ID = _numDocCliente; //TODO: Obtener de BD Se utiliza Compensar como Ej FACT:4680824

                Cliente.AdditionalAccountID = "1"; //TODO: Tipo de contribuyente. Buscar en SAHI

                datosCliente.PartyIdentification = idenCliente;
                datosCliente.Name = _razonSocial;//TODO: Obtener Razon Social de BD
                objDireccionCliente.Line = _direccionCliente; //TODO: Obtener Direccion de BD
                objDireccionCliente.CityName = _municipioCliente;  //TODO: Obtener Ciudad de BD
                objDireccionCliente.CountryCode = "CO"; //TODO: DEfinir de Donde Obtener(Pais del Cliente) de Base de Datos
                objDireccionCliente.CitySubdivisionName = ""; //TODO: De Donde  Se obtiene de la Base de Datos.
                objDireccionCliente.Department = _departamento; //TODO: Obtener Departamento de REsidencia el Cliente
                ubicacionFisicaCliente.Address = objDireccionCliente;
                datosCliente.PhysicalLocation = ubicacionFisicaCliente;

                RegimenCliente.TaxLevelCode = "2";// TODO: Validar si este codigo (TaxLevelCode) es Fijo
                datosCliente.PartyTaxScheme = RegimenCliente;
                string primerNombre = string.Empty;
                string segundoNombre = string.Empty;
                string Apellidos = string.Empty;
                if (_repLegal.Length > 1)
                {
                    string[] nombreRepresentante = _repLegal.Split(' ');
                    if (nombreRepresentante.Length > 2)
                    {
                        primerNombre = nombreRepresentante[0];
                        segundoNombre = nombreRepresentante[1];
                        Apellidos = nombreRepresentante[2];
                        if (nombreRepresentante.Length > 3)
                        {
                            Apellidos = Apellidos + nombreRepresentante[3];
                        }
                    }
                    else
                    {
                        primerNombre = nombreRepresentante[0];
                        Apellidos = nombreRepresentante[1];

                    }
                }
                else
                {

                }
                repLegalCliente.FirstName = primerNombre;
                repLegalCliente.MiddleName = segundoNombre;
                repLegalCliente.FamilyName = Apellidos;
                repLegalCliente.Telephone = _telefonoCliente;
                repLegalCliente.Email = _correoCliente; // TODO: DEfinir la ubicacion de Envio de las cuentas de correo.

                //Cliente.Party.PartyTaxScheme.TaxLevelCode = "2"; // Regimen Comun;
                datosCliente.Person = repLegalCliente;

                Cliente.Party = datosCliente;

                ivaFactura.ID = "01";
                taxIVA.Amount = 0;
                taxIVA.Currency = _monedaFactura;
                ivaFactura.TaxAmount = taxIVA;
                ivaFactura.TaxEvidenceIndicator = "true";

                ipoconsumoFactura.ID = "02";
                taxCONSUMO.Amount = 0;
                taxCONSUMO.Currency = _monedaFactura;
                ipoconsumoFactura.TaxAmount = taxCONSUMO;
                ipoconsumoFactura.TaxEvidenceIndicator = "true";

                icaFactura.ID = "03";
                taxICA.Amount = 0;
                taxICA.Currency = _monedaFactura;
                icaFactura.TaxAmount = taxICA;
                icaFactura.TaxEvidenceIndicator = "true";

                impuestosFactura.Add(ivaFactura);
                impuestosFactura.Add(ipoconsumoFactura);
                impuestosFactura.Add(icaFactura);

                // Queda pendiente Definir el porque de los campos adicionales en la documentacion del Servicio.
                lineaExtCant.Amount = Int32.Parse(_ValCobrar.ToString());
                lineaExtCant.Currency = _monedaFactura;
                subtotalesFactura.LineExtensionAmount = lineaExtCant;
                // aqui va el TaxExclusiveAmount

                totalImpuesto.Amount = _Valtotal; //Total Base Imponible (Importe Bruto + Cargos- Descuentos): Base imponible para el cálculo de los impuestos. Todos los impuestos los calculan sobre una misma base???
                totalImpuesto.Currency = _monedaFactura;
                subtotalesFactura.TaxExclusiveAmount = totalImpuesto;
                Decimal totalFacturapago = _Valtotal + _ValImpuesto - _ValPagos;
                totalPagar.Amount = totalFacturapago; //Total de Factura: Total importe bruto + Total Impuestos-Total Impuesto Retenidos
                totalPagar.Currency = _monedaFactura;
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
                        string strDetalleFac = @"select d.iddestino,atn.idcliente,clte.NumDocumento,clte.nomCliente,clte.Apecliente,b.idProducto,prd.CodProducto,prd.nomproducto,b.idMovimiento,b.Cantidad,b.Valtotal,b.ValDescuento  from facfactura a 
INNER JOIN facfacturadet b on a.idfactura=b.idfactura
INNER JOIN facmovimiento d on  b.idmovimiento=d.idmovimiento
INNER JOIN proProducto prd ON b.idProducto=prd.idProducto
INNER JOIN admAtencion atn ON d.idDestino=atn.idAtencion
INNER JOIN admCliente clte ON atn.idCliente=clte.idCliente
where a.idfactura=@idFactura";
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
                                    lineaProducto.InvoicedQuantity = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(9), 0).ToString());
                                    Int32 cantidad = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(9)).ToString());
                                    Int32 valor = Int32.Parse(Math.Round(rdDetalleFac.GetDouble(10)).ToString());
                                    Int32 totalProducto = cantidad * valor;
                                    extencionLinea.Amount = totalProducto;
                                    lineaProducto.LineExtensionAmount = extencionLinea;
                                    itemLinea.Description = rdDetalleFac.GetString(7);
                                    lineaProducto.Item = itemLinea;
                                    precioProd.Amount = valor;
                                    lineaProducto.Price = precioProd;

                                    ivaProducto.ID = "01";
                                    //ivaProducto.Percent = 0;
                                    ivaTax.Amount = 0;
                                    ivaTax.Currency = _monedaFactura;
                                    ivaProducto.TaxAmount = ivaTax;
                                    impuestoProducto.Add(ivaProducto);

                                    //                                lineaProducto.TaxLine.Add(ivaProducto);

                                    ipoconsumoProducto.ID = "02";
                                    //ipoconsumoProducto.Percent = 0;
                                    ipoConsumoTax.Amount = 0;
                                    ipoConsumoTax.Currency = _monedaFactura;
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
                    HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
                    logFacturas.Info("URL de Request:" + urlConsumo);
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