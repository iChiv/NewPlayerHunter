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

- 阶段：M1 可玩周循环与内容/存档迭代（Easy Save 3 自动存档已接入）
- 平台：Windows
- 当前可玩版本：Assets/Game/Scenes/Game.unity
- 最近更新：2026-08-19

## 在 Unity Editor 中试玩

1. 打开 Assets/Game/Scenes/Game.unity。
2. 确认 Game View 使用 16:9 比例。
3. 点击 Play。
4. 默认进入邮件页；点击“邮件”与“订阅期刊”对照来源、立场和互相冲突的球员信息。
5. 点击左上角“球员分配”进入工作台。
6. 点击或拖拽一张可用球员卡到左侧招聘槽位，再点击“结束本周”。
7. 多槽位委托允许空缺，但需要再次点击“结束本周”确认部分提交。
