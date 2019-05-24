using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using Hiper.Api.Helpers;

namespace Hiper.Api.Controllers
{
    [RoutePrefix("api/Locale")]
    public class LocalController : ApiController
    {
        [Route("json/{locale}")]
        public IHttpActionResult GetLocale(string locale)
        {
            var url = "~/Content/Locales/locale-" + locale + ".json";
            var serverPath = HostingEnvironment.MapPath(url);
            if (serverPath != null)
            {
                var fileInfo = new FileInfo(serverPath);

                return !fileInfo.Exists
                    ? (IHttpActionResult) NotFound()
                    : new FileResult(fileInfo.FullName, "application/json");
            }
            return BadRequest();
        }
    }
}