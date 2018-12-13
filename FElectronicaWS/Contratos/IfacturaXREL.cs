using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IfacturaXREL" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IfacturaXREL
    {
        [OperationContract]
        string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura);
    }
}
