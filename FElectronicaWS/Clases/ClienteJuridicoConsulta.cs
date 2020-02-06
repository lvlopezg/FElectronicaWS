using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public class ClienteJuridicoConsulta : ClienteJuridico
    {
        public int idContrato { get ; set; }
        public DateTime FecFactura { get;set; }
        public int IdUsuarioR { get;set; }
        public string nom_usua { get;set; }
        public string NumDocumento { get;set; }
        public byte IdTipoDoc { get;set; }
        public string NomTercero { get;set; }
        public int IdTercero { get;set; }
        public string idRegimen { get;set; }
        public short IdNaturaleza { get;set; }
        public string direccion { get;set; }
        public string ciudad { get;set; }
        public int idLugar { get;set; }
        public string codigoPais { get;set; }
        public string codMunicipio { get;set; }
        public string telefono { get;set; }
        public string Codigo_depto { get;set; }
        public string Nombre_Depto { get;set; }
        public string Codigo_Municipio { get;set; }
        public string Nom_Municipio { get;set; }
        public string cuenta_correo { get;set; }
        public string NroDoc_Cliente { get;set; }
        public byte TipoDoc_Cliente { get;set; }
    }
}