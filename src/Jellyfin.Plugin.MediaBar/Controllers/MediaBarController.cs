using System.Reflection;
using Jellyfin.Extensions;
using Jellyfin.Plugin.MediaBarAlexflix.Configuration;
using Jellyfin.Plugin.MediaBarAlexflix.Helpers;
using Jellyfin.Plugin.MediaBarAlexflix.Model;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using Microsoft.AspNetCore.Mvc;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.MediaBarAlexflix.Controllers
{
    [Route("[controller]")]
    public class MediaBarAlexflixController : ControllerBase
    {
        [HttpGet("{file}")]
        public ActionResult GetFile([FromRoute] string file)
        {
            Stream? fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Jellyfin.Plugin.MediaBarAlexflix.Inject." + file);

            if (fileStream == null)
            {
                return NotFound();
            }

            using StreamReader streamReader = new StreamReader(fileStream);
            string fileContents = streamReader.ReadToEnd();

            string contentType = "text/plain";

            if (Path.GetExtension(file) == ".js")
            {
                contentType = "text/javascript";
            }
            else if (Path.GetExtension(file) == ".css")
            {
                contentType = "text/css";
            }
            else if (Path.GetExtension(file) == ".html")
            {
                contentType = "text/html";
            }
        
            return Content(fileContents, contentType);
        }

        [HttpPost("Avatar/List")]
        public ActionResult GetAvatarsList([FromBody] PatchRequestPayload payload)
        {
            string? content = TransformationPatches.AvatarsList(payload);

            if (content == null)
            {
                return NotFound();
            }
            
            return Content(content, "text/plain");
        }

        [HttpGet("WebConfig")]
        public ActionResult<WebConfig> GetWebConfig()
        {
            return Ok(MediaBarPlugin.Instance.Configuration.WebConfig);
        }
    }
}