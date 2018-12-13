using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace FElectronicaWS.Contratos
{

    [ServiceContract]
    public interface IfacturaXACT
    {

        [OperationContract]
        string GetData(Int32 nroFactura, Int32 idCliente, Int32 nroAtencion, string urlPdfFactura);

    }
}
