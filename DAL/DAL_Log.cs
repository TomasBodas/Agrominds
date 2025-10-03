using Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Models;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DAL
{
    public class DAL_Log
    {
        private static readonly string CONNECTION_STRING = DataBaseServices.getConnectionString();
        List<Log> listlog = new List<Log>();
        public List<Log> getLog()
        {
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {

                string select = "SELECT b.id, b.fecha, u.email, b.modulo, b.descripcion FROM bitacora b JOIN usuario u ON b.fk_usuario_bitacora = u.id";
                SqlCommand command = new SqlCommand(select, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Log log = new Log
                        {
                             id = reader.GetInt32(0),
                             Fecha = reader.GetDateTime(1),
                             User = reader.GetString(2),
                             Modulo = reader.GetString(3),
                             Descripcion = reader.GetString(4)       
                        };
                        listlog.Add(log);                
                            
                    }  
                } 
            }
            return listlog;
        }
    }
}
