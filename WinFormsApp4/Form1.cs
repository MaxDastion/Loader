using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinFormsApp4
{
    public partial class Form1 : Form
    {
        private WebClient _webClient;
        private CancellationTokenSource _cancellationTokenSource;
        private string _downloadFilePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog saveFileDialog = new FolderBrowserDialog())
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    savePathBox.Text = saveFileDialog.SelectedPath;
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(urlBox.Text) || string.IsNullOrEmpty(savePathBox.Text))
            {
                MessageBox.Show("Введите URL и путь для сохранения.");
                return;
            }
            string fileName = GetFileNameFromUrl(urlBox.Text);
            _downloadFilePath = $"{savePathBox.Text}//{fileName}";

            _webClient = new WebClient();
            _cancellationTokenSource = new CancellationTokenSource();

            _webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            _webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            try
            {
                Uri downloadUri = new Uri(urlBox.Text);
                _webClient.DownloadFileAsync(downloadUri, _downloadFilePath );
                downloadsList.Items.Add($"Загрузка начата: {urlBox.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                downloadsList.Items.Add($"Загрузка отменена: {_downloadFilePath}");
            }
            else if (e.Error != null)
            {
                downloadsList.Items.Add($"Ошибка загрузки: {_downloadFilePath} - {e.Error.Message}");
            }
            else
            {
                downloadsList.Items.Add($"Загрузка завершена: {_downloadFilePath}");
            }

            progressBar.Value = 0;
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            if (_webClient != null)
            {
                _webClient.CancelAsync();
                downloadsList.Items.Add($"Загрузка приостановлена: {_downloadFilePath}");
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (_webClient != null)
            {
                _webClient.CancelAsync();
                downloadsList.Items.Add($"Загрузка отменена: {_downloadFilePath}");
                progressBar.Value = 0;

                if (File.Exists(_downloadFilePath))
                {
                    File.Delete(_downloadFilePath);
                }
            }
        }

        private string GetFileNameFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return Path.GetFileName(uri.LocalPath);
            }
            catch
            {
                return "downloaded_file";
            }
        }
    }
}
