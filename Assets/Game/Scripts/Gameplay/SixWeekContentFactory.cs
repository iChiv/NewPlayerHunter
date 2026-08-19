using System.Collections.Generic;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay
{
    internal static class SixWeekContentFactory
    {
        public static List<PlayerContentEntry> BuildPlayers()
        {
            return new List<PlayerContentEntry>
            {
                Player("player.rocket.rory", 0, 1, 3, "罗里·奎克", PlayerPosition.Forward, 88, 86, 76, 900, 1400,
                    "2023–2025 河谷联青训；2025–2026 连续租借至两支名字相近的俱乐部，进球归属仍在核对。",
                    "擅长反越位的冲刺型前锋。经纪人坚持只在下坡路段测试他的速度。",
                    "经纪公司宣称他上赛季攻入 41 球。",
                    "能找到的两场完整录像都证明跑位聪明，但没人找到那 41 个进球所在的联赛积分榜。",
                    EvidenceReliability.Medium,
                    "罗里不等了：已与山区速降队签约",
                    "罗里的经纪人表示，足球俱乐部迟迟没有报价，他决定接受一支山区球队的邀请。对方唯一承诺是主场确实有坡。"),
                Player("player.metronome.milo", 1, 2, 6, "米洛·陈", PlayerPosition.Midfielder, 81, 82, 91, 800, 1100,
                    "2021–2026 港口竞技中场，连续五个赛季出场率全队前三，采访字数全队倒数第一。",
                    "位置感出色的组织型中场，踢球安静，采访更安静。",
                    "前教练称他为“不会上热搜的发动机”。",
                    "三场完整比赛里，他始终能为持球队友提供接应，并让阵型保持在同一张战术板上。",
                    EvidenceReliability.High,
                    "米洛接受了数据分析师岗位",
                    "米洛等了六周仍没有正式安排，于是加入俱乐部分析部门。他说至少电子表格会按时回传球。"),
                Player("player.wall.walter", 2, 3, 4, "沃尔特·科瓦奇", PlayerPosition.Defender, 85, 73, 84, 750, 1000,
                    "2019–2026 北方铸造队中后卫，三次入选周最佳阵容，一次入选月度最佳撞墙。",
                    "强壮的中后卫，转身速度一般，但正面对抗很少吃亏。",
                    "他的表哥声称，同一个人从来不可能第二次过掉他。",
                    "录像证实第一下对抗很硬；偶尔也会因为追球太执着，跑出整条防线的邮政编码。",
                    EvidenceReliability.Medium,
                    "沃尔特回老家接管家具店",
                    "沃尔特没有等到合适合同，决定回家经营衣柜生意。经纪人强调，这与球探报告中频繁出现“像衣柜一样结实”毫无关系。"),
                Player("player.turbo.tess", 3, 2, 5, "泰丝·奥科罗", PlayerPosition.Winger, 77, 94, 67, 650, 950,
                    "2022–2026 海岸青年队边锋，保持队史最快 60 米纪录，传中落点纪录不予公开。",
                    "爆发力突出的边锋，启动很快，最后一传还在学习减速。",
                    "青训中心称她的第一步“任何边后卫都拦不住”。",
                    "速度确实突出；传中质量不稳定，有时球会比禁区里的队友早到下一块广告牌。",
                    EvidenceReliability.High,
                    "泰丝转投室内五人制联赛",
                    "泰丝接受了五人制球队的合同。她说场地更小，至少传中飞出边线后不会消失太久。"),
                Player("player.gloves.gus", 4, 4, 3, "格斯·贝克", PlayerPosition.Goalkeeper, 79, 78, 88, 600, 850,
                    "2020–2026 市政体育场梯队门将，扑点训练营常驻示范，公交车月票连续六年全勤。",
                    "处理高球稳健的门将，训练态度认真，偶尔会对公交车时刻表失误。",
                    "他声称曾在一个下午扑出五粒点球。",
                    "官方记录是四次扑救；第五名罚球手在助跑前请了病假。",
                    EvidenceReliability.High,
                    "格斯加入点球训练营担任教练",
                    "格斯没有收到职业合同，转而教授青少年门将扑点球。招生海报仍写着“五连扑”，第五名学员目前也请了病假。"),
                Player("player.showboat.sonny", 5, 5, 2, "桑尼·布莱兹", PlayerPosition.Forward, 63, 69, 34, 500, 1200,
                    "2023–2026 多支短约球队前锋，集锦总时长 47 分钟，完整出场总时长略少于集锦。",
                    "脚下花样很多的前锋，个人集锦比完整比赛录像长。",
                    "他的团队把每次穿裆都称为一次战术革命。",
                    "地方媒体认可他的灵感，同时记录了多次迟到，以及一次错过球队大巴后责怪导航软件。",
                    EvidenceReliability.Medium,
                    "桑尼签约短视频平台",
                    "桑尼表示传统足球无法完整容纳他的创意，已转型为全职技巧主播。首期节目是《如何在不回防的情况下保持镜头感》。"),
                Player("player.utility.uma", 6, 3, 7, "乌玛·费尔南德斯", PlayerPosition.WingBack, 75, 87, 86, 550, 750,
                    "2021–2026 两支乙级球队翼卫，左右两侧出场次数几乎完全相等，统计员因此获奖。",
                    "能踢左右两侧的翼卫，跑动积极，对临时改阵适应很快。",
                    "两家旧俱乐部都推荐她的适应能力。",
                    "独立比赛记录显示，她在两侧都能稳定回追、套边和接应，推荐内容可信度较高。",
                    EvidenceReliability.High,
                    "乌玛加入救火教练组",
                    "长期没有球队给出球员合同后，乌玛接受了青年队助教职位。她现在负责示范所有位置，除了财务主管。"),
                Player("player.cousin.carlo", 7, 1, 2, "卡洛·贝利尼", PlayerPosition.Midfielder, 42, 58, 51, 200, 350,
                    "2024–2026 雨城竞技青年队中场，训练出席率良好（恩佐叔叔代为签到）。",
                    "比赛样本极少的中场，简历由亲属转交，并附带一顿规格过高的午餐。",
                    "家人坚称他能提前三步读懂比赛。",
                    "近期唯一可查证的材料是一张替补席旁的照片，球衣号码被三明治挡住。",
                    EvidenceReliability.Low,
                    "卡洛改行经营高级三明治店",
                    "恩佐叔叔来信说卡洛决定尊重自己的真正天赋：把任何会议变成午餐。新店开业，包厢仍叫“中场控制区”。"),
                Player("player.zodiac.luna", 8, 3, 2, "露娜·萨维奇", PlayerPosition.Winger, 74, 81, 59, 450, 700,
                    "2022–2026 星光女足边锋（租借两次），满月夜比赛 11 场不败，其余夜晚战绩另询。",
                    "习惯内切的左边锋，做定位球决定前会查看星象应用。",
                    "经纪人说她在满月夜从不丢失球权。",
                    "白天比赛同样能看到不错的内切能力；定位球选择则与天气、月相和手机电量同时相关。",
                    EvidenceReliability.Medium,
                    "露娜暂停足球，等待水星顺行",
                    "露娜认为本月合同谈判窗口与行星运行方向冲突，决定暂时休息。经纪人拒绝给出回归日期，只发来一张星盘。"),
                Player("player.forms.felix", 9, 4, 5, "费利克斯·文书", PlayerPosition.Goalkeeper, 68, 72, 94, 450, 600,
                    "2018–2026 注册处体育场守门员，八年零漏表，扑救流程合规率 100%。",
                    "基本功可靠的门将，做任何出击前都希望流程完整。",
                    "他声称职业生涯从未漏交过一张出击申请表。",
                    "扑救动作稳定，职业态度很好；快速反击偶尔会被他的三联单拖慢。",
                    EvidenceReliability.High,
                    "费利克斯通过公务员考试",
                    "费利克斯接受了足协注册处职位。他说这里每一次盖章都算成功出击，而且不会有人突然起高球。"),
                Player("player.rivet.rita", 10, 5, 6, "丽塔·哈达德", PlayerPosition.Defender, 82, 78, 92, 800, 1100,
                    "2019–2026 铁桥队队长级中卫，七次全队赛季最佳，消防演练特邀指导。",
                    "位置感稳定的中后卫，处理危险球简单直接。",
                    "旧队长说她能让禁区通过安全检查。",
                    "多场录像显示她很少为了镜头贸然上抢，解围路线也比很多俱乐部的消防通道清楚。",
                    EvidenceReliability.High,
                    "丽塔接受海外联赛报价",
                    "丽塔与另一家俱乐部完成签约。对方体育主管只问了两个问题：能不能防守，以及能不能让其他人也认真防守。"),
                Player("player.throwin.dino", 11, 4, 2, "迪诺·马尔凯蒂", PlayerPosition.Defender, 70, 83, 48, 400, 600,
                    "2021–2026 河岸队边后卫，界外球均距联盟第一，停车场寻球次数也是第一。",
                    "拥有超远距离界外球的边后卫，落点控制比较随缘。",
                    "他把每次界外球都称为一次重新发明定位球。",
                    "投掷距离确实惊人；两次训练因为皮球飞出场馆，需要工作人员去停车场寻找。",
                    EvidenceReliability.Medium,
                    "迪诺加入标枪俱乐部",
                    "迪诺认为足球对手臂天赋缺乏尊重，已加入当地田径队。第一堂课的重点是学习不要把标枪扔向角旗区。"),
                Player("player.spreadsheet.nova", 12, 5, 8, "诺瓦·伊万诺娃", PlayerPosition.Midfielder, 84, 76, 89, 750, 1050,
                    "2020–2026 大学联赛中场后转入职业队，传球成功率 87%，早餐计划执行率未知。",
                    "擅长在高压下向前传球的中场，也热衷赛前数据准备。",
                    "她说自己能用三列公式找出传球线路。",
                    "完整比赛证明她不仅会做表格，也敢在逼抢下处理球；队友只是不喜欢收到带筛选器的早餐计划。",
                    EvidenceReliability.High,
                    "诺瓦加入冠军联赛数据部门",
                    "诺瓦接受了一家洲际赛事数据供应商的职位。她留下最后一句话：至少数据库不会在第八十分钟突然改踢四前锋。"),
                Player("player.bothfeet.echo", 13, 6, 8, "艾可·摩根", PlayerPosition.WingBack, 80, 90, 77, 700, 950,
                    "2021–2026 东西两翼均有完整赛季，左右脚进球差为 0，鞋柜分配仍未裁决。",
                    "左右脚均衡、能覆盖两条边路的翼卫。",
                    "经纪材料称她是“两名球员共用一双球鞋”。",
                    "左右两侧样本都很稳定，唯一争论是更衣室究竟该给她左边还是右边的柜子。",
                    EvidenceReliability.High,
                    "艾可加盟竞争对手",
                    "艾可没有继续等待，已与另一家俱乐部签约。发布会上她分别用左右脚颠球，公关部因此发了两份新闻稿。"),
                Player("player.thermos.vic", 14, 2, 3, "维克·哈珀", PlayerPosition.Forward, 78, 60, 95, 300, 500,
                    "2008–2026 六家俱乐部禁区前锋，六码区进球 91 个，保温杯续约四次。",
                    "经验丰富的禁区前锋，冲刺不多，但对落点判断很好，随身带保温杯。",
                    "他坚持速度会消失，但六码区不会搬家。",
                    "近年冲刺次数下降明显；六码区触球选择和职业态度仍然可靠。",
                    EvidenceReliability.High,
                    "维克宣布退役",
                    "维克决定结束球员生涯。他说膝盖已经投票通过，保温杯以全票当选下一任经纪人。"),
                Player("player.academy.perry", 15, 1, 5, "佩里·金", PlayerPosition.Forward, 67, 91, 43, 250, 450,
                    "2023–2026 青训营前锋，青年队金靴一次，成年队出场 0 分钟，纪录片两季。",
                    "速度出色的青年前锋，成年比赛为零，个人纪录片已经拍到第二季。",
                    "团队称他为“无法被定义的九号半”。",
                    "青年比赛能看到速度和射门潜力；成年对抗、决策和准时性仍未得到验证。",
                    EvidenceReliability.Medium,
                    "佩里进入真人秀封闭拍摄",
                    "佩里的团队暂停所有试训，因为纪录片平台要求剧情保密。经纪人承诺下一季会出现“至少一次真正的成年比赛”。")
            };
        }

        public static List<DemandContentEntry> BuildDemands()
        {
            return new List<DemandContentEntry>
            {
                Demand("demand.week1.emergency", "club.rainy", "雨城竞技",
                    "上赛季联赛第 4 名，目前参加洲际冠军联赛资格赛",
                    "历史最佳：洲际冠军联赛四强（12 年前）",
                    "预算充足、舆论压力巨大。董事会把“重返欧洲之巅”印在每一个咖啡杯上。",
                    "资格赛前锋告急",
                    "主力前锋在热身赛拉伤，俱乐部需要一名能立即参加合练、理解反越位并愿意积极逼抢的前锋。",
                    1, 2, 1200,
                    "提交后 1–2 周反馈；通过试训后付款。",
                    "雨城竞技关闭紧急前锋招聘",
                    "雨城竞技已从预备队提拔一名前锋。体育主管感谢所有推荐，并表示下次会在主力受伤前阅读体检报告。",
                    Slot("slot.week1.forward", PlayerPosition.Forward, 74, 68, 60)),
                Demand("demand.week2.group", "club.dockyard", "船坞联",
                    "上赛季联赛第 11 名，本季目标进入上半区",
                    "历史最佳：联赛亚军；最近一次发生在俱乐部仍用传真机的年代",
                    "中游俱乐部，青训和定位球出名，财务主管会认真核对每一杯训练饮料。",
                    "前场与中场联合试训",
                    "球队需要一名前锋和一名能维持攻守距离的中场。允许部分推荐，但完整方案报酬更高。",
                    2, 3, 2100,
                    "按实际完成的槽位结算；结果延迟 1–2 周。",
                    "船坞联结束联合试训",
                    "船坞联从青训队补进两人，招聘提前结束。主教练称阵容已经足够完整，财务主管称餐费也终于完整。",
                    Slot("slot.week2.forward", PlayerPosition.Forward, 70, 62, 58),
                    Slot("slot.week2.midfield", PlayerPosition.Midfielder, 72, 68, 70)),
                Demand("demand.week3.longterm", "club.oldcastle", "旧堡王冠",
                    "上赛季联赛第 2 名，连续六年参加洲际冠军联赛",
                    "历史最佳：3 次联赛冠军、1 次洲际冠军联赛决赛",
                    "传统豪门，球迷要求争冠，董事会每次换教练都称其为“长期规划的最后一块拼图”。",
                    "争冠阵容补强计划",
                    "俱乐部需要一名能守大空间的中后卫，以及一名能在欧战低位防守前制造一对一优势的边锋。",
                    3, 3, 2800,
                    "可部分完成；两个位置全部满足时支付完整顾问费。",
                    "旧堡王冠停止本轮补强",
                    "董事会决定把预算留给冬季窗口，并发布了第九版长期规划。第八版仍然有效，只是不再被任何人提起。",
                    Slot("slot.week3.defender", PlayerPosition.Defender, 78, 70, 76),
                    Slot("slot.week3.winger", PlayerPosition.Winger, 76, 80, 65)),
                Demand("demand.week4.keeper", "club.seaside", "海港蓝鸥",
                    "上赛季联赛第 15 名，通过附加赛保级",
                    "历史最佳：国内杯赛四强",
                    "预算有限但主场风很大。俱乐部把所有传中失误统称为“海洋气候变量”。",
                    "保级队门将短期招聘",
                    "一号门将肩部受伤，需要能处理高球、指挥防线并承受密集射门的门将。",
                    4, 2, 1300,
                    "通过体检和试训后结算，反馈延迟 1–2 周。",
                    "海港蓝鸥撤回门将需求",
                    "一号门将恢复速度快于预期，俱乐部撤回招聘。队医表示这次没有参考潮汐表。",
                    Slot("slot.week4.keeper", PlayerPosition.Goalkeeper, 72, 68, 74)),
                Demand("demand.week5.group", "club.foundry", "铁炉竞技",
                    "上赛季联赛第 7 名，正在争取洲际次级赛事资格",
                    "历史最佳：2 次国内杯赛冠军",
                    "强调纪律、对抗和二点球。主教练办公室只有两本书：《防守》与《继续防守》。",
                    "杯赛阵容双人补充",
                    "需要一名可靠中后卫和一名愿意参与前场压迫的前锋。个人摄制组必须留在训练基地外。",
                    5, 3, 2400,
                    "允许部分提交；职业态度会明显影响最终报酬。",
                    "铁炉竞技结束招聘",
                    "主教练从青年队提拔两名球员，并宣布他们已经学会最重要的战术：听见哨声就回防。",
                    Slot("slot.week5.defender", PlayerPosition.Defender, 76, 68, 78),
                    Slot("slot.week5.forward", PlayerPosition.Forward, 75, 72, 72)),
                Demand("demand.week6.future", "club.comets", "彗星城",
                    "卫冕联赛冠军，本季洲际冠军联赛夺冠热门",
                    "历史最佳：5 次联赛冠军、洲际冠军联赛四强",
                    "数据部门规模与一支青年队相当。每次训练结束后，球员先恢复体能，再恢复账号密码。",
                    "冠军阵容未来计划",
                    "寻找一名能在高压下组织进攻的中场，以及一名覆盖整条边路的翼卫，为联赛和洲际赛事双线轮换。",
                    6, 4, 3200,
                    "两个位置全部满足时支付完整顾问费；反馈延迟 1–2 周。",
                    "彗星城完成内部补强",
                    "俱乐部决定启用两名青训球员。数据部门表示模型早已预测到这一结果，只是报告直到今天才完成渲染。",
                    Slot("slot.week6.midfield", PlayerPosition.Midfielder, 80, 72, 78),
                    Slot("slot.week6.wingback", PlayerPosition.WingBack, 76, 82, 74))
            };
        }

        public static List<MailContentEntry> BuildMails(
            IReadOnlyList<PlayerContentEntry> players,
            IReadOnlyList<DemandContentEntry> demands)
        {
            var mails = new List<MailContentEntry>
            {
                Mail("mail.w1.rainy", MailContentKind.ClubRequest, 1, "雨城竞技 · 体育主管", "资格赛前锋告急：两周内完成推荐",
                    "洲际冠军联赛资格赛临近，我们需要能立刻参加合练的前锋。",
                    "主力前锋在热身赛拉伤。球队希望候选人具备反越位意识、基本逼抢能力，并能承受资格赛主场四万名球迷同时叹气。俱乐部实力、截止日期和委托价见下方固定信息。",
                    "俱乐部正式招聘 · 第一手信息", demandId: "demand.week1.emergency"),
                Mail("mail.w1.rory", MailContentKind.PlayerResume, 1, "极速经纪公司 · 巴里", "球员简历：罗里·奎克",
                    "冲刺能力突出；所谓 41 球仍缺少可核实的赛事记录。",
                    "罗里目前没有合同，可以立即参加试训。我们确认他的无球跑动和启动速度都很优秀。至于“上赛季 41 球”，统计人员正在寻找正确的联赛网页。",
                    "经纪人材料 · 宣传成分较高", playerId: "player.rocket.rory"),
                Mail("mail.w1.perry", MailContentKind.PlayerResume, 1, "王冠体育影业", "球员简历：佩里·金（成年比赛待补）",
                    "青年队明星，速度和曝光度都很高，成年比赛样本为零。",
                    "佩里希望进入一线队环境。他的纪录片已经拍到第二季，但成年比赛尚未开始。附件中有青年赛事集锦、品牌合作方案，以及一份长达十二页的进球庆祝计划。",
                    "球员团队材料 · 品牌倾向明显", playerId: "player.academy.perry"),
                Mail("mail.w1.carlo", MailContentKind.PrivateRequest, 1, "恩佐叔叔", "帮卡洛进雨城竞技，午饭和谢礼都算我的",
                    "请托指定上赛季前四、参加洲际资格赛的雨城竞技。",
                    "卡洛只是缺一次被豪门看见的机会。把他送进雨城竞技这次前锋招聘，位置不完全匹配也没关系。你会立即收到谢礼；球队以后若追问推荐依据，就说我们都相信他的潜力。",
                    "私人关系 · 明确利益冲突 · 证据不足",
                    playerId: "player.cousin.carlo", offer: 350,
                    offerTerms: "必须在本周把卡洛提交给雨城竞技；投给其他俱乐部不支付谢礼。",
                    requiredClubId: "club.rainy",
                    targetClub: "目标俱乐部：雨城竞技（上赛季第 4，参加洲际冠军联赛资格赛）",
                    risk: "不匹配安排可能在后续调查中损害声望。"),
                Mail("mail.w1.office", MailContentKind.General, 1, "球探事务所 · 系统通知", "新赛季工作说明：先看来源，再看报价",
                    "游戏从季前训练开始，共 52 周；顶部日期会自动推进。",
                    "阅读招聘邮件后，对应需求才会进入分配页；阅读简历或私人请托后，球员才会进入可用名单。球员一旦提交给俱乐部就会离开可用名单，结果和款项通常延迟一至两周。",
                    "系统规则 · 可直接信赖"),

                Mail("mail.w2.dockyard", MailContentKind.ClubRequest, 2, "船坞联 · 招聘办公室", "联合试训：前锋与中场各一名",
                    "中游球队准备调整进攻结构，需求有效三周。",
                    "我们需要一名能终结进攻的前锋，以及一名能让三条线保持联系的中场。可以只推荐其中一人，但如果两人互相不认识却能站在正确距离，主教练会非常感动。",
                    "俱乐部正式招聘 · 第一手信息", demandId: "demand.week2.group"),
                Mail("mail.w2.milo", MailContentKind.PlayerResume, 2, "妮娅教练", "球员简历：米洛·陈",
                    "不抢镜的组织中场，能让身边队友踢得更舒服。",
                    "米洛擅长观察、接应和转移球。他最好的处理通常发生在进球前两脚，因此个人集锦很短，但完整比赛录像很好看。",
                    "前教练推荐 · 可由完整比赛核实", playerId: "player.metronome.milo"),
                Mail("mail.w2.vic", MailContentKind.PlayerResume, 2, "老靴经纪社", "球员简历：维克·哈珀",
                    "经验丰富的禁区前锋，速度下降，但落点判断仍在。",
                    "维克不承诺赢下四十米冲刺，但知道大多数解围会落到哪里。他自带保温杯，也承诺不会占用俱乐部营养师的咖啡机。",
                    "多年比赛记录 · 部分数据时效较旧", playerId: "player.thermos.vic"),
                Mail("mail.w2.tess", MailContentKind.PlayerResume, 2, "闪电青训中心", "球员简历：泰丝·奥科罗",
                    "边锋，速度优势明显，最后一传仍需打磨。",
                    "泰丝的第一步足以迫使边后卫提前转身。她已经准备好参加成年队试训；我们也建议训练基地把边线外的广告牌固定得更牢。",
                    "青训报告 · 基本能力可核实", playerId: "player.turbo.tess"),
                Mail("mail.w2.window", MailContentKind.General, 2, "联盟竞赛部", "季前注册提醒：名单不是愿望清单",
                    "夏季注册窗口即将进入繁忙阶段。",
                    "各俱乐部必须在截止日前提交球员注册资料。联盟特别提醒：球员姓名、出生日期和实际存在的合同缺一不可；“董事长口头保证”仍不是文件类型。",
                    "联盟公告 · 赛季时间信息"),

                Mail("mail.w3.oldcastle", MailContentKind.ClubRequest, 3, "旧堡王冠 · 技术委员会", "争冠补强：中后卫与边锋",
                    "传统豪门为联赛和洲际赛事寻找轮换球员，需求有效三周。",
                    "中后卫需要适应高位防线，边锋需要在阵地战中制造一对一优势。董事会保证这是长期规划，附件文件名也只写到了“最终版_第八次修改”。",
                    "俱乐部正式招聘 · 第一手信息", demandId: "demand.week3.longterm"),
                Mail("mail.w3.walter", MailContentKind.PlayerResume, 3, "北区球员代理", "球员简历：沃尔特·科瓦奇",
                    "正面对抗强硬的中后卫，回追转身并非优势。",
                    "沃尔特适合保护禁区和处理直接对抗。如果球队长期把四名后卫留在对方半场，他也会努力回追，只是摄影师可能需要广角镜头。",
                    "代理人材料 · 有完整比赛旁证", playerId: "player.wall.walter"),
                Mail("mail.w3.luna", MailContentKind.PlayerResume, 3, "满月体育", "球员简历：露娜·萨维奇",
                    "能内切和处理定位球的左边锋，决策方式比较独特。",
                    "露娜的技术和速度都达到职业试训水平。她偶尔会根据星象调整罚球顺序；经纪公司认为这是赛前准备，前教练称其为“另一个需要管理的应用程序”。",
                    "经纪人材料 · 部分说法无法验证", playerId: "player.zodiac.luna"),
                Mail("mail.w3.uma", MailContentKind.PlayerResume, 3, "两侧都行经纪事务所", "球员简历：乌玛·费尔南德斯",
                    "能踢左右翼卫，跑动和适应能力可靠。",
                    "乌玛在四后卫和三中卫体系中都有比赛经验。她愿意踢左右两侧，唯一要求是赛前告诉她球队今天究竟打哪套阵型。",
                    "多家旧俱乐部推荐 · 可靠性较高", playerId: "player.utility.uma"),
                Mail("mail.w3.press", MailContentKind.General, 3, "《边线周刊》编辑部", "传闻核对：集锦里的速度不等于九十分钟速度",
                    "本周专栏比较短视频、完整比赛和经纪人口述的差异。",
                    "球探部门提醒，剪辑能隐藏回防、站位和比赛强度。看完十秒钟高光后，请至少再问一次：球丢掉以后这个人去了哪里？",
                    "独立媒体 · 方法说明"),

                Mail("mail.w4.seaside", MailContentKind.ClubRequest, 4, "海港蓝鸥 · 体育主管", "短期招聘：能处理高球的门将",
                    "保级球队一号门将受伤，需求仅保留两周。",
                    "我们需要一名能够指挥防线、处理传中并承受密集射门的门将。主场海风很大，但请不要把所有判断失误都写进气象报告。",
                    "俱乐部正式招聘 · 时效较高", demandId: "demand.week4.keeper"),
                Mail("mail.w4.gus", MailContentKind.PlayerResume, 4, "安全手套经纪公司", "球员简历：格斯·贝克",
                    "高球处理稳定，训练态度可靠，点球履历有一处夸张。",
                    "格斯可以立即参加试训。他确实在公开训练中扑出四粒点球；所谓第五次扑救，是第五名主罚者没有出场。",
                    "公开训练记录 · 可靠性较高", playerId: "player.gloves.gus"),
                Mail("mail.w4.felix", MailContentKind.PlayerResume, 4, "职业门将协会", "球员简历：费利克斯·文书",
                    "基本功稳定、职业态度优秀，出球速度偏慢。",
                    "费利克斯重视沟通和站位，也重视每一项流程。他不会忘记训练时间，但可能要求后卫在发动反击前确认收件人。",
                    "协会推荐 · 样本完整", playerId: "player.forms.felix"),
                Mail("mail.w4.dino", MailContentKind.PlayerResume, 4, "超远界外球实验室", "球员简历：迪诺·马尔凯蒂",
                    "边后卫，界外球距离惊人，防守纪律一般。",
                    "迪诺把界外球当作禁区传中使用。优势是能把球送得很远；风险是有时连本队中锋也不知道球会从哪片云下面落下。",
                    "专项训练报告 · 样本偏窄", playerId: "player.throwin.dino"),
                Mail("mail.w4.qualifier", MailContentKind.General, 4, "洲际赛事观察室", "资格赛首轮结束：豪门已经开始紧张",
                    "赛季仍在季前阶段，但洲际资格赛已经制造了第一批危机会议。",
                    "两支热门球队首回合未能获胜。电视评论员称这是战术问题，俱乐部董事会称这是体能问题，球迷认为唯一问题是夏季没有买够人。",
                    "赛事新闻 · 赛季进度"),

                Mail("mail.w5.foundry", MailContentKind.ClubRequest, 5, "铁炉竞技 · 主教练办公室", "杯赛阵容补充：中后卫与前锋",
                    "争夺洲际资格的球队需要两名可靠轮换，需求有效三周。",
                    "后卫需要守纪律，前锋需要参与逼抢。我们不反对个人品牌，但摄制组、无人机和庆祝动作设计师不能一起进入训练基地。",
                    "俱乐部正式招聘 · 第一手信息", demandId: "demand.week5.group"),
                Mail("mail.w5.rita", MailContentKind.PlayerResume, 5, "北方防线顾问", "球员简历：丽塔·哈达德",
                    "位置感出色、处理球果断的中后卫。",
                    "丽塔习惯先解决危险，再讨论镜头角度。她的完整比赛数据稳定，黄牌数量也低于经纪人邮件中的感叹号数量。",
                    "完整比赛记录 · 可靠性高", playerId: "player.rivet.rita"),
                Mail("mail.w5.sonny", MailContentKind.PlayerResume, 5, "闪耀十一人娱乐", "球员简历：桑尼·布莱兹",
                    "技术动作很多，防守参与和时间观念需要核实。",
                    "桑尼能创造普通球员想不到的处理，也会尝试教练没要求的处理。他的团队保证这次试训不会迟到，前提是开场时间适合直播。",
                    "球员团队宣传 · 风险较高", playerId: "player.showboat.sonny"),
                Mail("mail.w5.nova", MailContentKind.PlayerResume, 5, "东岸分析学院", "球员简历：诺瓦·伊万诺娃",
                    "能在逼抢下组织进攻的数据型中场。",
                    "诺瓦会把赛前分析真正带进比赛，而不是只带进会议室。她阅读压迫、寻找向前线路的能力都有完整录像支持。",
                    "学院与比赛数据联合报告 · 可靠性高", playerId: "player.spreadsheet.nova"),
                Mail("mail.w5.camera", MailContentKind.General, 5, "训练基地物业部", "通知：球员纪录片摄制组不属于医疗设备",
                    "本周已有三支摄制组申请进入更衣室。",
                    "物业部再次说明：摄影机不能占用冰浴池，导演椅不能放在战术通道，“寻找真实感”也不能成为翻越围栏的理由。",
                    "基地公告 · 信息真实"),

                Mail("mail.w6.comets", MailContentKind.ClubRequest, 6, "彗星城 · 足球战略部", "冠军阵容补强：中场与翼卫",
                    "卫冕冠军为双线赛程寻找轮换球员，需求有效四周。",
                    "我们需要一名能在高压下组织进攻的中场，以及一名覆盖整条边路的翼卫。数据部门会提供模型，但最终仍由教练看球。",
                    "俱乐部正式招聘 · 第一手信息", demandId: "demand.week6.future"),
                Mail("mail.w6.echo", MailContentKind.PlayerResume, 6, "双足体育", "球员简历：艾可·摩根",
                    "左右脚均衡、能够胜任两侧的翼卫。",
                    "艾可在两侧都有完整比赛样本，回追和套边能力稳定。经纪人唯一没有回答的问题，是她更愿意坐球队大巴左边还是右边。",
                    "多场比赛记录 · 可靠性高", playerId: "player.bothfeet.echo"),
                Mail("mail.w6.scout", MailContentKind.General, 6, "冠军联赛球探圆桌", "观察笔记：高位防线最怕的不是慢，是判断慢",
                    "洲际赛事对空间处理和决策速度要求更高。",
                    "评价后卫时，不要只看百米速度。提前移动、保护身后和与门将沟通，往往比一次漂亮的回追更能说明问题。",
                    "专业讨论 · 可作为判断依据"),
                Mail("mail.w6.finance", MailContentKind.General, 6, "事务所财务", "应收款提醒：已成交不等于已到账",
                    "结果与付款可能跨周到达，请分别查看现金和应收。",
                    "俱乐部确认试训后，顾问费会进入应收款；预计到账周到来后才计入现金。请不要拿尚未到账的钱预订赛季末庆功宴。",
                    "系统规则 · 可直接信赖"),
                Mail("mail.w6.calendar", MailContentKind.General, 6, "联盟赛程中心", "季前阶段即将结束：联赛开幕倒计时",
                    "下周开始进入夏季窗口与联赛准备的密集阶段。",
                    "俱乐部将陆续公布号码、注册名单和最后热身赛。赛程中心提醒各队：把友谊赛输球称为“负荷管理”不会改变比分。",
                    "联盟公告 · 赛季进度")
            };

            AddSeasonProgressMails(mails);
            AddExpiryMails(mails, players, demands);
            return mails;
        }

        public static List<MagazineIssueContent> BuildMagazineIssues()
        {
            return new List<MagazineIssueContent>
            {
                Issue("mag.touchline.01", 0, 1, "《边线周刊》", "资格赛前的前锋市场", "季前号 01",
                    Page(MagazinePageLayout.Cover, "封面故事", "四十一球到底在哪儿？", "从经纪人数字回到完整比赛。",
                        "一名前锋的宣传材料写着四十一球，但没有联赛名称、出场数和比赛级别。数字越整齐，越应该先问统计口径。",
                        "雨城竞技需要的是能立刻适应资格赛强度的前锋。跑位、逼抢和身体状态，比一张孤立的射手榜截图更重要。",
                        "“球探不是来证明邮件正确，而是来判断哪里可能不正确。”", "核对清单", "赛事级别、出场分钟、点球占比、完整录像。", "player.rocket.rory"),
                    Page(MagazinePageLayout.Feature, "战术室", "资格赛为什么比热身赛快半拍", "高压比赛首先考验决策。",
                        "洲际资格赛的空间更小，转换更快。前锋如果只等待最后一脚，球队很容易在第一道逼抢就少一人。",
                        "观察候选人时，应记录他丢球后的五秒、无球跑动和与中场的距离，而不只是射门。",
                        "“速度是到达空间，判断是提前知道空间会出现。”", "雨城竞技", "上季第四，目标进入洲际冠军联赛正赛。", "player.rocket.rory"),
                    Page(MagazinePageLayout.ScoutReport, "球探报告", "罗里·奎克：跑位可信，数字待查", "公开信息的可信部分与空白部分。",
                        "两场完整录像证明他的反越位和启动具备职业水准。",
                        "没有证据支持四十一球，也没有证据说明比赛强度足以对应顶级联赛。",
                        "“可以推荐试训，但不能把宣传数字当结论。”", "风险", "样本量小；经纪人叙述明显偏向球员。", "player.rocket.rory"),
                    Page(MagazinePageLayout.Feature, "文化页", "当每支球队都说自己在重建", "季前发布会常用词观察。",
                        "长期计划、年轻化和回归传统是最常见的三句话。",
                        "如果同一周出现第四位“最后一块拼图”，请检查俱乐部是否还保留前三块的收据。",
                        "“足球没有最终版文件，只有下一次修改。”", "本周术语", "负荷管理：热身赛输球后的常用解释。", "")),
                Issue("mag.touchline.02", 1, 2, "《边线周刊》", "中场价值与看不见的工作", "季前号 02",
                    Page(MagazinePageLayout.Cover, "封面故事", "为什么最重要的传球常常进不了集锦", "米洛式中场的观看方法。",
                        "安全接应、调整站位和吸引逼抢很少成为短视频标题。",
                        "但这些动作决定球队能否把一次控球变成稳定推进。",
                        "“好的中场让队友看起来都早到了一秒。”", "观察重点", "接球前扫描、身体朝向、丢球后的回位。", "player.metronome.milo"),
                    Page(MagazinePageLayout.Feature, "比赛分析", "船坞联需要的是连接，不是第二个前锋", "双人招聘的结构逻辑。",
                        "前锋负责威胁纵深，中场负责让威胁持续存在。",
                        "如果两人都只追求最后一脚，球队会像散开的集装箱一样失去联系。",
                        "“组合价值不等于两份个人集锦相加。”", "俱乐部档案", "上季第十一，定位球强，运动战组织不足。", "player.metronome.milo"),
                    Page(MagazinePageLayout.ScoutReport, "老将观察", "维克：速度下降之后还剩什么", "六码区经验是否仍有市场。",
                        "维克的冲刺频率明显下降，但对二点球和解围落点的判断仍然可靠。",
                        "需要持续高位压迫的球队应谨慎；需要短时间禁区效率的球队可以试训。",
                        "“老将的价值不是跑得和年轻人一样快。”", "适配条件", "有限出场时间、明确禁区职责、合理轮换。", "player.thermos.vic"),
                    Page(MagazinePageLayout.Feature, "数据页", "完整比赛比热区更会说话", "别让一张图替你看完九十分钟。",
                        "热区能说明球员在哪里触球，却不一定说明他为什么在那里。",
                        "把数据与比赛阶段、比分和对手强度放在一起，结论才有意义。",
                        "“没有上下文的数据，只是穿西装的传闻。”", "本周提醒", "先问样本，再问结论。", "")),
                Issue("mag.touchline.03", 2, 3, "《欧陆足球观察》", "豪门补强与高位防线", "第 03 期",
                    Page(MagazinePageLayout.Cover, "封面故事", "传统豪门需要怎样的中后卫", "高位防线把判断放大。",
                        "争冠球队的中后卫常在更大空间里防守。",
                        "正面对抗强不等于适合所有体系，提前移动和转身同样重要。",
                        "“高位防线不会隐藏短板，只会给短板更大的草地。”", "旧堡王冠", "上季第二，连续六年参加洲际冠军联赛。", "player.wall.walter"),
                    Page(MagazinePageLayout.Feature, "战术室", "边锋的一对一不是个人表演", "突破必须连接后续进攻。",
                        "真正有效的一对一会迫使防线移动，为队友创造射门或传中空间。",
                        "只有动作没有后续选择，往往只是把丢球地点向前移动十米。",
                        "“过掉第一个人以后，比赛才刚开始。”", "观察重点", "抬头频率、弱侧队友、丢球反应。", "player.zodiac.luna"),
                    Page(MagazinePageLayout.ScoutReport, "球探报告", "沃尔特：禁区可靠，高位有问号", "强项明确，体系适配需要试训。",
                        "沃尔特在禁区附近的对抗和解围值得信任。",
                        "当防线压到中线，他的转身和追踪线路可能成为风险。",
                        "“不是能力不足，而是问题必须问对。”", "建议", "安排高位防线专项试训，不以亲属评价作结论。", "player.wall.walter"),
                    Page(MagazinePageLayout.Feature, "看台文化", "每份五年计划为什么只活到星期五", "豪门董事会语言学。",
                        "成绩好时叫延续，成绩差时叫重建。",
                        "计划本身通常没有问题，问题是文件名里的“最终版”已经出现八次。",
                        "“长期主义的最短单位，是下一场比赛。”", "词汇表", "全面支持：通常持续到下一次主场嘘声。", "")),
                Issue("mag.touchline.04", 3, 4, "《门线月报》", "门将、传中与第五粒点球", "第 04 期",
                    Page(MagazinePageLayout.Cover, "封面故事", "五连扑传说的第五个人去哪了", "漂亮数字也要核对出场名单。",
                        "格斯确实完成四次扑救，这是很强的表现。",
                        "第五名主罚者没有出场，因此不能把缺席统计成一次扑救。",
                        "“好球探不会因为笑话有趣就停止核实。”", "门将指标", "站位、二次反应、高球判断、出球选择。", "player.gloves.gus"),
                    Page(MagazinePageLayout.Feature, "门将课堂", "海风不是技术指标", "处理传中需要可重复的判断。",
                        "风会影响球路，但门将仍需通过站位、启动和沟通降低不确定性。",
                        "把每次失误归因于天气，只会让下一次失误更准时地到来。",
                        "“环境能解释难度，不能替代责任。”", "海港蓝鸥", "上季附加赛保级，禁区防守压力大。", "player.gloves.gus"),
                    Page(MagazinePageLayout.ScoutReport, "球探报告", "费利克斯：可靠，但反击需要少一张表", "职业性很高，节奏偏慢。",
                        "费利克斯的站位和基础扑救稳定，训练记录也很完整。",
                        "快速出球时存在犹豫，适合强调控制而非持续转换的球队。",
                        "“手续齐全不会丢球，但也不会自动制造反击。”", "建议", "试训加入快速手抛球和长传决策。", "player.forms.felix"),
                    Page(MagazinePageLayout.Feature, "规则角", "界外球可以很远，不能离开球场", "迪诺专项观察。",
                        "超远界外球能创造禁区压力，但落点必须可预测。",
                        "如果队友也在寻找皮球，战术突然性就已经用过头了。",
                        "“距离是武器，控制才是瞄准器。”", "样本风险", "专项集锦无法替代防守录像。", "player.throwin.dino")),
                Issue("mag.touchline.05", 4, 5, "《九十分钟》", "纪律、压迫与摄制组", "第 05 期",
                    Page(MagazinePageLayout.Cover, "封面故事", "没有镜头时还愿不愿意回防", "铁炉竞技的招聘标准。",
                        "杯赛球队需要能执行重复任务的球员。",
                        "压迫、补位和保护二点球不漂亮，却决定淘汰赛能走多远。",
                        "“职业性，是镜头转开以后仍然做正确的事。”", "铁炉竞技", "上季第七，两次国内杯冠军。", "player.rivet.rita"),
                    Page(MagazinePageLayout.Feature, "防守课", "丽塔为什么很少出现在失误集锦里", "位置感的价值是让危险不发生。",
                        "她不会为了抢镜离开防区，处理危险球也很直接。",
                        "这类后卫的优秀表现通常是一场比赛没有发生值得剪辑的事故。",
                        "“最好的解围，有时是提前两步让传球根本进不来。”", "可靠性", "多场完整录像与不同来源评价一致。", "player.rivet.rita"),
                    Page(MagazinePageLayout.ScoutReport, "风险报告", "桑尼：灵感很贵，纪律也有价格", "能力与可用性必须一起评估。",
                        "桑尼能完成高难度动作，也会忽略简单的跑位要求。",
                        "如果球队没有管理空间，他的个人亮点可能抵不过无球阶段的损失。",
                        "“不是所有穿裆都能进入积分榜。”", "试训建议", "记录准时性、无球跑动和丢球后的反应。", "player.showboat.sonny"),
                    Page(MagazinePageLayout.Feature, "媒体页", "纪录片不是球探报告", "摄像机改变人们展示自己的方式。",
                        "球员知道镜头存在时，会更愿意尝试可传播的动作。",
                        "因此需要无剪辑训练、完整比赛和不同来源的观察。",
                        "“真实感不能由导演喊开始。”", "基地规定", "无人机不得进入定位球训练。", "")),
                Issue("mag.touchline.06", 5, 6, "《冠军之路》", "双线赛程的轮换答案", "第 06 期",
                    Page(MagazinePageLayout.Cover, "封面故事", "冠军球队为何仍然需要补强", "联赛与洲际赛事需要不同的体能账本。",
                        "卫冕冠军面对更多高强度比赛，轮换不是替补席人数问题。",
                        "球员必须能在不同对手和不同比赛状态下完成明确职责。",
                        "“冠军阵容不是一张合影，而是一整季都能用的工具箱。”", "彗星城", "卫冕冠军，洲际冠军联赛夺冠热门。", "player.bothfeet.echo"),
                    Page(MagazinePageLayout.Feature, "战术室", "翼卫为什么像两份工作", "一条边路的攻防都写在职位说明里。",
                        "翼卫需要提供宽度、回追、协防和反击出口。",
                        "能踢两侧的球员会提高阵容弹性，但必须确认左右脚能力不是宣传用语。",
                        "“覆盖全场不等于无目的地跑遍全场。”", "观察重点", "回追路线、弱侧站位、连续冲刺恢复。", "player.bothfeet.echo"),
                    Page(MagazinePageLayout.ScoutReport, "球探报告", "诺瓦：数据不是她唯一的语言", "高压下处理球的样本可靠。",
                        "诺瓦能在逼抢到来前调整身体朝向，并主动寻找向前线路。",
                        "她的数据习惯是额外工具，而不是掩盖比赛能力的包装。",
                        "“真正的数据型球员，首先仍然是球员。”", "适配", "适合需要控制节奏和压迫下出球的球队。", "player.spreadsheet.nova"),
                    Page(MagazinePageLayout.Feature, "赛程页", "从季前训练走向五十二周", "一年不会只发生在转会窗口。",
                        "联赛开幕、洲际小组赛、冬季窗口、淘汰赛和争冠保级会依次到来。",
                        "关注顶部日期和赛事邮件，不需要自己换算第几周。",
                        "“赛季很长，但每个截止日都来得比体育主管想象中快。”", "下一阶段", "夏季窗口与联赛开幕。", ""))
            };
        }

        private static void AddSeasonProgressMails(List<MailContentEntry> mails)
        {
            mails.AddRange(new[]
            {
                SeasonMail("mail.season.w7", 7, "联盟赛程中心", "联赛开幕月：新赛季正式进入比赛节奏",
                    "热身赛结束，联赛注册名单陆续公布。接下来几周，球队会在补强冲动和财务现实之间反复横跳。"),
                SeasonMail("mail.season.w10", 10, "转会窗口办公室", "夏季窗口关闭：传真机再次幸存",
                    "注册通道已经关闭。最后一分钟提交的文件中，有两份缺签名、一份咖啡渍，以及一张明显属于餐厅的菜单。"),
                SeasonMail("mail.season.w14", 14, "洲际赛事组委会", "冠军联赛阶段开打：周中夜赛开始",
                    "联赛之外的洲际赛程正式开始。阵容深度、旅行恢复和客场抗压将逐渐影响招聘口径。"),
                SeasonMail("mail.season.w18", 18, "足球新闻台", "秋季密集赛程：伤病名单开始比首发名单长",
                    "连续一周双赛让各队重新理解“轮换”的意义。部分体育主管已经开始给冬季目标发并不隐秘的问候。"),
                SeasonMail("mail.season.w22", 22, "洲际赛事组委会", "小组阶段收官：有人晋级，有人开始解释",
                    "晋级球队讨论淘汰赛签位，出局球队讨论赛制、草皮、天气和统计口径。没有人讨论夏天那份过度自信的演示文档。"),
                SeasonMail("mail.season.w26", 26, "联盟竞赛部", "节日赛程：四天一场，睡眠属于技术统计",
                    "密集联赛进入最困难阶段。球队需要控制伤病和体能，而不是把所有疲劳都解释成球员缺乏求胜欲。"),
                SeasonMail("mail.season.w29", 29, "冬季注册窗口", "冬季窗口开启：补洞、豪赌与租借",
                    "俱乐部可以再次注册新球员。谨慎的球队修补阵容，焦虑的球队修改整个计划，富有的球队两件事一起做。"),
                SeasonMail("mail.season.w32", 32, "冬季注册窗口", "冬季窗口关闭：所有人都说完成了主要目标",
                    "窗口已经关闭。没有买到人的球队称赞现有阵容，买了很多人的球队强调市场机会不可错过。"),
                SeasonMail("mail.season.w36", 36, "洲际赛事组委会", "淘汰赛阶段：一个失误可以写进整年纪录片",
                    "两回合淘汰赛开始。客场应对、领先后的控制和落后时的决策，会比普通联赛更放大球员特点。"),
                SeasonMail("mail.season.w40", 40, "联盟数据中心", "联赛进入冲刺：积分榜开始拒绝礼貌",
                    "争冠、欧战资格和保级区逐渐成形。每支球队都说只关注下一场，但所有办公室都开着积分计算表。"),
                SeasonMail("mail.season.w44", 44, "杯赛委员会", "杯赛半决赛周：替补席深度接受检查",
                    "淘汰赛没有平均值。一次定位球、一次扑救或一次错误换人，都可能决定整季叙事。"),
                SeasonMail("mail.season.w48", 48, "足球新闻台", "冠军联赛决赛月与国内联赛最后冲刺",
                    "赛季最重要的比赛集中到来。争冠球队谈冷静，保级球队谈奇迹，中游球队开始发布下赛季球衣。"),
                SeasonMail("mail.season.w52", 52, "联盟主席办公室", "赛季结束：年度结算将在本周完成",
                    "最后一轮已经结束。请处理本周事务，随后事务所会结算现金、应收、声望和全年安排记录。")
            });
        }

        private static void AddExpiryMails(
            List<MailContentEntry> mails,
            IReadOnlyList<PlayerContentEntry> players,
            IReadOnlyList<DemandContentEntry> demands)
        {
            foreach (var player in players)
            {
                mails.Add(new MailContentEntry
                {
                    id = "mail.expiry." + player.id,
                    kind = MailContentKind.General,
                    publishedWeek = player.LastAvailableWeek + 1,
                    sender = Zh("球员动态中心"),
                    subject = player.expiryMailSubject,
                    preview = Zh("该球员已经离开可安排名单。"),
                    body = player.expiryMailBody,
                    sourceNote = Zh("状态变更 · 球员不再可安排"),
                    expiredPlayerId = player.id
                });
            }

            foreach (var demand in demands)
            {
                mails.Add(new MailContentEntry
                {
                    id = "mail.expiry." + demand.id,
                    kind = MailContentKind.General,
                    publishedWeek = demand.DeadlineWeek + 1,
                    sender = demand.clubDisplayName,
                    subject = demand.expiryMailSubject,
                    preview = Zh("该招聘需求已经过期并从工作台移除。"),
                    body = demand.expiryMailBody,
                    sourceNote = Zh("俱乐部通知 · 招聘需求已关闭"),
                    expiredDemandId = demand.id
                });
            }
        }

        private static PlayerContentEntry Player(
            string id, int portraitIndex, int availableFromWeek, int availabilityWeeks,
            string name, PlayerPosition position, int ability, int fitness, int professionalism,
            int salaryMinWeekly, int salaryMaxWeekly, string careerHistory,
            string biography, string claim, string evidence, EvidenceReliability reliability,
            string expirySubject, string expiryBody)
        {
            return new PlayerContentEntry
            {
                id = id,
                portraitIndex = portraitIndex,
                availableFromWeek = availableFromWeek,
                availabilityWeeks = availabilityWeeks,
                displayName = Zh(name),
                biography = Zh(biography),
                publicPosition = position,
                hiddenAbility = ability,
                hiddenFitness = fitness,
                hiddenProfessionalism = professionalism,
                salaryMinWeekly = salaryMinWeekly,
                salaryMaxWeekly = salaryMaxWeekly,
                careerHistory = Zh(careerHistory),
                publicClaim = Zh(claim),
                publicEvidence = Zh(evidence),
                evidenceReliability = reliability,
                expiryMailSubject = Zh(expirySubject),
                expiryMailBody = Zh(expiryBody)
            };
        }

        private static DemandContentEntry Demand(
            string id, string clubId, string clubName, string standing, string bestAchievement,
            string clubProfile, string title, string description, int openedWeek, int activeWeeks,
            int reward, string paymentTerms, string expirySubject, string expiryBody,
            params DemandSlotContentEntry[] slots)
        {
            return new DemandContentEntry
            {
                id = id,
                clubId = clubId,
                clubDisplayName = Zh(clubName),
                clubStanding = Zh(standing),
                clubBestAchievement = Zh(bestAchievement),
                clubProfile = Zh(clubProfile),
                title = Zh(title),
                description = Zh(description),
                openedWeek = openedWeek,
                activeWeeks = activeWeeks,
                baseReward = reward,
                paymentTerms = Zh(paymentTerms),
                expiryMailSubject = Zh(expirySubject),
                expiryMailBody = Zh(expiryBody),
                slots = new List<DemandSlotContentEntry>(slots)
            };
        }

        private static DemandSlotContentEntry Slot(
            string id, PlayerPosition position, int ability, int fitness, int professionalism)
        {
            return new DemandSlotContentEntry
            {
                id = id,
                requiredPosition = position,
                minimumAbility = ability,
                preferredFitness = fitness,
                preferredProfessionalism = professionalism,
                isRequired = true
            };
        }

        private static MailContentEntry Mail(
            string id, MailContentKind kind, int week, string sender, string subject,
            string preview, string body, string source,
            string playerId = "", string demandId = "", int offer = 0,
            string offerTerms = "", string requiredClubId = "", string targetClub = "",
            string risk = "")
        {
            return new MailContentEntry
            {
                id = id,
                kind = kind,
                publishedWeek = week,
                sender = Zh(sender),
                subject = Zh(subject),
                preview = Zh(preview),
                body = Zh(body),
                sourceNote = Zh(source),
                relatedPlayerId = playerId,
                relatedDemandId = demandId,
                privateOfferAmount = offer,
                privateOfferTerms = Zh(offerTerms),
                privateRequiredClubId = requiredClubId,
                privateTargetClubRequirement = Zh(targetClub),
                privateRiskNote = Zh(risk)
            };
        }

        private static MailContentEntry SeasonMail(
            string id, int week, string sender, string subject, string body)
        {
            return Mail(id, MailContentKind.General, week, sender, subject,
                body, body, "赛事与赛季进度通知");
        }

        private static MagazineIssueContent Issue(
            string id, int coverIndex, int week, string publication, string title,
            string issueNumber, params MagazinePageContent[] pages)
        {
            return new MagazineIssueContent
            {
                id = id,
                coverIndex = coverIndex,
                publishedWeek = week,
                publicationName = Zh(publication),
                issueTitle = Zh(title),
                issueNumber = issueNumber,
                pages = new List<MagazinePageContent>(pages)
            };
        }

        private static MagazinePageContent Page(
            MagazinePageLayout layout, string kicker, string headline, string deck,
            string left, string right, string quote, string sidebarTitle, string sidebarBody,
            string relatedPlayerId)
        {
            return new MagazinePageContent
            {
                layout = layout,
                kicker = Zh(kicker),
                headline = Zh(headline),
                deck = Zh(deck),
                bodyLeft = Zh(left),
                bodyRight = Zh(right),
                pullQuote = Zh(quote),
                sidebarTitle = Zh(sidebarTitle),
                sidebarBody = Zh(sidebarBody),
                relatedPlayerId = relatedPlayerId
            };
        }

        private static LocalizedText Zh(string value)
        {
            return new LocalizedText
            {
                chineseSimplified = value ?? string.Empty,
                english = string.Empty
            };
        }
    }
}

