using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DEMKA_GERBER_1.MODELS;

namespace DEMKA_GERBER_1.Pages
{
    /// <summary>
    /// Логика взаимодействия для RequestListPage.xaml
    /// </summary>
    public partial class RequestListPage : Page
    {
        public RequestListPage()
        {
            InitializeComponent();
            LoadData();
        }
        public void LoadData()
        {
            lv_Kartochki.ItemsSource = null; // очищаем перед загрузкой

            using (var Context = new DEMKA_GERBER())
            {
                var request = Context.Product_request
                    .Include("Partners")
                    .Include("Partners.Partner_type")
                    .Include("Partners.Oblast")
                    .ToList();

                var list = request.Select(r => new Request
                {
                    Request_Id = r.Request_Id,

                    
                    Partner_Type = r.Partners?.Partner_type?.Partner_Type_Name ?? "Неизвестно",
                    Partner_Name = r.Partners?.Partner_Name ?? "Неизвестно",
                    Adress =  (r.Partners?.Adress ?? ""),
                    Telephone = r.Partners?.phone_number ?? "",
                    Dir_FIO = (r.Partners?.Partner_dir_familiya ?? "") + " " + (r.Partners?.Partner_dir_name ?? "") + " " + (r.Partners?.Partner_dir_otch ?? ""),
                    Rating = r.Partners?.Rating,
                    Partner_id = r.Partners?.Partner_ID ?? 0,
                    Email = r.Partners?.email ?? "",

                
                    Cost = (r.Request_Ammount ?? 0) * (double)(r.Price_For_edeniza ?? 0) - (double)(r.PredOplata_ammount ?? 0),
                    OblastName = r.Partners?.Oblast?.Oblast_Name ?? "",
                    City = r.Partners?.City ?? "",

                    Request_Ammount = r.Request_Ammount,
                    Price_For_edeniza = (double?)r.Price_For_edeniza,
                    PredOplata_ammount = (double?)r.PredOplata_ammount
                }).ToList();

                lv_Kartochki.ItemsSource = list;
            }
        }
        public class Request
        {
            public string OblastName { get; set; }
            public string City { get; set; }
            public int Request_Id { get; set; }
            public string Partner_Type { get; set; }
            public string Partner_Name { get; set; }
            public string Adress { get; set; }
            public string Dir_FIO { get; set; }
            public string Email { get; set; }
            public int Partner_id { get; set; }
            public string Telephone { get; set; }
            public double? Rating { get; set; }
            public double? Cost { get; set; }
            public double? Request_Ammount { get; set; }
            public double? Price_For_edeniza { get; set; }
            public double? PredOplata_ammount { get; set; }
        }

        private void lv_Kartochki_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selecteditem = lv_Kartochki.SelectedItem as Request;
            NavigationService.Navigate(new RedactDeletePage(selecteditem));
        }

        private void btn_sozdat_zayavku_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate( new RedactDeletePage(null));
        }
    }
}
