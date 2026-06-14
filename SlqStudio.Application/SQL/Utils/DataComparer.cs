using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlqStudio.Application.SQL.Utils
{
    public class DataComparer
    {
        public bool CompareResults(List<Dictionary<string, object>> result1, List<Dictionary<string, object>> result2)
        {
            if (!HaveSameRecordCount(result1, result2))
                return false;

            // Два пустых набора эквивалентны; столбцы сравнивать не из чего.
            if (result1.Count == 0)
                return true;

            if (!CompareColumnNames(result1.First().Keys.ToList(), result2.First().Keys.ToList()))
                return false;
            return CompareData(result1, result2);
        }

        private bool HaveSameRecordCount(List<Dictionary<string, object>> result1, List<Dictionary<string, object>> result2)
        {
            return result1.Count == result2.Count;
        }

        private bool CompareColumnNames(List<string> columns1, List<string> columns2)
        {
            if (columns1.Count != columns2.Count)
                return false;

            for (int i = 0; i < columns1.Count; i++)
            {
                if (!columns1[i].Equals(columns2[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        private bool CompareData(List<Dictionary<string, object>> result1, List<Dictionary<string, object>> result2)
        {
            for (int i = 0; i < result1.Count; i++)
            {
                var row1 = result1[i];
                // Поиск столбцов ведём без учёта регистра, согласованно с CompareColumnNames.
                var row2 = new Dictionary<string, object>(result2[i], StringComparer.OrdinalIgnoreCase);

                if (row1.Count != row2.Count)
                    return false;

                foreach (var key in row1.Keys)
                {
                    if (!row2.ContainsKey(key))
                        return false;

                    var value1 = row1[key];
                    var value2 = row2[key];

                    if (value1 == null && value2 == null)
                        continue;
                    if (value1 == null || value2 == null)
                        return false;

                    if (IsNumeric(value1) && IsNumeric(value2))
                    {
                        decimal num1 = Convert.ToDecimal(value1);
                        decimal num2 = Convert.ToDecimal(value2);
                        if (num1 != num2)
                            return false;
                        continue;
                    }

                    if (value1.GetType() != value2.GetType())
                    {
                        try
                        {
                            if (Convert.ChangeType(value1, value2.GetType())?.Equals(value2) == true)
                                continue;
                            if (Convert.ChangeType(value2, value1.GetType())?.Equals(value1) == true)
                                continue;
                        }
                        catch
                        {
                            return false;
                        }

                        // Конвертации удались, но значения не совпали — наборы различаются.
                        return false;
                    }
                    else if (!Equals(value1, value2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool IsNumeric(object obj)
        {
            return obj is byte || obj is short || obj is int || obj is long ||
                   obj is float || obj is double || obj is decimal;
        }

    }
}
