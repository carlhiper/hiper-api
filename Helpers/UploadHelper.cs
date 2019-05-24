using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using ImageProcessor;

namespace Hiper.Api.Helpers
{
    public static class UploadHelper
    {
        public static string SaveUploadedImage(string imageString, string username)
        {
            var myString = imageString.Split(',');
            var bytes = Convert.FromBase64String(myString[1]);

            using (var ms = new MemoryStream(bytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var imageFactory = new ImageFactory())
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(ms)
                            .Quality(30)
                            .Save(outStream);
                        var result = new Bitmap(outStream);
                        Image image = result;
                        foreach (var value in from prop in image.PropertyItems where (prop.Id == 0x0112 || prop.Id == 5029 || prop.Id == 274) select (int) prop.Value[0])
                        {
                            if (value == 6)
                            {
                                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            }
                            if (value == 8)
                            {
                                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            }
                            if (value == 3)
                            {
                                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            }
                        }


                        var url = "~/Content/Images/UserLogos/" + username;
                        image.Save(HostingEnvironment.MapPath(url), ImageFormat.Jpeg);

                        return url;
                    }
                }
            }
        }

        public static string GetCurrentProfileImageUrl(string username)
        {
            var result = "api/Image/UserLogos/" + username;
            return result;
        }

        public static string GetTeamProfileImageurl()
        {
            var result = "api/Image/TeamLogos";
            return result;
        }

        public static string LoadEmailTemplate(string url)
        {
            url = ConfigurationManager.AppSettings["mailTemplatesPath"] + url;
            var serverPath = HostingEnvironment.MapPath(url);
            if (File.Exists(serverPath))
            {
                if (serverPath != null)
                {
                    var lines = File.ReadAllLines(serverPath);
                    var result = String.Join("\n", lines);
                    return result;
                }
            }
            return "";
        }
    }
}