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

## 作者

- [@wyft2](https://github.com/wyft2)
- [烧脑时刻](https://wyft2.github.io/Echo-Layer-Page-7/)
