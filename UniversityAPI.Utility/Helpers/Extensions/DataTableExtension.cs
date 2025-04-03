using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data
{
    public static class DataTableExtension
    {
        public static DataTable ListToDataTable<T>(IEnumerable<T> list, string _tableName)
        {
            DataTable dt = new DataTable(_tableName);

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }
            foreach (T t in list)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    row[info.Name] = info.GetValue(t, null) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static void ApplyToColumns(this DataTable dataTable, IEnumerable<string> columns, Action<DataRow, string> function)
        {
            var existingColumns = new HashSet<string>(dataTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName));
            var validColumns = columns.Where(col => existingColumns.Contains(col));
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var col in validColumns)
                {
                    function(row, col);
                }
            }
        }

        public static void SetColOrdinal(this DataColumnCollection columns, string columnName, int referenceIndex)
        {
            if (columns.Contains(columnName))
            {
                columns[columnName].SetOrdinal(referenceIndex);
            }
        }

        public static void RemoveCol(this DataColumnCollection col, string columnName)
        {
            if (col.Contains(columnName))
            {
                col.Remove(columnName);
            }
        }

        public static void RenameCol(this DataColumnCollection col, string from, string to)
        {
            if (col.Contains(from) && !col.Contains(to))
            {
                col[from].ColumnName = to;
            }
        }

        public static void MoveColumnBefore(this DataColumnCollection columns, string columnName, string referenceColumnName)
        {
            MoveColumn(columns, columnName, referenceColumnName, insertBefore: true);
        }

        public static void MoveColumnAfter(this DataColumnCollection columns, string columnName, string referenceColumnName)
        {
            MoveColumn(columns, columnName, referenceColumnName, insertBefore: false);
        }

        private static void MoveColumn(DataColumnCollection columns, string columnName, string referenceColumnName, bool insertBefore)
        {
            DataColumn columnToMove = columns[columnName];
            DataColumn referenceColumn = columns[referenceColumnName];

            if (columnToMove == null || referenceColumn == null)
            {
                return;
            }

            int referenceIndex = columns.IndexOf(referenceColumn);
            int currentIndex = columns.IndexOf(columnName);

            if (currentIndex == referenceIndex)
            {
                return;
            }

            AdjustReferenceIndex(ref currentIndex, ref referenceIndex, insertBefore);
            SetColumnOrdinal(columns, columnName, referenceIndex);
        }

        private static void AdjustReferenceIndex(ref int currentIndex, ref int referenceIndex, bool insertBefore)
        {
            bool isAfterReference = !insertBefore && currentIndex > referenceIndex;
            bool isBeforeReference = insertBefore && currentIndex < referenceIndex;

            if (isAfterReference)
            {
                referenceIndex++;
            }
            else if (isBeforeReference)
            {
                referenceIndex--;
            }
        }

        private static void SetColumnOrdinal(DataColumnCollection columns, string columnName, int referenceIndex)
        {
            if (columns.Contains(columnName) && columns.IndexOf(columnName) != referenceIndex)
            {
                columns[columnName].SetOrdinal(referenceIndex);
            }
        }
    }
}