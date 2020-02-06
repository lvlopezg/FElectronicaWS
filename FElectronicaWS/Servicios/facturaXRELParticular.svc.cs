using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FElectronicaWS.Contratos;

namespace FElectronicaWS.Servicios
{

    public class facturaXRELParticular : IfacturaXRELParticular
    {

        public string GetData(int nroFactura, int idCliente, int nroAtencion, string urlPdfFactura)
        {
            throw new NotImplementedException();
        }
    }
}
