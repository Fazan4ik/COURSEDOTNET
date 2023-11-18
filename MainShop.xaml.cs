using Microsoft.Data.SqlClient;
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
using System.Windows.Shapes;

namespace COURSEDOTNET
{
    public partial class MainShop : Window
    {

        private String ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\visual studio projects\COURSEDOTNET\DataUsers.mdf"";Integrated Security=True";
        private const String picFolder = "D:\\visual studio projects\\COURSEDOTNET\\PICS";

        public MainShop()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem nameItem = (MenuItem)sender;
            ProductStackPanel.Children.Clear();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string categoryCheckQuery = $"SELECT COUNT(*) FROM ProductsList WHERE TRIM(Category) = '{nameItem.Name}'";
                using (SqlCommand categoryCheckCommand = new SqlCommand(categoryCheckQuery, connection))
                {
                    int categoryCount = (int)categoryCheckCommand.ExecuteScalar();
                    if (categoryCount == 0)
                    {
                        MessageBox.Show($"Категорія '{nameItem.Header}' не знайдена в базі даних.");
                        return;
                    }
                }

                string query = $"SELECT * FROM ProductsList WHERE TRIM(Category) = '{nameItem.Name}'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productId = reader["Id"].ToString();
                            string productName = reader["Name"].ToString();
                            double productPrice = Convert.ToDouble(reader["Price"]);
                            string productImage = reader["Image"].ToString();

                            AddProductList(productId,productName, productPrice, productImage, nameItem.Name);
                        }
                    }
                }
            }
        }

        private void AddProductList(string productId,string productName, double productPrice, string productImage,string categoryFolder)
        {
            StackPanel productPanel = new StackPanel { 
                Orientation = Orientation.Vertical, 
                Margin = new Thickness(10), 
                VerticalAlignment = VerticalAlignment.Top, 
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = $"product{productId}"
            };

            TextBlock productNameTextBlock = new TextBlock { Text = productName, FontSize = 16, FontWeight = FontWeights.Bold };
            TextBlock productPriceTextBlock = new TextBlock { Text = $"Ціна: {productPrice} грн", FontSize = 14};

            string productImagePath = $"{picFolder}\\{categoryFolder}\\{productImage}";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(productImagePath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            Image productImageControl = new Image { Source = bitmap, Width = 200, Height = 200, Margin = new Thickness(0, 1, 0, 0) };

            productPanel.Children.Add(productImageControl);
            productPanel.Children.Add(productNameTextBlock);
            productPanel.Children.Add(productPriceTextBlock);

            Button button = new Button();
            button.Name = $"buttonBuy";
            button.Content = "Придбати";
            button.Width = 80;
            button.Height = 30;
            button.Click += BuyButton_Click;

            productPanel.Children.Add(button);

            Border productBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5, 5, 5, 5),
                Height = 330,
                Width = 300
            };

            productBorder.Child = productPanel;

            ProductStackPanel.Children.Add(productBorder);
        }


        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button buyButton = (Button)sender;

                StackPanel productPanel = (StackPanel)buyButton.Parent;

                Border productBorder = (Border)productPanel.Parent;

                TextBlock productNameTextBlock = (TextBlock)productPanel.Children[1];
                TextBlock productPriceTextBlock = (TextBlock)productPanel.Children[2];

                string productName = productNameTextBlock.Text;
                string productId = productPanel.Name;
                double productPrice = double.Parse(productPriceTextBlock.Text.Split(' ')[1]);

                AddToBasket(productId, productName, productPrice);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при купівлі продукта: " + ex.Message);
            }
        }

        private void AddToBasket(string productId, string productName, double productPrice)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    int idList = int.Parse(productId.Substring(7));

                    int productQuantity = GetProductQuantity(connection, idList);

                    string checkProductQuery = $"SELECT COUNT(*) FROM BasketProduct WHERE ProductId = '{idList}'";
                    using (SqlCommand checkProductCommand = new SqlCommand(checkProductQuery, connection))
                    {
                        int productCount = (int)checkProductCommand.ExecuteScalar();

                        string getNumQuantityQuery = $"SELECT NumQuantity FROM BasketProduct WHERE ProductId = {idList}";
                        using (SqlCommand checkNumQuantity = new SqlCommand(getNumQuantityQuery, connection))
                        {
                            int numQuantity = Convert.ToInt32(checkNumQuantity.ExecuteScalar());
                            if (numQuantity > 0 && numQuantity < productQuantity)
                            {
                                UpdateProductQuantity(connection, idList);
                            }
                            else if (productCount == 0)
                            {
                                AddProductToBasket(connection, idList, productName, productPrice);
                            }
                            else
                            {
                                MessageBox.Show($"{productName} більше не має на складі, ви не можете його замовити.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні продукта до кошика: " + ex.Message);
                MessageBox.Show(ex.StackTrace);
            }
        }



        private int GetProductQuantity(SqlConnection connection, int productId)
        {
            try
            {
                string selectProductQuantityQuery = $"SELECT Quantity FROM ProductsList WHERE Id = {productId}";
                using (SqlCommand selectProductQuantityCommand = new SqlCommand(selectProductQuantityQuery, connection))
                {
                    object quantity = selectProductQuantityCommand.ExecuteScalar();

                    if (quantity != null)
                    {
                        return (int)quantity;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при отриманні кількості продукту: " + ex.Message);
            }
            return 0;
        }



        private void AddProductToBasket(SqlConnection connection, int productId, string productName, double productPrice)
        {
            try
            {
                string addProductQuery = "INSERT INTO BasketProduct (ProductId, Name, PriceWithOne, NumQuantity) VALUES (@ProductId, @ProductName, @ProductPrice, 1)";

                using (SqlCommand addProductCommand = new SqlCommand(addProductQuery, connection))
                {
                    addProductCommand.Parameters.AddWithValue("@ProductId", productId);
                    addProductCommand.Parameters.AddWithValue("@ProductName", productName);
                    addProductCommand.Parameters.AddWithValue("@ProductPrice", productPrice);

                    addProductCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні нового продукта до кошика: " + ex.Message);
            }
        }






        private void UpdateProductQuantity(SqlConnection connection, int productId)
        {
            try
            {
                string updateQuantityQuery = "UPDATE BasketProduct SET NumQuantity = NumQuantity + 1 WHERE ProductId = @ProductId";
                using (SqlCommand updateQuantityCommand = new SqlCommand(updateQuantityQuery, connection))
                {
                    updateQuantityCommand.Parameters.AddWithValue("@ProductId", productId);
                    updateQuantityCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при оновленні кількості продукта у кошику: " + ex.Message);
            }
        }









    }
}
