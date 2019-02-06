using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases.NotaDebito
{
    public class notaDebitoJson
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonProperty("UrlPdf")]
        public string UrlPdf { get; set; }
        [JsonProperty("originalRequest")]
        public OriginalRequest OriginalRequest { get; set; }
    }
    public class OriginalRequest
    {
        [JsonProperty("IdBusiness")]
        public string IdBusiness { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("DocumentType")]
        public string DocumentType { get; set; }

        [JsonProperty("BroadCastDate")]
        public string BroadCastDate { get; set; }

        [JsonProperty("BroadCastTime")]
        public string BroadCastTime { get; set; }

        [JsonProperty("IdMotivo")]
        public string IdMotivo { get; set; }

        [JsonProperty("Note")]
        public string Note { get; set; }

        [JsonProperty("InvoiceId")]
        public string InvoiceId { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        [JsonProperty("NumberRelatedDocument")]
        public string NumberRelatedDocument { get; set; }

        [JsonProperty("AccountingCustomerParty")]
        public AccountingCustomerParty AccountingCustomerParty { get; set; }

        [JsonProperty("TaxTotal")]
        public List<TaxTotal> TaxTotal { get; set; }

        [JsonProperty("LegalMonetaryTotal")]
        public LegalMonetaryTotal LegalMonetaryTotal { get; set; }

        [JsonProperty("DocumentLines")]
        public List<DocumentLine> DocumentLines { get; set; }
    }
    public class AccountingCustomerParty
    {
        [JsonProperty("Party")]
        public Party Party { get; set; }
        [JsonProperty("AdditionalAccountID")]
        public string AdditionalAccountID { get; set; }
    }
    public class Party
    {
        [JsonProperty("PartyIdentification")]
        public PartyIdentification PartyIdentification { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("PhysicalLocation")]
        public PhysicalLocation PhysicalLocation { get; set; }

        [JsonProperty("PartyTaxScheme")]
        public PartyTaxScheme PartyTaxScheme { get; set; }

        [JsonProperty("Person")]
        public Person Person { get; set; }
    }
    public class PartyIdentification
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("schemeID")]
        public string SchemeID { get; set; }
    }
    public class PhysicalLocation
    {
        [JsonProperty("Address")]
        public Address Address { get; set; }
    }
    public class Address
    {
        [JsonProperty("Line")]
        public string Line { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("Department")]
        public string Department { get; set; }

        [JsonProperty("CitySubdivisionName")]
        public string CitySubdivisionName { get; set; }

        [JsonProperty("CityName")]
        public string CityName { get; set; }
    }
    public class PartyTaxScheme
    {
        [JsonProperty("TaxLevelCode")]
        public string TaxLevelCode { get; set; }
    }
    public class Person
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("FamilyName")]
        public string FamilyName { get; set; }

        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        [JsonProperty("Telephone")]
        public string Telephone { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }
    }
    public class TaxTotal
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("TaxAmount")]
        public string TaxAmount { get; set; }

        [JsonProperty("TaxEvidenceIndicator")]
        public string TaxEvidenceIndicator { get; set; }

        [JsonProperty("TaxableAmount")]
        public string TaxableAmount { get; set; }

        [JsonProperty("Percent")]
        public string Percent { get; set; }
    }
    public class LegalMonetaryTotal
    {
        [JsonProperty("LineExtensionAmount")]
        public string LineExtensionAmount { get; set; }

        [JsonProperty("TaxExclusiveAmount")]
        public string TaxExclusiveAmount { get; set; }

        [JsonProperty("PayableAmount")]
        public string PayableAmount { get; set; }
    }
    public class DocumentLine
    {
        [JsonProperty("TypeLine")]
        public string TypeLine { get; set; }

        [JsonProperty("Item")]
        public Item Item { get; set; }

        [JsonProperty("PriceAmount")]
        public string PriceAmount { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("InvoicedQuantity")]
        public string InvoicedQuantity { get; set; }

        [JsonProperty("LineExtensionAmount")]
        public string LineExtensionAmount { get; set; }

        [JsonProperty("TaxLine")]
        public List<TaxLine> TaxLine { get; set; }

        [JsonProperty("SubDetalles")]
        public List<SubDetalle> SubDetalles { get; set; }
    }
    public class Item
    {
        [JsonProperty("Description")]
        public string Description { get; set; }
    }
    public class TaxLine
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("TaxAmount")]
        public string TaxAmount { get; set; }

        [JsonProperty("Percent")]
        public string Percent { get; set; }
    }
    public class SubDetalle
    {
        [JsonProperty("TypeLine")]
        public string TypeLine { get; set; }
        [JsonProperty("Item")]
        public Item Item { get; set; }
        [JsonProperty("Price")]
        public Price Price { get; set; }
        [JsonProperty("Tax")]
        public List<object> Tax { get; set; }
    }
    public class Price
    {
        [JsonProperty("PriceAmount")]
        public string PriceAmount { get; set; }
    }

}