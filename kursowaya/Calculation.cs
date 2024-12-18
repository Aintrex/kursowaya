using Gtk;
using System;
using System.Data.OleDb;
using System.Data;

class Calculation
{
    private static readonly string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=kursowaya.accdb;";
    public static void HandleShowTables(VBox dynamicContentContainer, Button showTablesButton, Window mainWin)
    {
        // Очищаем контейнер от предыдущих элементов
        foreach (var child in dynamicContentContainer.Children)
        {
            dynamicContentContainer.Remove(child);
        }

        if (showTablesButton.Label == "Показать все таблицы")
        {

            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                // Получаем список всех таблиц в базе данных
                DataTable schemaTable = connection.GetSchema("Tables");

                // Проходим по всем таблицам и отображаем их
                foreach (DataRow row in schemaTable.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();

                    // Получаем данные для каждой таблицы
                    // В данном примере предполагается, что название таблицы не начинается с "System"
                    if (!tableName.StartsWith("MSys"))
                    {
                        // Получаем структуру таблицы (например, имена столбцов)
                        DataTable tableStructure = connection.GetSchema("Columns", new string[] { null, null, tableName, null });

                        // Получаем список столбцов
                        var columnNames = tableStructure.Rows.Cast<DataRow>()
                                                            .Select(r => r["COLUMN_NAME"].ToString())
                                                            .ToArray();

                        // Отображаем таблицу
                        DisplayTable(dynamicContentContainer, tableName, tableName, columnNames);
                    }
                }
            }

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
        Label labelm = new Label("Введите модуль");
        Entry fieldm = new Entry();
        Label labelZ2 = new Label("Введите число зубьев колеса (28-80):");
        Entry fieldZ2 = new Entry();

        Label labelQ = new Label("Введите q (8, 9, 10, 12 или 14):");
        Entry fieldQ = new Entry();

        Label labelZ1 = new Label("Введите число витков червяка (1, 2 или 4):");
        Entry fieldZ1 = new Entry();

        Label labelkpd = new Label("Введите кпд:");
        Entry fieldkpd = new Entry();

       

        

        Button nextStepButton = new Button("Далее");
        Label resultLabel = new Label();

        // Добавляем начальные элементы в контейнер
        dynamicContentContainer.PackStart(labelGbt, false, false, 5);
        dynamicContentContainer.PackStart(comboGbt, false, false, 5);
        dynamicContentContainer.PackStart(labelm, false, false, 5);
        dynamicContentContainer.PackStart(fieldm, false, false, 5);
        dynamicContentContainer.PackStart(labelQ, false, false, 5);
        dynamicContentContainer.PackStart(fieldQ, false, false, 5);
        dynamicContentContainer.PackStart(labelZ1, false, false, 5);
        dynamicContentContainer.PackStart(fieldZ1, false, false, 5);
        dynamicContentContainer.PackStart(labelZ2, false, false, 5);
        dynamicContentContainer.PackStart(fieldZ2, false, false, 5);
        dynamicContentContainer.PackStart(labelkpd, false, false, 5);
        dynamicContentContainer.PackStart(fieldkpd, false, false, 5);
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
        double kpd=0;
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
            if(!double.TryParse(fieldkpd.Text, out kpd))
            {
                resultLabel.Text = "Ошибка: введите корректное значение кпд.";
                return;
            }
            
            if (!int.TryParse(fieldZ1.Text, out z1) || (z1 != 1 && z1 != 2 && z1 != 4))
            {
                resultLabel.Text = "Ошибка: введите корректное значение числа витков (1, 2 или 4).";
                return;
            }
            DataTable kpdtb = LoadTable("КПД");
            bool isvalid = kpdtb.Rows.Cast<DataRow>().Any(row =>
                int.Parse(row["Число витков червяка"].ToString())==z1 &&
                double.Parse(row["КПД мин"].ToString())<=kpd &&
                double.Parse(row["КПД макс"].ToString())>=kpd);
            if (!isvalid)
            {
                resultLabel.Text = "Ошибка: значение КПД не входит в диапазон";
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
                    tableName = "Бронза меньше 300";

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
                    if (tableName == "Бронза меньше 300")
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
                    if (tableName == "Бронза меньше 300")
                    {
                        KHL = Math.Pow((bshp / NHE), 1 / 8);
                        //if (!(0.67<=KHL && KHL>=1.7)) 
                        //{
                        //    resultLabel.Text = "Ошибка: KHL должен быть в пределах от 0.67 до 1.7";
                        //    dynamicContentContainer.ShowAll();
                        //    return;
                        //}
                    }
                    else
                        KHL = 1;
                    double Bhp = bshp * KHL;
                    double T = 9.55*Math.Pow(10,3)*N1*(kpd/n2);
                    double a = (z2 + q) * Math.Pow((3.4*Math.Pow(10,7)*T/(Math.Pow((Bhp*z2),2)*q)),1/3);
                    int ao = RoundValue(a, gbt, "Межосевые расстояния");
                    double q1 = q;
                    double q2 = 0;
                    double m1 = FindMAndAdjust(ao, q, z2, "Допускаемые сочетания");

                    double FindMAndAdjust(double a, double initialQ, int initialZ2, string tablenm)
                    {
                        DataTable table = LoadTable(tablenm);
                        double xx = 0;
                        double mm = 0;

                        // 1. Найти m для x = 0
                        double calculatedM = a / (0.5 * (initialQ + initialZ2));

                        // Найти ближайшее допустимое значение m из таблицы
                        var validMValues = table.Rows.Cast<DataRow>()
                                                     .Select(row => double.Parse(row["m"].ToString()))
                                                     .OrderBy(value => Math.Abs(value - calculatedM))
                                                     .ToList();
                        mm = validMValues.FirstOrDefault();

                        // Вычислить x
                        xx = (a / mm) - 0.5 * (initialQ + initialZ2);
                        if (xx >= -1 && xx <= 1)
                            return mm;
                        // 2. Проверить x и скорректировать z2
                        if (xx < -1 || xx > 1)
                        {
                            z2 = initialZ2;
                            z2 += 2;
                            xx = (a / mm) - 0.5 * (initialQ + z2);
                            if (xx < -1 || xx > 1)
                            {
                                z2 -= 4;
                                xx = (a / mm) - 0.5 * (initialQ + z2);
                            }
                            if (xx >= -1 && xx <= 1)
                                return mm;
                        }

                        // 3. Если z2 не помогло, изменить q
                        var validQValues = table.Rows.Cast<DataRow>()
                                                     .Where(row => double.Parse(row["m"].ToString()) == mm)
                                                     .Select(row => double.Parse(row["q"].ToString()))
                                                     .ToList();

                        double bestQ = initialQ;
                        double bestX = xx;

                        foreach (var qq in validQValues)
                        {
                            double tempX = (a / mm) - 0.5 * (qq + initialZ2);
                            if (Math.Abs(tempX - 1) < Math.Abs(bestX - 1) || (tempX >= -1 && tempX <= 1 && Math.Abs(tempX - 1) < Math.Abs(bestX)))
                            {
                                bestQ = qq;
                                bestX = tempX;
                            }
                        }
                        q2= bestQ;
                        // Вернуть скорректированное m
                        return mm;
                    }
                    if (q2 != 0)
                        q1 = q2;
                    double x = (a / m1) - 0.5 * (q1+z2);
                    Label dannie = new Label();
                    switch (tableName)
                    {
                        case "Чугун":
                            dannie.Text = $"Введенные данные:\nТип редуктора gbt: {gbt}\nЧисло витков червяка z1: {z1}\n" +
                               $"Число зубьев колеса z2: {z2}\nЗначение q: {q}\nМодуль m: {m}мм\n" +
                               $"Материал колеса matrl: {material}\nПредел прочности: strength {strength}\nМарка материала mark: {mark}\nСкорость скольжения v: {speed}m/s" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\na по табл: {ao}mm\nx: {x}";
                            break;
                        case "Бронза меньше 300":
                            dannie.Text= $"Введенные данные:\nТип редуктора: {gbt}\nЧисло витков червяка: {z1}\n" +
                               $"Число зубьев колеса: {z2}\nЗначение q: {q}\nМодуль: {m}мм\n" +
                               $"Материал колеса: {material}\nПредел прочности: {strength}\nМарка материала: {mark}\nСпособ отливки: {casting}\nHRC: {hardness}\nСкорость скольжения v: {speed}m/s" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\na по табл: {ao}mm\nm по табл: {m1}mm\nq по табл: {q2}mm\nx: {x}";
                            break;
                        case "Бронза латунь больше 350":
                            dannie.Text = $"Введенные данные:\nТип редуктора: {gbt}\nЧисло витков червяка: {z1}\n" +
                               $"Число зубьев колеса: {z2}\nЗначение q: {q}\nМодуль: {m}мм\n" +
                               $"Материал колеса: {material}\nПредел прочности: {strength}\nМарка материала: {mark}\nСпособ отливки: {casting}\nHRC: {hardness}" +
                               $"\nЧастота вращения вала червяка, n1: {n1}min^-1\n Частота вращения колеса n2: {n2}min^-1\nNHE: {NHE}\nKHL: {KHL}\nbshp: {bshp}\nBhp: {Bhp}\nT2: {T}\na: {a}mm\na по табл: {ao}mm\nx: {x}";
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

    static int RoundValue(double calculatedValue, string reducerType, string tablen)
    {
        DataTable table = new DataTable();
        table = LoadTable(tablen);
        if (reducerType == "общего назначения")
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table), "Таблица не предоставлена для редуктора общего назначения.");
            }

            // Получаем список допустимых значений из таблицы
            var validValues = table.Rows.Cast<DataRow>()
                                        .Select(row => double.Parse(row["1й ряд"].ToString()))
                                        .OrderBy(value => value)
                                        .ToList();

            // Находим ближайшее значение из таблицы
            double nearestValue = validValues.OrderBy(value => Math.Abs(value - calculatedValue)).FirstOrDefault();
            return (int)Math.Round(nearestValue);
        }
        else
        {
            // Для других редукторов: округление до ближайшего числа с окончанием на 0 или 5
            double remainder = calculatedValue % 10;

            if (remainder <= 2 || (remainder > 2 && remainder <= 5))
            {
                // Округляем вниз к ближайшему числу с окончанием на 0 или 5
                return (int)(calculatedValue - remainder + (remainder > 2 && remainder <= 5 ? 5 : 0));
            }
            else
            {
                // Округляем вверх к ближайшему числу с окончанием на 0
                return (int)(calculatedValue - remainder + 10);
            }
        }
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

    private static string[] GetAnythingFromTable(string tableName, string rowName)
    {
        DataTable table = LoadTable(tableName);
        return table.AsEnumerable()
                    .Select(row => row[rowName].ToString())
                    .Distinct()
                    .ToArray();
    }
    public static void HandleInputData1(VBox dynamicContentContainer)
    {
        List<(int id, int a, double x)> results = new List<(int id, int a, double x)>(); // Список для результатов (id детали, значение a)
        DataTable sostav = LoadTable("Состав СЕ");
        DataTable detailsTable = LoadTable("Деталь");
        DataTable SeTable = LoadTable("Сборочная единица");
        DataTable mqz1 = LoadTable("Допускаемые сочетания");
        DataTable kpdTable = LoadTable("КПД");
        var groupSE = sostav.AsEnumerable()
            .GroupBy(row => row["senom"].ToString())
            .ToDictionary(
            g => g.Key,
            g => g.Select(r => r["detkod"].ToString()).ToList()
            );
        foreach (var k in groupSE)
        {
            var kodSE = k.Key;
            int kodSE1 = Convert.ToInt32(kodSE);
            var detlist = k.Value;
            if (detlist.Count<2 || detlist.Count>2)
            {
                results.Add((kodSE1, 0, -2));
                continue; 
            }
            //Поиск червяка и колеса
            string kodCh = detlist.FirstOrDefault(code => detailsTable.AsEnumerable().Any(row => row["detkod"].ToString() == code && row["detname"].ToString() == "червяк"));
            string kodWh = detlist.FirstOrDefault(code => detailsTable.AsEnumerable().Any(row => row["detkod"].ToString() == code && row["detname"].ToString() == "колесо"));
            if (kodCh == null || kodWh== null)
            {
                results.Add((kodSE1, 0, -2));
                continue;
            }


            //Сбор переменных из таблиц
            DataRow chrow = detailsTable.AsEnumerable().FirstOrDefault(row => row["detkod"].ToString() == kodCh);
            //z1,q,matcher,HRC ДЛЯ ЧЕРВЯКА
            int z1 = int.TryParse(chrow["z1"].ToString(), out int tz1) ? tz1 : 0;
            int q = int.TryParse(chrow["q"].ToString(), out int tq) ? tq : 0;
            string matcher = chrow["matcher"].ToString();
            double HRC = double.TryParse(chrow["HRC"].ToString(), out double HRC2) ? HRC2 : 0;
            double N1 = double.TryParse(chrow["N"].ToString(), out double tn1) ? tn1 : 0;
            DataRow whrow = detailsTable.AsEnumerable().FirstOrDefault(row => row["detkod"].ToString() == kodWh);
            double n1 = double.TryParse(chrow["n1"].ToString(), out double tn11) ? tn11 : 0;
            //casting, mark, mw,vs ДЛЯ КОЛЕСА
            string casting = whrow["casting"].ToString();
            string mark = whrow["mark"].ToString();
            string mw = whrow["mw"].ToString();
            double vs = double.TryParse(whrow["vs"].ToString(), out double tvs)? tvs : 0;
            int bv = int.TryParse(whrow["bv"].ToString(), out int tbv) ? tbv : 0;
            int z2 = int.TryParse(whrow["z2"].ToString(), out int tz2) ? tz2 : 0;
            double n2 = double.TryParse(chrow["n2"].ToString(), out double tn2) ? tn2 : 0;
            int t = int.TryParse(whrow["t"].ToString(), out int tt) ? tt : 0;
            //gbt, n, m ДЛЯ СЕ
            DataRow serow = SeTable.AsEnumerable().FirstOrDefault(row => row["senom"].ToString()==kodSE);
            string gbt = serow["gbt"].ToString();
            double n = double.TryParse(serow["n"].ToString(), out double tn) ? tn : 0;
            double m = double.TryParse(serow["m"].ToString(), out double tm) ? tm : 0;


            //Проверка m q z1
            bool isValidCombination = mqz1.Rows.Cast<DataRow>().Any(row =>
                double.Parse(row["m"].ToString()) == m &&
                int.Parse(row["q"].ToString()) == q &&
                int.Parse(row["z1"].ToString()) == z1);
            if (!isValidCombination)
            {
                results.Add((kodSE1, 0, -2));
                continue;
            }


            //Проверка кпд
            bool isValidKPD = kpdTable.Rows.Cast<DataRow>().Any(row =>
            double.Parse(row["КПД мин"].ToString()) <= n && double.Parse(row["КПД макс"].ToString()) >= n);
            DataRow rowmat=null;
            bool rowcheck = true;
            DataTable materialTable;
            //bshp по таблицам
            int bshp = 0;
            if (mw=="Чугун")
            {
                materialTable = LoadTable("Чугун");
                rowmat = materialTable.Rows.Cast<DataRow>().FirstOrDefault(row =>
                row["Марка материала"].ToString() == mark &&
                row["Материал червяка"].ToString() == matcher &&
                double.TryParse(row["Скорость скольжения"].ToString(), out double velocity) && velocity == vs);

                if (rowmat != null)
                {
                    bshp = int.TryParse(rowmat["BHP"].ToString(), out int tbshp) ? tbshp : 0;
                }
                else rowcheck = false;
            }
            else if (mw =="Бронза" && bv<300)
            {
                materialTable = LoadTable("Бронза меньше 300");
               rowmat = materialTable.Rows.Cast<DataRow>().FirstOrDefault(row =>
               row["Марка материала"].ToString() == mark &&
               row["Способ отливки"].ToString() == casting &&
               bool.TryParse(row["HRC>=45"].ToString(), out bool thrc) && thrc == HRC>=45);

                if (rowmat !=null)
                {
                    bshp=int.TryParse(rowmat["BHP"].ToString(), out int tbshp) ? tbshp : 0;
                }
                else rowcheck = false;
            }
            else if ((mw == "Бронза" || mw=="Латунь")  &&  bv>350)
            {
                materialTable = LoadTable("Бронза меньше 300");
                rowmat = materialTable.Rows.Cast<DataRow>().FirstOrDefault(row =>
                row["Марка материала"].ToString() == mark &&
                row["Способ отливки"].ToString() == casting &&
                bool.TryParse(row["HRC>=45"].ToString(), out bool thrc) && (thrc == (HRC >= 45)) &&
                double.TryParse(row["Скорость скольжения"].ToString(), out double velocity) && velocity == vs);

                if (rowmat != null)
                {
                    bshp = int.TryParse(rowmat["BHP"].ToString(), out int tbshp) ? tbshp : 0;
                }
                else rowcheck = false;
            }    
            if (!rowcheck)
            {
                results.Add((kodSE1, 0, -2));
                continue;
            }
            if (bshp==0)
            {
                results.Add((kodSE1, 0, -2));
                continue;
            }
            double NH0 = Math.Pow(10, 7);
            //double n1 = 0;
            //switch (z1)
            //{
            //    case 1:
            //        n1 = 60;
            //        break;
            //    case 2:
            //        n1 = 30;
            //        break;
            //    case 4:
            //        n1 = 15;
            //        break;
            //}
            //double n2 = 30;
            double NHE = 60 * t * n2;
            double KHL = 0;
                KHL = Math.Pow((bshp / NHE), 1 / 8);
            double Bhp = bshp * KHL;
            double T = 9.55 * Math.Pow(10, 3) * N1 * (n / n2);
            double a = (z2 + q) * Math.Pow((3.4 * Math.Pow(10, 7) * T / (Math.Pow((Bhp * z2), 2) * q)), 1 / 3);
            int ao = RoundValue(a, gbt, "Межосевые расстояния");
            double q1 = q;
            double q2 = 0;
            double m1 = FindMAndAdjust(ao, q, z2, "Допускаемые сочетания");

            double FindMAndAdjust(double a, double initialQ, int initialZ2, string tablenm)
            {
                DataTable table = LoadTable(tablenm);
                double xx = 0;
                double mm = 0;

                // 1. Найти m для x = 0
                double calculatedM = a / (0.5 * (initialQ + initialZ2));

                // Найти ближайшее допустимое значение m из таблицы
                var validMValues = table.Rows.Cast<DataRow>()
                                             .Select(row => double.Parse(row["m"].ToString()))
                                             .OrderBy(value => Math.Abs(value - calculatedM))
                                             .ToList();
                mm = validMValues.FirstOrDefault();

                // Вычислить x
                xx = (a / mm) - 0.5 * (initialQ + initialZ2);
                if (xx >= -1 && xx <= 1)
                    return mm;
                // 2. Проверить x и скорректировать z2
                if (xx < -1 || xx > 1)
                {
                    z2 = initialZ2;
                    z2 += 2;
                    xx = (a / mm) - 0.5 * (initialQ + z2);
                    if (xx < -1 || xx > 1)
                    {
                        z2 -= 4;
                        xx = (a / mm) - 0.5 * (initialQ + z2);
                    }
                    if (xx >= -1 && xx <= 1)
                        return mm;
                }

                // 3. Если z2 не помогло, изменить q
                var validQValues = table.Rows.Cast<DataRow>()
                                             .Where(row => double.Parse(row["m"].ToString()) == mm)
                                             .Select(row => double.Parse(row["q"].ToString()))
                                             .ToList();

                double bestQ = initialQ;
                double bestX = xx;

                foreach (var qq in validQValues)
                {
                    double tempX = (a / mm) - 0.5 * (qq + initialZ2);
                    if (Math.Abs(tempX - 1) < Math.Abs(bestX - 1) || (tempX >= -1 && tempX <= 1 && Math.Abs(tempX - 1) < Math.Abs(bestX)))
                    {
                        bestQ = qq;
                        bestX = tempX;
                    }
                }
                q2 = bestQ;
                // Вернуть скорректированное m
                return mm;
            }
            if (q2 != 0)
                q1 = q2;
            double x = (a / m1) - 0.5 * (q1 + z2);
            results.Add((kodSE1, ao, x));
        }

        

    

        // Запись результатов в таблицу "Результаты"
        SaveResultsToDatabase(results);
        
    }


    private static void SaveResultsToDatabase(List<(int id, int a, double x)> results)
    {
        using (var connection = new OleDbConnection(connectionString)) // Укажите строку подключения
        {
            connection.Open();

            foreach (var (id, a, x) in results)
            {
                // Проверяем, существует ли запись с таким id
                using (var checkCommand = new OleDbCommand("SELECT COUNT(*) FROM [Результат] WHERE senom = ?", connection))
                {
                    checkCommand.Parameters.AddWithValue("?", id);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        // Если запись существует, обновляем значение a
                        using (var updateCommand = new OleDbCommand("UPDATE [Результат] SET a = ?, x=? WHERE senom = ?", connection))
                        {
                            updateCommand.Parameters.AddWithValue("?", a);
                            updateCommand.Parameters.AddWithValue("?", x);
                            updateCommand.Parameters.AddWithValue("?", id);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Если записи нет, добавляем новую
                        using (var insertCommand = new OleDbCommand("INSERT INTO [Результат] (senom, a, x) VALUES (?, ?, ?)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("?", id);
                            insertCommand.Parameters.AddWithValue("?", a);
                            insertCommand.Parameters.AddWithValue("?", x);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }


}