using System;
using System.Collections.Generic;
using Poliyo.Core;

namespace Poliyo.Simulation
{
/// <summary>
/// Minimal aggregate root for the campaign. New systems mutate it only through application commands.
/// </summary>
public sealed class CampaignState
{
    private readonly List<CauseRecord> _causeRecords = new List<CauseRecord>();

    public CampaignState(CampaignSeed seed, int currentDay = 1)
    {
        Seed = seed;
        Calendar = new CampaignCalendar(currentDay);
    }

    public CampaignSeed Seed { get; }
    public CampaignCalendar Calendar { get; }
    public IReadOnlyList<CauseRecord> CauseRecords => _causeRecords;

    public void RecordCause(CauseRecord causeRecord)
    {
        if (causeRecord == null)
        {
            throw new ArgumentNullException(nameof(causeRecord));
        }

        if (causeRecord.Day != Calendar.CurrentDay)
        {
            throw new InvalidOperationException("A cause must be recorded on the current campaign day.");
        }

        _causeRecords.Add(causeRecord);
    }

    public void RestoreCauseRecords(IEnumerable<CauseRecord> causes)
    {
        if (causes == null) throw new ArgumentNullException(nameof(causes));
        _causeRecords.Clear();
        foreach (CauseRecord cause in causes)
        {
            if (cause == null) throw new ArgumentException("A cause is required.", nameof(causes));
            if (cause.Day > Calendar.CurrentDay)
            {
                throw new InvalidOperationException("A restored cause cannot be from the future.");
            }

            _causeRecords.Add(cause);
        }
    }
}
}
