using Expedition;

using System;

namespace RainMeadow;

public abstract class ChallengeState : OnlineState
{
    [OnlineField]
    bool revealCheck;
    [OnlineField]
    int revealCheckDelay;
    [OnlineField]
    public bool completed;
    [OnlineField(nullable = true)]
    public string? description;
    [OnlineField]
    bool hidden;
    [OnlineField]
    bool revealed;

    public abstract Type ChallengeType { get; }
    public abstract Challenge GetChallenge { get; }

    public ChallengeState() { }
    public ChallengeState(Challenge challenge)
    {

        revealCheck = challenge.revealCheck;
        revealCheckDelay = challenge.revealCheckDelay;
        completed = challenge.completed;
        description = challenge.description;
        hidden = challenge.hidden;
        revealed = challenge.revealed;
    }

    public virtual void ReadTo(Challenge challenge)
    {
        challenge.revealCheck = revealCheck;
        challenge.revealCheckDelay = revealCheckDelay;
        challenge.completed = completed;
        challenge.description = description;
        challenge.hidden = hidden;
        challenge.revealed = revealed;
    }
}
