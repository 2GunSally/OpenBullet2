using OpenBullet2.Core.Services;
using System.IO;

namespace OpenBullet2.Services
{
    public class PersistentSettingsService
    {
        private readonly OpenBulletSettingsService openBulletSettingsService;

        public bool SetupComplete => File.Exists(openBulletSettingsService.FileName);
        public bool UseCultureCookie { get; set; } = true;

        public PersistentSettingsService(OpenBulletSettingsService openBulletSettingsService)
        {
            this.openBulletSettingsService = openBulletSettingsService;
        }
    }
}
