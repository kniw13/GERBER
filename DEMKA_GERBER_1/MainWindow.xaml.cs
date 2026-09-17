using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using DEMKA_GERBER_1.Helper;
using DEMKA_GERBER_1.MODELS;
using DEMKA_GERBER_1.Pages;

namespace DEMKA_GERBER_1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new RequestListPage());
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Если мы вернулись на страницу списка, обновить данные
            if (e.Content is RequestListPage page)
            {
                page.LoadData();
            }
        }

    }
}