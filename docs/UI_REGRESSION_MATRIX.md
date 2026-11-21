# UI 功能基准与 P6 回归矩阵

> 基准日期：2026-11-16；功能清单来源：`VioletToolBox/MainWindow.xaml`，起点 `c4ef6d2edc61fcbeff736cab3706a34169bbcc90`。此表是 **110 项代表性可操作功能** 的核对样本，并非声称项目仅有 110 个事件或已完整覆盖全部实现路径。

## 覆盖说明

- **P0 静态已覆盖**：`tests/FeatureRegressionBaseline/features.json` 的每一项都能追踪至真实 XAML 页面、控件（若存在）和事件处理器；CI 检查缺失、重命名与绑定漂移。
- **UI 完整性**：`tests/ThemeCoverageChecks/ui-parity-baseline.json` 固定了 P3 head 的 **536 个命名控件**、**377 个事件绑定**（含非本表功能）；新的检查以多重集进行验证，原有项不得丢失。
- **主题覆盖**：P3、P4、P5 页面级 DynamicResource 与控件样式静态检查；这不代表视觉上可读性已经通过真机验证。
- **尚未覆盖**：ADB/Fastboot/EDL 操作效果、实体设备、网速和下载续传、磁盘写入、通讯录恢复、权限异常、界面视觉、不同 DPI 和 Windows 系统强调色的实时变化。均放入 P6 实测，不得标记为通过。
- **高风险门槛**：刷写、擦除、回锁、格式化、卸载及恢复通讯录必须使用已备份且可恢复的专用测试设备，不纳入 CI 自动执行。

## 110 项功能对照（按页面）

### 主页与设备控制 (9 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F001 | 重启系统 | `RebootPhoneButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F002 | 重启至 Fastboot | `RebootToFastbootButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F003 | 重启至 FastbootD | `RebootToFastbootDButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F004 | 重启至 Recovery | `RebootToRecoveryButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F005 | 重启至 EDL 9008 | `RebootTo9008Button` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F006 | 刷新设备状态 | `RefreshDeviceButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F007 | 无线调试 | `WirelessDebugButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F008 | 切换启动槽位 | `SwitchSlotButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F009 | 设备自动检测 | `DeviceDetectionToggle` | 只读查询 | UI/事件/主题基准 | 待验证 |

### Root / AVB (8 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F010 | 脱机修补模式 | `OfflinePatchModeRadioButton` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F011 | 一加自动 ROOT 模式 | `OnePlusAutoRootModeRadioButton` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F012 | Magisk Alpha 修补方案 | `MagiskAlphaPatchRadioButton` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F013 | KernelSU LKM 修补方案 | `KernelSuLkmPatchRadioButton` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F014 | 选择 Boot 镜像 | `btnSelectBoot` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F015 | 执行自动 ROOT | `btnAutoRoot` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F016 | 分析 AVB vbmeta | `AvbAnalyzeVbmetaButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F017 | 执行 AVB 签名 | `AvbStartSignButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |

### 投屏 (8 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F018 | 强制竖屏 | `ForcePortraitButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F019 | 强制横屏 | `ForceLandscapeButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F020 | 设备截图 | `ScreenshotButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F021 | 模拟返回键 | `BackKeyButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F022 | 启动投屏 | `StartMirrorButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F023 | 停止投屏 | `StopMirrorButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F024 | 自动投屏开关 | `AutoMirrorCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F025 | 投屏标题设备名选项 | `MirrorTitleShowDeviceNameCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |

### 基础刷写 (7 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F026 | 选择刷写镜像 | `SelectBootFileButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F027 | 刷写 Boot | `FlashBootButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F028 | 回锁 Bootloader | `LockBLButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F029 | 执行小米线刷脚本 | `StartXiaomiFlashButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F030 | 设备格式化 | `FormatDeviceButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F031 | 设备锁定状态清理 | `EraseGoogleLockButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F032 | 修复 ADB 通路 | `FixAdbButton` | 设备交互 | UI/事件/主题基准 | 待验证 |

### 可视化 Fastboot (7 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F033 | 读取 Fastboot 分区表 | `ReadPartitionTableButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F034 | 写入指定分区 | `WritePartitionButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F035 | 擦除指定分区 | `ErasePartitionButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F036 | 备份基带字库 | `BackupBasebandButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F037 | 回读指定分区 | `ReadPartitionButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F038 | 备份 GPT | `BackupGptButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F039 | 停止 Fastboot 操作 | `StopFastbootVisualizationOperationButton` | 设备交互 | UI/事件/主题基准 | 待验证 |

### 隐藏环境 (5 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F040 | 选择环境资源包 | `SelectHiddenRootZipButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F041 | 部署隐藏环境 | `StartHiddenRootButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F042 | 下载环境资源包 | `DownloadHiddenPackageButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F043 | 安装检测工具 | `InstallDetectAppsButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F044 | 安装模块 | `StartModuleInstallButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |

### 断点续传下载器 (5 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F045 | 选择输出目录 | `VioletDownloadBrowseOutputButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F046 | 开始多线程下载 | `VioletDownloadStartButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F047 | 暂停下载 | `VioletDownloadPauseButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F048 | 恢复下载 | `VioletDownloadResumeButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F049 | 替换动态下载链接 | `VioletDownloadReplaceUrlButton` | 网络连接 | UI/事件/主题基准 | 待验证 |

### 文件管理 (5 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F050 | 加载目标目录 | `(LoadSystemZoneLocationButton_Click)` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F051 | 新建目录 | `SystemZoneNewFolderMenuItem` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F052 | 新建文件 | `SystemZoneNewFileMenuItem` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F053 | 重命名文件 | `SystemZoneRenameMenuItem` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F054 | 删除文件 | `SystemZoneDeleteMenuItem` | 高风险操作 | UI/事件/主题基准 | 待验证 |

### 下载资源 (4 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F055 | 打开驱动下载入口 | `(DriverTile_MouseLeftButtonDown)` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F056 | 打开工具软件下载入口 | `(UtilitySoftwareIcon_MouseLeftButtonDown)` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F057 | 打开 Root 工具入口 | `(RootManagerTile_MouseLeftButtonDown)` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F058 | 打开内核资源入口 | `(Kernel4Tile_MouseLeftButtonDown)` | 网络连接 | UI/事件/主题基准 | 待验证 |

### Payload (5 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F059 | 选择 Payload 输入 | `BrowseInputButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F060 | 选择导出目录 | `BrowseOutputButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F061 | 读取 Payload 信息 | `ReadInfoButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F062 | 提取 Payload 分区镜像 | `ExportButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F063 | 停止 Payload 操作 | `StopOperationButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |

### ROM 下载 (6 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F064 | 下载 ROM | `(RomDownloadStartButton_Click)` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F065 | 修复下载网络异常 | `RomNetworkRepairButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F066 | 复制 ROM 下载链接 | `RomSelectCopyLinksButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F067 | 联想 ROM 查询 | `LenovoQueryButton` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F068 | 复制联想下载链接 | `LenovoQueryCopyButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F069 | 动态解析选项 | `C16DynamicCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |

### EDL / 9008 (10 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F070 | 读取 EDL 分区表 | `EdlReadGptButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F071 | 写入 GPT | `EdlWriteGptButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F072 | EDL 回读分区 | `EdlReadPartButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F073 | EDL 写入分区 | `EdlWritePartButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F074 | EDL 擦除分区 | `EdlErasePartButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F075 | EDL 备份 GPT | `EdlBackupGptButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F076 | 格式化 LUN | `EdlFormatLunButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F077 | EDL 恢复出厂 | `EdlFactoryResetButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F078 | EDL 槽位管理 | `EdlSlotManagementButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F079 | 停止 EDL 操作 | `EdlStopOperationButton` | 设备交互 | UI/事件/主题基准 | 待验证 |

### 降级助手 (3 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F080 | 下载降级助手 APK | `BtnDownloadApk` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F081 | 查询降级包 | `BtnQueryOnly` | 网络连接 | UI/事件/主题基准 | 待验证 |
| F082 | 下载并推送降级包 | `StartBtn` | 设备交互 | UI/事件/主题基准 | 待验证 |

### 备份与恢复 (5 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F083 | 读取设备相册 | `btnLoadImages` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F084 | 备份选中图片 | `btnBackup` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F085 | 恢复通讯录 | `btnRestoreContacts` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F086 | 切换视频备份视图 | `btnNavVideos` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F087 | 切换通讯录备份视图 | `btnNavContacts` | 选项交互 | UI/事件/主题基准 | 待验证 |

### 应用管理 (7 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F088 | 读取应用列表 | `ReadAppListButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F089 | 冻结选中应用 | `FreezeSelectedAppsButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F090 | 解冻选中应用 | `UnfreezeSelectedAppsButton` | 设备交互 | UI/事件/主题基准 | 待验证 |
| F091 | 提取 APK | `ExtractApkButton` | 备份恢复 | UI/事件/主题基准 | 待验证 |
| F092 | 清理应用数据 | `ClearAppDataButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F093 | 卸载选中应用 | `UninstallSelectedAppsButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F094 | 复制包名 | `CopySelectedPackagesButton` | 只读查询 | UI/事件/主题基准 | 待验证 |

### 安卓通用工具 (7 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F095 | 打包 super 分区 | `StartSuperPackButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F096 | 解包 OFP | `StartOfpExtractButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F097 | 生成刷写脚本 | `GenerateFlashScriptButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F098 | 解包 OPS | `StartOpsExtractButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F099 | 执行转换后线刷 | `StartConvertFlashButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F100 | 合并 OFP super 分片 | `StartMergeOfpSuperButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F101 | 生成 OCDT | `GenerateOcdtButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |

### 欧加线刷 (8 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F102 | 修复 super 硬砖 | `RepairSuperHardBrickButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F103 | 提取分区 | `ExtractPartitionButton` | 文件输入输出 | UI/事件/主题基准 | 待验证 |
| F104 | AB 通刷模式 | `FlashABCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F105 | 强力线刷模式 | `FixSuperCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F106 | 仅 FastbootD 模式 | `PureFBDCheckBox` | 选项交互 | UI/事件/主题基准 | 待验证 |
| F107 | 检查 ARB 熔断 | `ArbFuseCheckButton` | 只读查询 | UI/事件/主题基准 | 待验证 |
| F108 | 售后包线刷 | `AfterSalesStartFlashButton` | 高风险操作 | UI/事件/主题基准 | 待验证 |
| F109 | 修复 FastbootD | `AfterSalesFixFastbootDButton` | 设备交互 | UI/事件/主题基准 | 待验证 |

### 关于与链接 (1 项)

| ID | 功能 | XAML 控件或处理器 | 类型 | 静态覆盖 | P6 实测 |
|---|---|---|---|---|---|
| F110 | 打开社区外部链接 | `(SocialMediaButton_Click)` | 网络连接 | UI/事件/主题基准 | 待验证 |

## P6 最小回归流程与证据

| 等级 | 进入条件 | 验收要点 | 必要证据 |
|---|---|---|---|
| 只读查询 | 已连接可控测试设备 | 正常响应、超时提示、空设备与权限错误处理 | 输入、日志、设备状态与截图 |
| 文件输入输出 | 临时数据与可写目录 | 输出路径、进度、校验、取消和异常数据处理 | 测试样本 SHA-256、日志与结果文件 |
| 网络连接 | 公网或本地模拟服务 | 重试、暂停续传、错误回退、下载文件校验 | URL 脱敏、日志、结果哈希 |
| 模式与配置 | 页面已加载 | 选项互斥、按钮启用与禁用、切换后状态 | 前后截图与关键属性 |
| 备份恢复 | 已脱敏测试资料 | 备份数据可读取、恢复前确认、不污染其他数据 | 前后样本摘要与日志 |
| 设备交互 | 专用测试设备 | ADB/Fastboot/EDL 模式切换、操作日志、失败回退 | 设备型号脱敏、前后状态、操作日志 |
| 高风险操作 | 已备份且可恢复测试设备 | 显式确认、写入目标核对、取消、中断恢复、回滚 | 备份信息、操作记录、校验结果 |

## 无法用现有 CI 证明的事项

- 未进行 Windows 多 DPI、1366×768/1920×1080 等显示器视觉复验，页面仍可能存在内部固定定位和必须滚动的区域。
- 未进行实际设备刷写、EDL、备份或恢复；回归表的 **“待验证”不是失败，也不是通过**。
- `MainWindow.xaml.cs` 中仍存在独立生成的对话框和分模块代码；本表以主窗可见 XAML 入口为样本。P6 应补充没有主窗控件入口的内部操作与辅助流程。
- 编译警告尚需按错误编号、发生文件、是否来自外部依赖进行分类；不得因 CI 成功忽略其潜在风险。

## 计数与复现

文件 `tests/FeatureRegressionBaseline/features.json` 记录 **18 个页面分组、110 项功能**，每项带 ID、处理器、风险、静态覆盖口径和 P6 待验证步骤。CI 以 **XAML Grid 真实闭合范围** 查找控件与主题引用；不采用“页名出现到下一页名”为最终统计边界。与 `ui-parity-baseline.json` 的完整 UI 命名/事件数分别核对，避免把 110 项代表功能误解为全量事件总数。
