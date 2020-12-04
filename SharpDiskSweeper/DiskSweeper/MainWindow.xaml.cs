using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private readonly List<string> History = new List<string>();
        private int HistoryPosition = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            this.PathTextBox.Text = App.StartupPath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            this.UpdateButtonStatus();
            await this.NewStart();
        }

        private async Task Start()
        {
            this.CurrentCancellationTokenSource?.Cancel();
            this.CurrentCancellationTokenSource = new CancellationTokenSource();
            var items = this.GetDiskItems(this.PathTextBox.Text);
            this.TheList.ItemsSource = items;
            this.TheList.SelectedItem = null;
            await Task.WhenAll(items.Select(item => item.Start(this.CurrentCancellationTokenSource.Token)));
        }

        private ObservableCollection<DiskItem> GetDiskItems(string dirPath)
        {
            return new ObservableCollection<DiskItem>(
                collection: new DirectoryInfo(dirPath)
                    .GetFileSystemInfos()
                    .Select(info => new DiskItem(info)));
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
            if (this.TheList.SelectedItem is DiskItem item)
            {
                if (item.Type != DiskItemType.Directory)
                {
                    return;
                }

                await this.NewStart(path: Path.Combine(this.PathTextBox.Text, item.Name));
            }
        }

        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", this.PathTextBox.Text);
        }
    }
}
