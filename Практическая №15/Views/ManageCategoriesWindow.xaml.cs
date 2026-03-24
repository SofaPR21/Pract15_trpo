using System;
using System.Linq;
using System.Windows;
using Практическая__15.Models;
using Практическая__15.Services;

namespace Практическая__15.Views
{
    public partial class ManageCategoriesWindow : Window
    {
        private readonly ElectronicsStoreBaseContext _context;

        public ManageCategoriesWindow()
        {
            InitializeComponent();
            _context = DBService.Instance.Context;
            LoadCategories();
        }

        private void LoadCategories()
        {
            listCategories.ItemsSource = _context.Categories
                .ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleInputDialog("Введите название категории:", "Добавление категории");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                var category = new Category { Name = dialog.InputValue };
                _context.Categories.Add(category);
                _context.SaveChanges();
                LoadCategories();

                MessageBox.Show("Категория добавлена!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (listCategories.SelectedItem is Category selected)
            {
                var dialog = new SimpleInputDialog("Введите новое название:", "Редактирование категории", selected.Name);
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
                {
                    selected.Name = dialog.InputValue;
                    _context.SaveChanges();
                    LoadCategories();

                    MessageBox.Show("Категория обновлена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listCategories.SelectedItem is Category selected)
            {
                bool hasProducts = _context.Products.Any(p => p.CategoryId == selected.Id);

                if (hasProducts)
                {
                    MessageBox.Show("Нельзя удалить категорию, в которой есть товары!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Удалить категорию \"{selected.Name}\"?",
                                            "Подтверждение",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Categories.Remove(selected);
                    _context.SaveChanges();
                    LoadCategories();

                    MessageBox.Show("Категория удалена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для удаления", "Внимание",
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