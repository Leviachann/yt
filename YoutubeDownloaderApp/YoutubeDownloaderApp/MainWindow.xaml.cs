using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace YoutubeDownloader
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string youtubeUrl;
        public string YoutubeUrl
        {
            get { return youtubeUrl; }
            set
            {
                youtubeUrl = value;
                OnPropertyChanged(nameof(YoutubeUrl));
            }
        }

        private int downloadProgress1;
        public int DownloadProgress1
        {
            get { return downloadProgress1; }
            set
            {
                downloadProgress1 = value;
                OnPropertyChanged(nameof(DownloadProgress1));
                OnPropertyChanged(nameof(DownloadPercentage1));
            }
        }

        private int downloadProgress2;
        public int DownloadProgress2
        {
            get { return downloadProgress2; }
            set
            {
                downloadProgress2 = value;
                OnPropertyChanged(nameof(DownloadProgress2));
                OnPropertyChanged(nameof(DownloadPercentage2));
            }
        }

        private int downloadProgress3;
        public int DownloadProgress3
        {
            get { return downloadProgress3; }
            set
            {
                downloadProgress3 = value;
                OnPropertyChanged(nameof(DownloadProgress3));
                OnPropertyChanged(nameof(DownloadPercentage3));
            }
        }

        public string DownloadPercentage1 => $"{DownloadProgress1}%";
        public string DownloadPercentage2 => $"{DownloadProgress2}%";
        public string DownloadPercentage3 => $"{DownloadProgress3}%";

        public ICommand DownloadCommand { get; }

        private SemaphoreSlim semaphore = new SemaphoreSlim(3);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            DownloadCommand = new AsyncRelayCommand(DownloadVideo);
        }

        private async Task DownloadVideo()
        {
            try
            {
                await semaphore.WaitAsync();

                using (var client = new WebClient())
                {
                    var downloadCompletedTask = new TaskCompletionSource<bool>();

                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        switch (Task.CurrentId)
                        {
                            case 1:
                                DownloadProgress1 = e.ProgressPercentage;
                                break;
                            case 2:
                                DownloadProgress2 = e.ProgressPercentage;
                                break;
                            case 3:
                                DownloadProgress3 = e.ProgressPercentage;
                                break;
                        }
                    };

                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        downloadCompletedTask.SetResult(true);
                    };

                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "MP4 Files (*.mp4)|*.mp4";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string fileName = saveFileDialog.FileName;
                        await client.DownloadFileTaskAsync(new Uri(YoutubeUrl), fileName);
                    }

                    await downloadCompletedTask.Task;

                    MessageBox.Show("Download completed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _action;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting;
        }

        public async void Execute(object parameter)
        {
            if (_isExecuting)
                return;

            _isExecuting = true;
            try
            {
                await _action();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
