# 内容人工填写与提交规范

- 适用项目：New Player Hunter
- 开发语言：中文；英文字段可暂时留空
- 推荐提交文件：[NewPlayerHunter_Content_Submission_Template.xlsx](../../output/spreadsheet/NewPlayerHunter_Content_Submission_Template.xlsx)

这份规范用于把球员、招聘需求、邮件和电子期刊分配给内容人员填写。填写人员不需要接触 Unity，也不要直接修改 `GameContentCatalog`。收集完成后，把原始 Excel 和附件文件夹交回，由开发人员统一校验稳定 ID、引用关系、UI 长度和隐藏信息边界，再整合进游戏。

## 1. 提交方式

1. 每一批内容只提交一个 Excel，文件名使用 `NewPlayerHunter_Content_Batch_<批次名>_<YYYYMMDD>.xlsx`。
2. 直接复制模板；不要修改工作表名称、列名、列顺序或下拉枚举值。
3. 每一条内容占一行。状态填写 `Draft`、`Ready` 或 `Approved`；只有 `Ready`/`Approved` 会进入整合检查。
4. 中文栏必填；英文栏开发阶段可留空，后续统一翻译。
5. 头像、期刊插图等附件放在与 Excel 同名的文件夹内，文件名必须与“附件 Assets”表一致。
6. 不要在单元格中插图、合并单元格或用颜色表达规则；备注写入专用备注列。

## 2. 稳定 ID

- ID 只使用小写英文字母、数字和点号，例如 `player.rocket.rory`、`demand.week2.group`。
- ID 一经进入游戏不得改名；显示名称可以修改。
- 推荐前缀：`player.`、`demand.`、`slot.`、`mail.`、`mag.`、`asset.`。
- `related_player_id`、`related_demand_id`、`demand_id`、`issue_id` 必须引用其他表中真实存在的 ID。
- 同一位置允许有任意多名球员，位置不是唯一键。

## 3. 球员格式

填写“球员 Players”表，一行一名球员。

- 公开字段：姓名、简介、公开位置、本人/经纪人声明、外部旁证、来源可靠性。
- 薪资字段：`salary_min_weekly` 与 `salary_max_weekly`，单位欧元/周，均为非负整数且下限不大于上限；薪资范围是公开简历信息，会显示在球员简历区。
- 经历字段：`career_history_zh`（英文可空），一两行俱乐部/年份履历，允许延续喜剧风格，但不得泄露隐藏数值。
- 隐藏字段：真实能力、体能、职业性，均为 `0–100`。这些数字仅供结算，严禁写进公开邮件、期刊或 UI 文本。
- 公开位置枚举：`Goalkeeper`、`Defender`、`WingBack`、`Midfielder`、`Winger`、`Forward`。
- 可靠性枚举：`Unverified`、`Low`、`Medium`、`High`。
- 建议长度：中文姓名 2–8 字；简介 30–80 字；声明与旁证各 25–90 字。
- 每名球员至少要有一封 `PlayerResume` 或其他明确允许解锁的关联邮件，否则不会进入可用球员列表。
- 填写 `available_from_week` 与 `availability_weeks`；后者表示从开放周起持续几周。另填到期邮件标题与正文，说明球员为何退出市场、转行或改变计划。
- 肖像附件必须提供 1:1 方形构图、原创/授权来源和禁止元素说明；整合后分配稳定 `portraitIndex`，内容人员不要自行修改图集坐标。

球员卡是通用列表视图，不按位置预留固定格子。多名前锋、多名中场可以同时存在；游戏按其第一封关联球员邮件的到达周和周内顺序，自上而下排列。

## 4. 招聘信息格式

招聘主体填写“招聘 Demands”表；每个招聘名额填写“招聘槽位 DemandSlots”表。

- 一条招聘必须有一个或多个槽位。
- `opened_week` 是招聘开放周；`active_weeks` 是从开放周起持续几周。不要手填绝对截止周，游戏会计算到期周和明确日期。
- 必填俱乐部显示名、上赛季排名、历史最好成绩与实力简介；这些信息用于玩家判断私人请托的目标俱乐部条件。
- 填写招聘到期邮件标题与正文，说明球队撤单、已从别处签人或改变计划的原因。
- `base_reward` 是基础委托价；付款时机与折扣说明写在 `payment_terms`。
- 槽位的真实门槛 `minimum_ability`、`preferred_fitness`、`preferred_professionalism` 为 `0–100`，不会直接显示给玩家。
- `is_required` 使用 `TRUE`/`FALSE`；多槽位委托允许部分提交。
- 建议长度：招聘标题 8–24 字；描述 40–120 字；付款条款 20–60 字。
- 每条招聘必须有一封 `ClubRequest` 邮件关联其 `demand_id`，玩家打开邮件后招聘才进入分配工作台。

## 5. 邮件格式

填写“邮件 Mails”表，一行一封真实收到的邮件。

- 类型枚举：`ClubRequest`、`PlayerResume`、`PrivateRequest`、`General`。
- `published_week` 决定到达周；`delivery_order_in_week` 决定同周先后顺序，数字越小越早。
- `received_time` 只填写显示时间，例如“周一 08:20”。
- 列表固定显示头像缩写、发件人、时间、已读标记、标题和正文开头摘要。摘要由正文自动截断并以省略号结尾。
- `preview` 用于内容编辑和未来通知，不要与 `body` 完全重复。
- `source_note` 写来源身份、立场和时效性，不写隐藏数值。
- 球员邮件填写 `related_player_id`；招聘邮件填写 `related_demand_id`；普通信息邮件可以都留空。
- 私人请托如指定俱乐部，填写 `private_required_club_id` 与公开可读的 `private_target_club_requirement`；即时收益只在实际安排满足该条件时触发。
- 建议长度：发件人不超过 18 字；标题不超过 30 字；正文 60–200 字；来源说明不超过 40 字。

## 6. 期刊格式

期刊主体填写“期刊 Issues”表；每一页填写“期刊页面 MagazinePages”表。

- 每期至少 3 页，第一页必须是 `Cover`；`page_number` 从 1 连续递增。
- 页面版式枚举：`Cover`、`Feature`、`ScoutReport`。
- `Cover`：短导语、主标题、副标题、封面摘要、导读和侧栏。
- `Feature`：专题文章；左右正文各 100–240 字，配引语和资料侧栏。
- `ScoutReport`：球探报告；左栏写优势/观察，右栏写疑问/风险，配样本或来源说明。
- `related_player_id` 只用于交叉线索。阅读期刊永远不会直接解锁球员。
- 标题建议不超过 28 字；副标题不超过 60 字；引语不超过 55 字；侧栏正文不超过 100 字。
- 如果版式需要图片，在 `image_brief` 写构图、人物、色调和禁止元素，在附件表登记文件；不要直接使用现实球员照片或球队徽章。
- 期刊封面图必须为 1:1 方形，不内嵌标题、期号或徽章，文字由 Unity 排版；整合后分配稳定 `coverIndex`。

## 7. 交付前检查

- 所有 `Ready`/`Approved` 行的中文必填字段均已填写。
- 所有 ID 唯一，且引用 ID 存在。
- 每名准备进入游戏的球员至少有一封关联解锁邮件。
- 每条招聘至少有一个槽位和一封关联招聘邮件。
- 同周邮件的 `delivery_order_in_week` 不重复。
- 每期刊物第一页为 `Cover`，页码连续，至少使用两种版式。
- 隐藏能力和门槛没有泄露进公开文本。
- 没有直接复制现实姓名、照片、球队徽章或完整履历。
- 文案与美术只表达足球，不含美式橄榄球术语、装备或构图暗示。
- 所有球员肖像、邮件头像和期刊封面附件均为 1:1。
- 附件的授权来源、创作者和用途已填写。

整合时会先复制原文件备份，再运行引用、枚举、重复 ID、长度和内容边界检查；发现问题会按行号退回，不会直接猜测或改写内容人员的原意。

## 8. 整合管线（2026-09-07 起）

1. `python output/spreadsheet/extract_batch.py <xlsx>`：提取为 `content_batch_XX.json` 并打印校验报告。
2. 整合者在 `content_design.json` 补齐结构字段（稳定 ID、周次、俱乐部、薪资、槽位门槛、图集索引）；缺失文案在独立 prose JSON 中补齐。
3. `python output/spreadsheet/build_factory.py`：生成 `LateSeasonContentFactory.cs`，由 `PopulateM1Defaults()` 合并进目录。
4. 美术：`make_art_manifest.py` 生成生图清单，`output/art_raw/gen_art_serial.sh` 调用本地 grok 批量生成，`rebuild_atlases.py` 合成 8×8 肖像与 6×6 封面图集。
5. 重建正式场景（Tools/New Player Hunter/Rebuild Game Scene）并跑 EditMode/PlayMode 测试。
