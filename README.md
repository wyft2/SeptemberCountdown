# 九月倒计时

Windows 桌面无边框液态玻璃倒计时，支持数字流光（颜色渐变）、透明玻璃数字、数字流沙、八套配色、星空模式，以及检查更新。

## 下载

请到 [Releases](https://github.com/wyft2/SeptemberCountdown/releases/latest) 下载最新的 Windows 版本。

国内网络如果访问 GitHub Releases 困难，可以使用 jsDelivr 镜像：

- [SeptemberCountdown_v1.7.0_Windows.zip](https://cdn.jsdelivr.net/gh/wyft2/SeptemberCountdown@main/download/SeptemberCountdown_v1.7.0_Windows.zip)

## 检查更新

程序会从本仓库读取 `version.json`：

```json
{
  "version": "1.7.0",
  "url": "https://cdn.jsdelivr.net/gh/wyft2/SeptemberCountdown@main/download/SeptemberCountdown_v1.7.0_Windows.zip",
  "page": "https://github.com/wyft2/SeptemberCountdown/releases/latest",
  "notes": "更新说明"
}
```

国内网络下，程序会自动尝试 GitHub、jsDelivr、Fastly、gh-proxy 国内代理、Gitee 和 GitHub Pages；也可以在设置里填写自定义更新源。

## 源码

v1.7 源码已开源，采用 [MIT 协议](LICENSE)，位于 [`src/`](src/)：

```
src/
├── Program.cs             程序入口、单实例与命令行参数
├── MainWindow.cs          主窗口、倒计时逻辑与交互
├── Ui.xaml                界面样式与动画
├── Starfield.cs           星空粒子系统
├── StarWindow.cs          全屏星空窗口
├── AppSettings.cs         设置持久化
├── UpdateService.cs       检查更新（多镜像源回退）
├── StartupRegistration.cs 开机自启动
├── NativeMethods.cs       Win32 互操作
├── AssemblyInfo.cs        程序集信息
├── app.manifest           高 DPI 与兼容性清单
├── build.ps1              构建脚本
├── version.json           更新清单
├── Assets/                图标、预览图与噪点纹理
└── tools/make_assets.py   生成素材
```

### 自行构建

需要 Windows 10/11 与 .NET Framework 4.8（系统自带 `csc.exe`，无需安装 SDK）：

```powershell
cd src
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

产物：`src\dist\九月倒计时\九月倒计时.exe`。程序不依赖外部运行库。

使用说明见 [docs/使用说明.md](docs/使用说明.md)。

## 作者

- [@wyft2](https://github.com/wyft2)
- [烧脑时刻](https://wyft2.github.io/Echo-Layer-Page-7/)
