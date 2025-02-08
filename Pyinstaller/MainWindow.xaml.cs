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

            // 设置窗口大小
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

            // 初始化 FileOpenPicker
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(openFilePicker, hwnd);

            // 选择文件
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

            // 初始化 FileOpenPicker
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(openFilePicker, hwnd);

            // 选择文件
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
                ShowMessage("错误", "请选择有效的 Python 脚本！");
                return;
            }

            try
            {
                // 加密
                if (EncryptCheckBox.IsChecked == true)
                {
                    Log("正在加密脚本...");
                    RunProcess("python", $"encryptor.py \"{scriptPath}\"");
                    scriptPath = Path.Combine(
                        Path.GetDirectoryName(scriptPath),
                        $"run_{Path.GetFileNameWithoutExtension(scriptPath)}.py"
                    );
                    Log($"加密完成，使用加载器脚本：{scriptPath}");
                }

                // 混淆
                if (ObfuscateCheckBox.IsChecked == true)
                {
                    Log("正在混淆脚本...");
                    RunProcess("pyarmor", $"obfuscate \"{scriptPath}\"");
                    scriptPath = Path.Combine("dist", Path.GetFileName(scriptPath));
                    Log($"混淆完成，脚本路径：{scriptPath}");
                }

                // 打包
                Log("正在打包为 EXE...");
                string pyinstallerCmd = $"pyinstaller {ModeComboBox.SelectedValue} {ConsoleComboBox.SelectedValue}";

                // 图标文件
                if (!string.IsNullOrEmpty(IconPathTextBox.Text))
                {
                    pyinstallerCmd += $" --icon \"{IconPathTextBox.Text}\"";
                }

                // 额外文件
                if (!string.IsNullOrEmpty(AddDataTextBox.Text))
                {
                    pyinstallerCmd += $" --add-data \"{AddDataTextBox.Text}\"";
                }

                // 隐藏导入
                if (!string.IsNullOrEmpty(HiddenImportTextBox.Text))
                {
                    foreach (var module in HiddenImportTextBox.Text.Split(','))
                    {
                        pyinstallerCmd += $" --hidden-import {module.Trim()}";
                    }
                }

                // UPX 压缩
                if (UpxCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --upx-dir=/path/to/upx";
                }

                pyinstallerCmd += $" \"{scriptPath}\"";
                Log($"执行命令：{pyinstallerCmd}");
                RunProcess("pyinstaller", pyinstallerCmd);

                Log("打包完成！EXE 文件位于 dist 目录下。");
                ShowMessage("完成", "打包成功！");
            }
            catch (Exception ex)
            {
                Log($"错误：{ex.Message}");
                ShowMessage("错误", $"打包失败：{ex.Message}");
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
                CloseButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
