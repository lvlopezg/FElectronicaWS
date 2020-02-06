using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Contratos
{
    [ServiceContract]
    public interface IfacturaXActParticular
    {
        [OperationContract]
        string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura);
    }
}
