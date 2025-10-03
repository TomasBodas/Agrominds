using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Models;
using System.Xml;
using Services.Perfiles;
using Services;
using System.Security.Policy;
using System.Data.SqlTypes;
using System.Security.Cryptography;

namespace DAL
{
    public class DAL_Permission
    {
        private static readonly string CONNECTION_STRING = DataBaseServices.getConnectionString();
        private SqlDataReader sqlReader;

        public void FillProfileAuths(Profile pProfile)
        {
            
            foreach (object[] permission in GetProfilePermissions(pProfile))
            {
                if (permission[2].ToString() != "")
                {
                    pProfile.AddChild(new Permission(permission));
                }
                else
                {
                    Role role = new Role(permission);
                    var list = GetAll(int.Parse(permission[1].ToString()));
                    foreach (var child in list)
                    {
                        role.AddChild(child);
                    }
                    pProfile.AddChild(role);
                }
            }
        }
        public List<object> GetProfilePermissions(Profile pProfile)
        {

            string selectProfilePermissionsQuery = $"SELECT * FROM perfil_permiso WHERE Id_perfil = {pProfile.id}";
            List<int> permissionIDs = new List<int>();
            List<object> list = new List<object>(); 
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (SqlCommand selectProfilePermissionsCommand = new SqlCommand(selectProfilePermissionsQuery, connection))
                {
                    sqlReader = selectProfilePermissionsCommand.ExecuteReader();
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            permissionIDs.Add((int)sqlReader[1]);
                        }
                    }
                    sqlReader.Close();
                }
                foreach (var permissionID in permissionIDs)
                {
                    string selectPermissionsQuery = $"SELECT * FROM permiso WHERE Id = {permissionID}";
                    using (SqlCommand selectPermissionsCommand = new SqlCommand(selectPermissionsQuery, connection))
                    {
                        sqlReader = selectPermissionsCommand.ExecuteReader();
                        if (sqlReader.HasRows)
                        {
                            while (sqlReader.Read())
                            {
                                list.Add(new object[] { (string)sqlReader[0], (int)sqlReader[1], sqlReader[2].ToString() });
                            }
                        }
                        sqlReader.Close();
                    }
                }
            }
            return list;
        }
        public List<Auth> GetAll(int roleID)
        {
            var authsList = new List<Auth>();
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                connection.Open();

                // Obtener los permisos hijos del rol
                string queryChildPermissions = "SELECT Id_Permiso_Hijo FROM Permiso_Permiso WHERE Id_Permiso_Padre = @role";
                List<int> listPermissionsIds = new List<int>();
                using (SqlCommand command = new SqlCommand(queryChildPermissions, connection))
                {
                    command.Parameters.AddWithValue("@role", roleID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listPermissionsIds.Add(reader.GetInt32(0)); // Obtener el Id_Permiso_Hijo
                        }
                    }
                }
                foreach (int permissionId in listPermissionsIds)
                {
                    string queryPermission = "SELECT Nombre, Id, PermisoAtomico FROM Permiso WHERE Id = @permissionId";
                    Auth auth = null;
                    using (SqlCommand command = new SqlCommand(queryPermission, connection))
                    {
                        command.Parameters.AddWithValue("@permissionId", permissionId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Verificamos si es un Permiso o un Rol según la lógica de la columna
                                if (!string.IsNullOrEmpty(reader[2].ToString()))
                                {
                                    auth = new Permission(new object[] { (string)reader[0], (int)reader[1], (int)reader[2] }); 
                                }
                                else
                                {
                                    auth = new Role(new object[] { (string)reader[0], (int)reader[1], reader[2].ToString()}); 
                                }
                            }
                        }
                    }
                    // Buscamos el padre en la lista
                    var father = GetAuth(roleID, authsList);
                    if (father == null)
                    {
                        authsList.Add(auth);
                    }
                    else
                    {
                        father.AddChild(auth);
                    }
                }
            }
            return authsList;
        }
        private Auth GetAuth(int pId, IList<Auth> pAuthsList)
        {
            Auth auth = pAuthsList != null ? pAuthsList.Where(authRecorrido => authRecorrido.Id.Equals(pId)).FirstOrDefault() : null;
            if (auth == null && pAuthsList != null)
            {
                foreach (var aux_auth in pAuthsList)
                {
                    var l = GetAuth(pId, aux_auth.Children);
                    if (l != null && l.Id == pId) return l;
                    else
                    if (l != null) return GetAuth(pId, l.Children);
                }
            }
            return auth;
        }
        public List<Role> GetRoles()
        {
            List<Role> roles = new List<Role>(); 
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {

                string query = "SELECT * FROM Permiso WHERE PermisoAtomico IS NULL";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Role role = new Role(new object[] { reader.GetString(0), reader.GetInt32(1)});
                        role.children = GetAll(reader.GetInt32(1));
                        roles.Add(role);
                    }
                }
            }
            return roles;
        }
        public List<Permission> GetPermissions()
        {
            List<Permission> permissions = new List<Permission>();
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {

                string query = "SELECT * FROM Permiso WHERE PermisoAtomico IS NOT NULL";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Permission permission = new Permission(new object[] { reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2) });
                        permissions.Add(permission);
                    }
                }
            }
            return permissions;
        }

        public void SaveProfile(string profileName)
        {
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    connection.Open();
                    string insertQuery = $"INSERT INTO perfil (Nombre) VALUES ('{profileName}')";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar los datos: " + ex.Message);
                }
            }
        }
        public void AddAuthToProfile(string authName)
        {
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    connection.Open();
                    int authID = 0;
                    int profileID = 0;
                    //Obtenemos el id del permiso
                    string selectQuery1 = $"SELECT * FROM Permiso WHERE Nombre = '{authName}'";
                    SqlCommand selectCommand1 = new SqlCommand(selectQuery1, connection);
                    using (SqlDataReader reader = selectCommand1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            authID = reader.GetInt32(1);
                        }
                    }
                    //Obtenemos el id del perfil
                    string selectQuery2 = "SELECT TOP 1 * FROM perfil ORDER BY Id DESC";
                    SqlCommand selectCommand2 = new SqlCommand(selectQuery2, connection);
                    using (SqlDataReader reader = selectCommand2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profileID = reader.GetInt32(0);
                        }
                    }
                    //Creamos la relación perfil - permiso
                    string insertQuery = $"INSERT INTO perfil_permiso (Id_Perfil, Id_Permiso) VALUES ({profileID}, {authID})";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al insertar los datos: " + ex.Message);
                }
            }
        }
        public static List<string> GetProfiles()
        {
            List<string> profiles = new List<string>();
            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    connection.Open();
                    string selectQuery = $"SELECT * FROM Perfil";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profiles.Add(reader.GetString(1));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener los datos: " + ex.Message);
                }
            }
            return profiles;
        }
    }
}
