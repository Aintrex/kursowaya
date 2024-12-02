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
//class SourceData
//{
//    // Исходные метки
//    public static readonly string LabelI = "Введите число передач:";
//    public static readonly string LabelZ1 = "Введите число зубьев:";
//    public static readonly string LabelN = "Введите мощность, передаваемая ремнем:";
//    public static readonly string LabelС1 = "Введите поправку на диаметр шкива,:";
//    public static readonly string Labeln2 = "Введите частоту вращения меньшего шкива:";



//    // Таблицы в нормальном виде для пользователя
//    public static readonly List<string> TableFileNames = new List<string>
//    {
//        "1_table.json",
//        "2_table.json",
//        "3_table.json",
//        "4_table.json",
//        "5_table.json",
//        "6_table.json"
//    };

//    // Таблица 1
//    public static readonly string[,] Table1 =
//    {
//        { "12", "1x7" },
//        { "14", "1x7" },
//        { "16", "1x7" },
//        { "18", "1x7" },
//        { "20", "1x7" },
//        { "16", "1x21" },
//        { "18", "1x21" },
//        { "20", "1x21" },
//        { "22", "1x21" },
//        { "24", "1x21" },
//        { "26", "1x21" }
//    };

//    // Таблица 2
//    public static readonly string[,] Table2 =
//    {
//        { "12", "1x7", "500", "3000" },
//        { "14", "1x7", "3500", "4500" },
//        { "16", "1x7", "5000", "6800" },
//        { "18", "1x7", "7000", "7500" },
//        { "14", "1x21", "500", "500" },
//        { "16", "1x21", "1000", "1500" },
//        { "18", "1x21", "2000", "3000" },
//        { "20", "1x21", "3500", "4000" },
//        { "22", "1x21", "500", "500" },
//        { "24", "1x21", "1000", "1500" },
//        { "26", "1x21", "1500", "2000" }
//    };

//    // Таблица 3
//    public static readonly string[,] Table3 =
//    {
//        { "12", "1x7", "2" },
//        { "14", "1x7", "2" },
//        { "16", "1x7", "2" },
//        { "18", "1x7", "2" },
//        { "20", "1x7", "3" },
//        { "22", "1x21", "4" },
//        { "24", "1x21", "5" },
//        { "26", "1x21", "7" }
//    };

//    // Таблица 4
//    public static readonly string[,] Table4 =
//    {
//        { "2", "6.283", "3", "1.2", "2", "50°", "0.36", "0.6" },
//        { "3", "9.425", "4", "1.8", "3", "50°", "0.36", "0.6" },
//        { "4", "12.566", "5", "2.4", "4", "50°", "0.65", "1.3" },
//        { "5", "15.708", "6", "3", "5", "50°", "0.65", "1.3" },
//        { "6", "21.991", "8", "4.2", "6", "50°", "0.65", "1.3" },
//        { "7", "31.416", "11", "6", "7", "50°", "0.65", "1.3" }
//    };

//    // Таблица 5
//    public static readonly string[,] Table5 =
//    {
//        { "2", "8" },
//        { "2", "10" },
//        { "2", "12.5" },
//        { "2", "16" },
//        { "3", "12.5" },
//        { "3", "16" },
//        { "3", "20" },
//        { "3", "25" },
//        { "4", "20" },
//        { "4", "25" },
//        { "4", "32" },
//        { "4", "40" },
//        { "5", "25" },
//        { "5", "32" },
//        { "5", "40" },
//        { "5", "50" },
//        { "7", "50" },
//        { "7", "63" },
//        { "7", "80" },
//        { "10", "50" },
//        { "10", "63" },
//        { "10", "80" }
//    };

//    // Таблица 6
//    public static readonly string[,] Table6 =
//    {
//        { "2", "0.0018", "0.16" },
//        { "3", "0.0025", "0.17" },
//        { "4", "0.003", "0.18" },
//        { "4", "0.0011", "0.19" },
//        { "5", "0.0013", "0.2" },
//        { "7", "0.0019", "0.21" },
//        { "10", "0.0025", "0.22" }
//    };
//}
