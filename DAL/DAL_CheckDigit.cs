using Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_CheckDigit
    {
		private static readonly string CONNECTION_STRING = DataBaseServices.getConnectionString();

		private SqlConnection sqlConnection;
		private SqlConnection sqlConnection1;
		private SqlCommand sqlCommand;
		private SqlDataReader sqlReader;

		public bool CheckVerticalDigit(string tableName)
		{
			string calculatedVerticalDigit = CalculateVerticalDigit(tableName);
            if (calculatedVerticalDigit != "")
			{
                calculatedVerticalDigit = calculatedVerticalDigit.GetHashCode().ToString();
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
				var verticalDigit = new System.Text.StringBuilder();
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

				var horizontalDigit = new System.Text.StringBuilder();
				int row = 1;
				while (data.Read())
				{
					for (int i = 0; i < data.FieldCount; i++)
					{
						if (!data.GetName(i).Equals("dvh") && !isForeignKey(data.GetName(i)))
						{
                            horizontalDigit.Append(data.GetValue(i).GetHashCode().ToString());
						}
					}

					string calculatedHorizontalDigit = horizontalDigit.ToString().GetHashCode().ToString();
					if (calculatedHorizontalDigit != data["dvh"].ToString())
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
			string calculatedVerticalDigit = CalculateVerticalDigit(tableName).GetHashCode().ToString();
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
				catch (Exception e)
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
					var updateHorizontalDigitQuery = new System.Text.StringBuilder();
					var horizontalDigit = new System.Text.StringBuilder();
					while (data.Read())
					{
						for (int i = 0; i < data.FieldCount; i++)
						{
							if (!data.GetName(i).Equals("dvh") && !isForeignKey(data.GetName(i)))
							{
                                horizontalDigit.Append(data.GetValue(i).GetHashCode().ToString());
							}
						}
						updateHorizontalDigitQuery.Append($"UPDATE {tableName} SET dvh = '{horizontalDigit.ToString().GetHashCode().ToString()}' WHERE id = {data["id"].ToString()};");
                        horizontalDigit.Clear();
					}
					sqlConnection.Close();
					sqlConnection.Open();
					SqlCommand updateCommand = new SqlCommand(updateHorizontalDigitQuery.ToString(), sqlConnection);
                    updateCommand.ExecuteNonQuery();
					sqlConnection.Close();
				}
				catch (Exception e)
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
			if (colName.Length < 3)
			{
				return false;
			}

			if (colName.Substring(0, 3).Equals("FK_", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}
	}
}
