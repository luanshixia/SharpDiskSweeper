using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DiskSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource CurrentCancellationTokenSource;
        private List<string> History = new List<string>();
        private string CurrentPath;
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
            var engine = new SweepEngine(this.PathTextBox.Text);
            var items = engine.GetDiskItems();
            this.TheList.ItemsSource = items;
            this.TheList.SelectedItem = null;
            await Task.WhenAll(items.Select(item => item.Start(this.CurrentCancellationTokenSource.Token)));
        }

        private async Task NewStart(string path = null)
        {
            if (path != null)
            {
                this.PathTextBox.Text = path;
            }

            if (!DirectoryExists(this.PathTextBox.Text))
            {
                this.PathTextBox.Foreground = Brushes.Red;
                return;
            }
            else
            {
                this.PathTextBox.Foreground = Brushes.Black;
                this.CurrentPath = this.PathTextBox.Text;
            }

            this.HistoryPosition++;
            this.History.RemoveRange(this.HistoryPosition, this.History.Count - this.HistoryPosition);
            this.History.Add(this.PathTextBox.Text);
            this.UpdateButtonStatus();

            await this.Start();
        }

        private static bool DirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            try
            {
                Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
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

                await this.NewStart(path: Path.Combine(this.CurrentPath, item.Name));
            }
        }

        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", this.PathTextBox.Text);
        }
    }
}
