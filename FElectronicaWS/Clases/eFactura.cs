using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FElectronicaWS.Clases
{
	/// <summary>
	///  clase raiz. de la factura
	/// </summary>

	public class eFactura
	{
		//[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
		public Data Data { get; set; }
	}
	/// <summary>
	/// Nodo principal de la factura
	/// </summary>
	public class Data
	{
		/// <summary>
		/// url del pedf
		/// </summary>
		[JsonProperty("UrlPdf", NullValueHandling = NullValueHandling.Ignore)]
		public string UrlPdf { get; set; }

		/// <summary>
		///  Peticion Original
		/// </summary>
		/// 
		[JsonProperty("originalRequest", NullValueHandling = NullValueHandling.Ignore)]
		//[JsonProperty("originalRequest")]
		public OriginalRequest OriginalRequest { get; set; }
	}
	/// <summary>
	/// Clase de la peticion
	/// </summary>
	public class OriginalRequest
	{
		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("IdBusiness", NullValueHandling = NullValueHandling.Ignore)]
		public string IdBusiness { get; set; }

		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }

		[JsonProperty("DocumentType", NullValueHandling = NullValueHandling.Ignore)]
		public string DocumentType { get; set; }

		[JsonProperty("BroadCastDate", NullValueHandling = NullValueHandling.Ignore)]
		public string BroadCastDate { get; set; }

		[JsonProperty("BroadCastTime", NullValueHandling = NullValueHandling.Ignore)]
		public string BroadCastTime { get; set; }

		[JsonProperty("IdMotivo", NullValueHandling = NullValueHandling.Ignore)]
		public string IdMotivo { get; set; }

		[JsonProperty("BillType", NullValueHandling = NullValueHandling.Ignore)]
		public string BillType { get; set; }

		[JsonProperty("Note", NullValueHandling = NullValueHandling.Ignore)]
		public string Note { get; set; }

		[JsonProperty("EventName", NullValueHandling = NullValueHandling.Ignore)]
		public string EventName { get; set; }

		[JsonProperty("InvoiceId", NullValueHandling = NullValueHandling.Ignore)]
		public string InvoiceId { get; set; }

		[JsonProperty("Prefix", NullValueHandling = NullValueHandling.Ignore)]
		public string Prefix { get; set; }

		[JsonProperty("AdditionalInformation", NullValueHandling = NullValueHandling.Ignore)]
		public List<AdditionalInformation> AdditionalInformation { get; set; }

		[JsonProperty("SellerSupplierParty", NullValueHandling = NullValueHandling.Ignore)]
		public List<SellerSupplierParty> SellerSupplierParty { get; set; }

		[JsonProperty("AccountingCustomerParty", NullValueHandling = NullValueHandling.Ignore)]
		public AccountingCustomerParty AccountingCustomerParty { get; set; }

		[JsonProperty("TaxTotal", NullValueHandling = NullValueHandling.Ignore)]
		public List<TaxTotal> TaxTotal { get; set; }

		[JsonProperty("LegalMonetaryTotal", NullValueHandling = NullValueHandling.Ignore)]
		public LegalMonetaryTotal LegalMonetaryTotal { get; set; }

/* 		[JsonProperty("extensionesSalud", NullValueHandling = NullValueHandling.Ignore)]
		public List<extensionSalud> extensionesSalud { get; set; } */


		[JsonProperty("DocumentLines", NullValueHandling = NullValueHandling.Ignore)]
		public List<DocumentLine> DocumentLines { get; set; }
	}
	/// <summary>
	/// 
	/// </summary>
	public class AdditionalInformation
	{
		[JsonProperty("Position", NullValueHandling = NullValueHandling.Ignore)]
		public string Position { get; set; }
		[JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
		public string Value { get; set; }
	}
	public class SellerSupplierParty
	{
		[JsonProperty("AdditionalAccountID", NullValueHandling = NullValueHandling.Ignore)]
		public string AdditionalAccountID { get; set; }
		[JsonProperty("Contract", NullValueHandling = NullValueHandling.Ignore)]
		public Contract Contract { get; set; }
		[JsonProperty("Party", NullValueHandling = NullValueHandling.Ignore)]
		public Party Party { get; set; }
	}
	public class Contract
	{
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }
		[JsonProperty("IssueDate", NullValueHandling = NullValueHandling.Ignore)]
		public string IssueDate { get; set; }
		[JsonProperty("ContractType", NullValueHandling = NullValueHandling.Ignore)]
		public string ContractType { get; set; }
	}
	public class Party
	{
		[JsonProperty("PartyIdentification", NullValueHandling = NullValueHandling.Ignore)]
		public PartyIdentification PartyIdentification { get; set; }
		[JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }
		[JsonProperty("PhysicalLocation", NullValueHandling = NullValueHandling.Ignore)]
		public PhysicalLocation PhysicalLocation { get; set; }
		[JsonProperty("PartyTaxScheme", NullValueHandling = NullValueHandling.Ignore)]
		public PartyTaxScheme PartyTaxScheme { get; set; }
		[JsonProperty("Person", NullValueHandling = NullValueHandling.Ignore)]
		public Person Person { get; set; }
	}
	public class PartyIdentification
	{
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }
		[JsonProperty("schemeID", NullValueHandling = NullValueHandling.Ignore)]
		public string SchemeID { get; set; }
	}
	public class PhysicalLocation
	{
		[JsonProperty("Address", NullValueHandling = NullValueHandling.Ignore)]
		public Address Address { get; set; }
	}
	public class Address
	{
		[JsonProperty("Line", NullValueHandling = NullValueHandling.Ignore)]
		public string Line { get; set; }
		[JsonProperty("CityName", NullValueHandling = NullValueHandling.Ignore)]
		public object CityName { get; set; }
		[JsonProperty("CountryCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CountryCode { get; set; }
		[JsonProperty("CitySubdivisionName", NullValueHandling = NullValueHandling.Ignore)]
		public string CitySubdivisionName { get; set; }
		[JsonProperty("Department", NullValueHandling = NullValueHandling.Ignore)]
		public string Department { get; set; }
	}
	public class PartyTaxScheme
	{
		[JsonProperty("TaxLevelCode", NullValueHandling = NullValueHandling.Ignore)]
		public string TaxLevelCode { get; set; }
	}
	public class Person
	{
		[JsonProperty("FirstName", NullValueHandling = NullValueHandling.Ignore)]
		public string FirstName { get; set; }
		[JsonProperty("FamilyName", NullValueHandling = NullValueHandling.Ignore)]
		public string FamilyName { get; set; }
		[JsonProperty("MiddleName", NullValueHandling = NullValueHandling.Ignore)]
		public string MiddleName { get; set; }
	}
	public class AccountingCustomerParty
	{
		[JsonProperty("Party", NullValueHandling = NullValueHandling.Ignore)]
		public Party1 Party { get; set; }
		[JsonProperty("AdditionalAccountID", NullValueHandling = NullValueHandling.Ignore)]
		public string AdditionalAccountID { get; set; }
	}
	public class Party1
	{
		[JsonProperty("PartyIdentification", NullValueHandling = NullValueHandling.Ignore)]
		public PartyIdentification PartyIdentification { get; set; }
		[JsonProperty("Name")]
		public string Name { get; set; }
		[JsonProperty("PhysicalLocation", NullValueHandling = NullValueHandling.Ignore)]
		public PhysicalLocation1 PhysicalLocation { get; set; }
		[JsonProperty("PartyTaxScheme", NullValueHandling = NullValueHandling.Ignore)]
		public PartyTaxScheme PartyTaxScheme { get; set; }
		[JsonProperty("Person", NullValueHandling = NullValueHandling.Ignore)]
		public Person1 Person { get; set; }
	}
	// public class extensionSalud
	// {
	// 	[JsonProperty("codigoPrestador", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string codigoPrestador{get;set;}

	// 	[JsonProperty("tipoDocumentoIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string tipoDocumentoIdentificacion{get;set;}
		
	// 	[JsonProperty("numeroIdentificacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroIdentificacion{get;set;}
		
	// 	[JsonProperty("primerApellido", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string primerApellido{get;set;}

	// 	[JsonProperty("segundoApellido", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string segundoApellido{get;set;}

	// 	[JsonProperty("primerNombre", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string primerNombre{get;set;}

	// 	[JsonProperty("otrosNombres", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string otrosNombres{get;set;}

	// 	[JsonProperty("tipoDeUsuario", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string tipoDeUsuario{get;set;}

	// 	[JsonProperty("modalidadesContratacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string modalidadesContratacion{get;set;}

	// 	[JsonProperty("cobertura", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string cobertura{get;set;}
		
	// 	[JsonProperty("numeroAutorizacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroAutorizacion{get;set;}

	// 	[JsonProperty("numeroMIPRES", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroMIPRES{get;set;}

	// 	[JsonProperty("numeroIdPrescripcion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroIdPrescripcion{get;set;}

	// 	[JsonProperty("numeroContrato", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroContrato { get; set; }

	// 	[JsonProperty("numeroPoliza", NullValueHandling = NullValueHandling.Ignore)]
	// 	public string numeroPoliza { get; set; }

	// 	[JsonProperty("fechaInicioFacturacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Date fechaInicioFacturacion { get; set; }

	// 	[JsonProperty("fechaFinFacturacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Date fechaFinFacturacion { get; set; }

	// 	[JsonProperty("copago", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Int64 copago { get; set; }

	// 	[JsonProperty("cuotaModeradora", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Int64 cuotaModeradora { get; set; }

	// 	[JsonProperty("cuotaRecuperacion", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Int64 cuotaRecuperacion { get; set; }

	// 	[JsonProperty("pagosCompartidos", NullValueHandling = NullValueHandling.Ignore)]
	// 	public Int64 pagosCompartidos { get; set; }

	// }
	public class PhysicalLocation1
	{
		[JsonProperty("Address", NullValueHandling = NullValueHandling.Ignore)]
		public Address1 Address { get; set; }
	}
	public class Address1
	{
		[JsonProperty("Line", NullValueHandling = NullValueHandling.Ignore)]
		public string Line { get; set; }
		[JsonProperty("CountryCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CountryCode { get; set; }
		[JsonProperty("Department", NullValueHandling = NullValueHandling.Ignore)]
		public string Department { get; set; }
		[JsonProperty("CitySubdivisionName", NullValueHandling = NullValueHandling.Ignore)]
		public string CitySubdivisionName { get; set; }
		[JsonProperty("CityName", NullValueHandling = NullValueHandling.Ignore)]
		public string CityName { get; set; }
	}
	public class Person1
	{
		[JsonProperty("FirstName", NullValueHandling = NullValueHandling.Ignore)]
		public string FirstName { get; set; }

		[JsonProperty("FamilyName", NullValueHandling = NullValueHandling.Ignore)]
		public string FamilyName { get; set; }

		[JsonProperty("MiddleName", NullValueHandling = NullValueHandling.Ignore)]
		public string MiddleName { get; set; }

		[JsonProperty("Telephone", NullValueHandling = NullValueHandling.Ignore)]
		public string Telephone { get; set; }

		[JsonProperty("Email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }
	}
	public class TaxTotal
	{
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }

		[JsonProperty("TaxAmount", NullValueHandling = NullValueHandling.Ignore)]
		public TaxAmount TaxAmount { get; set; }

		[JsonProperty("TaxEvidenceIndicator", NullValueHandling = NullValueHandling.Ignore)]
		public string TaxEvidenceIndicator { get; set; }

		[JsonProperty("Percent", NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)] //DefaultValueHandling = DefaultValueHandling.Ignore,
		public int? Percent { get; set; }

		[JsonProperty("TaxableAmount", NullValueHandling = NullValueHandling.Ignore)]
		public TaxableAmount TaxableAmount { get; set; }
	}
	public class TaxAmount
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public int Amount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class TaxableAmount
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount { get; set; } // se cambio de int a decimal, para factura internacional
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class LegalMonetaryTotal
	{
		[JsonProperty("LineExtensionAmount", NullValueHandling = NullValueHandling.Ignore)]
		public LineExtensionAmount LineExtensionAmount { get; set; }
		[JsonProperty("TaxExclusiveAmount", NullValueHandling = NullValueHandling.Ignore)]
		public TaxExclusiveAmount TaxExclusiveAmount { get; set; }
		[JsonProperty("PayableAmount", NullValueHandling = NullValueHandling.Ignore)]
		public PayableAmount PayableAmount { get; set; }
	}
	public class LineExtensionAmount
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class TaxExclusiveAmount
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class PayableAmount
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class DocumentLine
	{
		[JsonProperty("TypeLine", NullValueHandling = NullValueHandling.Ignore)]
		public int TypeLine { get; set; }
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public Int16 ID { get; set; }
		[JsonProperty("InvoicedQuantity", NullValueHandling = NullValueHandling.Ignore)]
		public int InvoicedQuantity { get; set; }
		[JsonProperty("LineExtensionAmount", NullValueHandling = NullValueHandling.Ignore)]
		public LineExtensionAmount LineExtensionAmount { get; set; }
		[JsonProperty("Item", NullValueHandling = NullValueHandling.Ignore)]
		public Item Item { get; set; }
		[JsonProperty("Price", NullValueHandling = NullValueHandling.Ignore)]
		public Price Price { get; set; }
		[JsonProperty("TaxLine", NullValueHandling = NullValueHandling.Ignore)]
		public List<TaxLine> TaxLine { get; set; }
		[JsonProperty("SellersItemIdentification", NullValueHandling = NullValueHandling.Ignore)]
		public SellersItemIdentification SellersItemIdentification { get; set; }
		[JsonProperty("AdditionalInformation", NullValueHandling = NullValueHandling.Ignore)]
		public List<AdditionalInformation> AdditionalInformation { get; set; }
		[JsonProperty("SubDetalle", NullValueHandling = NullValueHandling.Ignore)]
		public List<SubDetalle> SubDetalle { get; set; }
	}
	public class Item
	{
		[JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }
	}
	public class Price
	{
		[JsonProperty("Amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class TaxLine
	{
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }
		[JsonProperty("TaxAmount", NullValueHandling = NullValueHandling.Ignore)]
		public TaxAmount TaxAmount { get; set; }

		[JsonProperty("Percent", NullValueHandling = NullValueHandling.Ignore)]
		public int Percent { get; set; }

		[JsonProperty("TaxPerUnit", NullValueHandling = NullValueHandling.Ignore)]
		public TaxPerUnit TaxPerUnit { get; set; }
	}
	public class TaxPerUnit
	{
		[JsonProperty("BaseUnitMeasure", NullValueHandling = NullValueHandling.Ignore)]
		public int BaseUnitMeasure { get; set; }
		[JsonProperty("UnitCode", NullValueHandling = NullValueHandling.Ignore)]
		public string UnitCode { get; set; }
		[JsonProperty("PerUnitAmount", NullValueHandling = NullValueHandling.Ignore)]
		public int PerUnitAmount { get; set; }
		[JsonProperty("Currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }
	}
	public class SellersItemIdentification
	{
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }
		[JsonProperty("ExtendedID", NullValueHandling = NullValueHandling.Ignore)]
		public string ExtendedID { get; set; }
	}
	public class SubDetalle
	{
		[JsonProperty("TypeLine", NullValueHandling = NullValueHandling.Ignore)]
		public string TypeLine { get; set; }
		[JsonProperty("ID", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }
		[JsonProperty("InvoicedQuantity", NullValueHandling = NullValueHandling.Ignore)]
		public int InvoicedQuantity { get; set; }
		[JsonProperty("LineExtensionAmount", NullValueHandling = NullValueHandling.Ignore)]
		public LineExtensionAmount LineExtensionAmount { get; set; }
		[JsonProperty("Item", NullValueHandling = NullValueHandling.Ignore)]
		public Item Item { get; set; }
		[JsonProperty("Price", NullValueHandling = NullValueHandling.Ignore)]
		public Price Price { get; set; }
		[JsonProperty("TaxLine", NullValueHandling = NullValueHandling.Ignore)]
		public List<TaxLine> TaxLine { get; set; }
		[JsonProperty("SellersItemIdentification", NullValueHandling = NullValueHandling.Ignore)]
		public SellersItemIdentification SellersItemIdentification { get; set; }
		[JsonProperty("AdditionalInformation", NullValueHandling = NullValueHandling.Ignore)]
		public List<AdditionalInformation> AdditionalInformation { get; set; }
	}

}
