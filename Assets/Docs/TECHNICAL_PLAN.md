# 技术规划

- 状态：M1 实现基线
- 最近更新：2026-08-19

## 1. 技术基线

- Unity：6000.0.81f1。
- 渲染：URP 2D。
- 平台：Windows Standalone。
- UI：uGUI + TextMesh Pro，复用 GUI Pro - Casual Game 资源。
- 输入：M1 优先鼠标点击和拖拽，底层使用 Input System。
- 存档：Easy Save 3 保存运行时进度；核心规则不得直接依赖 Easy Save API。
- 编辑器操作：场景、GameObject、组件、Prefab 和材质优先通过 Unity MCP 完成。
- 场景结构：Game.unity 直接保存摄像机、2D 灯光、EventSystem、Canvas、信息/分配双工作区、80 个结构化邮件项、需求/简历/私人报价固定区、6 个期刊目录项与三类期刊页面、16 张可滚动球员卡、2 个招聘槽位、拖拽提示和周过渡遮罩；运行时不得创建或销毁这些结构对象。
- 测试与自动化：Unity Test Framework、Unity CLI 和 Unity Pipeline。

## 2. 分层建议

### 2.1 Content

ScriptableObject 或可导入的静态数据，描述球员原型、球队、委托模板、信息来源和事件模板。

### 2.2 Domain

不依赖场景和 UI 的纯 C# 规则：周推进、匹配评估、结果生成、付款排期、声望变化和事件条件。

### 2.3 Runtime State

当前周、现金、应收款、已收到信息、开放委托、球员状态、已提交安排和历史结果。

### 2.4 Presentation

收件箱、委托工作台、球员卡片、拖拽交互、周结算和顶部状态栏。Presentation 只能读取规则系统允许公开的信息。

M1 使用场景预置 View 池：80 个邮件项、6 个期刊目录项、封面/专题/球探报告页面、16 张通用球员卡 View、两个招聘槽位和拖拽提示均序列化在 Game.unity。`GameController` 从 `GameContentCatalog` 绑定内容，维护已读与解锁稳定 ID，并只更新文本、图集 UV、颜色、监听与显隐。

球员肖像与期刊封面使用两个固定图集。静态内容保存 `portraitIndex`/`coverIndex`，运行时只给 Scene 中已有 `RawImage` 绑定纹理与 UV；不会在运行时切图、创建图片对象或泄露隐藏信息。所有图片位使用“容器锚定区域 + 子 `RawImage` 容器内 1:1 `AspectRatioFitter` 适配”结构，图片不会溢出容器与文字重叠；内容附件统一按方形构图提交。球员简历同时展示公开薪资范围（欧元/周）与球员经历。

球员卡 View 不保存固定球员或固定位置映射。运行时从关联邮件的到达周与内容顺序得到稳定显示顺序，再过滤已放入本周招聘槽位、已经在历史周提交或已过可用期的球员并从顶部连续绑定；本周撤回安排后重新进入原排序位置。场景只提供容量，不决定球员类型。

### 2.5 Persistence

把 Runtime State 转换为带版本号的存档快照，再交给 Easy Save 3。加载后通过统一入口恢复，不直接序列化场景对象引用。

M1 已实现：`NewPlayerHunter.Domain` 提供纯 C# 的 `WeekStateSnapshot` 与 `WeekState.CreateSnapshot`/`ApplySnapshot`，随机序列通过 `SeededRandomSource` 的种子加消耗次数恢复。`NewPlayerHunter.Persistence` 程序集把快照映射为 JsonUtility 兼容的 `GameProgressSnapshot`（金额按字符串保存），再由 `SaveGameService` 以单键 JSON 字符串写入 Easy Save 3 默认存档文件；`schemaVersion` 当前为 1，版本不匹配即丢弃旧档并新开。Easy Save 3 自带 asmdef 已从 `.disabled` 启用（运行时装配件在插件根目录，Editor 装配件在 Editor 目录）。`GameController` 在每次结束本周后自动保存、启动时恢复、`RestartGame` 删除存档；自动保存失败只记录错误。

### 2.6 Content Submission

人工内容使用根目录 `output/spreadsheet/NewPlayerHunter_Content_Submission_Template.xlsx` 收集，规则说明位于 `Assets/Docs/CONTENT_SUBMISSION_GUIDE.md`。球员、招聘主体、招聘槽位、邮件、期刊期次、期刊页面和附件分表提交。

整合入口必须以稳定 ID 建立引用，按 `published_week` 与 `delivery_order_in_week` 保持邮件到达顺序，并在生成 `GameContentCatalog` 前校验枚举、必填中文、重复 ID、跨表引用、期刊页码和隐藏信息边界。内容人员不直接修改 Unity Scene 或 ScriptableObject。

Excel 中的英文栏允许为空；整合为 `LocalizedText` 后继续使用中文回退规则。

## 3. 核心数据概念

| 概念 | 责任 |
| --- | --- |
| `PlayerDefinition` | 球员身份、公开背景、头像与内容引用 |
| `PlayerTruth` | 永久隐藏的真实能力、性格、伤病和诚信数据 |
| `PlayerClaim` | 简历或推荐信中的可见声明 |
| `EvidenceItem` | 来源、发布时间、可靠性、偏差和定性线索 |
| `ClubDemand` | 球队委托、相对有效周数、俱乐部履历、报酬、紧急度和槽位列表 |
| `DemandSlot` | 单个位置或名额的必要条件与偏好条件 |
| `Assignment` | 玩家在某周把某球员提交到某个槽位的决定 |
| `PlacementOutcome` | 基于隐藏信息生成的定性结果与实际报酬 |
| `PendingPayment` | 金额、来源、创建周和到账周 |
| `WeekState` | 当前周以及本周输入、决定和待处理结算 |

具体类型名可在实现时调整，但“静态内容、隐藏真相、可见信息、玩家决定、延迟结果”五类数据必须保持分离。

## 4. 隐藏信息边界

- `PlayerTruth` 不得直接绑定到 UI ViewModel。
- 界面只能获得 `PlayerClaim`、`EvidenceItem` 和经过规则层生成的公开结果。
- 调试工具可以在 Editor 或 Development Build 中查看真相，但必须有明确开关。
- 匹配预览只能显示玩家根据现有信息可推断的条件，不显示真实匹配百分比。

## 5. 周推进状态机

建议阶段：

1. `WeekStart`：处理到期结果与款项，投递新内容。
2. `Research`：阅读邮件和订阅，整理球员。
3. `Assignment`：编辑球队槽位中的球员安排。
4. `Commit`：校验冲突并锁定本周决定。
5. `Advance`：周数加一，生成未来结果和付款计划。
6. `Review`：展示本周结算，再进入下一周。

状态切换应由一个明确的流程控制器负责，避免每个界面自行推进时间。

`SeasonCalendar` 以 2026-07-06 为第一周日期，每次推进七天，最多允许 52 个可玩周并映射赛季阶段。球员与招聘内容分别保存开放周和有效周数；最终到期周由内容模型计算，UI 再把它转换为明确日期。提交历史保存在 `WeekState.CommittedAssignments`，默认阻止球员跨周重新安排。

## 6. 结果生成

结果至少考虑：

- 球员真实能力与槽位要求的适配。
- 必要条件是否失败。
- 当前状态、伤病、性格和稳定性。
- 委托紧急程度、球队容忍度与安排人数。
- 受控随机波动。
- 利益事件或特殊剧情修正。

随机数应支持固定种子，使 EditMode 测试和问题复现稳定。

## 7. 存档原则

- 存档快照必须包含 `schemaVersion`。
- 保存当前周、经济状态、声望、开放委托、已读信息、安排、待结算结果和待到账款项。
- 静态内容通过稳定 ID 引用，不能依赖 Unity 实例 ID。
- M1 至少提供一个全局进度槽位和“重新开始”功能。
- 每次结束本周后自动保存；重要界面切换不强制写盘。

## 8. 测试计划

### EditMode

- 多槽位委托的提交校验。
- 同一球员同周重复安排限制。
- 已提交球员跨周不可再次安排。
- 球员与招聘按配置周数到期并触发条件邮件。
- 52 周日期与赛季阶段边界。
- 部分匹配的报酬折扣。
- 延迟一到两周的结果与付款排期。
- 固定随机种子下的结果可复现性。
- 存档快照往返和版本校验。
- 隐藏数据不会进入公开 ViewModel。

### PlayMode

- 从收件箱进入委托工作台。
- 拖拽球员到槽位、撤回和冲突提示。
- 结束本周确认流程。
- 下一周出现结果邮件与到账款项。
- 保存、退出 Play Mode 后重新加载进度。
- 1:1 肖像/封面约束、提交后离池、到期邮件与完整 52 周上限。

## 9. 建议目录

```text
Assets/
  Docs/
  Game/
    Content/
    Data/
    Prefabs/
    Scenes/
    Scripts/
      Domain/
      Runtime/
      Persistence/
      UI/
      Editor/
    Tests/
      EditMode/
      PlayMode/
```

在创建实际目录前，应先完成 M1 数据接口设计，避免产生只有文件夹、没有稳定边界的结构。

## 10. 当前技术风险

- MCP 在反射 Unity Pipeline 自带的 Roslyn 程序集时出现类型加载异常，可能影响 `unity_reflect` 的完整性。
- Unity CLI 与 Unity Pipeline 都处于实验版本，自动化脚本应固定版本并保留 Editor 批处理回退方案。
- Asset Store“我的资源”没有纳入当前 MCP 的 Package Manager 查询能力，远端已购资源检索需要单独设计登录与导入流程。
