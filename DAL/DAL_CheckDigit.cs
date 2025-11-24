using Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DAL
{
    public class DAL_CheckDigit
    {
        private static readonly string CONNECTION_STRING = DataBaseServices.getConnectionString();

        private SqlConnection sqlConnection;
        public bool CheckVerticalDigit(string tableName)
        {
            string calculatedVerticalDigit = CalculateVerticalDigit(tableName);

            if (!string.IsNullOrEmpty(calculatedVerticalDigit))
            {
                calculatedVerticalDigit = ComputeSha256(calculatedVerticalDigit);
            }

            return calculatedVerticalDigit.Equals(GetVerticalDigit(tableName));
        }

        public string CalculateVerticalDigit(string tableName)
        {
            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                string selectAllHorizontalDigitsQuery = $"SELECT dvh FROM {tableName}";
                SqlCommand command = new SqlCommand(selectAllHorizontalDigitsQuery, sqlConnection);
                sqlConnection.Open();
                SqlDataReader data = command.ExecuteReader();

                var verticalDigit = new StringBuilder();
                while (data.Read())
                {
                    verticalDigit.Append(data["dvh"]);
                }

                sqlConnection.Close();
                return verticalDigit.ToString();
            }
        }

        public string GetVerticalDigit(string tableName)
        {
            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                string selectVerticalDigitQuery = $"SELECT dvv FROM dvv WHERE nombreTabla = '{tableName}'";
                SqlCommand command = new SqlCommand(selectVerticalDigitQuery, sqlConnection);
                sqlConnection.Open();
                SqlDataReader data = command.ExecuteReader();

                string verticalDigit = "";
                while (data.Read())
                {
                    verticalDigit = data["dvv"].ToString();
                }

                sqlConnection.Close();
                return verticalDigit;
            }
        }

        public List<string> CheckHorizontalDigits(string tableName)
        {
            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                List<string> affectedRows = new List<string>();
                string selectAllRowsQuery = $"SELECT * FROM {tableName}";
                SqlCommand command = new SqlCommand(selectAllRowsQuery, sqlConnection);
                sqlConnection.Open();
                SqlDataReader data = command.ExecuteReader();

                var horizontalDigit = new StringBuilder();
                int row =1;
                while (data.Read())
                {
                    for (int i =0; i < data.FieldCount; i++)
                    {
                        if (!data.GetName(i).Equals("dvh") && !isForeignKey(data.GetName(i)))
                        {
                            var val = data.GetValue(i);
                            string s = (val == null || val == DBNull.Value) ? string.Empty : val.ToString();
                            // append with separator to avoid accidental collisions
                            horizontalDigit.Append(s).Append('|');
                        }
                    }

                    string calculatedHorizontalDigit = ComputeSha256(horizontalDigit.ToString());
                    if (!calculatedHorizontalDigit.Equals(Convert.ToString(data["dvh"])))
                    {
                        affectedRows.Add(row.ToString());
                    }

                    row++;
                    horizontalDigit.Clear();
                }

                sqlConnection.Close();
                return affectedRows;
            }
        }

        public void setVerticalDigit(string tableName)
        {
            string calculatedVerticalDigit = ComputeSha256(CalculateVerticalDigit(tableName));

            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    string updateVerticalDigitQuery = $"UPDATE dvv SET dvv = '{calculatedVerticalDigit}' WHERE nombreTabla = '{tableName}'";
                    sqlConnection.Open();
                    SqlCommand command = new SqlCommand(updateVerticalDigitQuery, sqlConnection);
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (Exception)
                {
                    DAL_User dalUser = new DAL_User();
                }
            }
        }

        public void setHorizontalDigits(string tableName)
        {
            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    string selectAllRowsQuery = $"SELECT * FROM {tableName}";
                    SqlCommand selectCommand = new SqlCommand(selectAllRowsQuery, sqlConnection);
                    sqlConnection.Open();
                    SqlDataReader data = selectCommand.ExecuteReader();

                    var updateHorizontalDigitQuery = new StringBuilder();
                    var horizontalDigit = new StringBuilder();

                    while (data.Read())
                    {
                        for (int i =0; i < data.FieldCount; i++)
                        {
                            if (!data.GetName(i).Equals("dvh") && !isForeignKey(data.GetName(i)))
                            {
                                var val = data.GetValue(i);
                                string s = (val == null || val == DBNull.Value) ? string.Empty : val.ToString();
                                horizontalDigit.Append(s).Append('|');
                            }
                        }

                        string hashed = ComputeSha256(horizontalDigit.ToString());
                        updateHorizontalDigitQuery.Append($"UPDATE {tableName} SET dvh = '{hashed}' WHERE id = {data["id"].ToString()};");
                        horizontalDigit.Clear();
                    }

                    sqlConnection.Close();
                    sqlConnection.Open();
                    SqlCommand updateCommand = new SqlCommand(updateHorizontalDigitQuery.ToString(), sqlConnection);
                    updateCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (Exception)
                {
                    DAL_User dalUser = new DAL_User();
                }
            }
        }

        public List<string> GetTableNames()
        {
            using (sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                string selectTableNamesQuery = "SELECT nombreTabla FROM dvv";
                SqlCommand command = new SqlCommand(selectTableNamesQuery, sqlConnection);
                sqlConnection.Open();
                SqlDataReader data = command.ExecuteReader();

                List<string> tableNames = new List<string>();
                while (data.Read())
                {
                    tableNames.Add(data["nombreTabla"].ToString());
                }

                sqlConnection.Close();
                return tableNames;
            }
        }

        public bool isForeignKey(string colName)
        {
            if (colName.Length <3)
            {
                return false;
            }

            if (colName.Substring(0,3).Equals("FK_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static string ComputeSha256(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                return string.Empty;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rawData);
                byte[] hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
