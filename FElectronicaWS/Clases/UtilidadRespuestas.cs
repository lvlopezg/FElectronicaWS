using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FElectronicaWS.Clases
{
    public static class UtilidadRespuestas
    {
        private static Logger logFacturas = LogManager.GetCurrentClassLogger();
        //private string valorRpta = string.Empty;

        public static string insertarErrorND(string tipoNota,Int32 NroND,string codigoError,string mensaje,DateTime fechaError, List<ErroresItem> DetalleError)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.DBConexion))
                {
                    conn.Open();
                    string qryInsertaError = @"INSERT INTO facNotaTempWEBServiceError (IdTipoNota,IdNota,CodError,DescripcionError,FecRegistro) 
VALUES(@idTipo,@IdNota, @CodError, @DescripcionError, @FecRegistro)";
                    SqlCommand cmdInsertarError = new SqlCommand(qryInsertaError, conn);
                    cmdInsertarError.Parameters.AddWithValue("@idTipo", SqlDbType.VarChar).Value = tipoNota;
                    cmdInsertarError.Parameters.Add("@IdNota", SqlDbType.Int).Value = NroND;
                    cmdInsertarError.Parameters.Add("@CodError", SqlDbType.VarChar).Value = codigoError;
                    cmdInsertarError.Parameters.Add("@DescripcionError", SqlDbType.NVarChar).Value = mensaje;
                    cmdInsertarError.Parameters.Add("@FecRegistro", SqlDbType.DateTime).Value = fechaError;
                    if (cmdInsertarError.ExecuteNonQuery() > 0)
                    {
                        string qryDetErr = @"INSERT INTO facNotaTempWSErrorDetalle (IdNota,CodError,consecutivo,FecRegistro,DescripcionError) 
VALUES(@IdNota, @CodError, @consecutivo, @FecRegistro, @DescripcionError)";
                        SqlCommand cmdDetErr = new SqlCommand(qryDetErr, conn);
                        cmdDetErr.Parameters.Add("@IdNota", SqlDbType.Int);
                        cmdDetErr.Parameters.Add("@CodError", SqlDbType.VarChar);
                        cmdDetErr.Parameters.Add("@consecutivo", SqlDbType.Int);
                        cmdDetErr.Parameters.Add("@FecRegistro", SqlDbType.DateTime);
                        cmdDetErr.Parameters.Add("@DescripcionError", SqlDbType.NVarChar);
                        List<ErroresItem> listaErrores = new List<ErroresItem>();
                        int consecutivo = 1;
                        foreach (ErroresItem itemErr in DetalleError)
                        {
                            cmdDetErr.Parameters["@IdNota"].Value = NroND;
                            cmdDetErr.Parameters["@CodError"].Value = itemErr.codigo;
                            cmdDetErr.Parameters["@consecutivo"].Value = consecutivo;
                            cmdDetErr.Parameters["@FecRegistro"].Value = fechaError;
                            cmdDetErr.Parameters["@DescripcionError"].Value = itemErr.mensaje;
                            if (cmdDetErr.ExecuteNonQuery() > 0)
                            {
                                logFacturas.Info($"Se Inserta Detalle de Errores Notas {tipoNota} :codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                                consecutivo++;
                            }
                            else
                            {
                                logFacturas.Info($"No es Posible Insertar Detalle de Errores Nota {tipoNota}: Codigo{itemErr.codigo} Mensaje:{itemErr.mensaje}");
                            }
                        }
                    }
                    else
                    {
                        return $"99-No fue posible Insertar el Error en la Tabla: facNotaTempWEBServiceError."; ;
                    }
                }
                return  $"99-Informacion de Nota No Encontrada."; ;
            }
            catch (Exception)
            {
                return $"Se ha presentado un a excepcion, Insertando errores en las tablas de Errores de Notas";
            }
        }
    }
}