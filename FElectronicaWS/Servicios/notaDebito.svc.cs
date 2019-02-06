using FElectronicaWS.Contratos;
using System;
using FElectronicaWS.Clases;
using System.Collections.Generic;

namespace FElectronicaWS.Servicios
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "notaDebito" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione notaDebito.svc o notaDebito.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class notaDebito : InotaDebito
    {
        public string getData(int nroNota, string monedaNota, int nroFactura, int idPaciente, int nroAtencion, string urlNota)
        {

            Data objData = new Data();
            OriginalRequest objPeticion = new OriginalRequest();
            objData.UrlPdf = urlNota;
            AccountingCustomerParty Cliente = new AccountingCustomerParty();
            Party1 DetCliente = new Party1();
            PartyIdentification idenCliente = new PartyIdentification();
            AdditionalInformation infAdicional = new AdditionalInformation();
            objPeticion.BroadCastDate = DateTime.Now.Date.ToString();
            objPeticion.BroadCastTime = DateTime.Now.TimeOfDay.ToString();
            objPeticion.Currency = monedaNota;
            List<DocumentLine> lineasNota = new List<DocumentLine>();
            DocumentLine lineaNota = new DocumentLine();
            objPeticion.DocumentType = "";
            objPeticion.EventName = "";
            objPeticion.IdBusiness = "";
            objPeticion.IdMotivo = "";
            objPeticion.InvoiceId = "";
            LegalMonetaryTotal 


            //************** Detalle de Nota.
            objPeticion.DocumentLines.Add(lineaNota);
            throw new NotImplementedException();
        }
    }
}
