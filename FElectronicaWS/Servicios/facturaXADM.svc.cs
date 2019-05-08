﻿using FElectronicaWS.Clases;
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
			logFacturas.Info("Se recibe factura con siguientes datos nroFactura: " + nroFactura + "  IdCliente:" + idCliente + " urlPdf:" + urlPdfFactura);
			try
			{
				Int32 _idContrato = 0;
				decimal _Valtotal = 0;
				Decimal _ValDescuento = 0;
				Decimal _ValDescuentoT = 0;
				Decimal _ValPagos = 0;
				Decimal _ValImpuesto = 0;
				decimal _ValCobrar = 0;
				DateTime _FecFactura = DateTime.Now;
				Decimal _valPos = 0;
				Decimal _valNoPos = 0;
				Decimal _valorIva = 0;
				Int32 _IdUsuarioR = 0;
				Int32 _idTercero = 0;
				string _usrNombre = string.Empty;
				string _usrNumDocumento = string.Empty;
				//Byte _usrIdTipoDoc = 0;
				string _numDocCliente = string.Empty;
				Byte _tipoDocCliente = 0;
				string _razonSocial = string.Empty;
				string _repLegal = string.Empty;
				using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
					conn.Open();
					string qryFacturaEnc = @"select  b.numdocrespaldo,IdTipoDocRespaldo,B.IdTercero,B.IdCliente,nomcliente,IdTipoDocCliente,B.NumDocumento,ValMonto,ValSaldo,FecRegistroC,FecRadicacion,IndEstado,ValFactura,a.IdUsuarioR,t.NomTercero from cxcfacmanual a
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
						string volorTf = Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString();
						_Valtotal = Decimal.Parse(volorTf);
						_ValDescuento = 0;
						_ValDescuentoT = 0;
						_ValPagos = 0;
						_ValImpuesto = 0;
						_ValCobrar = Decimal.Parse(Math.Round(rdFacturaEnc.GetDouble(12), 0).ToString());
						_FecFactura = rdFacturaEnc.GetDateTime(9);
						_valPos = 0;
						_valNoPos = 0;
						_IdUsuarioR = rdFacturaEnc.GetInt32(13);
						//_usrNombre = rdFacturaEnc.GetString(11);
						//_usrNumDocumento = rdFacturaEnc.GetString(12);
						//_usrIdTipoDoc = rdFacturaEnc.GetByte(13);
						_numDocCliente = rdFacturaEnc.GetString(6);
						_tipoDocCliente = rdFacturaEnc.GetByte(5);
						_razonSocial = rdFacturaEnc.GetString(4);
						_repLegal = rdFacturaEnc.GetString(4);
						_idTercero = rdFacturaEnc.GetInt32(2);
					}
					else
					{
						return "No se encontro Informacion General de Encabezado de Factura.";
					}
				}

				eFactura facturaEnviar = new eFactura();
				AdditionalInformation iteminfAdicionalEnc = new AdditionalInformation();
				List<AdditionalInformation> InformacionAdicionalEn = new List<AdditionalInformation>();
				Data objData = new Data
				{
					UrlPdf = urlPdfFactura
				};

				OriginalRequest peticion = new OriginalRequest
				{
					Currency = "COP",
					EventName = "FAC-SYNC",
					DocumentType = "Invoice",
					BroadCastDate = DateTime.Now.ToString("yyyy-MM-dd"),
					BroadCastTime = DateTime.Now.ToString("HH:mm:ss"),
					IdMotivo = "1",
					BillType = "1",
					InvoiceId = nroFactura.ToString(),
					Prefix = Properties.Settings.Default.Prefijo,
					IdBusiness = Properties.Settings.Default.NitHusi,
					AdditionalInformation = InformacionAdicionalEn
					// peticion.DocumentType = "01";
				};

				#region Cuenta Medica
				//todo: Campos cuenta medica- uso Futuro
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
				#endregion
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
				TaxableAmount camposAdicionalesICA = new TaxableAmount();
				//********** Fin Definicion de Detalle de Factura
				contrato.ID = _idContrato.ToString();
				contrato.IssueDate = _FecFactura.ToString("yyyy-MM-dd");
				contrato.ContractType = "1";

				// datosHUSI.Contract = contrato;
				//**********
				//   datosHUSI.AdditionalAccountID = "1";
				datosHospital.Name = "Hospital Universitario San Ignacio";
				idenHusi.ID = Properties.Settings.Default.NitHusi;
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
								_municipioCliente = rdDatosCliente1.GetString(2);
								_localizacionCliente = rdDatosCliente1.GetInt32(3);
							}
							else if (rdDatosCliente1.GetInt32(0) == 3)
							{
								_telefonoCliente = rdDatosCliente1.GetString(1);
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
					//else
					//{
					//	_correoCliente = "";
					//}
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
							//_valorIva = rdIvaFact.GetDecimal(0);
						}
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
					if (nombreRepresentante.Length == 2)
					{
						primerNombre = nombreRepresentante[0];
						segundoNombre = " ";
						Apellidos = nombreRepresentante[1];
					}
					else if (nombreRepresentante.Length > 2)
					{
						primerNombre = nombreRepresentante[0];
						segundoNombre = nombreRepresentante[1];
						Apellidos = nombreRepresentante[2];
					}
					if (nombreRepresentante.Length > 3)
					{
						Apellidos = Apellidos + nombreRepresentante[3];
					}

				}
				repLegalCliente.FirstName = _razonSocial;
				repLegalCliente.MiddleName = "";
				repLegalCliente.FamilyName = "";
				repLegalCliente.Telephone = "";
				repLegalCliente.Email = _correoCliente;

				//Cliente.Party.PartyTaxScheme.TaxLevelCode = "2"; // Regimen Comun;
				datosCliente.Person = repLegalCliente;
				Cliente.Party = datosCliente;
				Cliente.Party.PartyTaxScheme = RegimenCliente;
				//_valorIva = Math.Round(rdDetalleFac.GetDecimal(5));
				taxIVA.Amount = int.Parse(_valorIva.ToString());
				//taxIVA.Currency = "COP";
				ivaFactura.ID = "01";
				ivaFactura.TaxAmount = taxIVA;
				ivaFactura.TaxEvidenceIndicator = "true";

				ipoconsumoFactura.ID = "02";
				taxCONSUMO.Amount = 0;
				//taxCONSUMO.Currency = "COP";
				ipoconsumoFactura.TaxAmount = taxCONSUMO;
				ipoconsumoFactura.TaxEvidenceIndicator = "true";

				icaFactura.ID = "03";
				taxICA.Amount = 0;
				//taxICA.Currency = "COP";
				icaFactura.TaxAmount = taxICA;
				icaFactura.TaxEvidenceIndicator = "true";
				camposAdicionalesICA.Amount = _Valtotal.TomarDecimales(2);
				icaFactura.TaxableAmount = camposAdicionalesICA;
				icaFactura.Percent = 0;

				impuestosFactura.Add(ivaFactura);
				impuestosFactura.Add(ipoconsumoFactura);
				impuestosFactura.Add(icaFactura);
				// Queda pendiente Definir el porque de los campos adicionales en la documentacion del Servicio.
				lineaExtCant.Amount = _ValCobrar.TomarDecimales(2);
				lineaExtCant.Currency = "COP";
				subtotalesFactura.LineExtensionAmount = lineaExtCant;
				// aqui va el TaxExclusiveAmount

				totalImpuesto.Amount = _Valtotal.TomarDecimales(2); //Total Base Imponible (Importe Bruto + Cargos- Descuentos): Base imponible para el cálculo de los impuestos. Todos los impuestos los calculan sobre una misma base???
				totalImpuesto.Currency = "COP";
				subtotalesFactura.TaxExclusiveAmount = totalImpuesto;
				Decimal totalFacturapago = _Valtotal + _ValImpuesto.TomarDecimales(2) - _ValPagos.TomarDecimales(2);
				totalPagar.Amount = totalFacturapago.TomarDecimales(2);
				totalPagar.Currency = "COP";
				subtotalesFactura.PayableAmount = totalPagar;
				//************************************************************ Detalle de Factura por Administrativa ***********************************************************
				using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
					conexion01.Open();
					string strDetalleFac = @"select top 10 b.numdocrespaldo, c.* from cxcfacmanual a
inner join cxccuenta b on a.idcuenta = b.idcuenta
inner join cxcfacmanualconc c on c.IdCuenta = a.idcuenta
where c.val_unit_inte is null and NumDocRespaldo = @idFactura
order by c.num_regi";
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

								lineaProducto.InvoicedQuantity = rdDetalleFac.GetInt32(3);
								Int32 cantidad = rdDetalleFac.GetInt32(3);
								Decimal valor = Math.Round(rdDetalleFac.GetDecimal(5));
								Decimal totalProducto = cantidad * valor;
								extencionLinea.Amount = totalProducto;
								lineaProducto.LineExtensionAmount = extencionLinea;
								itemLinea.Description = rdDetalleFac.GetString(4);
								lineaProducto.Item = itemLinea;
								precioProd.Amount = valor;
								lineaProducto.Price = precioProd;
								ivaProducto.ID = "01";
								ivaTax.Amount = 0;
								if (_valorIva > 0)
								{
									ivaTax.Amount = int.Parse(_valorIva.ToString());
									Decimal porcentajeIva = (_valorIva / valor) * 100;
									porcentajeIva = Math.Round(porcentajeIva, 0);

									Int16 tarifaIva = Int16.Parse(porcentajeIva.ToString());
									ivaProducto.Percent = tarifaIva;

								}
								ivaTax.Currency = "COP";
								ivaProducto.TaxAmount = ivaTax;
								impuestoProducto.Add(ivaProducto);
								// lineaProducto.TaxLine.Add(ivaProducto);
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
				peticion.AccountingCustomerParty = Cliente;
				peticion.TaxTotal = impuestosFactura;
				peticion.LegalMonetaryTotal = subtotalesFactura;
				peticion.DocumentLines = detalleProductos;
				objData.OriginalRequest = peticion;
				facturaEnviar.Data = objData;
				#endregion
				try
				{
					string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
					logFacturas.Info("URL de Request:" + urlConsumo);
					HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
					request.Timeout = 60 * 1000;
					string facturaJson = JsonConvert.SerializeObject(facturaEnviar);
					logFacturas.Info("Json de la Factura:" + facturaJson);
					request.Method = "POST";
					request.ContentType = "application/json";
					string Usuario = "admin";
					string Clave = "super";
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
					//if (response.StatusCode != HttpStatusCode.OK)
					//{
					//	logFacturas.Info(response.StatusCode.ToString());
					//	throw new Exception();
					//}
					//else
					//{
					//	logFacturas.Info(response.StatusCode);
					//	logFacturas.Info(response.StatusDescription);
					//	return $"Se ha presentado una falla:Codigo:{response.StatusCode} Descripcion:{response.StatusDescription}";
					//}
					//Stream stream1 = response.GetResponseStream();
					logFacturas.Info("Codigo Status:" + response.StatusCode);
					logFacturas.Info("Descripcion Status:" + response.StatusDescription);
					StreamReader sr = new StreamReader(response.GetResponseStream());
					string strsb = sr.ReadToEnd();
					logFacturas.Info("Respuesta Recibida Transfiriendo:" + strsb);
					string valorRpta = "00";
					string validacion = "\"PDF\":";
					string validacionError = "\"Errors\":";
					if (strsb.Contains(validacion))
					{
						respuestaEntregaExitosa rptaEntrega = JsonConvert.DeserializeObject<respuestaEntregaExitosa>(strsb);
						logFacturas.Info("PDF:" + rptaEntrega.Resultado.PDF);
						logFacturas.Info("XML:" + rptaEntrega.Resultado.ZIP);
						logFacturas.Info("CUFE:" + rptaEntrega.Resultado.CUFE);
						if (rptaEntrega.EsExitoso)
						{
							using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
							{
								conn.Open();
								using (WebClient webClient = new WebClient())
								{
									try
									{
										System.Threading.Thread.Sleep(1000);
										string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.CUFE + ".pdf";
										webClient.DownloadFile(rptaEntrega.Resultado.PDF, carpetaDescarga);
										carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.CUFE + ".zip";
										webClient.DownloadFile(rptaEntrega.Resultado.ZIP, carpetaDescarga);
										using (SqlConnection conn3 = new SqlConnection(Properties.Settings.Default.DBConexion))
										{
											conn3.Open();
											string qryActualizaTempWEBService = @"UPDATE dbo.facFacturaTempWEBService SET CodCUFE=@cufe,cadenaQR=@cadenaQR, identificador=@identificador WHERE IdFactura=@nrofactura";
											SqlCommand cmdActualizaTempWEBService = new SqlCommand(qryActualizaTempWEBService, conn);
											cmdActualizaTempWEBService.Parameters.Add("@cufe", SqlDbType.VarChar).Value = rptaEntrega.Resultado.CUFE;
											cmdActualizaTempWEBService.Parameters.Add("@cadenaQR", SqlDbType.NVarChar).Value = rptaEntrega.Resultado.QR;
											cmdActualizaTempWEBService.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
											cmdActualizaTempWEBService.Parameters.Add("@nrofactura", SqlDbType.Int).Value = nroFactura;
											if (cmdActualizaTempWEBService.ExecuteNonQuery() > 0)
											{
												logFacturas.Info("Descarga Existosa de Archivos de la Factura con Identificadotr:" + rptaEntrega.Identificador + " Destino:" + carpetaDescarga);
												string qryInsertarControl = "INSERT INTO facturasProcesoDian () VALUES()";
												valorRpta = nroFactura.ToString();
												return valorRpta;
											}
											else
											{
												logFacturas.Info("No fuePusible realizar la Descarga de Archivos de la Factura con Identificadotr:" + rptaEntrega.Identificador);
												return "9X1";
											}
										}
									}
									catch (NotSupportedException nSuppExp)
									{
										logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de los objetos de la Factura:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
										valorRpta = "9X2";
										return valorRpta;
									}
									catch (ArgumentNullException argNull)
									{
										logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de los objetos de la Factura:" + argNull.Message + "     " + argNull.InnerException.Message);
										valorRpta = "9X3";
										return valorRpta;
									}
									catch (WebException webEx1)
									{
										logFacturas.Info("Se ha presentado una Falla durante la descarga de los objetos de la factura:" + webEx1.Message + "     " + webEx1.InnerException.Message);
										valorRpta = "93";
										return valorRpta;
									}
									catch (Exception exx)
									{
										logFacturas.Info("No fue posible descargar los archivos.PDF, ZIP,CUFE y QR  !!! Causa:" + exx.Message);
										valorRpta = "98";
										return valorRpta;
									}
								}
							}
						}
						else if (strsb.Contains(validacionError))
						{
							respuestaEntregaError rptaEntregaErrores = JsonConvert.DeserializeObject<respuestaEntregaError>(strsb);
							logFacturas.Info("!!!  La Entrega de la factura No fue Existosa. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
							logFacturas.Info("Errores:  Resultado:" + rptaEntregaErrores.Resultado);
							logFacturas.Info("Respuesta del Servicio" + strsb);
							valorRpta = "99";
							return valorRpta;
						}
						return valorRpta;
					}
					else
					{
						logFacturas.Info("!!!  Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
						logFacturas.Warn("Respuesta recibida:" + strsb);
						//*Aqui se debe insertar en la tabla de fallas
						return "Recuperacion Documentos de la Factura, No fue posible. No se Actualiza la Factura en facFacturaTempWEBService";
					}
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
