using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace steambridge
{
    public class SteamBridgeWebClient : WebClient
    {

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
            w.ProtocolVersion = Version.Parse("1.0");
            return (WebRequest)w;
        }
    }
}
