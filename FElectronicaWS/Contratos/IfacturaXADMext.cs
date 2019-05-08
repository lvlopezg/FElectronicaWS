using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Contratos
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IfacturaXADMext" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IfacturaXADMext
    {
        [OperationContract]
        string GetData(Int32 nroFactura, Int32 idCliente, string urlPdfFactura, string moneda);
    }
}
