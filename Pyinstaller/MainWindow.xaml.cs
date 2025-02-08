using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics;
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

            // ���ô��ڴ�С
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(800, 600));
        }

        private async void BrowseScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var openFilePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".py" }
            };

            // ��ʼ�� FileOpenPicker
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(openFilePicker, hwnd);

            // ѡ���ļ�
            StorageFile file = await openFilePicker.PickSingleFileAsync();
            if (file != null)
            {
                ScriptPathTextBox.Text = file.Path;
            }
        }

        private async void BrowseIconButton_Click(object sender, RoutedEventArgs e)
        {
            var openFilePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".ico" }
            };

            // ��ʼ�� FileOpenPicker
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(openFilePicker, hwnd);

            // ѡ���ļ�
            StorageFile file = await openFilePicker.PickSingleFileAsync();
            if (file != null)
            {
                IconPathTextBox.Text = file.Path;
            }
        }

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
                // ����
                if (EncryptCheckBox.IsChecked == true)
                {
                    Log("���ڼ��ܽű�...");
                    RunProcess("python", $"encryptor.py \"{scriptPath}\"");
                    scriptPath = Path.Combine(
                        Path.GetDirectoryName(scriptPath),
                        $"run_{Path.GetFileNameWithoutExtension(scriptPath)}.py"
                    );
                    Log($"������ɣ�ʹ�ü������ű���{scriptPath}");
                }

                // ����
                if (ObfuscateCheckBox.IsChecked == true)
                {
                    Log("���ڻ����ű�...");
                    RunProcess("pyarmor", $"obfuscate \"{scriptPath}\"");
                    scriptPath = Path.Combine("dist", Path.GetFileName(scriptPath));
                    Log($"������ɣ��ű�·����{scriptPath}");
                }

                // ���
                Log("���ڴ��Ϊ EXE...");
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

                // ���ص���
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

        private void Log(string message)
        {
            LogTextBox.Text += $"{message}\n";
        }

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
