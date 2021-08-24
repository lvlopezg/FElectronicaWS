using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
  public static class UtilidadesFactura
  {
    public static string[] separarApellidos(string apellidos)
    {
      List<string> Apellidos = new List<string>();
      foreach (string item in apellidos.Split(' '))
      {
        Apellidos.Add(item);
      }
      string primerApellido = string.Empty;
      string segundoApellido = string.Empty;

      for (int i = 0; i <= Apellidos.Count; i++)
      {
        if (Apellidos[i].Length < 3)
        {
          primerApellido = primerApellido + " " + Apellidos[i];
        }
        else
        {
          primerApellido = primerApellido + " " + Apellidos[i];
          for (int j = i + 1; j < Apellidos.Count; j++)
          {
            segundoApellido = segundoApellido + " " + Apellidos[j];
          }
          i = Apellidos.Count;
        }
      }
      Apellidos[0] = primerApellido;
      if (Apellidos.Count > 1)
      {
        Apellidos.Add(segundoApellido);
      }
      else
      {
        Apellidos.Add(" ");
      }
      return Apellidos.ToArray();
    }

    public static string[] separarNombres(string nombresWRK)
    {
      List<string> nombres= new List<string>();
      foreach (string item in nombresWRK.Split(' '))
      {
        nombres.Add(item);
      }
      string primerNombre = string.Empty;
      string segundoNombre = string.Empty;
      for (int i = 0; i <= nombres.Count; i++)
      {
        if (nombres[i].Length < 3)
        {
          primerNombre = primerNombre + " " + nombres[i];
        }
        else
        {
          primerNombre = primerNombre + " " + nombres[i];
          for (int j = i + 1; j < nombres.Count; j++)
          {
            segundoNombre = segundoNombre + " " + nombres[j];
          }
          i = nombres.Count;
        }
      }
      nombres[0] = primerNombre;
      if (nombres.Count > 1)
      {
        nombres.Add(segundoNombre);
      }
      else
      {
        nombres.Add(" ");
      }
      return nombres.ToArray();
    }



  }
}