using System;
using System.Collections.Generic;
using NewPlayerHunter.Domain;
using NUnit.Framework;
using UnityEngine;

namespace NewPlayerHunter.Persistence.Tests
{
    public sealed class SaveGameServiceTests
    {
        private ES3Settings _settings;
        private SaveGameService _service;

        [SetUp]
        public void SetUp()
        {
            _settings = new ES3Settings("nph-editmode-test-" + Guid.NewGuid() + ".es3");
            _service = new SaveGameService(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            if (ES3.FileExists(_settings))
            {
                ES3.DeleteFile(_settings);
            }
        }

        [Test]
        public void SaveLoadRoundtrip_PreservesProgressFields()
        {
            var snapshot = CreateSnapshot();

            _service.Save(snapshot);

            Assert.That(_service.HasProgress, Is.True);
            Assert.That(_service.TryLoad(out var loaded), Is.True);
            Assert.That(loaded.schemaVersion, Is.EqualTo(SaveGameService.CurrentSchemaVersion));
            Assert.That(loaded.weekState.currentWeek, Is.EqualTo(3));
            Assert.That(loaded.weekState.cash, Is.EqualTo("1234.50"));
            Assert.That(loaded.weekState.reputation, Is.EqualTo(7));
            Assert.That(loaded.weekState.payments[0].amount, Is.EqualTo("250.75"));
            Assert.That(loaded.weekState.payments[0].isPaid, Is.True);
            Assert.That(loaded.weekState.payments[0].paidWeek, Is.EqualTo(3));
            Assert.That(loaded.weekState.committedAssignments[0].playerId, Is.EqualTo("player.a"));
            Assert.That(loaded.readMailIds, Is.EqualTo(new[] { "mail.1", "mail.2" }));
            Assert.That(loaded.unlockedPlayerIds, Is.EqualTo(new[] { "player.a" }));
            Assert.That(loaded.unlockedDemandIds, Is.EqualTo(new[] { "demand.a" }));
            Assert.That(loaded.carloFavorAccepted, Is.True);
            Assert.That(loaded.carloFavorConsequenceApplied, Is.False);
            Assert.That(loaded.eventLog, Is.EqualTo(new[] { "entry.1" }));
            Assert.That(loaded.lastStatus, Is.EqualTo("测试状态"));
            Assert.That(loaded.randomDrawCount, Is.EqualTo(4));
        }

        [Test]
        public void TryLoad_ReturnsFalse_WhenNoSaveExists()
        {
            Assert.That(_service.HasProgress, Is.False);
            Assert.That(_service.TryLoad(out var snapshot), Is.False);
            Assert.That(snapshot, Is.Null);
        }

        [Test]
        public void TryLoad_RejectsMismatchedSchemaVersion()
        {
            var snapshot = CreateSnapshot();
            _service.Save(snapshot);

            var json = ES3.Load<string>(SaveGameService.SaveKey, _settings);
            var tampered = JsonUtility.FromJson<GameProgressSnapshot>(json);
            tampered.schemaVersion = SaveGameService.CurrentSchemaVersion + 1;
            ES3.Save(SaveGameService.SaveKey, JsonUtility.ToJson(tampered), _settings);

            Assert.That(_service.TryLoad(out var loaded), Is.False);
            Assert.That(loaded, Is.Null);
        }

        [Test]
        public void DeleteProgress_RemovesSavedKey()
        {
            _service.Save(CreateSnapshot());
            Assert.That(_service.HasProgress, Is.True);

            _service.DeleteProgress();

            Assert.That(_service.HasProgress, Is.False);
        }

        [Test]
        public void MapperRoundtrip_PreservesDecimalMoney()
        {
            var domain = new WeekStateSnapshot
            {
                CurrentWeek = 2,
                Cash = 1234.56m,
                Reputation = -2,
                Payments = new List<PaymentSnapshot>
                {
                    new PaymentSnapshot
                    {
                        Id = "payment:1:d:s:p",
                        DemandId = "d",
                        PlayerId = "p",
                        Amount = 0.10m,
                        CreatedWeek = 1,
                        ExpectedWeek = 2,
                        IsPaid = false,
                        PaidWeek = 0
                    }
                }
            };

            var restored = ProgressSnapshotMapper.ToDomain(
                ProgressSnapshotMapper.FromDomain(domain));

            Assert.That(restored.Cash, Is.EqualTo(1234.56m));
            Assert.That(restored.Payments[0].Amount, Is.EqualTo(0.10m));
            Assert.That(restored.Payments[0].IsPaid, Is.False);
        }

        private static GameProgressSnapshot CreateSnapshot()
        {
            return new GameProgressSnapshot
            {
                weekState = new WeekStateJson
                {
                    currentWeek = 3,
                    cash = "1234.50",
                    reputation = 7,
                    committedAssignments = new List<AssignmentJson>
                    {
                        new AssignmentJson
                        {
                            demandId = "demand.a",
                            slotId = "slot.a",
                            playerId = "player.a",
                            week = 1
                        }
                    },
                    payments = new List<PaymentJson>
                    {
                        new PaymentJson
                        {
                            id = "payment:1:demand.a:slot.a:player.a",
                            demandId = "demand.a",
                            playerId = "player.a",
                            amount = "250.75",
                            createdWeek = 1,
                            expectedWeek = 3,
                            isPaid = true,
                            paidWeek = 3
                        }
                    }
                },
                readMailIds = new List<string> { "mail.1", "mail.2" },
                unlockedPlayerIds = new List<string> { "player.a" },
                unlockedDemandIds = new List<string> { "demand.a" },
                carloFavorAccepted = true,
                carloFavorConsequenceApplied = false,
                eventLog = new List<string> { "entry.1" },
                lastStatus = "测试状态",
                randomDrawCount = 4
            };
        }
    }
}
