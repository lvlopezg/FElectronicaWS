using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
	public class respuestaEntregaError
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
		public List<string> Errors { get; set; }

		[JsonProperty("Aplication")]
		public string Aplication { get; set; }
	}

}