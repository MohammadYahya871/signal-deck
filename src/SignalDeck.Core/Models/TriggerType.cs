namespace SignalDeck.Core.Models;

public enum TriggerType
{
    ReturnAfterIdle = 1,
    SignIn = 2,
    AppLaunch = 3,
    SessionLock = 4,
    SessionUnlock = 5,
    ResumeFromSleep = 6
}
