# PCL-EDIT

[![Build](https://github.com/hanqindylan/PCL-EDIT/actions/workflows/build.yml/badge.svg)](https://github.com/hanqindylan/PCL-EDIT/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/hanqindylan/PCL-EDIT?label=Release&labelColor=444444&color=1F883D)](https://github.com/hanqindylan/PCL-EDIT/releases)

PCL-EDIT 是 [Plain Craft Launcher 2](https://github.com/Meloong-Git/PCL) 的第三方修改版，基于上游 `2.99.0.1`，当前版本 **3.0.0**。

本版本恢复了启动器内的 Minecraft 下载功能，并修复了上游截至 `2.99.0.1` 仍开放的一批问题。协议见仓库根目录的 `LICENCE`：原文保持不变，文末新增《PCL-EDIT 附加说明》。

## 下载

- 发布页：<https://github.com/hanqindylan/PCL-EDIT/releases>
- 最新正式版：`3.0.0`
- 每次推送到 `main` 后，GitHub Actions 会在 Windows 上编译 Release 与 Snapshot，Release 产物可在对应的 Actions 运行中下载（见 [Actions](https://github.com/hanqindylan/PCL-EDIT/actions/workflows/build.yml)）。

### 安装程序（推荐）

Actions 每次都会额外产出 `PCL-EDIT-安装程序`（约 3.4 MB）。它是**单个 exe，双击即可安装，全程中文，没有任何语言选择界面**：

1. 欢迎页 → 点击「下一步」
2. 确认安装位置与快捷方式 → 点击「开始安装」
3. 自动解压、创建快捷方式、注册卸载信息 → 完成

- 默认装到 `%LOCALAPPDATA%\PCL-EDIT`，**不需要管理员权限**，也不会触发 UAC。
- 可选装到自定义文件夹、选择是否创建桌面快捷方式、安装完是否立即启动。
- 安装后出现在「设置 → 应用 → 已安装的应用」里，可以正常卸载；卸载会保留存档、Mod 与配置文件。

### 免安装

也可以直接下载 `PCL2-Release`，解压后运行 `Plain Craft Launcher 2.exe`。

下载后解压即可使用。程序只支持 Windows，需要 [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)。

## 安装器界面

安装器用 WPF 自绘，配色与圆角直接取自 PCL 本体（`#343d4a` 深灰文字、`#0b5bcb`→`#1370f3` 蓝渐变），观感与启动器一致。源码在 `PCLInstaller/`。

| 页面 | 说明 |
| --- | --- |
| 欢迎 | 展示版本与核心特性，下一步即可 |
| 选项 | 选安装位置（默认当前用户 / 自定义文件夹）、桌面快捷方式、安装后启动 |
| 进度 | 细长圆角进度条 + 实时中文日志 |
| 完成 | 显示安装路径，可直接启动 |
| 失败 | 显示中文错误原因与完整技术日志 |

## 本版本做了什么

### 下载

- 恢复游戏本体、Mod 加载器、整合包和社区资源的下载入口。
- 修复启动器更新检查失效（[#9553](https://github.com/Meloong-Git/PCL/issues/9553)）：更新源改为本仓库的 GitHub Release，不再依赖已经失效的上游更新接口。

### 已修复的上游问题

| 编号 | 问题 | 处理 |
| --- | --- | --- |
| #9555 | 改了“类型”筛选后忘记，导致搜索不到内容 | 选择类型筛选时给出提示 |
| #9553 | 启动器更新功能失效 | 改用本仓库 Release 作为更新源 |
| #9536 | 资源下载导向错误的版本文件夹 | 缓存键加入资源类型、加载器与 MC 版本 |
| #9533 | Java 下载列表请求失败后显示“从未成功完成过” | 成功检查后再读结果，失败时提示检查网络 |
| #9501 | 低版本窗口标题为空时启动状态无法结束 | 放行 LWJGL 空标题窗口，窗口出现后立即更新进度 |
| #9499 | 扫描版本时单个损坏文件导致整个列表失败 | 单个文件解析失败时跳过并继续扫描 |
| #9495 | 启动日志底部多出一个空白行 | 换行只作为行间分隔 |
| #9482 | OptiFine 抗锯齿重建窗口后自定义标题失效 | 窗口被重建后重新获取句柄 |
| #9475 | 下载完毕时删除旧版本会留下幽灵版本 | 删除后强制重新扫描版本列表 |
| #9448 | 全透明皮肤在游戏内显示为黑色 | 选择皮肤时提前警告 |
| #9440 | 加载器状态并发修改导致异常 | 状态变更加锁 |
| #9426 | 按 MC 版本搜索 Mod 时列出该版本不支持的加载器 | 按该版本的实际文件核对加载器 |
| #9416 | 披风名称未翻译 | 补充 Hero、Twisted 的中文名 |
| #9412 | 整合包安装时文件名丢失 | 保留原始文件名 |
| #9404 | 部分版本不支持自定义分辨率参数 | 启动前判断该版本是否支持 |
| #9374 | 整合包显示名称错误 | 使用整合包声明的显示名称 |
| #9371 | SAC 阻止启动时没有解决办法 | 常见问题新增 Smart Application Control 条目 |
| #9370 | 管理员权限下拖入长路径文件失败 | 按实际长度分配拖放缓冲区 |
| #9369 | 首次进入任务管理页会被立刻弹回 | 仍有任务时不再自动返回 |
| #9360 | 同时更新相同 Mod 时替换失败 | 每个更新任务使用独立临时目录 |
| #9345 | 部分刷新按钮没有反馈 | 补齐刷新提示 |
| #9343 | 刚打开时主页的玩家名和版本标记为空 | 等登录与版本列表完成后再替换 |
| #9331 | 无法从 IntelliJ IDEA 拖入 Mod | 支持虚拟文件拖放 |
| #9325 | 超长错误日志导致提示框卡顿 | 弹窗只显示开头一段，完整内容可另开查看 |
| #9303 | Wiki 条目热度分组错位 | 修正条目换行与分组 |

共 25 项，均为上游在 `2.99.0.1` 时仍开放的问题。

## 编译

本机需要 Windows、Visual Studio 2022（含 .NET Framework 4.8 目标包）和 MSBuild。

```powershell
git clone --recurse-submodules https://github.com/hanqindylan/PCL-EDIT.git
cd PCL-EDIT
dotnet restore PCLCS\PCLCS.csproj
# VB 工程是旧式 MSBuild 工程，不会自动编译它引用的 SDK 式 C# 工程，必须先单独编译
dotnet build PCLCS\PCLCS.csproj -c Release
msbuild "Plain Craft Launcher 2\Plain Craft Launcher 2.vbproj" -p:Configuration=Release -m
```

产物位于 `Plain Craft Launcher 2\bin\Plain Craft Launcher 2.exe`。

要连安装程序一起打出来，先把 `bin` 的内容压成 `PCLInstaller\Payload\payload.zip`，再编译安装器：

```powershell
New-Item -ItemType Directory -Force PCLInstaller\Payload
Compress-Archive -Path "Plain Craft Launcher 2\bin\*" -DestinationPath PCLInstaller\Payload\payload.zip
dotnet build PCLInstaller\PCLInstaller.csproj -c Release
```

产物位于 `PCLInstaller\bin\Release\net48\PCL-EDIT 安装程序.exe`。程序文件以资源形式内嵌，所以这**一个 exe** 就是完整安装包。

仓库提供 `.github/workflows/build.yml`，当前编译状态为通过。它分两个任务：
1. `build` —— 在 `windows-latest` 上分别编译 Release 与 Snapshot，上传 Release 的 `bin` 为 Artifact。
2. `installer` —— 下载上一步的产物，压成 `payload.zip` 后编译安装器，上传 `PCL-EDIT-安装程序`。

编译失败时，错误行会以注解形式直接显示在 Actions 运行页面上。

## 协议

- 上游原文《PCL 分发有限许可》与《PCL 存储库合理使用指南》保持原样，见 `LICENCE`。
- 本修改版的额外约定见 `LICENCE` 文末的《PCL-EDIT 附加说明》。
- 功能说明（可供其他 AI 使用）见 `PROMPT.md`。

## 上游

本仓库是第三方修改版，不是官方版本。上游项目与作者：

- 项目：<https://github.com/Meloong-Git/PCL>
- 作者：龙腾猫跃（LTCat / Meloong）
- 赞助：<https://meloong.com/afd/a/LTCat>
