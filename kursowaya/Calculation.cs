using Gtk;
using System;
using System.Data.OleDb;
using System.Data;

class Calculation
{
    private static readonly string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"C:\\Users\\Acer\\Pictures\\kursach\\kursach.accdb\";";
    public static void HandleShowTables(VBox dynamicContentContainer, Button showTablesButton, Window mainWin)
    {
        foreach (var child in dynamicContentContainer.Children)
        {
            dynamicContentContainer.Remove(child);
        }

        if (showTablesButton.Label == "Показать все таблицы")
        {
            // Отображение таблицы "Бронза латунь больше 350"
            DisplayTable(dynamicContentContainer, "Бронза латунь больше 350", "Таблица 1", new string[] { "Код", "AAA" });

            // Отображение таблицы "Бронза меньше 300"
            DisplayTable(dynamicContentContainer, "Бронза менльше 300", "Таблица 2 Бронза менльше 300", new string[] { "Код", "Марка материала", "Способ отливки", "Предел прочности растяжения, МПа", "HRC>=45", "BHP" });

            // Отображение таблицы "Чугун"
            DisplayTable(dynamicContentContainer, "Чугун", "Таблица 3 Чугун", new string[] { "Код", "Марка материала", "Материал червяка", "Предел прочности при изгибе, МПа", "Скорость скольжения", "BHP" });

            showTablesButton.Label = "Назад";
        }
        else
        {
            showTablesButton.Label = "Показать все таблицы";
        }

        dynamicContentContainer.ShowAll();
    }

    private static void DisplayTable(VBox container, string tableName, string tableTitle, string[] columnHeaders)
    {
        Label tableLabel = new Label(tableTitle);
        container.PackStart(tableLabel, false, false, 5);

        TreeView treeView = new TreeView();

        // Создаем колонки
        for (int col = 0; col < columnHeaders.Length; col++)
        {
            TreeViewColumn column = new TreeViewColumn
            {
                Title = columnHeaders[col]
            };
            CellRendererText cell = new CellRendererText();
            column.PackStart(cell, true);
            column.AddAttribute(cell, "text", col);
            treeView.AppendColumn(column);
        }

        // Загружаем данные из базы
        DataTable tableData = LoadTable(tableName);

        ListStore store = new ListStore(GetColumnTypes(columnHeaders.Length));
        foreach (DataRow row in tableData.Rows)
        {
            // Преобразуем строки из DataTable в массив значений для добавления в ListStore
            object[] rowValues = new object[columnHeaders.Length];
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                rowValues[i] = row[columnHeaders[i]];  // Здесь используются имена столбцов, соответствующие в базе данных
            }
            store.AppendValues(rowValues);
        }

        treeView.Model = store;

        ScrolledWindow scrolledWindow = new ScrolledWindow();
        scrolledWindow.Add(treeView);
        scrolledWindow.SetSizeRequest(700, 200);
        container.PackStart(scrolledWindow, false, false, 5);
    }

    private static Type[] GetColumnTypes(int columnCount)
    {
        Type[] columnTypes = new Type[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            columnTypes[i] = typeof(string); // Все колонки в таблице предполагаются строковыми типами
        }
        return columnTypes;
    }

    private static DataTable LoadTable(string tableName)
    {
        using (OleDbConnection connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            string query = $"SELECT * FROM [{tableName}]";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }
    }

    ////private static Type[] GetColumnTypes(int columnCount)
    ////{
    ////    Type[] columnTypes = new Type[columnCount];
    ////    for (int i = 0; i < columnCount; i++)
    ////    {
    ////        columnTypes[i] = typeof(string);
    ////    }
    ////    return columnTypes;
    ////}
    public static void HandleInputData(VBox dynamicContentContainer)
    {
        // Создаем элементы для ввода исходных данных
        Label labelgbt = new Label("Введите тип редуктора");
        Entry entrygbt = new Entry();
        Label labelZ1 = new Label("Введите число витков червяка:");
        Entry fieldZ1 = new Entry();
        Label labelZ2 = new Label("Введите число зубьев колеса:");
        Entry fieldZ2 = new Entry();
        Button nextStepButton = new Button("Далее");
        Label resultLabel = new Label();

        // Добавляем элементы в контейнер
        dynamicContentContainer.PackStart(labelgbt, false, false, 5);
        dynamicContentContainer.PackStart(entrygbt, false, false, 5);
        dynamicContentContainer.PackStart(labelZ1, false, false, 5);
        dynamicContentContainer.PackStart(fieldZ1, false, false, 5);
        dynamicContentContainer.PackStart(nextStepButton, false, false, 5);
        dynamicContentContainer.PackStart(resultLabel, false, false, 5);

        nextStepButton.Clicked += (s, e) =>
        {
            if (double.TryParse(fieldZ1.Text, out double z1))
            {
                double.TryParse(fieldZ2.Text, out double z2);
                double u = z2 / z1;
                string gbtcheck =
                // Проверка в таблице 1
                string type_тр = null;
                bool found = false;

                // Загрузка данных из таблицы 1
                DataTable table1 = LoadTable("Бронза латунь больше 350");
                foreach (DataRow row in table1.Rows)
                {
                    if (double.TryParse(row["Число зубьев"].ToString(), out double tableZ1) && tableZ1 == z1)
                    {
                        type_тр = row["Тип троса"].ToString();
                        Console.WriteLine(type_тр, z1);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    resultLabel.Text = "Число зубьев не найдено в таблице 1.";
                    return;
                }

                // Проверка в таблице 2 для диапазона n1From и n1To
                found = false;
                double n1From = 0, n1To = 0;

                DataTable table2 = LoadTable("Бронза менльше 300");
                foreach (DataRow row in table2.Rows)
                {
                    if (double.TryParse(row["Число зубьев"].ToString(), out double tableZ1) &&
                        tableZ1 == z1 &&
                        type_тр == row["Тип троса"].ToString())
                    {
                        n1From = double.Parse(row["Частота от"].ToString());
                        n1To = double.Parse(row["Частота до"].ToString());
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    resultLabel.Text = "Диапазон частоты вращения не найден в таблице 2.";
                    return;
                }

                // Удаление старых элементов
                RemoveOldElements(dynamicContentContainer);

                // Добавление панели для ввода n_1
                Label labelN1Input = new Label($"Введите частоту вращения n_1 (от {n1From} до {n1To}):");
                Entry fieldN1Input = new Entry { PlaceholderText = $"Введите n_1 от {n1From} до {n1To}" };
                Button calculateButton = new Button("Рассчитать");

                dynamicContentContainer.PackStart(labelN1Input, false, false, 5);
                dynamicContentContainer.PackStart(fieldN1Input, false, false, 5);
                dynamicContentContainer.PackStart(calculateButton, false, false, 5);

                calculateButton.Clicked += (sender, args) =>
                {
                    if (double.TryParse(fieldN1Input.Text, out double n1) && n1 >= n1From && n1 <= n1To)
                    {
                        resultLabel.Text = $"Частота вращения n_1 подтверждена: {n1}";
                        // Добавить дополнительные расчеты
                    }
                    else
                    {
                        resultLabel.Text = $"Ошибка: введите n_1 в диапазоне от {n1From} до {n1To}.";
                    }
                };

                dynamicContentContainer.ShowAll();
            }
            else
            {
                resultLabel.Text = "Ошибка: введите корректное значение числа зубьев.";
            }
        };

        AddFixedPanels(dynamicContentContainer);
        dynamicContentContainer.ShowAll();
    }

    private static void RemoveOldElements(VBox container)
    {
        var elementsToRemove = container.Children
            .Where(c => c is Label || c is Entry || c is Button)
            .ToList();

        foreach (var element in elementsToRemove)
        {
            container.Remove(element);
        }
    }

    private static void AddFixedPanels(VBox container)
    {
        Label labelN = new Label("Введите мощность, передаваемую ремнем:");
        Entry fieldN = new Entry();
        container.PackStart(labelN, false, false, 5);
        container.PackStart(fieldN, false, false, 5);

        Label labelC1 = new Label("Введите поправку на диаметр шкива:");
        Entry fieldC1 = new Entry();
        container.PackStart(labelC1, false, false, 5);
        container.PackStart(fieldC1, false, false, 5);

        Label labeln2 = new Label("Введите частоту вращения меньшего шкива:");
        Entry fieldn2 = new Entry();
        container.PackStart(labeln2, false, false, 5);
        container.PackStart(fieldn2, false, false, 5);
    }

    //private static DataTable LoadTable(string tableName)
    //{
    //    using (OleDbConnection connection = new OleDbConnection(сonnectionString))
    //    {
    //        connection.Open();
    //        string query = $"SELECT * FROM [{tableName}]";
    //        OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
    //        DataTable dataTable = new DataTable();
    //        adapter.Fill(dataTable);
    //        return dataTable;
    //    }
    //}

}