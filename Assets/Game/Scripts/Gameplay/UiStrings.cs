using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewPlayerHunter.Gameplay
{
    public static class UiStrings
    {
        private const int Chinese = 0;
        private const int English = 1;

        private static readonly Dictionary<string, string[]> Entries =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                // Dates
                ["date.full"] = new[] { "yyyy年M月d日", "MMM d, yyyy" },

                // Main menu
                ["menu.title"] = new[] { "新星猎手", "New Player Hunter" },
                ["menu.continue"] = new[] { "继续游戏", "Continue" },
                ["menu.newGame"] = new[] { "新游戏", "New Game" },
                ["menu.newGameConfirm"] = new[] { "存档将被删除，再点一次确认", "Delete the save? Click again to confirm" },
                ["menu.language"] = new[] { "语言：中文", "Language: English" },
                ["menu.quit"] = new[] { "退出游戏", "Quit" },
                ["menu.resume"] = new[] { "返回游戏", "Back to Game" },

                // Header
                ["header.menu"] = new[] { "菜单", "Menu" },
                ["header.reset"] = new[] { "重新开始", "Restart" },
                ["header.informationTab"] = new[] { "收件箱 / 期刊", "Inbox / Magazines" },
                ["header.assignmentTab"] = new[] { "球员分配", "Assignments" },
                ["header.weekLine"] = new[] { "{0} · {1} · {2}/{3}周", "{0} · {1} · Week {2}/{3}" },
                ["header.weekPlaceholder"] = new[] { "2026年7月6日 · 季前训练 · 1/52周", "Jul 6, 2026 · Preseason · Week 1/52" },
                ["header.economy"] = new[] { "现金 {0}    应收 {1}    声望 {2}", "Cash {0}    Receivables {1}    Reputation {2}" },
                ["header.economyPlaceholder"] = new[] { "现金 €500.00    应收 €0.00    声望 10", "Cash €500.00    Receivables €0.00    Reputation 10" },

                // Status messages
                ["status.initial"] = new[] { "新赛季从季前训练开始，共 52 周。请先读邮件，再根据期刊线索交叉判断。", "A new season begins with preseason — 52 weeks in total. Read the mail first, then cross-check against magazine clues." },
                ["status.playerDeselected"] = new[] { "已取消选择球员。", "Player selection cleared." },
                ["status.playerSelected"] = new[] { "已选择 {0}，请点击或拖入招聘槽位。", "Selected {0}. Click or drag them into a recruitment slot." },
                ["status.demandSelected"] = new[] { "正在处理：{0} · {1}。", "Now handling: {0} · {1}." },
                ["status.slotCleared"] = new[] { "已清空该招聘槽位。", "That slot has been cleared." },
                ["status.playerAssigned"] = new[] { "已把 {0} 放入招聘槽位。", "{0} placed into the slot." },
                ["status.infoOpened"] = new[] { "信息中心已打开：邮件负责正式解锁，期刊负责交叉判断。", "Information hub: mail does the official unlocking; magazines help you cross-check." },
                ["status.assignmentOpenedEmpty"] = new[] { "当前没有已解锁且仍在有效期内的招聘；你仍可结束本周推进日期。", "No unlocked, still-active recruitment right now; you can still end the week to move the calendar." },
                ["status.assignmentOpened"] = new[] { "分配工作台只显示已读、未过期且从未提交给俱乐部的球员。", "The assignment desk only shows players who are read, unexpired, and never submitted to a club." },
                ["status.mailMode"] = new[] { "收件箱：打开邮件后，关联的招聘需求或球员简历才会进入工作台。", "Inbox: opening a mail moves its linked demand or résumé onto the assignment desk." },
                ["status.magazineMode"] = new[] { "订阅期刊：翻页比较报道，但期刊不会替代正式简历邮件。", "Subscriptions: flip through and compare stories, but magazines never replace a formal résumé mail." },
                ["status.mailReadUnlock"] = new[] { "已阅读《{0}》，关联档案已进入分配工作台。", "Read \"{0}\" — the linked file is now on the assignment desk." },
                ["status.mailRead"] = new[] { "已阅读《{0}》。", "Read \"{0}\"." },
                ["status.mailReopened"] = new[] { "重新打开《{0}》。", "Reopened \"{0}\"." },
                ["status.issueSelected"] = new[] { "正在阅读《{0}》。", "Now reading \"{0}\"." },
                ["status.emptySlotsWarning"] = new[] { "还有 {0} 个必需槽位为空。再次点击“结束本周”确认不完整提交或暂不推荐。", "{0} required slot(s) are still empty. Click \"End Week\" again to confirm an incomplete submission or skip it." },
                ["status.gameComplete"] = new[] { "一年赛季结束。现金 {0}，应收 {1}，声望 {2}。", "The season is over. Cash {0}, receivables {1}, reputation {2}." },
                ["status.newWeek"] = new[] { "{0}，{1}。请查看新邮件和仍在有效期内的招聘。", "{0}, {1}. Check the new mail and any still-active recruitment." },
                ["status.weekSettling"] = new[] { "本周结算中…", "Settling the week…" },
                ["status.seasonEndLine"] = new[] { "赛季结束 · {0}", "Season over · {0}" },
                ["status.weekLine"] = new[] { "{0} · {1}", "{0} · {1}" },

                // Event log
                ["log.seasonStart"] = new[] { "{0}：季前训练开始。先阅读邮件，需求和简历才会进入分配工作台。", "{0}: Preseason begins. Read your mail first — demands and résumés only reach the assignment desk from there." },
                ["log.saveLoaded"] = new[] { "已载入第 {0} 周存档。", "Loaded the week {0} save." },
                ["log.instantIncome"] = new[] { "即时收益：私人请托 +{0}；未披露推荐风险已记录。", "Instant income: private favour +{0}; the undisclosed-recommendation risk has been noted." },
                ["log.submitted"] = new[] { "{0}：向“{1}”提交 {2}。球员已从可用名单移除。", "{0}: Submitted {2} to \"{1}\". The players have left the available list." },
                ["log.feedbackEta"] = new[] { "{0} 的反馈预计在 {1} 到达。", "Feedback on {0} is expected on {1}." },
                ["log.noSubmission"] = new[] { "{0}：本周未向“{1}”推荐球员，需求仍会保留到截止日期。", "{0}: No players recommended to \"{1}\" this week; the demand stays open until its deadline." },
                ["log.noDemand"] = new[] { "{0}：本周没有有效招聘需求，事务所继续跟进赛事和市场消息。", "{0}: No active recruitment this week; the agency keeps following matches and market gossip." },
                ["log.paymentReceived"] = new[] { "已到账：{0}（{1}）。", "Payment received: {0} ({1})." },
                ["log.carloConsequence"] = new[] { "延迟后果：雨城竞技追查卡洛的推荐依据，声望 -3。恩佐叔叔提供的午餐发票未被视为球探报告。", "Delayed fallout: Rainy City Athletic traced Carlo's recommendation back to you. Reputation -3. Uncle Enzo's lunch receipts were not accepted as a scouting report." },
                ["log.seasonComplete"] = new[] { "{0}：年度结算完成。所有董事会都已宣布下赛季会吸取教训。", "{0}: The annual accounts are closed. Every board has announced it will learn from its mistakes next season." },
                ["log.phaseChange"] = new[] { "{0}：赛季进入“{1}”阶段。", "{0}: The season enters the \"{1}\" phase." },
                ["log.outcomeAccepted"] = new[] { "反馈：{0} 打动了俱乐部并获得正式机会。", "Verdict: {0} won the club over and earned a formal offer." },
                ["log.outcomeExtended"] = new[] { "反馈：{0} 获得继续考察，茶水间仍然意见不一。", "Verdict: {0} gets an extended look; the tea room remains split." },
                ["log.outcomeRejected"] = new[] { "反馈：{0} 的试训提前结束，俱乐部礼貌地换了话题。", "Verdict: {0}'s trial ends early; the club politely changed the subject." },
                ["log.resultMailReceived"] = new[] { "收到 {0} 的正式回函，详情见收件箱。", "An official reply from {0} has arrived — see your inbox." },

                // Submission errors
                ["error.noAssignments"] = new[] { "至少需要向一个招聘槽位安排一名球员。", "Assign at least one player to a recruitment slot." },
                ["error.duplicatePlayer"] = new[] { "同一名球员本周只能安排一次。", "The same player can only be placed once per week." },
                ["error.playerCommitted"] = new[] { "这名球员已经提交给其他俱乐部，不能再次安排。", "This player has already been submitted to another club and can't be placed again." },
                ["error.generic"] = new[] { "本周提交未通过，请检查招聘槽位和已读档案。", "Submission rejected — check the slots and the files you've actually read." },

                // Demand panel
                ["demand.label"] = new[] { "当前有效的球队招聘", "Active club recruitment" },
                ["demand.yearSummary"] = new[] { "年度工作总结", "End-of-Year Report" },
                ["demand.yearSummaryBody"] = new[] { "现金：{0}\n应收：{1}\n声望：{2}", "Cash: {0}\nReceivables: {1}\nReputation: {2}" },
                ["demand.seasonOver"] = new[] { "本赛季已经结束。", "The season has ended." },
                ["demand.noDemandTitle"] = new[] { "当前没有有效招聘", "No active recruitment" },
                ["demand.noDemandBody"] = new[] { "可能原因：招聘邮件尚未阅读、需求已经提交，或截止日期已过。你仍可结束本周推进赛程。", "Possible reasons: the recruitment mail is unread, the demand was already submitted, or the deadline has passed. You can still end the week to move the calendar." },
                ["demand.noDemandSelection"] = new[] { "查看收件箱中的新招聘或过期通知。", "Check your inbox for new recruitment or expiry notices." },
                ["demand.bodyFormat"] = new[] { "有效期 {0} 周 · 截止 {1} · 委托价 {2}\n{3}", "Runs {0} week(s) · Deadline {1} · Fee {2}\n{3}" },
                ["demand.noPlayerSelected"] = new[] { "未选择球员。已填槽位可在未选中球员时点击清空。", "No player selected. Click a filled slot while nothing is selected to clear it." },
                ["demand.playerSelected"] = new[] { "已选择：{0}", "Selected: {0}" },
                ["demand.itemMeta"] = new[] { "截止 {0} · 委托价 {1} · {2} 槽", "Deadline {0} · Fee {1} · {2} slot(s)" },
                ["demand.titlePlaceholder"] = new[] { "需求尚未录入", "No demand loaded yet" },
                ["demand.bodyPlaceholder"] = new[] { "请先打开招聘邮件。", "Open a recruitment mail first." },
                ["demand.selectionPlaceholder"] = new[] { "当前没有可分配的招聘需求。", "No demand available for assignment." },
                ["demand.itemTitlePlaceholder"] = new[] { "俱乐部 · 需求标题", "Club · Demand title" },
                ["demand.itemMetaPlaceholder"] = new[] { "截止 日期 · 委托价 €0 · 0 槽", "Deadline date · Fee €0 · 0 slots" },

                // Slots
                ["slot.required"] = new[] { "必需", "Required" },
                ["slot.optional"] = new[] { "可选", "Optional" },
                ["slot.filled"] = new[] { "{0} · 未选球员时点击可移除", "{0} · click with nothing selected to remove" },
                ["slot.hint"] = new[] { "把球员拖到这里 / 选中球员后点击", "Drag a player here / click with a player selected" },
                ["slot.requirementPlaceholder"] = new[] { "位置 · 必需", "Position · Required" },

                // Player cards
                ["players.label"] = new[] { "当前可用 {0} 人 · 已读 {1} / {2} · 已安排或过期球员不会再次出现", "{0} available · {1} / {2} résumés read · placed or expired players never reappear" },
                ["players.labelPlaceholder"] = new[] { "当前可用 0 人 · 已读简历 0 / 51 · 按收到顺序排列", "0 available · 0 / 51 résumés read · in order of arrival" },
                ["playerCard.namePlaceholder"] = new[] { "可用球员", "Available Player" },
                ["playerCard.positionPlaceholder"] = new[] { "位置", "Position" },
                ["playerCard.claimPlaceholder"] = new[] { "公开自述、传闻或推荐。", "Public claims, rumours, or references." },

                // Information toolbar
                ["info.browserLabel"] = new[] { "信息筛选中心 · 先读邮件，再交叉判断", "Information hub · read the mail first, then cross-check" },
                ["info.mailFilter"] = new[] { "邮件", "Mail" },
                ["info.subscriptionFilter"] = new[] { "订阅期刊", "Magazines" },
                ["info.counterPlaceholder"] = new[] { "第 1 周已到达邮件", "Week 1 mail arrivals" },

                // Mail browser
                ["mail.listLabel"] = new[] { "收件箱 · 打开邮件才算阅读", "Inbox · a mail only counts once opened" },
                ["mail.readingLabel"] = new[] { "邮件阅读区", "Reading pane" },
                ["mail.read"] = new[] { "已读", "Read" },
                ["mail.unread"] = new[] { "未读", "Unread" },
                ["mail.counter"] = new[] { "{0} · 已到达 {1} 封 · 已读 {2}", "{0} · {1} arrived · {2} read" },
                ["mail.senderPrefix"] = new[] { "发件人：", "From: " },
                ["mail.emptySender"] = new[] { "收件箱", "Inbox" },
                ["mail.emptySubject"] = new[] { "请选择并打开一封邮件", "Select a mail to open it" },
                ["mail.emptyMeta"] = new[] { "只有实际阅读后，关联内容才会进入分配工作台", "Linked content only reaches the assignment desk once you've actually read the mail" },
                ["mail.emptyBody"] = new[] { "招聘邮件会解锁需求；简历或私人请托邮件会解锁对应球员。期刊报道只作为判断证据。", "Recruitment mail unlocks demands; résumé and private-favour mail unlocks players. Magazine stories are evidence, nothing more." },
                ["mail.noBody"] = new[] { "（邮件没有正文）", "(This mail has no body.)" },
                ["mail.ellipsis"] = new[] { "……", "…" },
                ["mail.avatarFallback"] = new[] { "邮", "M" },
                ["mail.itemSenderPlaceholder"] = new[] { "发件人", "Sender" },
                ["mail.itemTimestampPlaceholder"] = new[] { "第1周 周一 08:20", "Week 1 Mon 08:20" },
                ["mail.itemSubjectPlaceholder"] = new[] { "邮件主题", "Mail subject" },
                ["mail.itemPreviewPlaceholder"] = new[] { "邮件正文开头会在这里显示……", "The start of the mail body appears here…" },
                ["mail.avatarPlaceholder"] = new[] { "俱", "C" },

                // Mail demand block
                ["mail.demandBlock.title"] = new[] { "固定信息 · 招聘需求", "Attached · Recruitment Demand" },
                ["mail.demandBlock.slotsPrefix"] = new[] { "所需位置：", "Positions wanted: " },
                ["mail.demandBlock.requiredSuffix"] = new[] { "（必需）", " (required)" },
                ["mail.demandBlock.optionalSuffix"] = new[] { "（可选）", " (optional)" },
                ["mail.demandBlock.deadline"] = new[] { "有效期 {0} 周 · 截止 {1} · 剩余 {2} 周", "Runs {0} week(s) · Deadline {1} · {2} week(s) left" },
                ["mail.demandBlock.price"] = new[] { "委托价：{0}", "Fee: {0}" },
                ["mail.demandBlock.payment"] = new[] { "付款：", "Payment: " },
                ["mail.demandBlock.clubProfilePlaceholder"] = new[] { "俱乐部实力与历史成绩", "Club stature and honours" },
                ["mail.demandBlock.slotsPlaceholder"] = new[] { "所需位置：", "Positions wanted: " },
                ["mail.demandBlock.deadlinePlaceholder"] = new[] { "截止：", "Deadline: " },
                ["mail.demandBlock.pricePlaceholder"] = new[] { "委托价：", "Fee: " },
                ["mail.demandBlock.paymentPlaceholder"] = new[] { "付款：", "Payment: " },

                // Mail resume block
                ["mail.resumeBlock.title"] = new[] { "固定信息 · 球员简历", "Attached · Player Résumé" },
                ["mail.resumeBlock.position"] = new[] { "公开位置：", "Public position: " },
                ["mail.resumeBlock.salary"] = new[] { "薪资期望：€{0}–€{1} / 周", "Wage demand: €{0}–€{1} / week" },
                ["mail.resumeBlock.career"] = new[] { "经历：", "Career: " },
                ["mail.resumeBlock.claim"] = new[] { "自述：", "Claim: " },
                ["mail.resumeBlock.evidence"] = new[] { "旁证：", "Evidence: " },
                ["mail.resumeBlock.source"] = new[] { "可信度：", "Reliability: " },
                ["mail.resumeBlock.availability"] = new[] { "可安排至 {0} · 剩余 {1} 周", "Available until {0} · {1} week(s) left" },
                ["mail.resumeBlock.playerPlaceholder"] = new[] { "球员姓名", "Player Name" },
                ["mail.resumeBlock.positionPlaceholder"] = new[] { "公开位置", "Public position" },
                ["mail.resumeBlock.salaryPlaceholder"] = new[] { "薪资期望", "Wage demand" },
                ["mail.resumeBlock.biographyPlaceholder"] = new[] { "公开简介", "Public biography" },
                ["mail.resumeBlock.careerPlaceholder"] = new[] { "经历", "Career" },
                ["mail.resumeBlock.claimPlaceholder"] = new[] { "自述", "Claim" },
                ["mail.resumeBlock.evidencePlaceholder"] = new[] { "旁证", "Evidence" },
                ["mail.resumeBlock.sourcePlaceholder"] = new[] { "可信度", "Reliability" },
                ["mail.resumeBlock.availabilityPlaceholder"] = new[] { "可安排至", "Available until" },

                // Mail private offer block
                ["mail.offerBlock.title"] = new[] { "固定信息 · 私人请托", "Attached · Private Favour" },
                ["mail.offerBlock.offer"] = new[] { "即时酬谢：", "Instant reward: " },
                ["mail.offerBlock.terms"] = new[] { "要求：", "Terms: " },
                ["mail.offerBlock.risk"] = new[] { "延迟风险：", "Delayed risk: " },
                ["mail.offerBlock.offerPlaceholder"] = new[] { "即时酬谢：€0", "Instant reward: €0" },
                ["mail.offerBlock.termsPlaceholder"] = new[] { "要求：", "Terms: " },
                ["mail.offerBlock.targetClubPlaceholder"] = new[] { "目标俱乐部：", "Target club: " },
                ["mail.offerBlock.riskPlaceholder"] = new[] { "延迟风险：", "Delayed risk: " },

                // Magazine browser
                ["magazine.railTitle"] = new[] { "我的电子期刊", "My E-Magazines" },
                ["magazine.railTip"] = new[] { "期刊可翻页阅读，用来核对邮件里的说法。", "Flip through issues to verify what the mail claims." },
                ["magazine.prevPage"] = new[] { "上一页", "Previous" },
                ["magazine.nextPage"] = new[] { "下一页", "Next" },
                ["magazine.counter"] = new[] { "已订阅 {0} 期 · 期刊不会直接解锁球员", "{0} issue(s) subscribed · magazines never unlock players directly" },
                ["magazine.empty"] = new[] { "暂无可读期刊", "No issues to read yet" },
                ["magazine.pageIndicator"] = new[] { "{0} · {1}    第 {2} / {3} 页", "{0} · {1}    Page {2} / {3}" },
                ["magazine.pageIndicatorPlaceholder"] = new[] { "第 1 / 4 页", "Page 1 / 4" },
                ["magazine.issueNumber"] = new[] { "第 {0} 期", "Issue {0}" },
                ["magazine.captionPrefix"] = new[] { "插图 · ", "Illustration · " },
                ["magazine.captionPlaceholder"] = new[] { "插图 · 主题", "Illustration · Topic" },
                ["magazine.itemPublicationPlaceholder"] = new[] { "期刊名称", "Publication" },
                ["magazine.itemIssuePlaceholder"] = new[] { "第 01 期 · 主题", "Issue 01 · Topic" },
                ["magazine.coverPublicationPlaceholder"] = new[] { "边线周刊", "Touchline Weekly" },
                ["magazine.coverIssuePlaceholder"] = new[] { "第 01 期", "Issue 01" },
                ["magazine.coverHeadlinePlaceholder"] = new[] { "本期封面故事", "This Week's Cover Story" },
                ["magazine.coverDeckPlaceholder"] = new[] { "本期封面故事与导读。", "The cover story and this week's guide." },
                ["magazine.coverNote"] = new[] { "独立报道 · 球探观察 · 足球文化", "Independent reporting · Scout notes · Football culture" },
                ["magazine.featureKicker"] = new[] { "专题", "Feature" },
                ["magazine.featureHeadline"] = new[] { "专题标题", "Feature Headline" },
                ["magazine.featureDeck"] = new[] { "专题导语", "Feature deck" },
                ["magazine.featureBodyLeft"] = new[] { "左栏正文", "Left column" },
                ["magazine.featureBodyRight"] = new[] { "右栏正文", "Right column" },
                ["magazine.featurePullQuote"] = new[] { "“重点引语”", "\"Pull quote\"" },
                ["magazine.featureSidebarTitle"] = new[] { "边栏", "Sidebar" },
                ["magazine.featureSidebarBody"] = new[] { "补充资料", "Extra notes" },
                ["magazine.scoutKicker"] = new[] { "球探报告", "Scout Report" },
                ["magazine.scoutHeadline"] = new[] { "报告标题", "Report Headline" },
                ["magazine.scoutDeck"] = new[] { "观察摘要", "Observation summary" },
                ["magazine.scoutBodyLeft"] = new[] { "优势观察", "Strengths" },
                ["magazine.scoutBodyRight"] = new[] { "风险观察", "Risks" },
                ["magazine.scoutPullQuote"] = new[] { "编辑判断", "Editor's verdict" },
                ["magazine.scoutSidebarTitle"] = new[] { "来源", "Sources" },
                ["magazine.scoutSidebarBody"] = new[] { "资料来源与偏差说明", "Sources and bias notes" },

                // Footer
                ["footer.eventLogPlaceholder"] = new[] { "每周结果、延迟反馈与到账记录会显示在这里。", "Weekly results, delayed feedback, and payment records appear here." },
                ["footer.statusPlaceholder"] = new[] { "请先阅读邮件，再安排本周工作。", "Read the mail first, then plan the week." },
                ["footer.endWeek"] = new[] { "结束本周", "End Week" },

                // Generic
                ["button.defaultLabel"] = new[] { "球员姓名", "Player Name" },
                ["list.and"] = new[] { "、", ", " },

                // Positions
                ["position.goalkeeper"] = new[] { "门将", "Goalkeeper" },
                ["position.defender"] = new[] { "中卫", "Centre-Back" },
                ["position.wingBack"] = new[] { "翼卫", "Wing-Back" },
                ["position.midfielder"] = new[] { "中场", "Midfielder" },
                ["position.winger"] = new[] { "边锋", "Winger" },
                ["position.forward"] = new[] { "前锋", "Forward" },

                // Mail kinds
                ["mailKind.clubRequest"] = new[] { "球队招聘", "Club Recruitment" },
                ["mailKind.playerResume"] = new[] { "球员简历", "Player Résumé" },
                ["mailKind.privateRequest"] = new[] { "私人请托", "Private Favour" },
                ["mailKind.clubFeedback"] = new[] { "俱乐部回函", "Club Reply" },
                ["mailKind.general"] = new[] { "普通邮件", "General Mail" },

                // Evidence reliability
                ["reliability.high"] = new[] { "较高", "High" },
                ["reliability.medium"] = new[] { "中等", "Medium" },
                ["reliability.low"] = new[] { "较低", "Low" },
                ["reliability.unverified"] = new[] { "未经核实", "Unverified" },

                // Season phases
                ["phase.preseason"] = new[] { "季前训练", "Preseason" },
                ["phase.summerWindow"] = new[] { "夏季转会窗口", "Summer Window" },
                ["phase.leagueOpening"] = new[] { "联赛开幕", "League Kickoff" },
                ["phase.groupStage"] = new[] { "洲际小组赛", "Continental Group Stage" },
                ["phase.winterSchedule"] = new[] { "冬季密集赛程", "Winter Fixture Rush" },
                ["phase.winterWindow"] = new[] { "冬季转会窗口", "Winter Window" },
                ["phase.knockoutStage"] = new[] { "洲际淘汰赛", "Continental Knockouts" },
                ["phase.runIn"] = new[] { "争冠与保级冲刺", "Title & Relegation Run-in" },
                ["phase.finals"] = new[] { "决赛阶段", "Finals" },
                ["phase.summary"] = new[] { "赛季总结", "Season Review" },
            };

        private static readonly string[] IllustrationCaptionsChinese =
        {
            "转会窗", "合同与佣金", "伤病室", "战术板", "夜场", "酒馆",
            "训练场", "球探席", "编辑部", "更衣室", "装备静物", "哨与牌",
            "奖杯", "雨战", "金元足球", "街头青训", "老将更衣柜", "门将手套",
            "截止日传真", "看台", "冬窗", "体检室", "经纪人来电", "数据板"
        };

        private static readonly string[] IllustrationCaptionsEnglish =
        {
            "Transfer Window", "Contracts & Commissions", "Treatment Room", "Tactics Board", "Night Match", "The Pub",
            "Training Ground", "Scout's Seat", "Newsroom", "Dressing Room", "Kit Still Life", "Whistles & Cards",
            "Silverware", "Rain Game", "Money Football", "Street Academy", "Veteran's Locker", "Keeper's Gloves",
            "Deadline-Day Fax", "The Stands", "Winter Window", "Medical Room", "Agent Calling", "Data Board"
        };

        public static string Get(string key, GameLanguage language)
        {
            if (!Entries.TryGetValue(key, out var pair))
            {
                Debug.LogWarning($"[NewPlayerHunter] Missing UI string '{key}'.");
                return "[" + key + "]";
            }

            return language == GameLanguage.English ? pair[English] : pair[Chinese];
        }

        public static string Format(
            string key,
            GameLanguage language,
            params object[] args)
        {
            return string.Format(Get(key, language), args);
        }

        public static string IllustrationCaption(int index, GameLanguage language)
        {
            var captions = language == GameLanguage.English
                ? IllustrationCaptionsEnglish
                : IllustrationCaptionsChinese;
            return captions[Mathf.Clamp(index, 0, captions.Length - 1)];
        }
    }
}
