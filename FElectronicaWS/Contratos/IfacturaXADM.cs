using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IfacturaXADM" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IfacturaXADM
    {
        [OperationContract]
        string GetData(Int32 nroFactura, Int32 idCliente, Int32 nroAtencion, string urlPdfFactura,string moneda);
    }
}
