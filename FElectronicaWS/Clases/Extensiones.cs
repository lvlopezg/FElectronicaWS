using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public static class Extensiones
    {
        public static decimal TomarDecimales(this decimal value, int nroDecimales)
        {
            if (nroDecimales < 0)
                throw new ArgumentException("El numero de Decimales Debe Ser Mayor o Igual a 0.");

            var modifier = Convert.ToDecimal(0.5 / Math.Pow(10, nroDecimales));
            return Math.Round(value >= 0 ? value - modifier : value + modifier, nroDecimales);
        }

        public static double TomarDecimales(this double value, int nroDecimales)
        {
            if (nroDecimales < 0)
                throw new ArgumentException("El numero de Decimales DEbe Ser Mayor o Igual a 0.");

            var modifier = Convert.ToDouble(0.5 / Math.Pow(10, nroDecimales));
            return Math.Round(value >= 0 ? value - modifier : value + modifier, nroDecimales);
        }
    }
}