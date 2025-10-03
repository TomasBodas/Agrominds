using DAL;
using Services;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_CheckDigitsManager
    {

		static List<string> errors = new List<string>();
		static DAL_CheckDigit dalCheckDigit = new DAL_CheckDigit();

		public static bool CheckHorizontalDigits(string tableName)
		{
			List<string> affectedRows = dalCheckDigit.CheckHorizontalDigits(tableName);

			if (affectedRows.Count == 0)
			{
				return true;
			}
			foreach (string row in affectedRows)
			{
				string errorMessage = "Se modificó la fila " + row + " de la tabla " + tableName;
				errors.Add(errorMessage);
				SessionManager sm = new SessionManager();
			}
			return false;
		}

		public bool CheckDigits()
		{
            errors.Clear();
            bool isSuccessful = true;
			foreach (string tableName in dalCheckDigit.GetTableNames())
			{
				if (!dalCheckDigit.CheckVerticalDigit(tableName))
				{
                    isSuccessful = false;
					string errorMessage = "Se eliminaron uno o más registros de la tabla " + tableName;
					errors.Add(errorMessage);
					SessionManager sm = new SessionManager();
				}
				if (!CheckHorizontalDigits(tableName))
				{
                    isSuccessful = false;
				}
			}
			return isSuccessful;
		}

		public static List<string> GetErrors()
		{
			return errors;
		}

		public void SetCheckDigits()
		{
			try
			{
				foreach (string tableName in dalCheckDigit.GetTableNames())
				{
					dalCheckDigit.setHorizontalDigits(tableName);
					dalCheckDigit.setVerticalDigit(tableName);
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
		}
	}
}
