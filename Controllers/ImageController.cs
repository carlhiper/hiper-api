using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using Hiper.Api.Helpers;

namespace Hiper.Api.Controllers
{
    [RoutePrefix("api/Image")]
    public class ImageController : ApiController
    {
        [Route("UserLogos/{username}")]
        public IHttpActionResult GetImage(string username)
        {
            var url = "~/Content/Images/UserLogos/" + username;
            var serverPath = HostingEnvironment.MapPath(url);
            if (serverPath != null)
            {
                var fileInfo = new FileInfo(serverPath);

                return !fileInfo.Exists
                    ? (IHttpActionResult) NotFound()
                    : new FileResult(fileInfo.FullName, "image/png");
            }
            return BadRequest();
        }

        [Route("TeamLogos")]
        public IHttpActionResult GetImageTeam()
        {
            const string url = "~/Content/Images/Team/teamPicture.jpg";
            var serverPath = HostingEnvironment.MapPath(url);
            if (serverPath != null)
            {
                var fileInfo = new FileInfo(serverPath);

                return !fileInfo.Exists
                    ? (IHttpActionResult) NotFound()
                    : new FileResult(fileInfo.FullName, "image/png");
            }
            return BadRequest();
        }
    }
}
