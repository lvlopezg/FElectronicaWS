using FElectronicaWS.Clases;
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
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace FElectronicaWS.Servicios
{
	public class facturaXINT : IfacturaXINT
	{
		private static Logger logFacturas = LogManager.GetCurrentClassLogger();
		public string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura)
		{
			logFacturas.Info("Se recibe factura con siguietnes datos:Factura Internacional:" + nroFactura + "  IdCliente:" + idCliente + " nroAtencion:" + nroAtencion + " urlPdf:" + urlPdfFactura);
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
                string _direccionCliente = string.Empty;
				string _IdUsuarioR = string.Empty;
				string _usrNombre = string.Empty;
				string _usrNumDocumento = string.Empty;
				//Byte _usrIdTipoDoc = 0;
                Int32 _idTercero = 0;
                string _numDocCliente = string.Empty;
				string _telefonoCliente = string.Empty;
				string _municipioCliente = string.Empty;
                string _ciudad = string.Empty;
				string _departamento = string.Empty;
				string _correoCliente = string.Empty;
                Byte _tipoDocCliente = 0;
                string _razonSocial = string.Empty;
				string _repLegal = string.Empty;
                string _RegimenFiscal = string.Empty;
                Byte _tipoDocClienteDian = 0;
                Int16 _idNaturaleza = 0;
                //int concepto = 0;
                FormaPago formaPagoTmp = new FormaPago();
                //string monedaFactura = "USD";
                //Fin de Inicializacion
                documentoRoot documentoF2 = new documentoRoot();
                Documento facturaEnviar = new Documento();
                facturaEnviar.identificadorTransaccion ="bf37ed2a-ea9b-436a-88d7-2dbf9e1e0006" ;
                facturaEnviar.URLPDF = urlPdfFactura;
                facturaEnviar.NITFacturador = Properties.Settings.Default.NitHusi;
                facturaEnviar.prefijo = Properties.Settings.Default.Prefijo;
                facturaEnviar.numeroDocumento = nroFactura.ToString();
                facturaEnviar.tipoDocumento = 1;
                facturaEnviar.subTipoDocumento = "01";
                facturaEnviar.tipoOperacion = "05";
                facturaEnviar.generaRepresentacionGrafica = false;

                //ClienteInternacional cliente;
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
                
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
					conn.Open();
					string qryFacturaEnc = @"SELECT c.idtercero,b.idcontrato,a.valtotalUSD as ValorTotalFactura, a.*
FROM facfacturapacint a
INNER JOIN facfactura b on a.idfactura=b.idfactura
INNER JOIN concontrato c on b.idcontrato=c.idcontrato
WHERE a.indhabilitado=1 and  a.idfactura=@nroFactura";
					SqlCommand cmdFacturaEnc = new SqlCommand(qryFacturaEnc, conn);
					cmdFacturaEnc.Parameters.Add("@nroFactura", SqlDbType.Int).Value = nroFactura;
					SqlDataReader rdFacturaEnc = cmdFacturaEnc.ExecuteReader();
					if (rdFacturaEnc.HasRows)
					{
						rdFacturaEnc.Read();
						_idContrato = rdFacturaEnc.GetInt32(1);
						_Valtotal = Math.Round(rdFacturaEnc.GetDecimal(36), 2);
						_ValDescuento = Math.Round(rdFacturaEnc.GetDecimal(38), 2); //ValDescuentoUSD
						_ValDescuentoT = Math.Round(rdFacturaEnc.GetDecimal(38), 2);
						_ValPagos = Math.Round(rdFacturaEnc.GetDecimal(40), 2);
						_ValImpuesto = 0;// Math.Round(rdFacturaEnc.GetDecimal(5), 0);
						_ValCobrar = _Valtotal - _ValDescuento - _ValDescuentoT + _ValImpuesto;
						_FecFactura = rdFacturaEnc.GetDateTime(13);
						_valPos = Math.Round(rdFacturaEnc.GetDecimal(36), 2);
                        _idTercero = rdFacturaEnc.GetInt32(0);
//                        Decimal _valNoPos = 0;
                        _IdUsuarioR = rdFacturaEnc.GetString(43);
						//_usrNombre = $"{rdFacturaEnc.GetString(16)} {rdFacturaEnc.GetString(17)}";
						//_usrNumDocumento = rdFacturaEnc.GetString(18);
						//_usrIdTipoDoc = rdFacturaEnc.GetByte(49);
						_razonSocial = rdFacturaEnc.GetString(6);
						_repLegal = rdFacturaEnc.GetString(11);
						_direccionCliente = rdFacturaEnc.GetString(8);
						_numDocCliente = rdFacturaEnc.GetString(7);
						_municipioCliente = rdFacturaEnc.GetString(9);
						_telefonoCliente = rdFacturaEnc.GetString(10);
					}

                    string qryDatosGenerales = @"SELECT ter.NumDocumento,ter.IdTipoDoc,ter.NomTercero,ter.CodTercero,con.NomRepComercial,ter.IdTercero,ter.idRegimen,ter.IdNaturaleza  
FROM facFactura fact
INNER JOIN ASI_USUA usr ON fact.IdUsuarioR = usr.IdUsuario
INNER JOIN conContrato con ON fact.IdContrato = con.IdContrato
INNER JOIN genTercero ter ON con.IdTercero = ter.IdTercero
WHERE IdFactura = @idFactura";
                    SqlCommand cmdDatosGenerales = new SqlCommand(qryDatosGenerales, conn);
                    cmdDatosGenerales.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
                    SqlDataReader rdDatosGenerales = cmdDatosGenerales.ExecuteReader();
                    if (rdDatosGenerales.HasRows)
                    {
                        rdDatosGenerales.Read();
                        _idNaturaleza = rdDatosGenerales.GetInt16(7);
                        _tipoDocCliente = rdDatosGenerales.GetByte(1);
                    }
                }

                string formatoWrk = formatosFecha.formatofecha(_FecFactura);
                facturaEnviar.fechaEmision = formatoWrk.Split('T')[0];
                facturaEnviar.horaEmision = formatoWrk.Split('T')[1];
                facturaEnviar.moneda = "USD";
                formaPagoTmp.tipoPago = 1;
                formaPagoTmp.codigoMedio = "10";
                facturaEnviar.formaPago = formaPagoTmp;

                List<DetallesItem> detalleProductos = new List<DetallesItem>();
                //****************** CLIENTE
                //  Variables Inicializacion
                //string _direccionCliente = string.Empty;
                //string _telefonoCliente = string.Empty;
                //string _municipioCliente = string.Empty;
                //string _departamento = string.Empty;
                int _localizacionCliente = 0;
                //string _correoCliente = string.Empty;
                using (SqlConnection connx = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
                    connx.Open();
                    string qryDatosCliente1 = @"SELECT IdLocalizaTipo,DesLocalizacion,B.nom_dipo,A.IdLugar,RIGHT(B.cod_dipo,5),cod_dipo FROM genTerceroLocaliza A
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
                                //_municipioCliente = rdDatosCliente1.GetString(5);
                                //_municipioCliente = "533";
                                _municipioCliente = "00000";
                                int inicio = _direccionCliente.Length - 12;
                                _ciudad = _direccionCliente.Substring(inicio,10);
                                _localizacionCliente = rdDatosCliente1.GetInt32(3);
                            }
                            else if (rdDatosCliente1.GetInt32(0) == 3)
                            {
                                _telefonoCliente = rdDatosCliente1.GetString(1);
                                if (_telefonoCliente.Length>10)
                                {
                                    _telefonoCliente = _telefonoCliente.Substring(0, 10);
                                }
                            }
                        }
                    }

                    //connx.Open();
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
                        _correoCliente = "";
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
                //adquirienteTmp.identificacion = _numDocCliente;
                //if (_tipoDocCliente == 1)//TODO: validar la Homologacion para este campo
                //{
                //    adquirienteTmp.tipoIdentificacion = 31;
                //}
                //else if (_tipoDocCliente == 2)
                //{
                //    adquirienteTmp.tipoIdentificacion = 13;
                //}

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

                adquirienteTmp.identificacion = cliente.NroDoc_Cliente;
                adquirienteTmp.codigoInterno = cliente.IdTercero.ToString();
                adquirienteTmp.razonSocial = cliente.NomTercero;
                adquirienteTmp.nombreSucursal = cliente.NomTercero;
                adquirienteTmp.correo = cliente.cuenta_correo.Trim();
                adquirienteTmp.telefono = cliente.telefono;

                //adquirienteTmp.codigoInterno = _idTercero.ToString();
                //adquirienteTmp.razonSocial = _razonSocial;
                //adquirienteTmp.nombreSucursal = _razonSocial;
                //adquirienteTmp.correo = _correoCliente.Trim();
                //adquirienteTmp.telefono = _telefonoCliente;

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
                responsanbilidadesR.Add("R-12-PJ");
                adquirienteTmp.responsabilidadesRUT = responsanbilidadesR;
                Ubicacion ubicacionCliente = new Ubicacion();
                ubicacionCliente.pais = cliente.codigoPais;// "AW";
                ubicacionCliente.codigoMunicipio = _municipioCliente;
                ubicacionCliente.departamento = cliente.Nombre_Depto;// "Aruba";
                ubicacionCliente.direccion = cliente.direccion;// _direccionCliente;
                ubicacionCliente.ciudad = cliente.Nom_Municipio;// _ciudad;
                adquirienteTmp.ubicacion = ubicacionCliente;
                documentoF2.adquiriente = adquirienteTmp;
                double TotalGravadoIva = 0;
                //double TotalGravadoIca = 0;
                #region MyRegion
                //            idenCliente.SchemeID = "31";
                //idenCliente.ID = _numDocCliente;

                //Cliente.AdditionalAccountID = "1"; //TODO: Tipo de contribuyente. Buscar en SAHI

                //datosCliente.PartyIdentification = idenCliente;
                //datosCliente.Name = _razonSocial;
                //objDireccionCliente.Line = _direccionCliente;
                //objDireccionCliente.CityName = _municipioCliente;
                //objDireccionCliente.CountryCode = "AW"; //TODO: DEfinir de Donde Obtener(Pais del Cliente) desde Base de Datos
                //objDireccionCliente.CitySubdivisionName = ""; //TODO: De Donde  Se obtiene de la Base de Datos. Especificidad en la direccion(Barrio, Edifico etc)
                //objDireccionCliente.Department = "";
                //ubicacionFisicaCliente.Address = objDireccionCliente;
                //datosCliente.PhysicalLocation = ubicacionFisicaCliente;

                //RegimenCliente.TaxLevelCode = "2";// TODO: Validar si este codigo (TaxLevelCode) es Fijo. ??
                //datosCliente.PartyTaxScheme = RegimenCliente;
                //string primerNombre = string.Empty;
                //string segundoNombre = string.Empty;
                //string Apellidos = string.Empty;
                //if (_repLegal.Length > 1)
                //{
                //	string[] nombreRepresentante = _repLegal.Split(' ');
                //	if (nombreRepresentante.Length == 2)
                //	{
                //		primerNombre = nombreRepresentante[0];
                //		segundoNombre = " ";
                //		Apellidos = nombreRepresentante[1];
                //	}
                //	else if (nombreRepresentante.Length > 2)
                //	{
                //		primerNombre = nombreRepresentante[0];
                //		segundoNombre = nombreRepresentante[1];
                //		Apellidos = nombreRepresentante[2];
                //	}
                //	if (nombreRepresentante.Length > 3)
                //	{
                //		Apellidos = Apellidos + nombreRepresentante[3];
                //	}
                //}
                //repLegalCliente.FirstName = _razonSocial;
                //repLegalCliente.MiddleName = " ";
                //repLegalCliente.FamilyName = " ";
                //repLegalCliente.Telephone = " ";
                //repLegalCliente.Email = _correoCliente; // TODO: DEfinir la ubicacion de Envio de las cuentas de correo.

                ////Cliente.Party.PartyTaxScheme.TaxLevelCode = "2"; // Regimen Comun;
                //datosCliente.Person = repLegalCliente;

                //Cliente.Party = datosCliente; 
                #endregion

                #region MyRegion
                //            ivaFactura.ID = "01";
                //taxIVA.Amount = 0;
                //taxIVA.Currency = monedaFactura;
                //ivaFactura.TaxAmount = taxIVA;
                //ivaFactura.TaxEvidenceIndicator = "true";

                //ipoconsumoFactura.ID = "02";
                //taxCONSUMO.Amount = 0;
                //taxCONSUMO.Currency = monedaFactura;
                //ipoconsumoFactura.TaxAmount = taxCONSUMO;
                //ipoconsumoFactura.TaxEvidenceIndicator = "true";

                //icaFactura.ID = "03";
                //taxICA.Amount = 0;
                //taxICA.Currency = monedaFactura;
                //icaFactura.TaxAmount = taxICA;
                //icaFactura.TaxEvidenceIndicator = "true";

                //camposAdicionalesICA.Amount = _Valtotal.TomarDecimales(2);
                //camposAdicionalesICA.Currency = monedaFactura;
                //icaFactura.TaxableAmount = camposAdicionalesICA;
                //icaFactura.Percent = 0;

                //impuestosFactura.Add(ivaFactura);
                //impuestosFactura.Add(ipoconsumoFactura);
                //impuestosFactura.Add(icaFactura);
                //// Queda pendiente Definir el porque de los campos adicionales en la documentacion del Servicio.
                //lineaExtCant.Amount = _ValCobrar.TomarDecimales(2);
                //lineaExtCant.Currency = monedaFactura;
                //subtotalesFactura.LineExtensionAmount = lineaExtCant;
                //// aqui va el TaxExclusiveAmount

                //totalImpuesto.Amount = _Valtotal; //Total Base Imponible (Importe Bruto + Cargos- Descuentos): Base imponible para el cálculo de los impuestos. Todos los impuestos los calculan sobre una misma base???
                //totalImpuesto.Currency = monedaFactura;
                //subtotalesFactura.TaxExclusiveAmount = totalImpuesto;
                //Decimal totalFacturapago = _Valtotal.TomarDecimales(2) + _ValImpuesto.TomarDecimales(2) - _ValPagos.TomarDecimales(2);
                //totalPagar.Amount = totalFacturapago.TomarDecimales(2);
                //totalPagar.Currency = monedaFactura;
                //subtotalesFactura.PayableAmount = totalPagar; 
                #endregion

                //************************************************************ Detalle de Factura Internacional ***********************************************************
                using (SqlConnection conexion01 = new SqlConnection(Properties.Settings.Default.DBConexion))
				{
					string qryFactura = @"select c.idtercero,b.idcontrato,a.* from facfacturapacint a 
inner join facfactura b on a.idfactura = b.idfactura
inner join concontrato c on b.idcontrato = c.idcontrato 
where a.indhabilitado = 1 and a.idfactura =@idFactura";
					conexion01.Open();
					SqlCommand cmdFactura = new SqlCommand(qryFactura, conexion01);
					cmdFactura.Parameters.Add("@idFactura", SqlDbType.Int).Value = nroFactura;
					//cmdFactura.Parameters.Add("@idAtencion", SqlDbType.Int).Value = nroAtencion;
					SqlDataReader rdFactura = cmdFactura.ExecuteReader();
					if (rdFactura.HasRows)
					{
						rdFactura.Read();
						string strDetalleFac = @"SELECT b.idfactura,d.cantidad,case when d.cantidad=1 then d.valtotalusd else d.valunitusd end as Valorunitario, d.valtotalUSD as Valortotal , d.*  
FROM facfacturapacint a
INNER JOIN facfactura b on a.idfactura=b.idfactura
INNER JOIN facfacturapacintdet d on a.idfacturapacint=d.idfacturapacint
INNER JOIN concontrato c on b.idcontrato=c.idcontrato
WHERE a.indhabilitado=1 and a.idfactura=@idFactura";

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
                                    List<TibutosDetalle> listaTributos = new List<TibutosDetalle>();
                                    DetallesItem lineaProducto = new DetallesItem();
                                    lineaProducto.tipoDetalle = 1; // Linea Normal
                                    string codigoProducto = rdDetalleFac.GetString(7);
                                    lineaProducto.valorCodigoInterno = codigoProducto;

                                    double valorUnitario = double.Parse(rdDetalleFac.GetDecimal(2).ToString());
                                    double cantidad = double.Parse(rdDetalleFac.GetInt32(1).ToString()).TomarDecimales(2);

                                    lineaProducto.codigoEstandar = "999";
                                    lineaProducto.valorCodigoEstandar = codigoProducto;
                                    lineaProducto.descripcion = rdDetalleFac.GetString(8);
                                    lineaProducto.unidades = double.Parse(rdDetalleFac.GetInt32(1).ToString()).TomarDecimales(2);
                                    lineaProducto.unidadMedida = "94";// rdDetalleFac.GetString(19);
                                    lineaProducto.valorUnitarioBruto = double.Parse(rdDetalleFac.GetDecimal(2).ToString());
                                    //lineaProducto.valorBruto = double.Parse(rdDetalleFac.GetDecimal(3).ToString()).TomarDecimales(2);
                                    lineaProducto.valorBruto = valorUnitario*cantidad;
                                    lineaProducto.valorBrutoMoneda = "USD";

                                    TibutosDetalle tributosWRKIva = new TibutosDetalle();
                                    tributosWRKIva.id = "01";
                                    tributosWRKIva.nombre = "Iva";
                                    tributosWRKIva.esImpuesto = true;
                                    tributosWRKIva.porcentaje = 0;
                                    tributosWRKIva.valorBase = double.Parse(rdDetalleFac.GetDecimal(3).ToString());
                                    tributosWRKIva.valorImporte = double.Parse(rdDetalleFac.GetDecimal(3).ToString()) * 0;
                                    TotalGravadoIva = TotalGravadoIva + double.Parse(rdDetalleFac.GetDecimal(3).ToString());
                                    tributosWRKIva.tributoFijoUnidades = 0;
                                    tributosWRKIva.tributoFijoValorImporte = 0;
                                    listaTributos.Add(tributosWRKIva);
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
						logFacturas.Info("No se encontro Informacion de Factura y Atencion." + qryFactura);
					}
				}

                #region MyRegion

                //peticion.AccountingCustomerParty = Cliente;
                ////peticion.TaxTotal = impuestosFactura;
                //peticion.LegalMonetaryTotal = subtotalesFactura;
                //peticion.DocumentLines = detalleProductos;
                //objData.OriginalRequest = peticion;
                //facturaEnviar.Data = objData;

                //facturaEnviar.Data.OriginalRequest.AccountingCustomerParty.Party = Cliente.Party;
                //facturaEnviar.Data.OriginalRequest.TaxTotal = impuestosFactura;
                //facturaEnviar.Data.OriginalRequest.LegalMonetaryTotal = subtotalesFactura;
                //facturaEnviar.Data.OriginalRequest.DocumentLines = detalleProductos; 
                #endregion

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
                double difRedondeo = 0;
                if (Math.Abs(TotalGravadoIva - double.Parse(_Valtotal.ToString()))>0)
                {
                    difRedondeo = Math.Abs(TotalGravadoIva - double.Parse(_Valtotal.ToString()));
                }
                if (difRedondeo>1.04)
                {
                    throw new Exception("El valor de Redondeos, es superior a 1.04"); 
                }
                ///<summary>
                ///Inicio de Totales de la Factura
                /// </summary
                Totales totalesTmp = new Totales()
                {
                    valorBruto = double.Parse(_Valtotal.ToString()),
                    valorAnticipos = double.Parse(_ValPagos.ToString()),
                    valorTotalSinImpuestos = double.Parse(_Valtotal.ToString()),
                    valorTotalConImpuestos = double.Parse(_Valtotal.ToString()),
                    valorNeto = double.Parse(_ValCobrar.ToString()),
                    valorRedondeo = difRedondeo.TomarDecimales(2)
                };
                double tasa = 0;
                string monedaDestino = String.Empty;
                DateTime fechaTrm = DateTime.Now;
                using (SqlConnection connX1=new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    connX1.Open();
                    string qryTasa = "SELECT Trm,FechaTrm,MonedaOrigen FROM facFacturaTasaTrm WHERE idFactura=@idFactura";
                    SqlCommand cmdTasa = new SqlCommand(qryTasa,connX1);
                    cmdTasa.Parameters.Add("@idFactura",SqlDbType.Int).Value=nroFactura;
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
                tasaInf.monedaDestino= monedaDestino;
                tasaInf.fecha = formatoWrk.Split('T')[0];
                facturaEnviar.TRM = tasaInf;
                
                documentoF2.totales = totalesTmp;
                logFacturas.Info("Numero de Productos Procesados, para JSon:" + detalleProductos.Count);
                //****************************************
				try
				{
                    //string urlConsumo = Properties.Settings.Default.urlFacturaElectronica + Properties.Settings.Default.recursoFacturaE;
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
                    StreamReader LecturaRespuesta = new StreamReader(response.GetResponseStream());
                    string strsb = LecturaRespuesta.ReadToEnd();
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
                                                        cmdInsertarAdvertencia.Parameters["@CodError"].Value = itemAdv.codigo;
                                                        //cmdInsertarAdvertencia.Parameters["@consecutivo"].Value = consecutivo;
                                                        cmdInsertarAdvertencia.Parameters["@FecRegistro"].Value = DateTime.Now;
                                                        cmdInsertarAdvertencia.Parameters["@DescripcionError"].Value = itemAdv.mensaje;
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
