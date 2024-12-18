using System.Collections.Generic;
using System.Data.OleDb;

class DatabaseHelper
{
    private static readonly string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"C:\\Users\\Acer\\Pictures\\kursach\\kursach.accdb\";";

    public static List<string[]> GetTableData(string tableName)
    {
        List<string[]> tableData = new List<string[]>();
        using (OleDbConnection connection = new OleDbConnection(ConnectionString))
        {
            connection.Open();
            string query = $"SELECT * FROM {tableName}";
            using (OleDbCommand command = new OleDbCommand(query, connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string[] row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader[i].ToString();
                    }
                    tableData.Add(row);
                }
            }
        }
        return tableData;
    }
}

