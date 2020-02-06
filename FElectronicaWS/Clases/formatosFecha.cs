using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FElectronicaWS.Clases
{
    public static class formatosFecha
    {
        public static string formatofecha(DateTime fechaIn)
        {
            string Mes = fechaIn.Month.ToString();
            if (fechaIn.Month < 10) { Mes = "0" + Mes; }

            string Dia = fechaIn.Day.ToString();
            if (fechaIn.Day < 10) { Dia = "0" + Dia; }

            string Hora = fechaIn.Hour.ToString();
            if (fechaIn.Hour < 10) { Hora = "0" + Hora; }

            string Minutos = fechaIn.Minute.ToString();
            if (fechaIn.Minute < 10) { Minutos = "0" + Minutos; }

            string Segundos = $"{fechaIn.Second.ToString()}";
            if (fechaIn.Second < 10) { Segundos = $"0{Segundos}"; }

            return $"{fechaIn.Year.ToString()}-{Mes}-{Dia}T{Hora}:{Minutos}:{Segundos}-05:00";

        }
    }
}
