using System;

namespace NewPlayerHunter.Domain
{
    public enum SeasonPhase
    {
        Preseason,
        SummerWindow,
        LeagueOpening,
        GroupStage,
        WinterSchedule,
        WinterWindow,
        KnockoutStage,
        RunIn,
        Finals,
        SeasonReview
    }

    public sealed class SeasonCalendar
    {
        public const int MaximumPlayableWeeks = 52;

        public SeasonCalendar(DateTime startDate)
        {
            StartDate = startDate.Date;
        }

        public DateTime StartDate { get; }

        public DateTime DateForWeek(int week)
        {
            if (week < 1 || week > MaximumPlayableWeeks)
            {
                throw new ArgumentOutOfRangeException(nameof(week));
            }

            return StartDate.AddDays((week - 1) * 7);
        }

        public DateTime DateAfterFinalWeek =>
            StartDate.AddDays(MaximumPlayableWeeks * 7);

        public SeasonPhase PhaseForWeek(int week)
        {
            if (week < 1 || week > MaximumPlayableWeeks)
            {
                throw new ArgumentOutOfRangeException(nameof(week));
            }

            if (week <= 4) return SeasonPhase.Preseason;
            if (week <= 9) return SeasonPhase.SummerWindow;
            if (week <= 14) return SeasonPhase.LeagueOpening;
            if (week <= 22) return SeasonPhase.GroupStage;
            if (week <= 27) return SeasonPhase.WinterSchedule;
            if (week <= 31) return SeasonPhase.WinterWindow;
            if (week <= 40) return SeasonPhase.KnockoutStage;
            if (week <= 47) return SeasonPhase.RunIn;
            if (week <= 50) return SeasonPhase.Finals;
            return SeasonPhase.SeasonReview;
        }
    }
}
