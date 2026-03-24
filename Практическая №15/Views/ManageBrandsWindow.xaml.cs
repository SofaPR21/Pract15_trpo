using System;
using System.Linq;
using System.Windows;
using Практическая__15.Models;
using Практическая__15.Services;

namespace Практическая__15.Views
{
    public partial class ManageBrandsWindow : Window
    {
        private readonly ElectronicsStoreBaseContext _context;

        public ManageBrandsWindow()
        {
            InitializeComponent();
            _context = DBService.Instance.Context;
            LoadBrands();
        }

        private void LoadBrands()
        {
            listBrands.ItemsSource = _context.Brands
                .ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleInputDialog("Введите название бренда:", "Добавление бренда");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                var brand = new Brand { Name = dialog.InputValue };
                _context.Brands.Add(brand);
                _context.SaveChanges();
                LoadBrands();

                MessageBox.Show("Бренд добавлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (listBrands.SelectedItem is Brand selected)
            {
                var dialog = new SimpleInputDialog("Введите новое название:", "Редактирование бренда", selected.Name);
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
                {
                    selected.Name = dialog.InputValue;
                    _context.SaveChanges();
                    LoadBrands();

                    MessageBox.Show("Бренд обновлен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите бренд для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listBrands.SelectedItem is Brand selected)
            {
                bool hasProducts = _context.Products.Any(p => p.BrandId == selected.Id);

                if (hasProducts)
                {
                    MessageBox.Show("Нельзя удалить бренд, у которого есть товары!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Удалить бренд \"{selected.Name}\"?",
                                            "Подтверждение",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Brands.Remove(selected);
                    _context.SaveChanges();
                    LoadBrands();

                    MessageBox.Show("Бренд удален!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите бренд для удаления", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}