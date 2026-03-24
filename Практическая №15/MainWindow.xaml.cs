using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Практическая__15.Models;
using Практическая__15.Services;
using Практическая__15.Views;

namespace Практическая__15
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ElectronicsStoreBaseContext _context;

        public ObservableCollection<Product> Products { get; set; } = new();
        public ICollectionView ProductsView { get; set; }

        private string _searchQuery = string.Empty;

        public string UserRole { get; }
        public MainWindow(string role)
        {
            UserRole = role;
            InitializeComponent();
            DataContext = this;

            _context = DBService.Instance.Context;

            if (role == "Manager")
            {
                txtRoleTitle.Text = "Вход как менеджер";
                txtRoleTitle.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 196, 180));
            }
            else
            {
                txtRoleTitle.Text = "Вход как посетитель";
            }

            LoadData();

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            if (role == "Guest")
            {
                ManagerPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadData()
        {
            try
            {
                var products = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Tags)
                    .OrderBy(p => p.Name)
                    .ToList();

                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                cmbCategory.ItemsSource = _context.Categories.OrderBy(c => c.Name).ToList();
                cmbBrand.ItemsSource = _context.Brands.OrderBy(b => b.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterProducts(object obj)
        {
            if (obj is not Product product) return false;

            if (!string.IsNullOrWhiteSpace(_searchQuery) &&
                !product.Name.Contains(_searchQuery, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(txtPriceFrom.Text) &&
                decimal.TryParse(txtPriceFrom.Text, out decimal priceFrom) &&
                product.Price < priceFrom)
                return false;

            if (!string.IsNullOrWhiteSpace(txtPriceTo.Text) &&
                decimal.TryParse(txtPriceTo.Text, out decimal priceTo) &&
                product.Price > priceTo)
                return false;

            if (cmbCategory.SelectedItem is Category selectedCategory &&
                product.CategoryId != selectedCategory.Id)
                return false;

            if (cmbBrand.SelectedItem is Brand selectedBrand &&
                product.BrandId != selectedBrand.Id)
                return false;

            return true;
        }

        private void DeleteProduct(int productId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var product = _context.Products
                        .Include(p => p.Tags)
                        .FirstOrDefault(p => p.Id == productId);

                    if (product != null)
                    {
                        product.Tags.Clear();

                        _context.Products.Remove(product);
                        _context.SaveChanges();

                        LoadData();
                        ProductsView.Refresh();

                        MessageBox.Show("Товар успешно удален!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearch.Text;
            ProductsView?.Refresh();
        }

        private void PriceFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProductsView?.Refresh();
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductsView?.Refresh();
        }

        private void BrandFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductsView?.Refresh();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            txtPriceFrom.Text = string.Empty;
            txtPriceTo.Text = string.Empty;
            cmbCategory.SelectedItem = null;
            cmbBrand.SelectedItem = null;

            ProductsView?.Refresh();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsView == null) return;

            ProductsView.SortDescriptions.Clear();

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selected)
            {
                string sortBy = selected.Tag.ToString();

                switch (sortBy)
                {
                    case "Name":
                        ProductsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        break;
                    case "PriceAsc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                        break;
                    case "PriceDesc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                        break;
                    case "QuantityAsc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
                        break;
                    case "QuantityDesc":
                        ProductsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Descending));
                        break;
                }
            }

            ProductsView.Refresh();
        }

        private void ProductsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (UserRole != "Manager") return;

            var selectedProduct = ProductsListView.SelectedItem as Product;
            if (selectedProduct != null)
            {
                EditProduct(selectedProduct.Id);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadData();
                ProductsView.Refresh();
            }
        }

        private void EditProduct(int productId)
        {
            var product = _context.Products
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                var editWindow = new AddEditProductWindow(product);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                    ProductsView.Refresh();
                }
            }
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new ManageCategoriesWindow();
            if (manageWindow.ShowDialog() == true)
            {
                LoadData();
                ProductsView.Refresh();
            }
        }

        private void ManageBrands_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new ManageBrandsWindow();
            if (manageWindow.ShowDialog() == true)
            {
                LoadData();
                ProductsView.Refresh();
            }
        }

        private void ManageTags_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new ManageTagsWindow();
            if (manageWindow.ShowDialog() == true)
            {
                LoadData();
                ProductsView.Refresh();
            }
        }

        private void DeleteSelectedProduct_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = ProductsListView.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Пожалуйста, выберите товар для удаления.", "Нет выбора",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeleteProduct(selectedProduct.Id);
        }
    }
}