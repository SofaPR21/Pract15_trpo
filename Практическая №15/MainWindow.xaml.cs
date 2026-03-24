using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Практическая__15.Models;
using Практическая__15.Services;
using Практическая__15.Validation;
using Практическая__15.Views;

namespace Практическая__15
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly ElectronicsStoreBaseContext _context;
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public ObservableCollection<Product> Products { get; set; } = new();
        public ICollectionView ProductsView { get; set; }

        private string _searchQuery = string.Empty;
        private string _priceFrom;
        private string _priceTo;

        public string PriceFrom
        {
            get => _priceFrom;
            set
            {
                if (_priceFrom != value)
                {
                    _priceFrom = value;
                    OnPropertyChanged();
                    ValidatePriceFrom();
                    ValidatePriceRange();
                    ProductsView?.Refresh();
                }
            }
        }

        public string PriceTo
        {
            get => _priceTo;
            set
            {
                if (_priceTo != value)
                {
                    _priceTo = value;
                    OnPropertyChanged();
                    ValidatePriceTo();
                    ValidatePriceRange();
                    ProductsView?.Refresh();
                }
            }
        }

        public string UserRole { get; }

        private void ValidatePriceFrom()
        {
            ClearErrors(nameof(PriceFrom));

            if (string.IsNullOrWhiteSpace(PriceFrom))
                return;

            var validator = new PriceRangeValidationRule();
            var result = validator.Validate(PriceFrom, CultureInfo.CurrentCulture);

            if (!result.IsValid)
            {
                AddError(nameof(PriceFrom), result.ErrorContent.ToString());
            }
        }

        private void ValidatePriceTo()
        {
            ClearErrors(nameof(PriceTo));

            if (string.IsNullOrWhiteSpace(PriceTo))
                return;

            var validator = new PriceRangeValidationRule();
            var result = validator.Validate(PriceTo, CultureInfo.CurrentCulture);

            if (!result.IsValid)
            {
                AddError(nameof(PriceTo), result.ErrorContent.ToString());
            }
        }

        private void ValidatePriceRange()
        {
            ClearErrors("PriceRange");

            if (string.IsNullOrWhiteSpace(PriceFrom) || string.IsNullOrWhiteSpace(PriceTo))
                return;

            if (!decimal.TryParse(PriceFrom.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal from))
                return;

            if (!decimal.TryParse(PriceTo.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal to))
                return;

            if (from > to)
            {
                string error = $"Цена 'от' ({from}) не может быть больше цены 'до' ({to})";
                AddError(nameof(PriceFrom), error);
                AddError(nameof(PriceTo), error);
                AddError("PriceRange", error);
            }
        }

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName) 
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(e => e.Value).ToList();

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

            if (!string.IsNullOrWhiteSpace(PriceFrom))
            {
                if (decimal.TryParse(PriceFrom.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceFrom))
                {
                    if (product.Price < priceFrom) return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(PriceTo))
            {
                if (decimal.TryParse(PriceTo.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceTo))
                {
                    if (product.Price > priceTo) return false;
                }
            }

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
            _searchQuery = string.Empty;
            PriceFrom = string.Empty;
            PriceTo = string.Empty;
            cmbCategory.SelectedItem = null;
            cmbBrand.SelectedItem = null;

            if (cmbSort.SelectedIndex != -1)
            {
                cmbSort.SelectedIndex = -1;
            }

            ProductsView?.SortDescriptions.Clear();

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
                    case "StockAsc":  
                        ProductsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
                        break;
                    case "StockDesc":
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