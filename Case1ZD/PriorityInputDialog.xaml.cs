using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Case1ZD
{
    public partial class PriorityInputDialog : Window
    {
        public double Priority { get; private set; } = 0.5;

        public PriorityInputDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void TxtPriority_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Разрешаем только цифры и одну запятую/точку
            e.Handled = !(double.TryParse(newText.Replace('.', ','), out _)) ||
                        newText.Count(c => c == ',' || c == '.') > 1;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtPriority.Text.Replace('.', ','), out double result) &&
                result >= 0 && result <= 1)
            {
                Priority = result;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите число от 0,0 до 1,0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtPriority.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}