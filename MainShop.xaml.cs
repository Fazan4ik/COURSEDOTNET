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
        private string activeEmail;
        public MainShop(string activeEmail)
        {
            InitializeComponent();
            this.activeEmail = activeEmail;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            textStart.Visibility = Visibility.Hidden;
            MenuItem nameItem = (MenuItem)sender;
            ProductStackPanel.Children.Clear();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                if (nameItem.Name == "All")
                {
                    string query = "SELECT * FROM ProductsList";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productId = reader["Id"].ToString();
                                string productName = reader["Name"].ToString();
                                double productPrice = Convert.ToDouble(reader["Price"]);
                                string categoryFolder = reader["Category"].ToString();
                                string productImage = reader["Image"].ToString();
                                string productQuantity = reader["Quantity"].ToString();

                                AddProductList(productId, productName, productPrice, productImage, categoryFolder, productQuantity);
                            }
                        }
                    }
                }
                else
                {
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
                                string categoryFolder = reader["Category"].ToString();
                                string productImage = reader["Image"].ToString();
                                string productQuantity = reader["Quantity"].ToString();

                                AddProductList(productId, productName, productPrice, productImage, categoryFolder, productQuantity);
                            }
                        }
                    }
                }
            }
        }


        private void AddProductList(string productId,string productName, double productPrice, string productImage,string categoryFolder,string productQuantity)
        {
            StackPanel productPanel = new StackPanel { 
                Orientation = Orientation.Vertical, 
                Margin = new Thickness(10), 
                VerticalAlignment = VerticalAlignment.Top, 
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = $"product{productId}"
            };

            TextBlock productNameTextBlock = new TextBlock { Text = productName, FontSize = 16, FontWeight = FontWeights.Bold,TextAlignment = TextAlignment.Center };
            TextBlock productPriceTextBlock = new TextBlock { Text = $"Ціна: {productPrice} грн", FontSize = 14, TextAlignment = TextAlignment.Center };
            TextBlock productQuantityTextBlock = new TextBlock { Text = $"Кількість: {productQuantity} ", FontSize = 10, TextAlignment = TextAlignment.Center };
            string productImagePath = $"{picFolder}\\{categoryFolder}\\{productImage}";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(productImagePath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            Image productImageControl = new Image { Source = bitmap, Width = 200, Height = 200, Margin = new Thickness(0, 1, 0, 0) };

            productPanel.Children.Add(productImageControl);
            productPanel.Children.Add(productNameTextBlock);
            productPanel.Children.Add(productPriceTextBlock);
            productPanel.Children.Add(productQuantityTextBlock);

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center };

            Button buttonBuy = new Button
            {
                Name = $"buttonBuy",                
                Content = "+",
                Width = 30,
                Height = 30,
                FontSize = 20,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 5, 5)
            };
            buttonBuy.Click += BuyButton_Click;

            TextBox numProductsTextBox = new TextBox
            {
                Name = $"numProductsTextBox",
                Text = "1",
                Width = 40,
                Height = 30,
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                MaxLength = 3,

                Margin = new Thickness(5, 5, 5, 5)
            };
            numProductsTextBox.PreviewTextInput += NumProductsTextBox_PreviewTextInput;
            Button buttonRemove = new Button
            {
                Name = $"buttonRemove",
                Content = "-",
                Width = 30,
                Height = 30,
                FontSize = 20,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 5, 5, 5)
            };
            buttonRemove.Click += RemoveButton_Click;

            buttonPanel.Children.Add(buttonBuy);
            buttonPanel.Children.Add(numProductsTextBox);
            buttonPanel.Children.Add(buttonRemove);

            productPanel.Children.Add(buttonPanel);

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

                StackPanel buttonPanel = (StackPanel)buyButton.Parent;

                StackPanel productPanel = (StackPanel)buttonPanel.Parent;

                Border productBorder = (Border)productPanel.Parent;

                TextBlock productNameTextBlock = (TextBlock)productPanel.Children[1];
                TextBlock productPriceTextBlock = (TextBlock)productPanel.Children[2];

                TextBox numProductsTextBox = (TextBox)buttonPanel.Children[1];
                int numProducts = int.Parse(numProductsTextBox.Text);
                string productName = productNameTextBlock.Text;
                string productId = productPanel.Name;
                double productPrice = double.Parse(productPriceTextBlock.Text.Split(' ')[1]);

                AddToBasket(productId, productName, productPrice, numProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при купівлі продукта: " + ex.Message);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button removeButton = (Button)sender;

                StackPanel buttonPanel = (StackPanel)removeButton.Parent;

                StackPanel productPanel = (StackPanel)buttonPanel.Parent;

                Border productBorder = (Border)productPanel.Parent;

                TextBlock productNameTextBlock = (TextBlock)productPanel.Children[1];
                TextBlock productPriceTextBlock = (TextBlock)productPanel.Children[2];

                TextBox numProductsTextBox = (TextBox)buttonPanel.Children[1];
                int numProducts = int.Parse(numProductsTextBox.Text);
                string productName = productNameTextBlock.Text;
                string productId = productPanel.Name;
                double productPrice = double.Parse(productPriceTextBlock.Text.Split(' ')[1]);

                RemoveFromBasket(productId, productName, productPrice, numProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні продукта з кошика: " + ex.Message);
            }
        }


        private void NumProductsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void AddToBasket(string productId, string productName, double productPrice, int numProducts)
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

                            if (numQuantity + numProducts <= productQuantity)
                            {
                                if (numQuantity > 0 && numQuantity < productQuantity)
                                {
                                    UpdateProductQuantity(connection, idList, numProducts);
                                }
                                else if (productCount == 0)
                                {
                                    AddProductToBasket(connection, idList, productName, productPrice, numProducts);
                                }
                                else
                                {
                                    MessageBox.Show($"{productName} більше не має на складі, ви не можете його замовити.");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Ви не можете додати {numProducts} одиниць продукта {productName} у кошик. Загальна кількість перевищує ліміт. Зараз їх у кошику {numQuantity} одиниць продукта");
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
        private void RemoveFromBasket(string productId, string productName, double productPrice, int numProducts)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    int idList = int.Parse(productId.Substring(7));

                    string checkProductExistenceQuery = $"SELECT COUNT(*) FROM BasketProduct WHERE ProductId = {idList}";
                    using (SqlCommand checkProductExistenceCommand = new SqlCommand(checkProductExistenceQuery, connection))
                    {
                        int productExistenceCount = (int)checkProductExistenceCommand.ExecuteScalar();

                        if (productExistenceCount == 0)
                        {
                            MessageBox.Show($"{productName} відсутній у кошику.");
                            return; 
                        }
                    }
                    string removeFromBasketQuery = $"UPDATE BasketProduct SET NumQuantity = NumQuantity - {numProducts} WHERE ProductId = {idList}";
                    using (SqlCommand removeFromBasketCommand = new SqlCommand(removeFromBasketQuery, connection))
                    {
                        removeFromBasketCommand.ExecuteNonQuery();
                    }

                    string checkNumQuantityQuery = $"SELECT NumQuantity FROM BasketProduct WHERE ProductId = {idList}";
                    using (SqlCommand checkNumQuantity = new SqlCommand(checkNumQuantityQuery, connection))
                    {
                        int numQuantity = Convert.ToInt32(checkNumQuantity.ExecuteScalar());

                        if (numQuantity <= 0)
                        {
                            string deleteProductQuery = $"DELETE FROM BasketProduct WHERE ProductId = {idList}";
                            using (SqlCommand deleteProductCommand = new SqlCommand(deleteProductQuery, connection))
                            {
                                deleteProductCommand.ExecuteNonQuery();
                            }

                            MessageBox.Show($"{productName} було повністю видалено з кошика.");
                        }
                        else
                        {
                            MessageBox.Show($"{numProducts} одиниць продукта {productName} було видалено з кошика. Залишилося {numQuantity} одиниць продукта.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при видаленні продукта з кошика: " + ex.Message);
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



        private void AddProductToBasket(SqlConnection connection, int productId, string productName, double productPrice, int numProducts)
        {
            try
            {
                string addProductQuery = "INSERT INTO BasketProduct (ProductId, Name, PriceWithOne, NumQuantity) VALUES (@ProductId, @ProductName, @ProductPrice, @ProductQuantity)";

                using (SqlCommand addProductCommand = new SqlCommand(addProductQuery, connection))
                {
                    addProductCommand.Parameters.AddWithValue("@ProductId", productId);
                    addProductCommand.Parameters.AddWithValue("@ProductName", productName);
                    addProductCommand.Parameters.AddWithValue("@ProductPrice", productPrice);
                    addProductCommand.Parameters.AddWithValue("@ProductQuantity", numProducts);


                    addProductCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні нового продукта до кошика: " + ex.Message);
            }
        }






        private void UpdateProductQuantity(SqlConnection connection, int productId, int numProducts)
        {
            try
            {
                string updateQuantityQuery = $"UPDATE BasketProduct SET NumQuantity = NumQuantity + {numProducts} WHERE ProductId = @ProductId";
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

        private BasketShop basketProductWindow; 

        private void BasketButton_Click(object sender, RoutedEventArgs e)
        {
            if (basketProductWindow == null || !basketProductWindow.IsVisible)
            {
                basketProductWindow = new BasketShop(activeEmail);
                basketProductWindow.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (basketProductWindow != null && basketProductWindow.IsVisible)
            {
                basketProductWindow.Hide();
                if (Application.Current.Windows.OfType<Window>().Count(w => w.IsVisible) == 0)
                {
                    Application.Current.Shutdown();
                }
            }
        }


    }
}
