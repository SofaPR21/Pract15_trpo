using System;
using System.Linq;
using System.Windows;
using Практическая__15.Models;
using Практическая__15.Services;

namespace Практическая__15.Views
{
    public partial class ManageTagsWindow : Window
    {
        private readonly ElectronicsStoreBaseContext _context;

        public ManageTagsWindow()
        {
            InitializeComponent();
            _context = DBService.Instance.Context;
            LoadTags();
        }

        private void LoadTags()
        {
            listTags.ItemsSource = _context.Tags
                .ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleInputDialog("Введите название тега:", "Добавление тега");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                var tag = new Tag { Name = dialog.InputValue };
                _context.Tags.Add(tag);
                _context.SaveChanges();
                LoadTags();

                MessageBox.Show("Тег добавлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (listTags.SelectedItem is Tag selected)
            {
                var dialog = new SimpleInputDialog("Введите новое название:", "Редактирование тега", selected.Name);
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
                {
                    selected.Name = dialog.InputValue;
                    _context.SaveChanges();
                    LoadTags();

                    MessageBox.Show("Тег обновлен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите тег для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listTags.SelectedItem is Tag selected)
            {
                bool hasProducts = _context.Products.Any(p => p.Tags.Any(t => t.Id == selected.Id));
                if (hasProducts)
                {
                    MessageBox.Show("Нельзя удалить тег, который используется в товарах!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Удалить тег \"{selected.Name}\"?",
                                            "Подтверждение",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Tags.Remove(selected);
                    _context.SaveChanges();
                    LoadTags();

                    MessageBox.Show("Тег удален!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите тег для удаления", "Внимание",
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