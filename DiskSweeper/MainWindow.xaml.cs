using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DiskSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource CurrentCancellationTokenSource;
        private List<string> History = new List<string>();
        private int HistoryPosition = -1;

        public MainWindow()
        {
            InitializeComponent();

            this.PathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            this.UpdateButtonStatus();
        }

        private async Task Start()
        {
            this.CurrentCancellationTokenSource?.Cancel();
            this.CurrentCancellationTokenSource = new CancellationTokenSource();
            var engine = new SweepEngine(this.PathTextBox.Text);
            var items = engine.GetDiskItems();
            this.TheList.ItemsSource = items;
            await Task.WhenAll(items.Select(item => item.Start(this.CurrentCancellationTokenSource.Token)));
        }

        private async Task NewStart(string path = null)
        {
            if (path != null)
            {
                this.PathTextBox.Text = path;
            }

            this.HistoryPosition++;
            this.History.RemoveRange(this.HistoryPosition, this.History.Count - this.HistoryPosition);
            this.History.Add(this.PathTextBox.Text);
            this.UpdateButtonStatus();

            await this.Start();
        }

        private async Task Navigate(int position)
        {
            this.HistoryPosition = position < 0 
                ? 0 
                : position > this.History.Count 
                    ? this.History.Count
                    : position;

            this.PathTextBox.Text = this.History[this.HistoryPosition];
            this.UpdateButtonStatus();

            await this.Start();
        }

        private void UpdateButtonStatus()
        {
            this.BackButton.IsEnabled = this.HistoryPosition > 0;
            this.ForwardButton.IsEnabled = this.HistoryPosition < this.History.Count - 1;
            this.UpButton.IsEnabled = this.PathTextBox.Text.Length > 3;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await this.NewStart();
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Navigate(this.HistoryPosition - 1);
        }

        private async void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Navigate(this.HistoryPosition + 1);
        }

        private async void UpButton_Click(object sender, RoutedEventArgs e)
        {
            await this.NewStart(path: Path.GetDirectoryName(this.PathTextBox.Text));
        }

        private async void PathTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.NewStart();
            }
        }

        private async void TheList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = this.TheList.SelectedItem as DiskItem;
            await this.NewStart(path: Path.Combine(this.PathTextBox.Text, item.Name));
        }
    }
}
