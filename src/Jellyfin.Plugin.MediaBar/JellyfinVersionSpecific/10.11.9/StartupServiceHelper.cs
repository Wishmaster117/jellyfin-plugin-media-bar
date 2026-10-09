using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.MediaBarAlexflix.JellyfinVersionSpecific
{
    public static class StartupServiceHelper
    {
        public static IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo()
            {
                Type = TaskTriggerInfoType.StartupTrigger
            };
        }
    }
}