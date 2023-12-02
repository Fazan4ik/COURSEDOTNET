using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace COURSEDOTNET
{
    /// <summary>
    /// Interaction logic for BasketProduct.xaml
    /// </summary>
    public partial class BasketShop : Window
    {
        private String ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\visual studio projects\COURSEDOTNET\DataUsers.mdf"";Integrated Security=True";
        private string activeEmail;
        public BasketShop(string activeEmail)
        {
            InitializeComponent();
            LoadBasketInWrapPanel();
            this.activeEmail = activeEmail;
        }
        private SmtpClient? GetSmtpClient()
        {
            #region get and check config
            String? host = App.GetConfiguration("smtp:host");
            if (host == null)
            {
                MessageBox.Show("Error getting host");
                return null;
            }
            String? portString = App.GetConfiguration("smtp:port");
            if (portString == null)
            {
                MessageBox.Show("Error getting port");
                return null;
            }
            int port;
            try { port = int.Parse(portString); }
            catch
            {
                MessageBox.Show("Error parsing port");
                return null;
            }
            String? email = App.GetConfiguration("smtp:email");
            if (email == null)
            {
                MessageBox.Show("Error getting email");
                return null;
            }
            String? password = App.GetConfiguration("smtp:password");
            if (password == null)
            {
                MessageBox.Show("Error getting password");
                return null;
            }
            String? sslString = App.GetConfiguration("smtp:ssl");
            if (sslString == null)
            {
                MessageBox.Show("Error getting ssl");
                return null;
            }
            bool ssl;
            try { ssl = bool.Parse(sslString); }
            catch
            {
                MessageBox.Show("Error parsing ssl");
                return null;
            }
            #endregion

            return new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
        }



        private void LoadBasketInWrapPanel()
        {
            BasketWrap.Children.Clear();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string getBasketQuery = "SELECT Name, NumQuantity, PriceWithOne FROM BasketProduct";
                    using (SqlCommand getBasketCommand = new SqlCommand(getBasketQuery, connection))
                    using (SqlDataReader reader = getBasketCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            int numQuantity = reader.GetInt32(1);
                            double priceWithOne = reader.GetDouble(2); 
                            TextBlock productTextBlock = new TextBlock
                            {
                                Text = $"Назва -> {productName} - Кількість - {numQuantity} - ціна од. - {priceWithOne} грн",
                                FontSize = 16,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(5, 5, 5, 5)
                            };
                            BasketWrap.Children.Add(productTextBlock);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні кошика: " + ex.Message);
            }
            AddTotalAmount();
        }

        private void AddTotalAmount()
        {
            float totalAmount = CalculateTotalAmount();
            TextBlock totalAmountTextBlock = new TextBlock
            {
                Text = $"Загальна сума: {totalAmount} грн",
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 5, 5, 5)
            };
            BasketWrap.Children.Add(totalAmountTextBlock);
        }

        private float CalculateTotalAmount()
        {
            float totalAmount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string getTotalAmountQuery = "SELECT SUM(NumQuantity * PriceWithOne) FROM BasketProduct";
                    using (SqlCommand getTotalAmountCommand = new SqlCommand(getTotalAmountQuery, connection))
                    {
                        object result = getTotalAmountCommand.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalAmount = Convert.ToSingle(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при обчисленні загальної суми: " + ex.Message);
            }

            return totalAmount;
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SmtpClient? smtpClient = GetSmtpClient();

                string messageBody = GetProductsFromDatabase();


                messageBody += $"\nСумма замволення ->{CalculateTotalAmount()} грн\nПідтвердіть замовлення, надіславши відповідь.";

                if (smtpClient != null)
                {
                    string activeEmail = this.activeEmail;

                    MailMessage mailMessage = new MailMessage(App.GetConfiguration("smtp:email")!, activeEmail, "Замовлення", messageBody);
                    smtpClient.Send(mailMessage);

                    MessageBox.Show("Замовлення відправлено. Підтвердіть його, відправивши відповідь на електронний лист.");
                }
                else
                {
                    MessageBox.Show("Не вдалося отримати SmtpClient. Перевірте конфігурацію SMTP.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при відправці замовлення: " + ex.Message);
            }
        }


        public string GetProductsFromDatabase()
        {
            List<BasketProduct> products = new List<BasketProduct>();
            string messageBody = "Ваше замовлення:\n";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string getProductsQuery = $"SELECT Name, NumQuantity, PriceWithOne FROM BasketProduct";
                    using (SqlCommand getProductsCommand = new SqlCommand(getProductsQuery, connection))
                    using (SqlDataReader reader = getProductsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            int numQuantity = reader.GetInt32(1);
                            double priceWithOne = reader.GetDouble(2);
                            messageBody += $"Назва -> {productName} - Кількість - {numQuantity} - ціна од. - {priceWithOne} грн\n";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при отриманні продуктів з бази даних: " + ex.Message);
            }

            return messageBody;
        }



        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
