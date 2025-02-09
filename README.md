# PyInstaller GUI

**PyInstaller GUI** 是一个基于 WinUI 3 的桌面应用程序，用于简化 Python 脚本的打包，加上我的加密脚本。用户可以通过图形界面选择 Python 和 PyInstaller 版本，配置打包参数，并生成可执行文件。

## 功能特点

1. **选择 Python 版本**：
- 支持扫描系统中安装的 Python 版本，并选择目标版本。

2. **选择 PyInstaller 版本**：
- 支持获取系统中安装的 PyInstaller 版本，并选择目标版本。

3. **常用 PyInstaller 功能**：
- 支持以下 PyInstaller 参数：
- `--onefile`：打包为单个文件。
- `--onedir`：打包为文件夹。
- `--console`：显示控制台窗口。
- `--windowed`：不显示控制台窗口。
- `--icon`：设置图标。
- `--add-data`：添加额外文件。
- `--hidden-import`：隐藏导入模块。
- `--upx-dir`：启用 UPX 压缩。
- `--clean`：清理临时文件。
- `--noconfirm`：不确认输出目录。

4. **日志输出**：
- 实时显示 PyInstaller 的执行日志，方便用户查看打包过程。

---

## 使用方法

### 1. 安装依赖

确保系统中已安装以下依赖：
- **Python 3.x**
- **PyInstaller**（通过 `pip install pyinstaller` 安装）
- **WinUI 3 运行时**（通过 Microsoft Store 安装）

### 2. 运行应用程序

1. 克隆或下载本项目代码。
2. 打开项目文件夹，使用 Visual Studio打开解决方案文件（`.sln`）。
3. 按下 `F5` 运行应用程序。

### 3. 配置和打包

1. **选择 Python 版本**：从下拉菜单中选择系统中安装的 Python 版本。
2. **选择 PyInstaller 版本**：从下拉菜单中选择系统中安装的 PyInstaller 版本。
3. **选择 Python 脚本**：点击“浏览...”按钮，选择需要打包的 Python 脚本文件。
4. **配置打包选项**：
- 选择打包模式（单文件或文件夹）。
- 选择是否显示控制台窗口。
- 配置图标、额外文件、隐藏导入模块等参数。
5. **开始打包**：点击“开始打包”按钮，等待打包完成。生成的 EXE 文件将位于 `dist` 目录下。

---

## 打包项目

### 1. 打包为 MSIX 安装包

1. 在 Visual Studio 中，右键点击项目，选择 **发布 > 创建应用包**。
2. 按照向导完成打包。
3. 生成的 `.msix` 文件可以通过双击安装。

### 2. 打包为 exe

1. 在 Visual Studio 中，右键点击项目，选择 **发布 > 发布为单个文件**。
2. 生成的 `.exe` 文件可以直接运行。


## 项目结构

```
PyInstaller/
├── App.xaml # 应用程序资源文件
├── App.xaml.cs # 应用程序逻辑
├── MainWindow.xaml # 主窗口界面
├── MainWindow.xaml.cs # 主窗口逻辑
├── Package.appxmanifest # 应用程序清单文件
├── Program.cs # 应用程序入口
└── Pyinstaller.csproj # 项目文件
```

---

## 注意事项

1. **Python 和 PyInstaller**：
- 确保系统中已安装 Python 和 PyInstaller。
- 如果需要自定义 PyInstaller 参数，可以通过界面配置。

2. **日志输出**：
- 打包过程中，日志会实时显示在界面中，方便查看。

---

## 许可证

本项目采用 [MIT 许可证](LICENSE)。

---

## 反馈与贡献

如果你发现任何问题或有改进建议，欢迎提交 Issue 或 Pull Request。

---
