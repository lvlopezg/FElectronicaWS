using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace FElectronicaWS.Contratos
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "INotaDebitoEspecial" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface INotaDebitoEspecial
    {
        [OperationContract]
        void DoWork();
    }
}
