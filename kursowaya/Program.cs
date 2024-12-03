using Gtk;
using System;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();

        // Главное окно
        Window mainWin = new Window("Proektirovochniy raschet");
        mainWin.SetDefaultSize(1080, 720);
        mainWin.DeleteEvent += delegate { Application.Quit(); };

        // Основной контейнер
        VBox mainVBox = new VBox(false, 5);

        // Кнопка "Показать все таблицы"
        Button showTablesButton = new Button("Показать все таблицы");
        mainVBox.PackStart(showTablesButton, false, false, 5);

        // Кнопка "Ввести данные и провести расчет"
        Button inputDataButton = new Button("Ввести данные и провести расчет");
        mainVBox.PackStart(inputDataButton, false, false, 5);

        // Прокручиваемый контейнер для отображения таблиц и результата
        VBox dynamicContentContainer = new VBox(false, 5);
        ScrolledWindow scrolledWindow = new ScrolledWindow();
        scrolledWindow.AddWithViewport(dynamicContentContainer);
        scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        mainVBox.PackStart(scrolledWindow, true, true, 5);

        // Обработчик кнопки "Показать все таблицы"
        showTablesButton.Clicked += (sender, e) =>
        {
            // Если кнопка переключена на "Назад"
            if (showTablesButton.Label == "Назад")
            {
                dynamicContentContainer.Children.ToList().ForEach(child => dynamicContentContainer.Remove(child));
                showTablesButton.Label = "Показать все таблицы";
                inputDataButton.Label = "Ввести данные и провести расчет";
            }
            else
            {
                // Отображение таблиц
                Calculation.HandleShowTables(dynamicContentContainer, showTablesButton, mainWin);
                inputDataButton.Label = "Ввести данные и провести расчет";
            }
        };

        //// Обработчик кнопки "Ввести данные и провести расчет"
        inputDataButton.Clicked += (sender, e) =>
        {
           // dynamicContentContainer.Children.ToList().ForEach(child => dynamicContentContainer.Remove(child));
            Calculation.HandleInputData(dynamicContentContainer);
            inputDataButton.Label = "Ввести данные заново";
        };

        // Добавление содержимого в главное окно
        mainWin.Add(mainVBox);
        mainWin.ShowAll();

        Application.Run();
    }
}
