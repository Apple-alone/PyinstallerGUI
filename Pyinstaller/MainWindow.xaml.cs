using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Pyinstaller
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            LoadPythonVersions();
            LoadPyInstallerVersions();

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 800, Height = 600 });
        }

        /// <summary>
        /// ����ϵͳ�а�װ�� Python �汾
        /// </summary>
        private void LoadPythonVersions()
        {
            // ɨ��ϵͳ PATH �е� python.exe
            var paths = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (var path in paths)
            {
                if (File.Exists(Path.Combine(path, "python.exe")))
                {
                    PythonVersionComboBox.Items.Add(path);
                }
            }
            PythonVersionComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// ����ϵͳ�а�װ�� PyInstaller �汾
        /// </summary>
        private async void LoadPyInstallerVersions()
        {
            // ��ȡ PyInstaller �汾
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pip",
                    Arguments = "show pyinstaller",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            var lines = output.Split('\n');
            var versionLine = lines.FirstOrDefault(l => l.StartsWith("Version:"));
            if (versionLine != null)
            {
                var version = versionLine.Split(':')[1].Trim();
                PyInstallerVersionComboBox.Items.Add(version);
                PyInstallerVersionComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// ��� Python �ű��ļ�
        /// </summary>
        private async void BrowseScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var openFilePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".py" }
            };
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openFilePicker, hwnd);
            StorageFile file = await openFilePicker.PickSingleFileAsync();
            if (file != null)
            {
                ScriptPathTextBox.Text = file.Path;
            }
        }

        /// <summary>
        /// ���ͼ���ļ�
        /// </summary>
        private async void BrowseIconButton_Click(object sender, RoutedEventArgs e)
        {
            var openFilePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".ico" }
            };
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openFilePicker, hwnd);
            StorageFile file = await openFilePicker.PickSingleFileAsync();
            if (file != null)
            {
                IconPathTextBox.Text = file.Path;
            }
        }

        /// <summary>
        /// ��ʼ���
        /// </summary>
        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            string scriptPath = ScriptPathTextBox.Text;
            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                ShowMessage("����", "��ѡ����Ч�� Python �ű���");
                return;
            }

            try
            {
                // ���� PyInstaller ����
                string pyinstallerCmd = $"pyinstaller {ModeComboBox.SelectedValue} {ConsoleComboBox.SelectedValue}";

                // ͼ���ļ�
                if (!string.IsNullOrEmpty(IconPathTextBox.Text))
                {
                    pyinstallerCmd += $" --icon \"{IconPathTextBox.Text}\"";
                }

                // �����ļ�
                if (!string.IsNullOrEmpty(AddDataTextBox.Text))
                {
                    pyinstallerCmd += $" --add-data \"{AddDataTextBox.Text}\"";
                }

                // ���ص���ģ��
                if (!string.IsNullOrEmpty(HiddenImportTextBox.Text))
                {
                    foreach (var module in HiddenImportTextBox.Text.Split(','))
                    {
                        pyinstallerCmd += $" --hidden-import {module.Trim()}";
                    }
                }

                // UPX ѹ��
                if (UpxCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --upx-dir=/path/to/upx";
                }

                // ������ʱ�ļ�
                if (CleanCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --clean";
                }

                // ��ȷ�����Ŀ¼
                if (NoConfirmCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --noconfirm";
                }

                pyinstallerCmd += $" \"{scriptPath}\"";
                Log($"ִ�����{pyinstallerCmd}");
                RunProcess("pyinstaller", pyinstallerCmd);

                Log("�����ɣ�EXE �ļ�λ�� dist Ŀ¼�¡�");
                ShowMessage("���", "����ɹ���");
            }
            catch (Exception ex)
            {
                Log($"����{ex.Message}");
                ShowMessage("����", $"���ʧ�ܣ�{ex.Message}");
            }
        }

        /// <summary>
        /// �����ⲿ����
        /// </summary>
        private void RunProcess(string fileName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
            {
                Log(output);
            }
            if (!string.IsNullOrEmpty(error))
            {
                Log(error);
            }
        }

        /// <summary>
        /// ��¼��־
        /// </summary>
        private void Log(string message)
        {
            LogTextBox.Text += $"{message}\n";
        }

        /// <summary>
        /// ��ʾ��Ϣ�Ի���
        /// </summary>
        private async void ShowMessage(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "ȷ��",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
