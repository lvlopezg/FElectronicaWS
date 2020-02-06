using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
	public class RptaErrorReproceso
	{
		[JsonProperty("Resultado")]
		public string Resultado { get; set; }
		[JsonProperty("EsExitoso")]
		public bool EsExitoso { get; set; }
		[JsonProperty("Identificador")]
		public string Identificador { get; set; }
		[JsonProperty("Errors")]
		public List<Error> Errors { get; set; }
	}
	public class Error
	{
		[JsonProperty("Errors")]
		public Errors Errors { get; set; }
		[JsonProperty("Aplication")]
		public string Aplication { get; set; }
	}
	public class Errors
	{
		[JsonProperty("Error")]
		public string Error { get; set; }
		[JsonProperty("PDF")]
		public string PDF { get; set; }
		[JsonProperty("ZIP")]
		public string ZIP { get; set; }
		[JsonProperty("CUFE")]
		public string CUFE { get; set; }
		[JsonProperty("QR")]
		public string QR { get; set; }
	}

}