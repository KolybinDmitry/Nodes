using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nodes
{
    class Node
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Node(string title, string content) 
        {
            Title = title;
            Content = content;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<DateTime?, List<Node>> nodes;
        DateTime? selectedDate = null;

        public MainWindow()
        {
            InitializeComponent();
            // устанавливаем дату сегодняшнюю
            datePicker.SelectedDate = DateTime.Now;
            selectedDate = datePicker.SelectedDate;
            // десериализация заметок
            nodes = File.Exists("nodes.json")
                ? JsonConvert.DeserializeObject<Dictionary<DateTime?, List<Node>>>(File.ReadAllText("nodes.json"))
                : new Dictionary<DateTime?, List<Node>>();
            printListBoxItems();
        }

        ~MainWindow() 
        {
            // сериализация заметок
            File.WriteAllText("nodes.json", JsonConvert.SerializeObject(nodes));
        }

        // Вывод списка заметок по выбранной дате
        private void printListBoxItems()
        {
            listBox.Items.Clear();
            if (nodes.ContainsKey(selectedDate))
                foreach (var node in nodes[selectedDate])
                    listBox.Items.Add(node.Title);
        }

        // Проверка на то, что новое название заметки по данной дате уникально
        private bool checkOnOriginalTitle()
        {
            if (textBoxTitle.Text.Equals(""))
            {
                MessageBox.Show("Нельзя создать заметку без названия! Переименуйте заметку", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!nodes.ContainsKey(selectedDate))
                return true;

            foreach (var node in nodes[selectedDate])
                if (node.Title.Equals(textBoxTitle.Text))
                {
                    MessageBox.Show("Заметка с таким названием уже есть! Переименуйте заметку", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            return true;
        }

        // Выбор даты
        private void buttonChooseDate_Click(object sender, RoutedEventArgs e)
        {
            selectedDate = datePicker.SelectedDate;
            printListBoxItems();
        }

        // Создание новой заметки
        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDate == null)
            {
                MessageBox.Show("Выберете дату", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!checkOnOriginalTitle())
                return;
            
            Node node = new Node(textBoxTitle.Text, textBoxContent.Text);
            if (!nodes.ContainsKey(selectedDate))
                nodes[selectedDate] = new List<Node>();
            nodes[selectedDate].Add(node);
            printListBoxItems();
        }

        // Изменение выбранной заметки заметки
        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            var title = listBox.SelectedItem;
            if (title == null)
            {
                MessageBox.Show("Невыбрана заметка", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            for (int i = 0; i < nodes[selectedDate].Count; i++)
                if (nodes[selectedDate][i].Title.Equals(title))
                {
                    if (!textBoxTitle.Text.Equals(title) && !checkOnOriginalTitle())
                        return;

                    nodes[selectedDate][i].Title = textBoxTitle.Text;
                    nodes[selectedDate][i].Content = textBoxContent.Text;
                }
            printListBoxItems();
        }

        // Удаление выбранной заметки 
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var title = listBox.SelectedItem;
            if (title == null)
            {
                MessageBox.Show("Невыбрана заметка", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            for (int i = 0; i < nodes[selectedDate].Count; i++)
                if (nodes[selectedDate][i].Title.Equals(title))
                    nodes[selectedDate].RemoveAt(i);
            printListBoxItems();
        }

        // Выбор заметки из списка по выбранной дате
        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBoxTitle.Text = listBox.SelectedItem.ToString();
            foreach (var node in nodes[selectedDate])
                if (node.Title == textBoxTitle.Text)
                {
                    textBoxContent.Text = node.Content;
                    break;
                }
        }

        // Очистка полей "Название" и "Описание" на форме
        private void buttonNewNode_Click(object sender, RoutedEventArgs e)
        {
            textBoxTitle.Text = "";
            textBoxContent.Text = "";
        }

        // Удлаение всего хранилища заметок
        private void buttonDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            nodes.Clear();
            printListBoxItems();
        }
    }
}