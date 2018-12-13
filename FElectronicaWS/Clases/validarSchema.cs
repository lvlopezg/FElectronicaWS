using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Web;


namespace FElectronicaWS.Clases
{
    public class validarSchema
    {

        public bool validar(string facturaJSon)
        {
            bool rpta = false;
            //////try
            //////{
            //////    //string ruta = HttpContext.Current.Server.MapPath(@"C:\Desarrollo_TEST\FElectronicaWS\FElectronicaWS\Schema\schemaFacturaElectronica.json"); //     "../Schema/schemaFacturaElectronica.json"
            //////    string ruta = @"C:\Desarrollo_TEST\FElectronicaWS\FElectronicaWS\Schema\schemaFacturaElectronica.json";
            //////    //StreamReader schemaValidar = new StreamReader(ruta);
            //////    //string stringJson = schemaValidar.ReadToEnd();
            //////    //////JSchema schema = JSchema.Parse(File.ReadAllText(ruta));
            //////    //////JObject factura = JObject.Parse(facturaJSon);
            //////    //////IList<ValidationError> errores; //= new IList<ValidationError>();
            //////    rpta = factura.IsValid(schema, out errores);
            //////}
            //////catch (HttpException httpExp1)
            //////{
            //////    throw new System.Exception("Se ha presentado una HttpException, Validando Schema de La Factura" + httpExp1.Message, httpExp1.InnerException);
            //////}
            //////catch (JSchemaException schExp)
            //////{
            //////    throw new System.Exception("Se ha presentado una JSchemaException, Validando Schema de La Factura" + schExp.Message, schExp.InnerException);
            //////}
            //////catch (System.Exception ex)
            //////{
            //////    throw new System.Exception("Se ha presentado una Exception, Validando Schema de La Factura" + ex.Message, ex.InnerException);
            //////}
            return rpta;
        }
    }
}
