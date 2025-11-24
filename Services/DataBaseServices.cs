using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace Services
{
    public static class DataBaseServices
    {
        private static readonly string CONNECTION_STRING = "Data Source=DESKTOP-4OC5GG6\\SQLEXPRESS;Initial Catalog = Agrominds; Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static string getConnectionString()
        {
            return CONNECTION_STRING;
        }
        public static bool BackupDatabase()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
                {
                    connection.Open();
                    string backupQuery = $"BACKUP Database Agrominds TO DISK = 'D:\\backups\\MiBackup - {DateTime.Now.ToString("yyyyMMddHHmmss")}.bak'";
                    using (SqlCommand command = new SqlCommand(backupQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            } catch (Exception ex) { throw ex; }
        }
        public static bool RestoreDatabase(string pPath)
        {
            try
            {
                //Creamos una conexion a la base de datos
                using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
                {
                    //Abrir la conexión
                    connection.Open();
                    //Ponemos la base de datos en estado offline
                    SqlCommand command = new SqlCommand("alter database Agrominds set offline with rollback immediate", connection);
                    command.ExecuteNonQuery();
                    try
                    {
                        //Intentamos restaurarla base de datos
                        command = new SqlCommand($"RESTORE DATABASE Agrominds FROM DISK = '{pPath}'", connection);
                        command.ExecuteNonQuery();
                    }
                    catch(Exception)
                    {
                        // Intentamos poner la base de datos online nuevamente
                        SqlCommand onlineCommand = new SqlCommand("ALTER DATABASE Agrominds SET ONLINE", connection);
                        onlineCommand.ExecuteNonQuery();
                        return false;
                    }
                    command = new SqlCommand("alter database Agrominds set online", connection);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la restauración: {ex.Message}");
                return false;
            }
        }
    }
}
