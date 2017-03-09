using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steambridge
{

    public enum LoginFailReason
    {
        WrongInformation,
        SteamGuardCodeWrong,
        TwoFactorWrong,
        ExpiredCode,
        AlreadyLoggedIn,
        SteamGuardNotSupported
    }

    public enum SteamExitReason
    {
        NothingSpecial,
        NonEnglishCharachers
    }

}
