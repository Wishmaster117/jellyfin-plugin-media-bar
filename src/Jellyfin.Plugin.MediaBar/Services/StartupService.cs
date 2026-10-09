using System.IO.Pipes;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Jellyfin.Plugin.MediaBarAlexflix.Helpers;
using Jellyfin.Plugin.MediaBarAlexflix.JellyfinVersionSpecific;
using Jellyfin.Plugin.MediaBarAlexflix.Model;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.MediaBarAlexflix.Services
{
    public class StartupService : IScheduledTask
    {
        public string Name => "Media Bar ALEXFLIX Startup";

        public string Key => "Jellyfin.Plugin.MediaBarAlexflix.Startup";
        
        public string Description => "Startup Service for MediaBar";
        
        public string Category => "Startup Services";
        
        private readonly IServerApplicationHost m_serverApplicationHost;
        private readonly ILogger<MediaBarPlugin> m_logger;
        private readonly IUserManager m_userManager;
        private readonly IPlaylistManager m_playlistManager;

        public StartupService(IServerApplicationHost serverApplicationHost, ILogger<MediaBarPlugin> logger,
            IUserManager userManager, IPlaylistManager playlistManager)
        {
            m_serverApplicationHost = serverApplicationHost;
            m_logger = logger;
            m_userManager = userManager;
            m_playlistManager = playlistManager;
        }

        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            m_logger.LogInformation($"MediaBar Startup. Registering file transformations.");
            
            List<JObject> payloads = new List<JObject>();

            {
                JObject payload = new JObject();
                payload.Add("id", "7680e282-b969-4b4f-98d8-ba9a65309c0a");
                payload.Add("fileNamePattern", "index.html");
                payload.Add("callbackAssembly", GetType().Assembly.FullName);
                payload.Add("callbackClass", typeof(TransformationPatches).FullName);
                payload.Add("callbackMethod", nameof(TransformationPatches.IndexHtml));
                
                payloads.Add(payload);
            }
            {
                JObject payload = new JObject();
                payload.Add("id", "cc4388bb-e016-4ab0-ba1f-a86c9f68dd5e");
                payload.Add("fileNamePattern", "avatars/list.txt");
                payload.Add("callbackAssembly", GetType().Assembly.FullName);
                payload.Add("callbackClass", typeof(TransformationPatches).FullName);
                payload.Add("callbackMethod", nameof(TransformationPatches.AvatarsList));
                
                payloads.Add(payload);
            }

            Assembly? fileTransformationAssembly =
                AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x =>
                    x.FullName?.Contains(".FileTransformation") ?? false);

            if (fileTransformationAssembly != null)
            {
                Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");

                if (pluginInterfaceType != null)
                {
                    foreach (JObject payload in payloads)
                    {
                        pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { payload });
                    }
                }
            }

            return Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => StartupServiceHelper.GetDefaultTriggers();
    }
}