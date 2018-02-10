using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steambridge
{

    public enum LoginResult
    {
        OK,
        RateLimitedExceeded,
        WrongInformation,
        WaitingForSteamGuard,
        SteamGuardCodeWrong,
        TwoFactorWrong,
        ExpiredCode,
        AlreadyLoggedIn,
        SteamGuardNotSupported,
        Timeout
    }

    public enum SteamExitReason
    {
        NothingSpecial,
        NonEnglishCharachers
    }

    public enum UpdateStateStage
    {
        Validating,
        Downloading,
        Commiting
    }

}
