# New Player Hunter 文档索引

本目录用于保存游戏设计、技术方案、决策记录与开发进度。游戏代码、场景和资源发生实质变化时，应同步更新相关文档。

## 文档入口

- [GAME_DESIGN.md](GAME_DESIGN.md)：游戏愿景、核心循环、玩法规则与 M1 范围。
- [TECHNICAL_PLAN.md](TECHNICAL_PLAN.md)：当前技术基线、系统边界、存档与测试方案。
- [DECISIONS.md](DECISIONS.md)：已经确认且会影响后续实现的关键决策。
- [PROGRESS.md](PROGRESS.md)：当前项目状态、已完成工作、验证结果和已知问题。
- [TODO.md](TODO.md)：尚未完成的工作项及优先级。
- [SIX_WEEK_CONTENT_PLAN.md](SIX_WEEK_CONTENT_PLAN.md)：已接入游戏的六周剧情、候选比较、期刊主题与跨周回收。
- [CONTENT_SUBMISSION_GUIDE.md](CONTENT_SUBMISSION_GUIDE.md)：人工填写球员、招聘、邮件、期刊与附件的提交规范。
- [内容提交 Excel 模板](../../output/spreadsheet/NewPlayerHunter_Content_Submission_Template.xlsx)：可直接发给内容人员填写并收回整合。

## 维护约定

1. `DECISIONS.md` 是已确认产品规则的来源；规则改变时保留旧决定，并追加替代决定。
2. `TODO.md` 只记录未完成或正在进行的工作；完成后勾选，并在 `PROGRESS.md` 写入结果与验证方式。
3. `PROGRESS.md` 记录事实，不把“代码已生成”写成“功能已完成”；Gameplay 功能必须注明编译、测试和 Play Mode 验证状态。
4. `GAME_DESIGN.md` 描述玩家体验；具体类名、接口和序列化细节放在 `TECHNICAL_PLAN.md`。
5. 日期统一使用 `YYYY-MM-DD`，里程碑使用 `M1`、`M2` 等稳定编号。

## 当前阶段

- 阶段：M1-B 内容与存档主体已完成（批次 01 覆盖全部 52 周：51 球员、35 招聘、193 目录邮件、34 期期刊）；剩余英文翻译与发布构建
- 平台：Windows
- 当前可玩版本：Assets/Game/Scenes/Game.unity
- 最近更新：2026-09-07

## 在 Unity Editor 中试玩

1. 打开 Assets/Game/Scenes/Game.unity。
2. 确认 Game View 使用 16:9 比例。
3. 点击 Play。
4. 默认进入邮件页；点击“邮件”与“订阅期刊”对照来源、立场和互相冲突的球员信息。
5. 点击左上角“球员分配”进入工作台；多条招聘同时有效时，点击左上列表切换当前委托。
6. 点击或拖拽一张可用球员卡到左侧招聘槽位，再点击“结束本周”。
7. 多槽位委托允许空缺，但需要再次点击“结束本周”确认部分提交。
8. 试训回函会在 1–2 周后到达收件箱，包含定性评价（可能有意外发挥、性格冲突、体能疑虑、隐藏伤病变体）与报酬到账说明。

## 内容与美术管线（批次整合）

1. `python output/spreadsheet/extract_batch.py <xlsx>`：提取内容人员 Excel 并打印校验报告。
2. 编辑 `content_design.json`（结构字段）与 prose JSON（文案）。
3. `python output/spreadsheet/build_factory.py`：重新生成 `LateSeasonContentFactory.cs`。
4. 美术：`make_art_manifest.py` 生成清单（肖像、封面、期刊题图）→ `bash output/art_raw/gen_art_serial.sh` 调本地 grok 批量生图（注意先建好对应子目录）→ `python output/spreadsheet/rebuild_atlases.py` 合成 8×8 肖像、6×6 封面与 6×6 题图图集。期刊内容页在 prose JSON 中用 `illustration` 主题键标注，`build_factory.py` 映射为图集索引。
5. Unity 菜单 Tools/New Player Hunter/Rebuild Game Scene 重建正式场景（会同时刷新内容目录资产），然后跑 EditMode/PlayMode 测试。

## 测试与已知环境问题

- EditMode/PlayMode 全部通过（35 + 7，2026-09-07）。通过 Unity MCP `run_tests` 执行。
- 域重载后立即 `run_tests` 偶发“初始化超时”，重试即通过。
- Console 既有噪音（不影响编译与测试）：`scripting_class_is_subclass_of(NULL)` 断言、MCP/Pipeline Roslyn 反射异常、"Editor is not in automated mode" 警告。
- 存档版本为 schemaVersion 2；开发期内不做旧档兼容（D-026），游戏完成后统一梳理。
