using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
  public static class UtilidadesFactura
  {
    public static string[ ] separarApellidos(string apellidos)
    {
      string[ ] Apellidos = apellidos.Split(' ');
      string primerApellido = string.Empty;
      string segundoApellido = string.Empty;
      for (int i = 0; i <= Apellidos.Length; i++)
      {
        if (Apellidos[i].Length < 3)
        {
          primerApellido = primerApellido + " " + Apellidos[i];
        }
        else
        {
          primerApellido = primerApellido + " " + Apellidos[i];
          for (int j = i + 1; j < Apellidos.Length; j++)
          {
            segundoApellido = segundoApellido + " " + Apellidos[j];
          }
          i = Apellidos.Length;
        }

      }
      Apellidos[0] = primerApellido;
      if (Apellidos.Length > 1)
      {
        Apellidos[1] = segundoApellido;
      }
      return Apellidos;
    }

    public static string[ ] separarNombres(string nombres)
    {
      string[ ] Nombres = nombres.Split(' ');
      string primerNombre = string.Empty;
      string segundoNombre = string.Empty;
      for (int i = 0; i <= Nombres.Length; i++)
      {
        if (Nombres[i].Length < 3)
        {
          primerNombre = primerNombre + " " + Nombres[i];
        }
        else
        {
          primerNombre = primerNombre + " " + Nombres[i];
          for (int j = i + 1; j < Nombres.Length; j++)
          {
            segundoNombre = segundoNombre + " " + Nombres[j];
          }
          i = Nombres.Length;
        }

      }
      Nombres[0] = primerNombre;
      if (Nombres.Length > 1)
      {
        Nombres[1] = segundoNombre;
      }
      return Nombres;
    }



  }
}