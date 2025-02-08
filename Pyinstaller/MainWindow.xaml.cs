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
        /// 加载系统中安装的 Python 版本
        /// </summary>
        private void LoadPythonVersions()
        {
            // 扫描系统 PATH 中的 python.exe
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
        /// 加载系统中安装的 PyInstaller 版本
        /// </summary>
        private async void LoadPyInstallerVersions()
        {
            // 获取 PyInstaller 版本
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
        /// 浏览 Python 脚本文件
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
        /// 浏览图标文件
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
        /// 开始打包
        /// </summary>
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
                // 构建 PyInstaller 命令
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

                // 隐藏导入模块
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

                // 清理临时文件
                if (CleanCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --clean";
                }

                // 不确认输出目录
                if (NoConfirmCheckBox.IsChecked == true)
                {
                    pyinstallerCmd += " --noconfirm";
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

        /// <summary>
        /// 运行外部进程
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
        /// 记录日志
        /// </summary>
        private void Log(string message)
        {
            LogTextBox.Text += $"{message}\n";
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
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
