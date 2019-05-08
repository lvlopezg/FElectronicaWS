﻿using FElectronicaWS.Contratos;
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
		public string getData(int nroNotaDebito, int idCliente, int nroAtencion, string urlPdfNotaDebito)
		{
			logFacturas.Info("Se recibe Nota Debito con siguientes datos:nroNota:" + nroNotaDebito + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfNotaDebito);
			try
			{
				#region inicializar
				Decimal _Valtotal = 0;
				Decimal _ValDescuento = 0;
				Decimal _ValDescuentoT = 0;
				Decimal _ValPagos = 0;
				Decimal _ValImpuesto = 0;
				Decimal _ValCobrar = 0;
				DateTime _FecNotaCredito = DateTime.Now;
				Int32 _idMovimiento = 0;
				Int32 _IdUsuarioR = 0;
				Int32 _idTercero = 0;
				Int16 _naturalezaCliente = 0;
				Int16 _tipoPersona = 0;
				Int32 _facturaRelacionada = 0;
				string _nombrePaciente = string.Empty;
				string _nroDocumentopaciente = string.Empty;
				Byte _TipoDocPaciente = 0;
				string _numDocCliente = string.Empty;
				Byte _tipoDocCliente = 0;
				string _razonSocial = string.Empty;
				string _repLegal = string.Empty;

				//string pathurl = "https://pre.ifacturasalud.transfiriendo.com/IFacturaSanIgnacioWebapi/api/taxDocument/getPdf/Cufe/413dee677e3d22c676d5befb9748a0b7b504193c";
				//string[] partesUrl = pathurl.Split('/');
				//int posicion = pathurl.LastIndexOf('/');
				string xx = "";
				#endregion
				using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
					conn.Open();
					string qryNotaCredito1 = @"SELECT B.IdCuenta,B.NumDocumento,B.IdCausal,B.fecMovimiento,B.FecRegistro,B.ValMonto,cxcCta.IdCuenta,cxcCta.IdTercero,cxcCta.IdCliente,cxcCta.NomCliente,cxcCta.IdTipoDocCliente,cxcCta.NumDocumento,cxcCta.NumDocRespaldo,cxcCta.ValFactura,B.IdMovimiento FROM cxcTipoMovimientoEfectoNotas A
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
and C.IdNumeroNota=@nroNotaDebito2 and A.IndTipoNota='D'
";
					SqlCommand cmdNotaCredito1 = new SqlCommand(qryNotaCredito1, conn);
					cmdNotaCredito1.Parameters.Add("@nroNotaDebito", SqlDbType.VarChar).Value = nroNotaDebito.ToString();
					cmdNotaCredito1.Parameters.Add("@nroNotaDebito2", SqlDbType.Int).Value = nroNotaDebito;
					SqlDataReader rdNotaCredito1 = cmdNotaCredito1.ExecuteReader();
					if (rdNotaCredito1.HasRows)
					{
						rdNotaCredito1.Read();
						_idMovimiento = rdNotaCredito1.GetInt32(14);
						var valorTNota = rdNotaCredito1["ValMonto"];
						_Valtotal = Decimal.Parse(valorTNota.ToString());
						_ValCobrar = Decimal.Parse(valorTNota.ToString());
						_FecNotaCredito = rdNotaCredito1.GetDateTime(4);
						_facturaRelacionada = Int32.Parse(rdNotaCredito1.GetString(12));
						_idTercero = rdNotaCredito1.GetInt32(7);
					}

					string strTercero = "SELECT IdTipoDoc,NumDocumento,NomTercero,idnaturaleza FROM genTercero WHERE IdTercero=@tercero";
					SqlCommand cmdTercero = new SqlCommand(strTercero, conn);
					cmdTercero.Parameters.Add("@tercero", SqlDbType.Int).Value = _idTercero;
					SqlDataReader rdtercero = cmdTercero.ExecuteReader();
					if (rdtercero.HasRows)
					{
						rdtercero.Read();
						_tipoDocCliente = rdtercero.GetByte(0);
						_numDocCliente = rdtercero.GetString(1);
						_razonSocial = rdtercero.GetString(2);
						_naturalezaCliente = rdtercero.GetInt16(3);
					}


					#region definiciones
					eFactura notaEnviar = new eFactura();
					AdditionalInformation itemInfAdicionalEnc = new AdditionalInformation();
					List<AdditionalInformation> InformacionAdicionalEn = new List<AdditionalInformation>();
					Data objData = new Data
					{
						UrlPdf = urlPdfNotaDebito
					};

					OriginalRequest peticion = new OriginalRequest
					{
						EventName = "FAC-SYNC",
						Currency = "COP",
						DocumentType = "DebitNote",
						BroadCastDate = DateTime.Now.ToString("yyyy-MM-dd"),
						BroadCastTime = DateTime.Now.ToString("HH:mm:ss"),
						IdMotivo = "1",
						BillType = "1",
						Prefix = "",
						IdBusiness = "860015536",
						InvoiceId = nroNotaDebito.ToString(),
						//peticion.DocumentType = "01";
						// TODO: Tener en cuenta si se van a incluir campos adicionales
						AdditionalInformation = InformacionAdicionalEn
					};
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
					#endregion
					//********** Fin Definicion de Detalle de Factura
					//contrato.ID = _idContrato.ToString();
					//contrato.IssueDate = _FecFactura.ToString("yyyy-MM-dd");
					//contrato.ContractType = "1";

					// datosHUSI.Contract = contrato;
					//**********
					//   datosHUSI.AdditionalAccountID = "1";
					#region defHospital
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
					#endregion
					//****************** CLIENTE
					#region defCliente
					//  Variables Inicializacion
					string _direccionCliente = string.Empty;
					string _telefonoCliente = string.Empty;
					string _municipioCliente = string.Empty;
					string _departamento = string.Empty;
					int _localizacionCliente = 0;
					string _correoCliente = string.Empty;
					//**** 
					using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
					{     // Informacion General
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
						cmdDatosCliente3.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroNotaDebito;
						SqlDataReader rdDatosCliente3 = cmdDatosCliente3.ExecuteReader();
						if (rdDatosCliente3.HasRows)
						{
							rdDatosCliente3.Read();
							_correoCliente = rdDatosCliente3.GetString(0);
						}

					}
					idenCliente.SchemeID = "31";
					idenCliente.ID = _numDocCliente;
					datosCliente.PartyIdentification = idenCliente;
					datosCliente.Name = _razonSocial;
					objDireccionCliente.Line = _direccionCliente;
					objDireccionCliente.CityName = _municipioCliente;
					objDireccionCliente.CountryCode = "CO";
					objDireccionCliente.CitySubdivisionName = ""; //TODO: De Donde  Se obtiene de la Base de Datos. Especificidad en la direccion(Barrio, Edifico etc)
					objDireccionCliente.Department = _departamento;
					ubicacionFisicaCliente.Address = objDireccionCliente;
					datosCliente.PhysicalLocation = ubicacionFisicaCliente;
					if (_naturalezaCliente == 3)
					{
						RegimenCliente.TaxLevelCode = "2";
						Cliente.AdditionalAccountID = "1"; //TODO: Tipo de contribuyente. Buscar en SAHI
					}
					else
					{
						RegimenCliente.TaxLevelCode = "0";
						Cliente.AdditionalAccountID = "2"; //TODO: Tipo de contribuyente. Buscar en SAHI
					}
					string qryConsulta = @"SELECT con.IdContrato,con.IdTercero,con.NomRepComercial FROM conContrato con
INNER JOIN admAtencionContrato admC ON con.IdContrato = admC.IdContrato
WHERE admC.IdAtencion =@idAtencion";
					SqlCommand cmdConsulta = new SqlCommand(qryConsulta, conn);
					cmdConsulta.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
					SqlDataReader rdConsulta = cmdConsulta.ExecuteReader();
					if (rdConsulta.HasRows)
					{
						rdConsulta.Read();
						_repLegal = rdConsulta.GetString(2);
					}
					else
					{
						_repLegal = _razonSocial;
					}
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
						}
						Apellidos = nombreRepresentante[2];
						if (nombreRepresentante.Length > 3)
						{
							Apellidos = Apellidos + nombreRepresentante[3];
						}
					}
					repLegalCliente.FirstName = _razonSocial;
					repLegalCliente.MiddleName = " ";
					repLegalCliente.FamilyName = " ";
					repLegalCliente.Telephone = " ";
					repLegalCliente.Email = _correoCliente;

					//Cliente.Party.PartyTaxScheme.TaxLevelCode = "2"; // Regimen Comun;
					datosCliente.Person = repLegalCliente;
					Cliente.Party = datosCliente;
					#endregion

					#region impuesto
					ivaFactura.ID = "01";
					taxIVA.Amount = 0;
					//taxIVA.Currency = "COP";
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
					#endregion
					//************************************************************ Detalle de Nota Credito   ***********************************************************
					using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
					{
						conexion01.Open();
						try
						{
							string qryTipoNota = @"SELECT IdMovimiento,NumDocumento,DesMovimiento,b.IdTipoHomologoDian from cxccarteramovi a
inner join CXCTIPOMOVIMIENTOEFECTONOTAS b on a.idtipomovimiento=b.idtipomovimiento and b.indhabilitado=1
WHERE a.IdMovimiento=@idMovimiento";
							SqlCommand cmdTipoNota = new SqlCommand(qryTipoNota, conexion01);
							cmdTipoNota.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = _idMovimiento;
							SqlDataReader rdTipoNota = cmdTipoNota.ExecuteReader();
							rdTipoNota.Read();
							string DescNota = rdTipoNota.GetString(2);

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

							lineaProducto.TypeLine = 1; // Linea Normal
							lineaProducto.ID = 1;
							lineaProducto.InvoicedQuantity = 1;
							Int32 cantidad = 1;
							Int32 valor = Int32.Parse(_Valtotal.ToString());
							Int32 totalProducto = cantidad * valor;
							extencionLinea.Amount = totalProducto;
							lineaProducto.LineExtensionAmount = extencionLinea;
							itemLinea.Description = DescNota;
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
						}
						catch (Exception sqlExp)
						{
							string error = sqlExp.Message + "   " + sqlExp.StackTrace;
							throw;
						}
					}
					peticion.AccountingCustomerParty = Cliente;
					peticion.TaxTotal = impuestosFactura;
					peticion.LegalMonetaryTotal = subtotalesFactura;
					peticion.DocumentLines = detalleProductos;
					objData.OriginalRequest = peticion;
					notaEnviar.Data = objData;
					try
					{
						string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
						logFacturas.Info("URL de Request:" + urlConsumo);
						HttpWebRequest request = WebRequest.Create(urlConsumo) as HttpWebRequest;
						request.Timeout = 60 * 1000;
						string facturaJson = JsonConvert.SerializeObject(notaEnviar);
						logFacturas.Info("Json de la Nota Debito:" + facturaJson);
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
						//    logFacturas.Info(response.StatusCode.ToString());
						//    throw new Exception();
						//}
						//else
						//{
						//    logFacturas.Info(response.StatusCode);
						//    logFacturas.Info(response.StatusDescription);
						//}

						////Stream stream1 = response.GetResponseStream();
						//logFacturas.Info("Codigo Status:" + response.StatusCode);
						//logFacturas.Info("Descripcion Status:" + response.StatusDescription);
						StreamReader sr = new StreamReader(response.GetResponseStream());
						string strsb = sr.ReadToEnd();
						object objResponse = JsonConvert.DeserializeObject(strsb);//, JSONResponseType);

						//logFacturas.Info("Respuesta:" + strsb);
						////////respuestaJson rptaEntrega = JsonConvert.DeserializeObject<respuestaJson>(strsb);
						////////if (rptaEntrega.EsExitoso)
						////////{
						////////    using (SqlConnection conn2 = new SqlConnection(Properties.Settings.Default.DBConexion))
						////////    {
						////////        conn2.Open();
						////////        string strActualiza = @"UPDATE dbo.facNotaTempWEBService SET identificador=@identificador WHERE IdNota=@nroNota AND IdTipoNota=@idTipoNota";
						////////        SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
						////////        cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
						////////        cmdActualiza.Parameters.Add("@nroNota", SqlDbType.Int).Value = nroNotaDebito;
						////////        cmdActualiza.Parameters.Add("@idTipoNota", SqlDbType.VarChar).Value = "NC";
						////////        if (cmdActualiza.ExecuteNonQuery() > 0)
						////////        {
						////////            logFacturas.Info("Nota Debito Actualizada en facNotaTempWEBService");
						////////        }
						////////        else
						////////        {
						////////            logFacturas.Info("!!!   No fue posible Actualizar la Nota Debito en facNotaTempWEBService   !!!");
						////////        }
						////////    }
						////////}
						////////return nroNotaDebito.ToString();
						//StreamReader sr = new StreamReader(response.GetResponseStream());
						//string strsb = sr.ReadToEnd();
						//                    object objResponse = JsonConvert.DeserializeObject(strsb);//, JSONResponseType);
						logFacturas.Info("Respuesta Transfiriendo:" + strsb);
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
								using (SqlConnection conn2 = new SqlConnection(Properties.Settings.Default.DBConexion))
								{
									conn2.Open();
									string strActualiza = @"UPDATE dbo.facNotaTempWEBService SET identificador=@identificador WHERE IdNota=@nroNota AND IdTipoNota=@idTipoNota";
									SqlCommand cmdActualiza = new SqlCommand(strActualiza, conn);
									cmdActualiza.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
									cmdActualiza.Parameters.Add("@nroNota", SqlDbType.Int).Value = nroNotaDebito;
									cmdActualiza.Parameters.Add("@idTipoNota", SqlDbType.VarChar).Value = "ND";
									if (cmdActualiza.ExecuteNonQuery() > 0)
									{
										logFacturas.Info("Nota Debito Actualizada en facNotaTempWEBService");
										using (WebClient webClient = new WebClient())
										{
											try
											{

												System.Threading.Thread.Sleep(1000);
												string[] arregloPDF = rptaEntrega.Resultado.PDF.Split('/');
												string nombreArchivo = arregloPDF[arregloPDF.Length - 1];

												string carpetaDescarga = Properties.Settings.Default.urlDescargaPdfND + DateTime.Now.Year + @"\" + nombreArchivo + ".pdf";
												webClient.DownloadFile(rptaEntrega.Resultado.PDF, carpetaDescarga);
												//System.Threading.Thread.Sleep(1000);
												carpetaDescarga = Properties.Settings.Default.urlDescargaPdfFACT + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.CUFE + ".zip";
												webClient.DownloadFile(rptaEntrega.Resultado.ZIP, carpetaDescarga);
												//webClient.DownloadFile(rptaEntrega.Resultado.ZIP, @"D:\NotaDebitoQR\" + DateTime.Now.Year + @"\" + rptaEntrega.Resultado.CUFE + ".zip");
												using (SqlConnection conn3 = new SqlConnection(Properties.Settings.Default.DBConexion))
												{
													conn3.Open();
													string qryActualizaTempWEBService = @"UPDATE dbo.facNotaTempWEBService SET CodCUFE=@cufe,cadenaQR=@cadenaQR WHERE identificador=@identificador";
													SqlCommand cmdActualizaTempWEBService = new SqlCommand(qryActualizaTempWEBService, conn);
													cmdActualizaTempWEBService.Parameters.Add("@cufe", SqlDbType.VarChar).Value = nombreArchivo;
													cmdActualizaTempWEBService.Parameters.Add("@cadenaQR", SqlDbType.NVarChar).Value = rptaEntrega.Resultado.QR;
													cmdActualizaTempWEBService.Parameters.Add("@identificador", SqlDbType.VarChar).Value = rptaEntrega.Identificador;
													if (cmdActualizaTempWEBService.ExecuteNonQuery() > 0)
													{
														logFacturas.Info("Descarga Existosa de Archivos de la Nota Debito con Identificadotr:" + rptaEntrega.Identificador + "    Destino:" + carpetaDescarga);
														valorRpta = nroNotaDebito.ToString();
													}
													else
													{
														logFacturas.Info("No fue Posible realizar la Actualizacion de la Tabla facNotaTempWEBService de la Factura con Identificadotr:" + rptaEntrega.Identificador);
													}
												}
											}
											catch (NotSupportedException nSuppExp)
											{
												logFacturas.Info("Se ha presentado una NotSupportedException durante la descarga de los objetos de la Nota Debito:" + nSuppExp.Message + "     " + nSuppExp.InnerException.Message);
												valorRpta = "9X";
											}
											catch (ArgumentNullException argNull)
											{
												logFacturas.Info("Se ha presentado una ArgumentNullException durante la descarga de los objetos de la Nota Debito:" + argNull.Message + "     " + argNull.InnerException.Message);
												valorRpta = "9X";
											}
											catch (WebException webEx1)
											{
												logFacturas.Info("Se ha presentado una Falla durante la descarga de los objetos de la factura:" + webEx1.Message + "     " + webEx1.InnerException.Message);
												valorRpta = "93";
											}
											catch (Exception exx)
											{
												logFacturas.Info("No fue posible descargar los archivos.PDF, ZIP,CUFE y QR  !!! Causa:" + exx.Message);
												valorRpta = "98";
											}

										}

									}
									else
									{
										logFacturas.Info("!!!   No fue posible Actualizar la Nota Debito en la Tabla: facNotaTempWEBService   !!!");
									}
								}
							}
							else
							{
								logFacturas.Info("!!!  La Recuperacion de Documentos de la Nota Debito, No fue posible. No se Actualiza la Factura en facNotaTempWEBService   !!!");
								logFacturas.Warn("la respuesta recibida:" + strsb);
								//**** aqui se debe insertar en la tabla de fallos

							}
						}
						else if (strsb.Contains(validacionError))
						{
							respuestaEntregaError rptaEntregaErrores = JsonConvert.DeserializeObject<respuestaEntregaError>(strsb);
							logFacturas.Info("!!!  La Entrega de la factura No fue Existosa. No se Actualiza la Factura en facFacturaTempWEBService   !!!");
							logFacturas.Info("Errores:  Resultado:" + rptaEntregaErrores.Resultado);
							logFacturas.Info("Respuesta del Servicio" + strsb);
							valorRpta = "99";
						}
						return valorRpta;

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
			catch (Exception ex1)
			{
				logFacturas.Warn("Se ha presentado una excepcion:" + ex1.Message + " Pila de LLamados:" + ex1.StackTrace);
				return "98";
			}


		}
	}

}
