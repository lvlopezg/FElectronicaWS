using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Contratos
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "InotaDebito" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface InotaDebito
    {
        [OperationContract]
        string getData(Int32 nroNota, string monedaNota, Int32 nroFactura, Int32 idPaciente, Int32 nroAtencion, string urlNota);
    }
}
