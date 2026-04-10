using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NervaOneWalletMiner.Views
{
    public partial class ViewLogsView : UserControl
    {
        private const string TabAppLogs = "app";
        private const string TabCliLogs = "cli";
        private const string TabExports = "exports";
        private const int MaxDisplayLines = 10000;

        private string _currentTab = TabAppLogs;
        private string _currentFilePath = string.Empty;
        private List<string> _filePaths = [];

        public ViewLogsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.CONS", ex);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            try
            {
                base.OnDataContextChanged(e);
                if (DataContext is ViewLogsViewModel vm)
                {
                    SetActiveTab(vm.InitialTab);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.DCCH", ex);
            }
        }

        private void SetActiveTab(string tab)
        {
            try
            {
                _currentTab = tab;
                _currentFilePath = string.Empty;
                tbxLogContent.Text = string.Empty;

                btnTabAppLogs.FontWeight = tab == TabAppLogs ? FontWeight.Bold : FontWeight.Normal;
                btnTabCliLogs.FontWeight = tab == TabCliLogs ? FontWeight.Bold : FontWeight.Normal;
                btnTabExports.FontWeight = tab == TabExports ? FontWeight.Bold : FontWeight.Normal;

                PopulateFileList();
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.SATB", ex);
            }
        }

        private void PopulateFileList()
        {
            try
            {
                List<string> files = [];

                switch (_currentTab)
                {
                    case TabAppLogs:
                        if (Directory.Exists(GlobalData.LogsDir))
                        {
                            files = [.. Directory.GetFiles(GlobalData.LogsDir, "*.log")
                                .OrderByDescending(f => f)];
                        }
                        break;

                    case TabCliLogs:
                        if (Directory.Exists(GlobalData.CliToolsDir))
                        {
                            var logFiles = Directory.GetFiles(GlobalData.CliToolsDir, "*.log");
                            var oldFiles = Directory.GetFiles(GlobalData.CliToolsDir, "*.old");
                            files = [.. logFiles.Concat(oldFiles).OrderBy(f => Path.GetFileName(f))];
                        }
                        break;

                    case TabExports:
                        if (Directory.Exists(GlobalData.ExportsDir))
                        {
                            files = [.. Directory.GetFiles(GlobalData.ExportsDir)
                                .OrderByDescending(f => File.GetLastWriteTime(f))];
                        }
                        break;
                }

                _filePaths = files;
                cbxFiles.ItemsSource = files.Select(f => Path.GetFileName(f)).ToList();
                cbxFiles.SelectedIndex = files.Count > 0 ? 0 : -1;
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.PPFL", ex);
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                _currentFilePath = filePath;

                if (!File.Exists(filePath))
                {
                    tbxLogContent.Text = "(File not found)";
                    return;
                }

                List<string> lines = [];
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(fs))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }

                if (lines.Count > MaxDisplayLines)
                {
                    IEnumerable<string> lastLines = lines.Skip(lines.Count - MaxDisplayLines);
                    tbxLogContent.Text = $"[Showing last {MaxDisplayLines:N0} of {lines.Count:N0} lines. Use Save Full File to export complete log.]\n\n"
                        + string.Join(Environment.NewLine, lastLines);
                }
                else
                {
                    tbxLogContent.Text = string.Join(Environment.NewLine, lines);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.LDFIL", ex);
            }
        }

        public void TabAppLogs_Clicked(object sender, RoutedEventArgs args)
        {
            SetActiveTab(TabAppLogs);
        }

        public void TabCliLogs_Clicked(object sender, RoutedEventArgs args)
        {
            SetActiveTab(TabCliLogs);
        }

        public void TabExports_Clicked(object sender, RoutedEventArgs args)
        {
            SetActiveTab(TabExports);
        }

        public void FileSelection_Changed(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                int index = cbxFiles.SelectedIndex;
                if (index >= 0 && index < _filePaths.Count)
                {
                    LoadFile(_filePaths[index]);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.FSCG", ex);
            }
        }

        public void Refresh_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string previousFile = _currentFilePath;
                PopulateFileList();

                if (!string.IsNullOrEmpty(previousFile))
                {
                    int idx = _filePaths.IndexOf(previousFile);
                    if (idx > 0)
                    {
                        cbxFiles.SelectedIndex = idx;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.RFRS", ex);
            }
        }

        public async void SaveFullFile_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
                {
                    return;
                }

                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null)
                {
                    return;
                }

                string ext = Path.GetExtension(_currentFilePath).TrimStart('.');

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Log File",
                    SuggestedFileName = Path.GetFileName(_currentFilePath),
                    DefaultExtension = string.IsNullOrEmpty(ext) ? "log" : ext
                });

                if (file != null)
                {
                    await using var destStream = await file.OpenWriteAsync();
                    using var sourceStream = File.OpenRead(_currentFilePath);
                    await sourceStream.CopyToAsync(destStream);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("VLV.SVFF", ex);
            }
        }

        public void Back_Clicked(object sender, RoutedEventArgs args)
        {
            Logger.LogDebug("VLV.BKCK", "Navigating back to Settings");
            UIManager.NavigateToPage(SplitViewPages.Settings);
        }
    }
}
