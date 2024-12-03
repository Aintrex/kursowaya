using Gtk;
using System;
using System.Data.OleDb;
using System.Data;
using System.Net.Cache;

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
        foreach (var child in dynamicContentContainer.Children)
        {
            dynamicContentContainer.Remove(child);
        }
        // Создаем начальные элементы для ввода
        Label labelGbt = new Label("Выберите тип редуктора (общего назначения, специального назначения, не редукторный):");
        string[] gbtOptions = { "общего назначения", "специального назначения", "не редукторный" };
        ComboBox comboGbt = new ComboBox(gbtOptions);

        Label labelZ1 = new Label("Введите число витков червяка (1, 2 или 4):");
        Entry fieldZ1 = new Entry();

        Label labelQ = new Label("Введите q (8, 9, 10, 12 или 14):");
        Entry fieldQ = new Entry();

        Label labelZ2 = new Label("Введите число зубьев колеса (28-80):");
        Entry fieldZ2 = new Entry();

        Label labelm = new Label("Введите модуль");
        Entry fieldm = new Entry();

        Button nextStepButton = new Button("Далее");
        Label resultLabel = new Label();

        // Добавляем начальные элементы в контейнер
        dynamicContentContainer.PackStart(labelGbt, false, false, 5);
        dynamicContentContainer.PackStart(comboGbt, false, false, 5);
        dynamicContentContainer.PackStart(labelZ1, false, false, 5);
        dynamicContentContainer.PackStart(fieldZ1, false, false, 5);
        dynamicContentContainer.PackStart(labelQ, false, false, 5);
        dynamicContentContainer.PackStart(fieldQ, false, false, 5);
        dynamicContentContainer.PackStart(labelZ2, false, false, 5);
        dynamicContentContainer.PackStart(fieldZ2, false, false, 5);
        dynamicContentContainer.PackStart(labelm, false, false, 5);
        dynamicContentContainer.PackStart(fieldm, false, false, 5);
        dynamicContentContainer.PackStart(nextStepButton, false, false, 5);
        dynamicContentContainer.PackStart(resultLabel, false, false, 5);
       
        string gbt = comboGbt.ToString(); // Получаем текст активного элемента
        int z1=0;
        int z2=0;
        int q=0;
        double m=0;
        string material;
        double strength=0;
        string tableName = "";
        dynamicContentContainer.ShowAll();
        nextStepButton.Clicked += (s, e) =>
        {

            int activeIndex = comboGbt.Active; // Получаем индекс выбранного элемента
            string gbt = (activeIndex >= 0) ? gbtOptions[activeIndex] : null;
            if (string.IsNullOrEmpty(gbt))
            {
                resultLabel.Text = "Ошибка: выберите тип редуктора.";
                return;
            }

            if (!int.TryParse(fieldZ1.Text, out z1) || (z1 != 1 && z1 != 2 && z1 != 4))
            {
                resultLabel.Text = "Ошибка: введите корректное значение числа витков (1, 2 или 4).";
                return;
            }

            if (!int.TryParse(fieldQ.Text, out q) || (q != 8 && q != 9 && q != 10 && q != 12 && q != 14))
            {
                resultLabel.Text = "Ошибка: введите корректное значение q (10, 12 или 14).";
                return;
            }

            if (!int.TryParse(fieldZ2.Text, out z2) || z2 < 28 || z2 > 80)
            {
                resultLabel.Text = "Ошибка: введите число зубьев колеса в диапазоне от 28 до 80.";
                return;
            }
            if (!double.TryParse(fieldm.Text, out m))
            {
                resultLabel.Text = "Ошибка: введите корректное число m";
                return;
            }
            // Проверка по таблице "Допускаемые сочетания"
            DataTable allowableTable = LoadTable("Допускаемые сочетания");
            bool isValidCombination = allowableTable.Rows.Cast<DataRow>().Any(row =>
                double.Parse(row["m"].ToString()) == m &&
                int.Parse(row["q"].ToString()) == q &&
                int.Parse(row["z1"].ToString()) == z1);

            if (!isValidCombination)
            {
                resultLabel.Text = "Ошибка: сочетание m, q и z1 не соответствует допустимым значениям.";
                return;
            }
            dynamicContentContainer.ShowAll();
       
            // Очистка предыдущих элементов
            foreach (var child in dynamicContentContainer.Children)
            {
                dynamicContentContainer.Remove(child);
            }

            // Добавление новых элементов для проверки материала и предела прочности
            Label labelMaterial = new Label("Выберите материал колеса (бронза, латунь, чугун):");
            string[] matopt = { "бронза", "латунь", "чугун" };
            ComboBox comboMaterial = new ComboBox(matopt );

            Label labelStrength = new Label("Введите предел прочности материала:");
            Entry fieldStrength = new Entry();

            Button checkMaterialButton = new Button("Далее");

            dynamicContentContainer.PackStart(labelMaterial, false, false, 5);
            dynamicContentContainer.PackStart(comboMaterial, false, false, 5);
            dynamicContentContainer.PackStart(labelStrength, false, false, 5);
            dynamicContentContainer.PackStart(fieldStrength, false, false, 5);
            dynamicContentContainer.PackStart(checkMaterialButton, false, false, 5);
            dynamicContentContainer.PackStart(resultLabel, false, false, 5);
            material = comboMaterial.ToString();
            dynamicContentContainer.ShowAll();
            checkMaterialButton.Clicked += (sender, args) =>
            {
                int activeIndex = comboMaterial.Active; // Получаем индекс выбранного элемента
                string material = (activeIndex >= 0) ? matopt[activeIndex] : null;
                if (string.IsNullOrEmpty(material))
                {
                    resultLabel.Text = "Ошибка: выберите материал.";
                    return;
                }

                if (!double.TryParse(fieldStrength.Text, out strength) || strength <= 0)
                {
                    resultLabel.Text = "Ошибка: введите корректный предел прочности.";
                    return;
                }

                if (material == "бронза" || material == "латунь")
                {
                    if ((material == "латунь" && strength < 350))
                    {
                        resultLabel.Text = "Ошибка: предел прочности не соответствует выбранному материалу.";
                        return;
                    }
                    if (material=="бронза" && (strength >= 300 && strength <= 350))
                    {
                        resultLabel.Text = "Ошибка: предел прочности не соответствует выбранному материалу.";
                        return;
                    }
                }

                // Следующие шаги после успешной проверки
                resultLabel.Text = "Материал и предел прочности успешно проверены.";
                // Добавить дальнейшую логику
           
                if (material == "чугун")
                    tableName = "Чугун";
                else if ((material == "бронза" || material == "латунь") && (strength > 350))
                    tableName = "Бронза латунь больше 350";
                else if ((material == "бронза") && strength < 300)
                    tableName = "Бронза менльше 300";

                // Очистка предыдущих элементов
                RemoveOldElements(dynamicContentContainer);
                string casting = "";
                double speed=0;
                string[] matcheropt= {"0" };
                string[] castopt = { "0" };
                string[] speedopt = { "0" };
                double hardness = 0;
                string sspeed = "";
                string matcher = "";
                if (tableName=="Чугун")
                {
                    matcheropt = GetAnythingFromTable(tableName, "Материал червяка");
                    speedopt = GetAnythingFromTable(tableName, "Скорость скольжения");
                }    
                else if (tableName=="Бронза латунь больше 350")
                {
                    castopt = GetAnythingFromTable(tableName, "Способ отливки");
                    speedopt = GetAnythingFromTable(tableName, "Скорость скольжения");
                }
                else
                {
                    castopt = GetAnythingFromTable(tableName, "Способ отливки");
                }
                // Добавление элементов для марки материала и способа отливки
                Label labelMark = new Label("Выберите марку материала:");
                string[] markopt = GetAnythingFromTable(tableName, "Марка материала");
                ComboBox comboMark = new ComboBox(markopt);
                Label labelMatCher = new Label("Введите материал червяка");
                ComboBox comboMatCher = new ComboBox(matcheropt);
                Label labelCasting = new Label("Введите способ отливки:");
                
                ComboBox comboCasting = new ComboBox(castopt);
                Label labelHardness = new Label("Введите твердость червяка:");
                Entry fieldHardness = new Entry();
                Label labelSpeed = new Label("Введите скорость скольжения");
                ComboBox comboSpeed = new ComboBox(speedopt);
                

                dynamicContentContainer.PackStart(labelMark, false, false, 5);
                dynamicContentContainer.PackStart(comboMark, false, false, 5);
                if (material != "чугун")
                {
                   
                    dynamicContentContainer.PackStart(labelCasting, false, false, 5);
                    dynamicContentContainer.PackStart(comboCasting, false, false, 5);
                   
                }
                if (material != "чугун")
                {
                    
                    dynamicContentContainer.PackStart(labelHardness, false, false, 5);
                    dynamicContentContainer.PackStart(fieldHardness, false, false, 5);
                   
                }
                else {
                   
                    dynamicContentContainer.PackStart(labelMatCher, false, false, 5);
                    dynamicContentContainer.PackStart(comboMatCher, false, false, 5);
                    
                }
                if (tableName == "Чугун" || tableName == "Бронза латунь больше 350")
                {
                   
                    dynamicContentContainer.PackStart(labelSpeed, false, false, 5);
                    dynamicContentContainer.PackStart(comboSpeed, false, false, 5);
                   
                }
                Button finalizeButton = new Button("Далее");

               
               
                dynamicContentContainer.PackStart(finalizeButton, false, false, 5);
                dynamicContentContainer.PackStart(resultLabel, false, false, 5);
                string mark = "";
               
                dynamicContentContainer.ShowAll();
                finalizeButton.Clicked += (finalSender, finalArgs) =>
                {
                   
                    int activeIndexmark = comboMark.Active; // Получаем индекс выбранного элемента
                    mark = (activeIndexmark >= 0) ? markopt[activeIndexmark] : null;
                   
                   
                    if (string.IsNullOrEmpty(mark))
                    {
                        resultLabel.Text = "Ошибка: выберите марку материала.";
                        return;
                    }
                    if (tableName == "Чугун")
                    {
                        int activeIndexmatcher = comboMatCher.Active; // Получаем индекс выбранного элемента
                        matcher = (activeIndexmatcher >= 0) ? matcheropt[activeIndexmatcher] : null;
                        if (string.IsNullOrEmpty(matcher))
                        {
                            resultLabel.Text = "Ошибка: введите материал червяка";
                        }
                        DataTable cherTable = LoadTable(tableName);
                        bool isMatCherValid = cherTable.Rows.Cast<DataRow>().Any(row =>
                            row["Марка материала"].ToString()==mark &&
                            row["Материал червяка"].ToString()==matcher);
                        if(!isMatCherValid)
                        {
                            resultLabel.Text = "Ошибка: материал червяка не соответсвует выбранной марке материала";
                            return;
                        }
                        int activeIndexspd = comboSpeed.Active; // Получаем индекс выбранного элемента
                        sspeed = (activeIndexspd >= 0) ? speedopt[activeIndexspd] : null;
                        if (string.IsNullOrEmpty(sspeed))
                        {
                            resultLabel.Text = "Ошибка: введите скорость скольжения";
                        }
                        speed = double.Parse(sspeed);
                    }
                    else if ((tableName == "Бронза латунь больше 350"))
                    {
                        int activeIndexcast = comboCasting.Active; // Получаем индекс выбранного элемента
                        casting = (activeIndexcast >= 0) ? castopt[activeIndexcast] : null;
                        int activeIndexspd = comboSpeed.Active; // Получаем индекс выбранного элемента
                        sspeed = (activeIndexspd >= 0) ? speedopt[activeIndexspd] : null;
                        if (string.IsNullOrEmpty(sspeed))
                        {
                            resultLabel.Text = "Ошибка: введите скорость скольжения";
                        }
                        
                        speed = double.Parse(sspeed);
                        if (string.IsNullOrEmpty(casting))
                        {
                            resultLabel.Text = "Ошибка: введите способ отливки.";
                            return;
                        }
                        // Проверка способа отливки в таблице
                        DataTable materialTable = LoadTable(tableName);
                        bool isCastingValid = materialTable.Rows.Cast<DataRow>().Any(row =>
                            row["Марка материала"].ToString() == mark &&
                            row["Способ отливки"].ToString() == casting);

                        if (!isCastingValid)
                        {
                            resultLabel.Text = "Ошибка: способ отливки не соответствует выбранной марке материала.";
                            return;
                        }
                        if (!double.TryParse(fieldHardness.Text, out hardness) || hardness <= 0)
                        {
                            resultLabel.Text = "Ошибка: введите корректное значение твердости червяка.";
                            return;
                        }
                    }
                    else
                    {
                        int activeIndexcast = comboCasting.Active; // Получаем индекс выбранного элемента
                        casting = (activeIndexcast >= 0) ? castopt[activeIndexcast] : null;
                        if (string.IsNullOrEmpty(casting))
                        {
                            resultLabel.Text = "Ошибка: введите способ отливки.";
                            return;
                        }
                        // Проверка способа отливки в таблице
                        DataTable materialTable = LoadTable(tableName);
                        bool isCastingValid = materialTable.Rows.Cast<DataRow>().Any(row =>
                            row["Марка материала"].ToString() == mark &&
                            row["Способ отливки"].ToString() == casting);

                        if (!isCastingValid)
                        {
                            resultLabel.Text = "Ошибка: способ отливки не соответствует выбранной марке материала.";
                            return;
                        }
                        if (!double.TryParse(fieldHardness.Text, out hardness) || hardness <= 0)
                        {
                            resultLabel.Text = "Ошибка: введите корректное значение твердости червяка.";
                            return;
                        }
                    }
                   
                   
                   

                 

                    // Все проверки пройдены, продолжаем логику
                    resultLabel.Text = "Все данные успешно проверены. Переход к следующему шагу.";

                    // Очистка и добавление логики расчета
                    RemoveOldElements(dynamicContentContainer);
                    foreach (var child in dynamicContentContainer.Children)
                    {
                        dynamicContentContainer.Remove(child);
                    }
                   
                    double bshp = 0;
                    if (tableName == "Бронза менльше 300")
                    {


                        bool hrc = false;
                        if (hardness >= 45)
                            hrc = true;
                        else
                            hrc = false;
                        DataTable materialTable1 = LoadTable(tableName);
                        var validRow = materialTable1.Rows.Cast<DataRow>().FirstOrDefault(row =>
                             row["Марка материала"].ToString() == mark &&
                             row["Способ отливки"].ToString() == casting &&
                             bool.TryParse(row["HRC>=45"].ToString(), out bool hrcValue) && hrcValue == hrc);

                        if (validRow != null)
                        {
                            // Получаем значение BHP и присваиваем переменной
                            if (double.TryParse(validRow["BHP"].ToString(), out bshp))
                            {
                                resultLabel.Text = $"Все данные успешно проверены. Найдено значение BHP: {bshp}.";
                            }
                            else
                            {
                                resultLabel.Text = "Ошибка: значение BHP в таблице некорректно.";
                            }
                        }
                        else
                        {
                            resultLabel.Text = "Ошибка: не найдена строка, соответствующая заданным критериям.";
                        }
                    }
                    else if (tableName =="Бронза латунь больше 350")
                    {
                        bool hrc = false;
                        if (hardness >= 45)
                            hrc = true;
                        else
                            hrc = false;
                        DataTable bronz = LoadTable(tableName);
                        var validRow = bronz.Rows.Cast<DataRow>().FirstOrDefault(row =>
                             row["Марка материала"].ToString() == mark &&
                             row["Способ отливки"].ToString() == casting &&
                             double.TryParse(row["Скрость скольжения"].ToString(), out double velocity) && velocity== speed &&
                             bool.TryParse(row["HRC>=45"].ToString(), out bool hrcValue) && hrcValue == hrc);

                        if (validRow != null)
                        {
                            // Получаем значение BHP и присваиваем переменной
                            if (double.TryParse(validRow["BHP"].ToString(), out bshp))
                            {
                                resultLabel.Text = $"Все данные успешно проверены. Найдено значение BHP: {bshp}.";
                            }
                            else
                            {
                                resultLabel.Text = "Ошибка: значение BHP в таблице некорректно.";
                            }
                        }
                        else
                        {
                            resultLabel.Text = "Ошибка: не найдена строка, соответствующая заданным критериям.";
                        }
                    }
                    else
                    {
                        DataTable chugun= LoadTable(tableName);
                        var validRow = chugun.Rows.Cast<DataRow>().FirstOrDefault(row =>
                            row["Марка материала"].ToString() == mark &&
                            row["Материал червяка"].ToString() == matcher &&
                            double.TryParse(row["Скорость скольжения"].ToString(), out double velocity) && velocity==speed);
                        if (validRow != null)
                        {
                            if (double.TryParse(validRow["BHP"].ToString(), out bshp))
                            {
                                resultLabel.Text = $"Все данные успешно проверены. Найдено значение BHP: {bshp}.";
                            }
                            else
                            {
                                resultLabel.Text = "Ошибка: значение BHP в таблице некорректно.";
                            }
                        }
                        else
                        {
                            resultLabel.Text = "Ошибка: не найдена строка, соответствующая заданным критериям.";
                        }
                    }
                    double NH0 = Math.Pow(10, 7);
                    double n1 = 0;
                    switch (z1)
                    {
                        case 1:
                            n1 = 60;
                            break;
                        case 2:
                            n1 = 30;
                            break;
                        case 4:
                            n1 = 15;
                            break;
                    }
                    double n2 = 30;
                    double N1 = 0.15;
                    double NHE = 60 * 2 * n2;
                    double KHL = 0;
                    if (tableName == "Бронза менльше 300")
                        KHL = Math.Pow((bshp / NHE), 1 / 8);
                    else
                        KHL = 1;
                    double Bhp = bshp * KHL;
                    double T = 9.55*Math.Pow(10,3)*N1*(0.75/n2);
                    double a = (z2 + q) * Math.Pow((3.4*Math.Pow(10,7)*T/(Math.Pow((Bhp*z2),2)*q)),1/3);
                    double x = (a / m) - 0.5 * (q+z2);
                    Label dannie = new Label();
                    switch (tableName)
                    {
                        case "Чугун":
                            dannie.Text = $"Введенные данные:\nТип редуктора gbt: {gbt}\nЧисло витков червяка z1: {z1}\n" +
                               $"Число зубьев колеса z2: {z2}\nЗначение q: {q}\nМодуль m: {m}мм\n" +
                               $"Материал колеса matrl: {material}\nПредел прочности: strength {strength}\nМарка материала mark: {mark}\nСкорость скольжения v: {speed}m/s" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\nx: {x}";
                            break;
                        case "Бронза менльше 300":
                            dannie.Text= $"Введенные данные:\nТип редуктора: {gbt}\nЧисло витков червяка: {z1}\n" +
                               $"Число зубьев колеса: {z2}\nЗначение q: {q}\nМодуль: {m}мм\n" +
                               $"Материал колеса: {material}\nПредел прочности: {strength}\nМарка материала: {mark}\nСпособ отливки: {casting}\nHRC: {hardness}\nСкорость скольжения v: {speed}m/s" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\nx: {x}";
                            break;
                        case "Бронза латунь больше 350":
                            dannie.Text = $"Введенные данные:\nТип редуктора: {gbt}\nЧисло витков червяка: {z1}\n" +
                               $"Число зубьев колеса: {z2}\nЗначение q: {q}\nМодуль: {m}мм\n" +
                               $"Материал колеса: {material}\nПредел прочности: {strength}\nМарка материала: {mark}\nСпособ отливки: {casting}\nHRC: {hardness}" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\nx: {x}";
                            break;
                    }
                    //Label dannie = new Label($"Введенные данные:\nТип редуктора: {gbt}\nЧисло витков червяка: {z1}\n" +
                    //           $"Число зубьев колеса: {z2}\nЗначение q: {q}\nМодуль: {m}мм\n" +
                    //           $"Материал колеса: {material}\nПредел прочности: {strength}\nМарка материала: {mark}\nСпособ отливки: {casting}\nHRC: {hardness}" +
                    //           $"\nЧастота вращения вала червяка, n1: {n1}\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}\nx: {x}");
                    dynamicContentContainer.PackStart(dannie, false, false, 5);
                    dynamicContentContainer.ShowAll();
                    // Здесь можно добавить следующую логику расчета, исходя из собранных данных
                };
            };

        };
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
    private static string[] GetAnythingFromTable(string tableName, string rowName)
    {
        DataTable table = LoadTable(tableName);
        return table.AsEnumerable()
                    .Select(row => row[rowName].ToString())
                    .Distinct()
                    .ToArray();
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