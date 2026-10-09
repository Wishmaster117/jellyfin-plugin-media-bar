using Jellyfin.Plugin.MediaBarAlexflix.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MediaBarAlexflix
{
    public class MediaBarPlugin : BasePlugin<PluginConfiguration>, IHasPluginConfiguration, IHasWebPages
    {
        public override Guid Id => Guid.Parse("d61bd3d1-8bd3-4e5a-b81b-95184275dd4b");

        public override string Name => "Media Bar ALEXFLIX";

        public static MediaBarPlugin Instance { get; private set; } = null!;
        
        public IServiceProvider ServiceProvider { get; }
        
        public MediaBarPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IServiceProvider serviceProvider) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            
            ServiceProvider = serviceProvider;
        }
        
        public IEnumerable<PluginPageInfo> GetPages()
        {
            string? prefix = GetType().Namespace;

            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{prefix}.Configuration.config.html"
            };
        }
    }
}