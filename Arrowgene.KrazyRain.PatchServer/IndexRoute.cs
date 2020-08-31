using System.Threading.Tasks;
using Arrowgene.WebServer;
using Arrowgene.WebServer.Route;

namespace Arrowgene.KrazyRain.PatchServer
{
    public class IndexRoute : WebRoute
    {
        public override string Route => "/";

        public override Task<WebResponse> Get(WebRequest request)
        {
            return base.Get(request);
        }
    }
}