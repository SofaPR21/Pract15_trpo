using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Практическая__15.Models;
using Практическая__15.Services;

namespace Практическая__15.Views
{
    public partial class AddEditProductWindow : Window, INotifyPropertyChanged
    {
        private readonly ElectronicsStoreBaseContext _context;
        private readonly Product _editingProduct;
        private bool _isEditMode;

        private int? _categoryId;
        private int? _selectedBrandId;
        private string _productName;
        private string _description;
        private decimal _price;
        private int _stock;

        public int? CategoryId
        {
            get => _categoryId;
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? SelectedBrandId
        {
            get => _selectedBrandId;
            set
            {
                if (_selectedBrandId != value)
                {
                    _selectedBrandId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProductName
        {
            get => _productName;
            set
            {
                if (_productName != value)
                {
                    _productName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Stock
        {
            get => _stock;
            set
            {
                if (_stock != value)
                {
                    _stock = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class TagCheckBox
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }

        public ObservableCollection<TagCheckBox> TagsList { get; set; } = new ObservableCollection<TagCheckBox>();

        public AddEditProductWindow(Product product = null)
        {
            InitializeComponent();
            _context = DBService.Instance.Context;
            DataContext = this;

            LoadComboBoxes();
            LoadTags();

            if (product != null)
            {
                _isEditMode = true;
                _editingProduct = product;
                LoadProductData();
                Title = "Редактирование товара";
            }
            else
            {
                _isEditMode = false;
                Title = "Добавление товара";
            }

            TagsItemsControl.ItemsSource = TagsList;
        }

        private void LoadComboBoxes()
        {
            cmbCategory.ItemsSource = _context.Categories.OrderBy(c => c.Name).ToList();
            cmbBrand.ItemsSource = _context.Brands.OrderBy(b => b.Name).ToList();
        }

        private void LoadTags()
        {
            var allTags = _context.Tags.OrderBy(t => t.Name).ToList();
            TagsList.Clear();
            foreach (var tag in allTags)
            {
                TagsList.Add(new TagCheckBox
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    IsSelected = false
                });
            }
        }

        private void LoadProductData()
        {
            ProductName = _editingProduct.Name;
            Description = _editingProduct.Description;
            Price = _editingProduct.Price;
            Stock = _editingProduct.Stock;
            CategoryId = _editingProduct.CategoryId;
            SelectedBrandId = _editingProduct.BrandId;

            var nameBinding = txtName.GetBindingExpression(TextBox.TextProperty);
            nameBinding?.UpdateTarget();

            var descBinding = txtDescription.GetBindingExpression(TextBox.TextProperty);
            descBinding?.UpdateTarget();

            var priceBinding = txtPrice.GetBindingExpression(TextBox.TextProperty);
            priceBinding?.UpdateTarget();

            var stockBinding = txtQuantity.GetBindingExpression(TextBox.TextProperty);
            stockBinding?.UpdateTarget();

            double rating = (double)_editingProduct.Rating;
            string ratingStr = rating.ToString("0.0");
            foreach (ComboBoxItem item in cmbRating.Items)
            {
                if (item.Content.ToString() == ratingStr)
                {
                    cmbRating.SelectedItem = item;
                    break;
                }
            }

            cmbCategory.SelectedValue = _editingProduct.CategoryId;
            cmbBrand.SelectedValue = _editingProduct.BrandId;

            var productTags = _editingProduct.Tags.Select(t => t.Id).ToList();
            foreach (var tag in TagsList)
            {
                tag.IsSelected = productTags.Contains(tag.Id);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Product product;

                if (_isEditMode)
                {
                    product = _editingProduct;
                }
                else
                {
                    product = new Product();
                    _context.Products.Add(product);
                }

                product.Name = ProductName.Trim();
                product.Description = Description.Trim();
                product.Price = Price;
                product.Stock = Stock;

                if (cmbRating.SelectedItem is ComboBoxItem selectedRating)
                {
                    string ratingValue = selectedRating.Content.ToString();
                    product.Rating = decimal.Parse(ratingValue, System.Globalization.CultureInfo.InvariantCulture);
                }

                product.CategoryId = ((Category)cmbCategory.SelectedItem).Id;
                product.BrandId = ((Brand)cmbBrand.SelectedItem).Id;
                product.CreatedAt = DateOnly.FromDateTime(DateTime.Now);

                _context.SaveChanges();

                product.Tags.Clear();
                foreach (var tagCheck in TagsList.Where(t => t.IsSelected))
                {
                    var tag = _context.Tags.Find(tagCheck.Id);
                    if (tag != null)
                    {
                        product.Tags.Add(tag);
                    }
                }

                _context.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}