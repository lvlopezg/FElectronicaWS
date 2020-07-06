using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Contratos
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "INotaCreditoEspecial" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface INotaCreditoEspecial
    {
        [OperationContract]
        string getData(int nroNotaCredito, int idCliente, int nroAtencion, string monedaNota, int nroFactura, string urlPdfNotaCredito);
    }
}
