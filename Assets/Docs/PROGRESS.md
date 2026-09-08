# 开发进度

- 当前阶段：M1-B 完成（含英文翻译全套与主菜单）；下一候选：试玩打磨、Windows 发布构建、M2 立项
- 当前状态：51 名球员、193 封目录邮件、34 期杂志全部中英双语；主菜单（继续/新游戏/语言切换/退出）与游戏内菜单入口、读信解锁、有效期、利益事件、52 周年度循环、自动存档、俱乐部结果回函（含伤病/冲突/意外变体）、可切换多委托列表与转场/点击反馈均可在 Unity Editor 中运行
- 最近更新：2026-09-08

## 当前项目快照

- Unity 6000.0.81f1，URP 2D，目标平台为 Windows Standalone。
- 当前 Build Settings 只启用 `Assets/Game/Scenes/Game.unity`。
- `Game.unity` 直接保存 Main Camera、Global Light 2D、EventSystem、WeeklyGame、Canvas、信息/分配双工作区、256 个邮件项、36 个期刊目录项、34 期期刊页面、51 张可滚动球员卡、8 项可切换委托列表、2 个招聘槽位和 DragGhost。
- 已实现 M1-A 纯 C# Domain、六周固定邮件与订阅、正式周循环工作台、EditMode 与 PlayMode 测试。
- Unity MCP 3.4.6 已连接，编辑器状态可读取，核心查询工具可用。
- Unity CLI 1.0.0-beta.3 已安装。
- Easy Save 3 与 GUI Pro - Casual Game 已导入项目。

## 已完成

### 2026-09-08：英文翻译全套与主菜单

- [x] 建立 `output/spreadsheet/glossary_en.json` 术语表（51 球员、35 俱乐部、9 期刊、73 条固定梗译法），仲裁同一俱乐部多中文名与 Mendes/Mendez 等姓氏撞车。
- [x] 内容全量双语：prose JSON 增加 `*En` 兄弟键（35 球员、29 委托、28 期期刊 112 页、28 期刊名映射），`SixWeekContentFactory` 571 处 `L(zh, en)` 就地双语（中文原文逐字未动），`build_factory.py` 生成双语 `LateSeasonContentFactory`，`check_en_coverage.py` 零缺失验收（D-030）。
- [x] 界面字符串表 `UiStrings`（205 键双语）覆盖 GameController 与场景构建器全部 UI 文本；日期/期号/收件时间等格式化串显示层本地化（D-031）；结果邮件 18 条评价模板双语（`ResultMailFactory.Create` 感知语言）。
- [x] 主菜单：Scene 预置 `MainMenuOverlay`（继续/新游戏二次确认/语言切换/退出），头部"重新开始"替换为"菜单"入口，运行时对象 null 容忍（D-029）；语言偏好存独立 ES3 键 `ui.language`，不进进度快照、不升 schemaVersion（D-028）。
- [x] 跨文件译名一致性脚本化核查，修复"河谷联"一处冲突；`GDKEditionAutoGen`（com.unity.microsoft.gdk 自动生成资产）入库。

验证：

- Unity 脚本编译：通过（Pipeline recompile_status completed, failed=false）。
- EditMode：39 项全部通过（新增 `AllContent_EnglishTranslationsPresent` 反射遍历全目录 LocalizedText、`UiStrings_AllKeysHaveBothLanguages`、英文结果邮件结构）。
- PlayMode：8 项全部通过（新增主菜单启动即开、语言切换更新头部按钮、清理语言偏好键）。
- 场景重建：菜单命令 "Tools/New Player Hunter/Rebuild Game Scene" 执行成功，目录资产已写入双语。
- 实际 Play Mode：主菜单启动即开（CanvasGroup 激活）、头部"菜单"按钮在档；Console 无新增 Gameplay 错误。中英文模式排版目检待用户确认（英文正文长度约为中文 1.6–2.7 倍，PlayMode 溢出断言未触发）。
- 工具事件：Pipeline HTTP 服务曾因错误参数触发的测试超时卡死，经 "Pipeline/Stop Server" + "Start Server" 菜单重启恢复，未重启编辑器。

### 2026-09-08：期刊内容页题图与界面文本清理

- [x] 建立 24 张共享主题题图（转会、合同、伤病、战术板、球场、酒馆等），grok 本地批量生成，合成 6×6 `MagazineIllustrationAtlas.png`。
- [x] `MagazinePageContent` 新增 `illustrationIndex`；28 期后段期刊在 prose JSON 以主题键标注、`build_factory.py` 映射索引，前 6 期在 `SixWeekContentFactory` 直接指定；封面页恒为 -1。
- [x] Feature / ScoutReport 版式按杂志版式接入题图：Feature 页双横版图（`illustrationIndex2`，未标则通栏横幅），ScoutReport 页右栏下单图；全部带墨色细框与主题图注（"插图 · xxx"），按每页正文实际渲染高度动态摆放，不足 12% 版高自动隐藏。
- [x] 清理界面开发阶段与元描述文本：页脚默认状态"开发语言：中文"、收件箱标签"真实收件箱"、封面默认导语"真实电子杂志式"。
- [x] 内容文案全量扫描，未发现 AI 套话（"值得注意的是/综上所述/赋能/闭环"等零命中）与英文残留。
- [x] 回退 Editor 自动安装的 com.unity.ai.assistant / com.unity.ai.inference 及其设置变更。
- [x] `gen_art_serial.sh` 支持指定清单文件与 PID 隔离临时文件，可安全多开；`make_art_manifest.py` 与 `rebuild_atlases.py` 覆盖题图管线。

验证：

- Unity 脚本编译：通过（Pipeline recompile_status completed, failed=false）。
- EditMode：36 项全部通过（新增 `MagazineContentPages_ReferenceValidIllustrationSlots`）。
- PlayMode：7 项全部通过（新增 Feature 页题图槽位激活且纹理非空断言）。
- 实际 Play Mode：双图/通栏/单图版式、墨框图注与长文隐藏兜底均经用户目检确认。

### 2026-08-10：项目发现与设计确认

- [x] 检查项目结构、Packages、场景、Git 状态和 Unity 版本。
- [x] 读取两张初始界面规划图。
- [x] 确认 Windows、16:9、鼠标优先的目标平台与交互。
- [x] 确认按周推进、多人委托、永久隐藏能力和延迟一到两周结算。
- [x] 确认虚构足球喜剧的内容方向。

验证：本阶段仅完成只读审计和设计讨论，没有宣称 Gameplay 功能已实现。

### 2026-08-10：文档基线

- [x] 创建 `Assets/Docs` 文档目录。
- [x] 创建游戏设计、技术规划、决策、进度和 TODO 文档。
- [x] 建立文档维护约定。

验证：文档内容已依据已确认的五项产品决定整理；未修改游戏场景或业务代码。

### 2026-08-10：M1-A 可测试的周循环规则

- [x] 在 `Assets/Game/Scripts/Domain` 建立不依赖 UnityEngine、场景、UI 或 Easy Save 的 Domain 程序集。
- [x] 实现球员公开资料、隐藏真相、声明与证据、多人委托槽位、安排、结果和应收款模型。
- [x] 使用手工稳定字符串 ID；Domain 不依赖 Unity Instance ID。
- [x] 实现可配置的同周重复安排限制，默认同一球员每周只能安排一次。
- [x] 实现部分匹配报酬折扣、基础声望变化，以及延迟一到两周的结果和付款。
- [x] 通过 `IRandomSource` 与 `SeededRandomSource` 支持固定种子复现。
- [x] 建立公开 ViewModel 工厂，输入和输出均不包含 `PlayerTruth`。
- [x] 添加包含固定数据六周模拟的 EditMode 测试。

验证：

- Unity 脚本编译：通过；最终 Console 没有 `Assets/Game` 编译错误。
- EditMode：`NewPlayerHunter.Domain.EditModeTests` 共 7 项，7 通过、0 失败、0 跳过。
- 最终测试作业：`8b1e208d9c044dcea18709a15103b98f`，结果 `Passed`。
- Play Mode：未执行；M1-A 没有场景、GameObject、Prefab 或 UI 变更。
- 既有工具链日志仍存在：MCP/Pipeline Roslyn 反射异常、Pipeline 非 automated 警告及域重载 Unity 内部 assert；未发现它们阻断本次编译或 EditMode 测试。

### 2026-08-10：正式 Game 场景与可玩六周工作台

- [x] 创建正式 `Assets/Game/Scenes/Game.unity` 并设为唯一启用的 Build Settings 场景。
- [x] 在场景中预置 Main Camera、Global Light 2D、EventSystem、Canvas、顶部经济栏、委托区、8 张球员卡、2 个招聘槽位、底部反馈区和 DragGhost。
- [x] 实现 8 名虚构球员和 6 个周委托，覆盖紧急替补、集体试训和多位置长期引援。
- [x] 点击或拖拽球员到槽位，支持撤回、重复安排限制、空缺二次确认、周推进、延迟结果、应收款和到账日志。
- [x] 控制器只更新场景中已有对象，不在运行时创建或销毁结构性 GameObject。
- [x] 拆分可挂载 MonoBehaviour 脚本，修复场景重载后的 Missing Script 与空引用。
- [x] 添加场景层级 PlayMode 测试，验证摄像机、灯光、输入、8 张卡、2 个槽位、DragGhost、文本不溢出与推进一周。

验证：

- Unity 脚本编译：通过；没有 `Assets/Game` 编译错误。
- EditMode：`NewPlayerHunter.Domain.EditModeTests` 共 7 项，7 通过、0 失败、0 跳过；最终作业 `077002228c584becbd5f2dd3d8019f51`。
- PlayMode：`NewPlayerHunter.Gameplay.PlayModeTests` 共 1 项，1 通过、0 失败、0 跳过；最终作业 `a39d3d573d2a4e908ad3e94a4f24ec8a`。
- 实际 Play Mode：已进入 `Game.unity` 并检查 Game View；球员姓名、当前委托、经济栏、槽位和按钮显示正常。
- Console：本次运行没有新增 Gameplay 错误或警告；仍会在域重载时重复出现既有 Roslyn/Pipeline 5 条环境信息。
- Windows 构建：未执行；按当前约定，游戏整体完成后再运行发布构建。

### 2026-08-10：邮件/订阅信息筛选工作区

- [x] 新增不包含 `PlayerTruth` 的公开信息模型，按邮件与订阅频道、发布时间周、来源说明和关联球员组织内容。
- [x] 创建 8 封直接邮件与 8 篇订阅报道；宣传、亲友请托、俱乐部需求和独立报道会提供可交叉验证或互相矛盾的信息。
- [x] 每周默认进入信息工作区，左上角可切换到球员分配台；MAIL 与 SUBSCRIPTIONS 可独立筛选。
- [x] 在正式场景中预置工具栏、8 个消息列表项、详情阅读窗和双工作区按钮，运行时只绑定文本、颜色、显隐与按钮监听。
- [x] 信息按当前周逐步解锁，正文显示定性来源可靠性与相关球员，不泄露隐藏能力数值。

验证：

- Unity 脚本编译：通过；没有新增 `Assets/Game` 编译错误。
- EditMode：领域规则共 7 项，7 通过、0 失败、0 跳过；最终作业 `77057f41e6ee4a1ca7bed8009f32c879`。
- PlayMode：正式场景共 1 项，1 通过、0 失败、0 跳过；最终作业 `bfad555a5da046ae8b6bec7527533d94`，并直接调用场景按钮 `onClick` 验证订阅筛选和工作区切换。
- 实际 Play Mode：已在 16:9 Game View 检查默认信息页；顶部切换、三封首周邮件、来源说明、正文、关联球员和底部状态均清晰可见。
- Console：没有新增 Gameplay 错误或警告；仍会出现既有 Roslyn/Pipeline 5 条环境信息。
- Windows 构建：未执行；按约定仅在游戏整体完成后运行发布构建。

### 2026-08-10：正式中文内容池、读信解锁与多页期刊

- [x] 创建 `GameContentCatalog` ScriptableObject，配置 8 名球员、6 个需求、14 封邮件和 2 期各 4 页的电子期刊；所有关联使用稳定 ID。
- [x] 建立 `LocalizedText` 与中文优先开发语言；英文栏位已保留，缺少英文时安全回退中文，后续统一翻译不改数据结构。
- [x] 邮件只有在玩家实际点开后才标记已读，并解锁其关联需求或球员；未读内容不会出现在球员分配页。
- [x] 邮件详情使用固定招聘需求区与固定球员简历区，需求、槽位、截止周、委托价、付款条款、公开简介、声明和旁证位置固定。
- [x] 邮件列表项固定包含头像、发件人、标题、收件时间、已读圆点/文字和取自正文开头且以省略号结尾的摘要。
- [x] 订阅改为独立电子期刊浏览器，包含目录、封面、专题双栏和球探报告三类页面，以及上一页/下一页；阅读期刊不会绕过正式简历邮件解锁球员。
- [x] 创建并预热 Windows 中文动态 TMP 字体资产；所有 UI、邮件和期刊对象均预置在正式 Scene 中。

验证：

- Unity 脚本编译：通过；最终 Console 没有 `Assets/Game` 编译错误或本次新增警告。
- EditMode：共 12 项，12 通过、0 失败、0 跳过；覆盖既有领域规则、内容池引用、双语回退、邮件时间和期刊多页结构。
- PlayMode：正式场景 1 项，1 通过、0 失败、0 跳过；最终作业 `67ee9483860e466b8a19b83ffc8a90f4`。
- PlayMode 流程覆盖：读招聘邮件解锁需求、读简历解锁球员、期刊翻页不解锁、进入分配页、提交球员并推进到第 2 周。
- 实际 Game View：已检查中文收件箱、结构化招聘邮件、期刊专题内页和优化后的头像/时间/状态/标题/摘要三行邮件列表。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-08-10：动态可用球员池与人工内容提交模板

- [x] 移除顶部左上角游戏标题，将信息/分配工作区按钮前移并通过正式场景构建命令保存到 `Game.unity`。
- [x] 将八张预置球员卡改为通用 View 池，不再按球员或位置永久绑定；同一位置可以显示多名球员。
- [x] 可用球员按第一封关联邮件的到达周和内容顺序从上到下绑定，即使玩家倒序打开邮件也不改变顺序。
- [x] 球员放入招聘槽位后从可用列表隐藏；清空槽位后按原顺序恢复，剩余球员自动向上补位。
- [x] 新增人工内容填写规范与 Excel 模板，分表收集球员、招聘、槽位、邮件、期刊期次、期刊页面和附件。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：共 13 项，13 通过、0 失败、0 跳过；最终作业 `543994abbfd64232a83decd12a04b4d4`。
- PlayMode：正式场景 1 项，1 通过、0 失败、0 跳过；最终作业 `8148514d539545bbbcea8e513ef61efa`。
- PlayMode 覆盖：标题不存在、倒序读信仍按到达顺序显示、分配后隐藏、撤回后恢复、通用卡片向上补位，以及原有读信/期刊/周推进流程。
- 内容工作簿：已用 openpyxl 生成、重新打开并校验 9 张工作表、关键列名和检查公式；当前环境无 LibreOffice，未进行 PDF 渲染。
- Windows 构建：未执行；继续遵循整体完成后再构建的约定。

### 2026-08-10：完整六周内容季、原创测试美术与利益事件

- [x] 把默认内容扩展为 16 名虚构球员、6 条逐周需求、30 封邮件和 6 期各 4 页电子杂志；每周固定 5 封邮件和 1 期刊物。
- [x] 建立跨周内容弧线，回收“41 球”“致敬乐队门将”“三明治挡号码”“第五次扑点”“十九张表”和“保温杯”等线索。
- [x] 使用原创生成美术制作 4×4 球员肖像图集与 3×2 期刊封面图集，并绑定到球员卡、邮件简历和期刊封面固定图片位。
- [x] 将球员区改为 Scene 预置的 16 卡可滚动 View 池；邮件列表扩为 36 个预置项，期刊目录扩为 6 个预置项；运行时仍不创建结构对象。
- [x] 新增私人请托固定信息区；第 1 周把卡洛提交到任一槽位立即获得 €350，第 4 周调查造成声望 -3。
- [x] 新增 `SIX_WEEK_CONTENT_PLAN.md`，记录每周选择、期刊主题、信息矛盾与跨周回收。

验证：

- Unity 脚本编译：通过；本次 `Assets/Game` 没有编译错误。
- EditMode：17 项，17 通过、0 失败、0 跳过；最终作业 `e30935c61ddc406fbf80c909eb367641`。
- PlayMode：正式场景 2 项，2 通过、0 失败、0 跳过；最终作业 `5de2759300bb443d8ac069ee683c2b01`。
- PlayMode 覆盖：16 卡/36 邮件项/6 期刊项层级、肖像与封面纹理、邮件解锁、期刊翻页、分配隐藏与恢复、周推进，以及卡洛即时收益到第 4 周延迟声望后果。
- 普通 Editor Play Mode：已进入 `Game.unity` 并捕获 1920×1080 Game View；首周 5 封中文邮件、顶部经济栏、信息/分配切换与列表排版清晰可见。
- 已知工具日志：仍存在既有 `scripting_class_is_subclass_of(NULL)` Editor 断言与 MCP/Pipeline Roslyn 反射异常；未指向 Gameplay，也未阻断编译和测试。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-08-10：52 周足球年度、内容有效期与俱乐部履历

- [x] 新增 `SeasonCalendar`，以 2026-07-06 为第一周日期，覆盖季前、转会窗、联赛、欧战、淘汰赛、决赛和赛季总结，最多允许 52 个可玩周。
- [x] 顶部状态改为明确日期、赛季阶段与 `n/52` 进度；第 7–52 周加入 13 封赛事节点邮件。
- [x] 修复已提交球员次周重新出现的问题；提交历史默认永久阻止再次安排，并保留领域规则开关。
- [x] 为 16 名球员和 6 条招聘加入相对有效周数、到期移除及条件式离队/撤单邮件。
- [x] 招聘固定显示俱乐部实力、上赛季排名、历史最好成绩、持续周数、明确截止日期、委托价与付款条款。
- [x] 卡洛请托改为指定雨城竞技；只有满足目标俱乐部要求才支付即时 €350，延迟调查后果保持不变。
- [x] 以中文重新编写 16 名球员、6 条招聘、前六周 30 封邮件与 24 页期刊，并补充 13 封赛季邮件和 22 封条件到期邮件；未发现美式橄榄球术语。
- [x] 所有 Scene 预置球员肖像、简历头像和期刊封面加入 1:1 显示约束；邮件池扩为 80 个 Scene 对象。
- [x] 通过 Unity MCP 重建并保存正式 `Game.unity`，没有直接编辑场景 YAML。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：20 项通过、0 失败、0 跳过；最终作业 `449c706169fa4ec19ea3c746a5846ae1`，覆盖跨周重复提交、相对有效期、52 周日期/阶段、内容引用和隐藏数据边界。
- PlayMode：3 项通过、0 失败、0 跳过；最终作业 `c9cb34289f684880a70d067e598776c1`，覆盖提交后离池、到期邮件、俱乐部信息、1:1 图片、正式 Scene 层级和完整推进至第 53 状态周后停止。
- 普通 Editor Play Mode：已检查 1920×1080 Game View；首周中文邮件列表、日期/季前阶段、经济栏、信息详情和底部日志均正常显示。
- 已知工具日志：既有 `scripting_class_is_subclass_of(NULL)` Editor 断言与 MCP/Pipeline Roslyn 反射异常仍存在；未指向 Gameplay，也未阻断编译和测试。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-08-19：Git 基线与 Easy Save 3 存档接入

- [x] 建立第三方资源与工具链基线提交（Layer Lab、Easy Save 3、TextMesh Pro、Packages 与 ProjectSettings），游戏代码与文档单独提交；远端推送未执行。
- [x] 启用 Easy Save 3 自带的 asmdef（运行时装配件位于插件根目录，Editor 装配件位于 Editor 目录），清除装配期间的 TMP 示例脚本误装配件问题。
- [x] Domain 新增 `WeekStateSnapshot` 等纯 C# 快照模型与 `WeekState.CreateSnapshot`/`ApplySnapshot`；`SeededRandomSource` 记录消耗次数并支持快进恢复。
- [x] 新增 `NewPlayerHunter.Persistence` 程序集：`GameProgressSnapshot`（JsonUtility 兼容 JSON 模型，金额以字符串保存）、`ProgressSnapshotMapper` 和 `SaveGameService`；Easy Save 3 仅作键值存储，Domain 不依赖 Easy Save API。
- [x] `GameController` 每次结束本周后自动保存；启动时存在存档则恢复；`RestartGame` 删除存档并重置；自动保存失败只记录错误，不阻断周推进。
- [x] 存档决策写入 `DECISIONS.md`（D-020 schemaVersion 与迁移、D-021 随机序列恢复、D-022 周初粒度与自动保存、D-023 键值存储边界）。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：28 项通过、0 失败、0 跳过；最终作业 `741634d4b6b8497d8f81050abb1efd25`；新增覆盖快照往返、无效快照拒绝、随机快进复现、ES3 存取往返、版本不匹配拒绝、删除存档和金额精度。
- PlayMode：4 项通过、0 失败、0 跳过；最终作业 `7c3b0e320e0e4363b188c9a07b3c62ed`；新增覆盖结束本周自动保存、重载恢复周数/现金/已读/解锁/事件标记、提交球员不离池以及重新开始删除存档。
- PlayMode 测试前后均清理默认存档，避免测试写入污染开发进度。
- 已知工具日志：既有 `scripting_class_is_subclass_of(NULL)` Editor 断言与 MCP/Pipeline Roslyn 反射异常仍存在；未指向 Gameplay，也未阻断编译和测试。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-08-19：俱乐部正式回函与游戏化反馈

- [x] 提交球员后 1–2 周收到俱乐部正式回函邮件：按结果类型 × 匹配度分档的多档定性评价（不泄露隐藏数值）、报酬金额与到账说明；邮件 ID 稳定（result.w周.需求.槽位.球员），已读状态随存档保存，读档后从 DeliveredOutcomes 重新生成，不新增存档字段。
- [x] 收件箱改为未读优先 + 到达周倒序；超过 80 个预置项容量时裁掉最旧已读邮件并记录警告，避免晚赛季结果邮件挤掉未读解锁邮件。
- [x] 结束本周黑屏转场：场景预置 WeekTransitionOverlay（CanvasGroup + 中文 TMP），淡出 → 周推进 → 显示新日期与赛季阶段 → 淡入；提交被拦下时不展示新日期并直接淡回；转场期间锁定输入；测试仍走无动画同步路径。
- [x] 信息/分配工作区切换约 0.1 秒交叉淡入淡出；两个工作区根节点已预置 CanvasGroup。
- [x] 点击反馈：顶部与工具栏按钮、邮件列表项和球员卡点击时缩放回弹（0.12 秒），拖拽路径不受影响。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：28 项通过、0 失败、0 跳过；最终作业 `ccc7e1a1c3f24ce19db811960da0aa6c`。
- PlayMode：5 项通过、0 失败、0 跳过；最终作业 `92ef70cc81fd4309b50769663ac007b0`；新增覆盖转场遮罩层级、结果邮件到达/俱乐部署名/金额、结果邮件随存档恢复与已读状态保持。
- 已知工具行为：`run_tests` 在域重载后立即启动时偶发“初始化超时”，重试即通过；与 Gameplay 无关。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-08-19：动效节奏、图片布局修复与简历薪资/经历

- [x] 动效整体调慢，匹配思考型节奏：黑屏结算 0.5 秒淡出 + 1.0 秒日期停留 + 0.5 秒淡入（提交被拦下时 0.3 秒淡回）；工作区切换 0.22/0.25 秒交叉淡入淡出；点击回弹 0.2 秒且幅度收窄到 0.95。
- [x] 修复头像/图片与文字重叠：根因是 `CreateRawImage` 直接使用 `AspectRatioFitter.FitInParent`，图片相对父级放大而非限制在锚定区域内；改为“容器锚定区域 + 子 RawImage 容器内 1:1 适配居中”，球员卡、邮件简历肖像和期刊封面统一该结构（查找路径变为 `Portrait/Image` 与 `CoverImage/Image`）。
- [x] 球员简历新增薪资范围（欧元/周、公开字段）与球员经历；16 名球员全部补齐中文内容；简历区按新十行布局重排。
- [x] 场景构建器纳入周过渡遮罩与工作区 CanvasGroup，并通过显式菜单命令重建正式场景；构建命令与 MCP 场景编辑不再互相覆盖。
- [x] 内容提交规范与 Excel 模板补充 `salary_min_weekly`、`salary_max_weekly`、`career_history_zh/en` 列。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：29 项通过、0 失败、0 跳过；最终作业 `d430a29bac7b4ee19640e3735d6b81b1`，新增薪资范围与经历内容校验。
- PlayMode：5 项通过、0 失败、0 跳过；最终作业 `f073e04cf83a42f29eb9f5dfc61b86b0`，覆盖简历薪资/经历显示与新图片路径。
- 实际 Game View 截图：简历区肖像、姓名、薪资、经历与球员卡头像/文字均无重叠，图片保持 1:1。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

### 2026-09-07：内容批次 01 全赛季整合与结果文本库

- [x] 建立 Excel → JSON → C# 工厂的内容管线：`extract_batch.py` 提取校验、`content_design.json` 补齐结构字段、`build_factory.py` 生成 `LateSeasonContentFactory.cs`；`PopulateM1Defaults()` 合并六周与赛季工厂（D-024）。
- [x] 接入内容人员批次 01：35 名新球员、29 条新招聘、64 封解锁邮件，排布第 7–52 周；新增 28 期期刊（转会窗与决赛周加密），全部中文、跨周回收既有梗。
- [x] 分配页新增可切换多委托列表（8 项预置池、选中高亮），选中委托随存档保存；`schemaVersion` 升为 2，旧档按 D-026 作废。
- [x] 场景池扩容：邮件项 80→256、期刊目录项 6→36、球员卡 16→51；肖像/封面图集网格参数常量化（8×8 / 6×6，D-025）。
- [x] 结果文本库：NarrativeKey 新增意外发挥、性格冲突、体能疑虑、隐藏伤病变体，由既有数据确定性派生、不新增随机消耗（D-027）；回函文案扩至 18 条。

验证：

- Unity 脚本编译：通过；没有本次变更造成的 `Assets/Game` 编译错误或警告。
- EditMode：35 项通过、0 失败、0 跳过；最终作业 `25b48c9e04bf4e8aa3bd00c284a9671e`；新增覆盖批次 01 计数/引用/周密度/图集索引与结果邮件变体。
- PlayMode：7 项通过、0 失败、0 跳过；最终作业 `f1a1f8c9db354beb863742c70c534c04`；新增覆盖多委托列表切换、切换后提交/撤回与选中随存档恢复。
- 实际 Game View：已检查 1280×720 首周收件箱；新布局与既有内容显示正常。
- 美术：35 张新肖像与 28 张新封面已由本地 grok CLI 生成并经 `rebuild_atlases.py` 合成为 8×8 / 6×6 图集；修复委托列表与槽位文本带高度不足导致 TMP 省略号模式下零渲染的问题，并新增渲染字形数断言；Game View 已验证第 7 周多委托列表、新球员肖像与槽位文本显示正常。
- Windows 构建：未执行；遵循约定，整体完成后再构建。

## 已知问题与风险

1. MCP 反射 Unity Pipeline 自带的 `Microsoft.CodeAnalysis.CSharp` 时出现程序集类型加载异常。核心场景和 Console 查询可用，但 `unity_reflect` 可能不完整。
2. 正式收件箱、52 周全程内容（批次 01：35 球员、29 招聘、28 期刊）、多委托切换列表、条件式结果邮件（含伤病/冲突/意外变体）与 Easy Save 3 自动存档已可玩；英文翻译已全套接入（内容 + 界面，见 D-028 至 D-031）。
3. 第 7–52 周内容密度已由批次 01 补齐；后续批次如需扩容，沿用 D-024 管线并在 `content_design.json` 中分配新周次与图集索引。
4. Asset Store 远端已购资源不在当前 MCP Package Manager 查询范围内，后续需要单独验证半自动导入流程。

## 下一里程碑

### M1-B：内容与存档 ✅ 已完成（2026-09-08 收尾）

- ~~创建一条即时收益、延迟风险与声望后果的利益事件链。~~（2026-08-10 已完成卡洛请托链）
- ~~定义版本化运行时快照并接入 Easy Save 3。~~（2026-08-19 已完成，见 D-020 至 D-023）
- ~~跨周结果邮件与条件式正式回函。~~（2026-09-07 已完成，含叙事变体，见 D-027）
- ~~52 周全程内容密度与多委托切换列表。~~（2026-09-07 批次 01 完成，见 D-024）
- ~~英文翻译（内容 + 界面全套）。~~（2026-09-08 完成，含主菜单与游戏内语言切换，见 D-028 至 D-031）

### M2 候选方向（续作起点）

1. 试玩打磨：真机手感（拖拽目标、滚动速度、列表密度）与喜剧节奏检查；英文模式排版目检。
2. Later 项优先级排序：球队/经纪人关系值、更多期刊立场事件、谈判与佣金、伤病与成长。
3. Windows 发布构建（约定：游戏整体完成后执行）。
4. Asset Store 已购资源的半自动导入验证。
