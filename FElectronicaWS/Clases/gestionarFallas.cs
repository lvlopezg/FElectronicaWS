using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
	public class gestionarFallas
	{
		public gestionarFallas(respuestaEntregaError respuestaError, int idcliente, int nroAtencion)
		{
			string qryAlmacenarError = "INSERT INTO gestionFacturasError (nroFactura,fechaReg,idcliente,nroAtencion,exitoso,identificador,resultado,aplicacion,errores) VALUES(@nroFactura,@fechaReg,@idcliente,@nroAtencion,@exitoso,@identificador,@resultado,@aplicacion,@errores)";
			using (SqlConnection conx = new SqlConnection(Properties.Settings.Default.DBConexion))
			{
				conx.Open();
				SqlCommand cmdAlmacenarError = new SqlCommand(qryAlmacenarError, conx);
				cmdAlmacenarError.Parameters.Add("@nroFactura");
				cmdAlmacenarError.Parameters.Add("@fechaReg");
				cmdAlmacenarError.Parameters.Add("@idcliente");
				cmdAlmacenarError.Parameters.Add("@nroAtencion");
				cmdAlmacenarError.Parameters.Add("@exitoso");
				cmdAlmacenarError.Parameters.Add("@identificador");
				cmdAlmacenarError.Parameters.Add("@resultado");
				cmdAlmacenarError.Parameters.Add("@aplicacion");
				cmdAlmacenarError.Parameters.Add("@errores");

				string identificador = respuestaError.Identificador;
				bool exitoso = respuestaError.EsExitoso;
				string resultado = respuestaError.Resultado;
				foreach (Error item in respuestaError.Errors)
				{
					string aplicacion = item.Aplication;
					string textoErrores = string.Empty;
					List<Error> errores = respuestaError.Errors;
					foreach (Error itemError in errores)
					{
						textoErrores = $"{respuestaError.Resultado}  {itemError.Errors.Error}";
					}
				}
				//cmdAlmacenarError.Parameters.Add();

			}

		}
	}
}