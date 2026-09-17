using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DEMKA_GERBER_1.MODELS;
using static DEMKA_GERBER_1.Pages.RequestListPage;

namespace DEMKA_GERBER_1.Pages
{
    public partial class RedactDeletePage : Page
    {
        private Request _currentRequest;
        private DEMKA_GERBER db => Helper.Helper.GetContext();

        public RedactDeletePage(Request request)
        {
            InitializeComponent();
            _currentRequest = request;
            lb_name.Content = _currentRequest == null ? "Создание заявки" : "Редактирование заявки";
            LoadBoxes();
            LoadData();
        }

        private int GetNextPartnerId()
        {
            int maxId = db.Partners.Any() ? db.Partners.Max(p => p.Partner_ID) : 0;
            return maxId < 22 ? 22 : maxId + 1;
        }
        private int GetNextRequestId()
        {
            int maxId = db.Product_request.Any() ? db.Product_request.Max(r => r.Request_Id) : 0;
            return maxId < 80 ? 80 : maxId + 1;
        }
        private void LoadBoxes()
        {
            type_box.ItemsSource = db.Partner_type.ToList();
            type_box.DisplayMemberPath = "Partner_Type_Name";

            cmb_Oblast.ItemsSource = db.Oblast.ToList();
            cmb_Oblast.DisplayMemberPath = "Oblast_Name";
            cmb_Oblast.SelectedValuePath = "Oblast_ID";
        }

        private void LoadData()
        {
            if (_currentRequest == null) return;

            txt_Address.Text = _currentRequest.Adress;
            txt_Director.Text = _currentRequest.Dir_FIO;
            txt_Email.Text = _currentRequest.Email;
            txt_Name.Text = _currentRequest.Partner_Name;
            txt_Phone.Text = _currentRequest.Telephone;
            txt_Rating.Text = _currentRequest.Rating?.ToString() ?? "";
            txt_City.Text = _currentRequest.City ?? "";
            txt_Ammount.Text = _currentRequest.Request_Ammount?.ToString() ?? "";
            txt_PricePerUnit.Text = _currentRequest.Price_For_edeniza?.ToString() ?? "";
            txt_Prepayment.Text = _currentRequest.PredOplata_ammount?.ToString() ?? "";

            // Выбор области
            if (cmb_Oblast.ItemsSource != null)
            {
                var oblasts = cmb_Oblast.ItemsSource as System.Collections.IEnumerable;
                var selected = oblasts?.Cast<Oblast>().FirstOrDefault(o => o.Oblast_Name == _currentRequest.OblastName);
                cmb_Oblast.SelectedItem = selected;
            }

            // Выбор типа партнёра
            if (type_box.ItemsSource != null)
            {
                var types = type_box.ItemsSource as System.Collections.IEnumerable;
                var selected = types?.Cast<Partner_type>().FirstOrDefault(pt => pt.Partner_Type_Name == _currentRequest.Partner_Type);
                type_box.SelectedItem = selected;
            }

            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            if (double.TryParse(txt_Ammount.Text, out var amount) &&
                double.TryParse(txt_PricePerUnit.Text, out var price) &&
                double.TryParse(txt_Prepayment.Text, out var prepayment))
                txt_TotalPrice.Text = (amount * price - prepayment).ToString("F2");
            else
                txt_TotalPrice.Text = "0.00";
        }

        private void txt_Ammount_TextChanged(object sender, TextChangedEventArgs e) => UpdateTotalPrice();
        private void txt_PricePerUnit_TextChanged(object sender, TextChangedEventArgs e) => UpdateTotalPrice();
        private void txt_Prepayment_TextChanged(object sender, TextChangedEventArgs e) => UpdateTotalPrice();

        private void btn_Nazad_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields(out var partnerName, out var address, out var phone, out var email,
                out var directorFio, out var rating, out var city, out var selectedType, out var selectedOblast))
                return;

            if (_currentRequest != null)
                UpdateRequest(partnerName, address, phone, email, directorFio, rating, city, selectedType, selectedOblast);
            else
                CreateRequest(partnerName, address, phone, email, directorFio, rating, city, selectedType, selectedOblast);
        }

        private bool ValidateFields(out string partnerName, out string address, out string phone, out string email,
            out string directorFio, out int rating, out string city, out Partner_type selectedType, out Oblast selectedOblast)
        {
            partnerName = txt_Name.Text.Trim();
            address = txt_Address.Text.Trim();
            phone = txt_Phone.Text.Trim();
            email = txt_Email.Text.Trim();
            directorFio = txt_Director.Text.Trim();
            city = txt_City.Text.Trim();

            selectedType = type_box.SelectedItem as Partner_type;
            selectedOblast = cmb_Oblast.SelectedItem as Oblast;

            if (string.IsNullOrEmpty(partnerName) || string.IsNullOrEmpty(address))
            { MessageBox.Show("Наименование и адрес обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); }

            if (!int.TryParse(txt_Rating.Text.Trim(), out rating))
            { MessageBox.Show("Рейтинг должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }

            if (selectedType == null)
            { MessageBox.Show("Выберите тип партнера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }

            if (selectedOblast == null)
            { MessageBox.Show("Выберите область.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }

            if (string.IsNullOrWhiteSpace(city))
            { MessageBox.Show("Введите город.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }

            return true;
        }

        private void UpdateRequest(string partnerName, string address, string phone, string email,
            string directorFio, int rating, string city, Partner_type selectedType, Oblast selectedOblast)
        {
            try
            {
                var existing = db.Product_request.Include("Partners").FirstOrDefault(r => r.Request_Id == _currentRequest.Request_Id);
                if (existing == null) { MessageBox.Show("Запись не найдена.", "Ошибка"); return; }

                var partner = existing.Partners;
                if (partner != null)
                {
                    partner.Partner_Name = partnerName;
                    partner.Adress = address;
                    partner.City = city;
                    partner.Oblast_ID = selectedOblast.Oblast_ID;
                    partner.Rating = rating;
                    partner.phone_number = phone;
                    partner.email = email;
                    partner.Partner_Type_id = selectedType.Partner_Type_ID;

                    var fio = directorFio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    partner.Partner_dir_familiya = fio.Length >= 3 ? fio[0] : directorFio;
                    partner.Partner_dir_name = fio.Length >= 3 ? fio[1] : "";
                    partner.Partner_dir_otch = fio.Length >= 3 ? fio[2] : "";
                }

                existing.Request_Ammount = double.TryParse(txt_Ammount.Text, out var amt) ? amt : 0;
                existing.Price_For_edeniza = decimal.TryParse(txt_PricePerUnit.Text, out var price) ? price : 0;
                existing.PredOplata_ammount = decimal.TryParse(txt_Prepayment.Text, out var prepay) ? prepay : 0;

                db.SaveChanges();
                MessageBox.Show("Данные сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex) { ShowFullException(ex); }
        }

        private void CreateRequest(string partnerName, string address, string phone, string email,
    string directorFio, int rating, string city, Partner_type selectedType, Oblast selectedOblast)
        {
            try
            {
               
                if (db.Partners.Any(p => p.Partner_Name == partnerName && p.Partner_Type_id == selectedType.Partner_Type_ID))
                {
                    MessageBox.Show("Партнер с таким названием и типом уже существует.", "Предупреждение");
                    return;
                }

                
                var newPartner = new Partners
                {
                    Partner_ID = GetNextPartnerId(),   
                    Partner_Type_id = selectedType.Partner_Type_ID,
                    Partner_Name = partnerName,
                    Adress = address,
                    City = city,
                    phone_number = phone,
                    email = email,
                    Rating = rating,
                    Oblast_ID = selectedOblast.Oblast_ID
                };

                var fio = directorFio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                newPartner.Partner_dir_familiya = fio.Length >= 3 ? fio[0] : directorFio;
                newPartner.Partner_dir_name = fio.Length >= 3 ? fio[1] : "";
                newPartner.Partner_dir_otch = fio.Length >= 3 ? fio[2] : "";

                db.Partners.Add(newPartner);
                db.SaveChanges();

                
                var newRequest = new Product_request
                {
                    Request_Id = GetNextRequestId(),  
                    Partner_id = newPartner.Partner_ID,
                    Request_Ammount = double.TryParse(txt_Ammount.Text, out var amt) ? amt : 0,
                    Price_For_edeniza = decimal.TryParse(txt_PricePerUnit.Text, out var price) ? price : 0,
                    PredOplata_ammount = decimal.TryParse(txt_Prepayment.Text, out var prepay) ? prepay : 0,
                    Date_of_request = DateTime.Now,
                    Request_status_id = 1,
                    Product_id = null,
                    Srok_Izgotovlenia = null,
                    Sotrudnik_prinyal_id = null,
                    Date_of_deliver = null
                };

                db.Product_request.Add(newRequest);
                db.SaveChanges();

                MessageBox.Show("Заявка успешно создана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                ShowFullException(ex);
            }
        }


        private void ShowFullException(Exception ex)
        {
            string full = "";
            while (ex != null) { full += ex.Message + "\n"; ex = ex.InnerException; }
            MessageBox.Show($"Ошибка: {full}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Это действие необратимо! Вы готовы продолжить?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var existing = db.Product_request.FirstOrDefault(r => r.Request_Id == _currentRequest.Request_Id);
                    if (existing != null)
                    {
                        db.Product_request.Remove(existing);
                        db.SaveChanges();
                        MessageBox.Show("Заявка удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService.GoBack();
                    }
                }
                catch (Exception ex) { ShowFullException(ex); }
            }
        }
    }
}