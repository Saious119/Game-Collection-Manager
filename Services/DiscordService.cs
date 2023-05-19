using Microsoft.AspNetCore.Mvc;

namespace GameCollectionManager.Services
{
    public class DiscordService : Controller
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly ISession? _session;
        public static string? userDiscordID { get; set; }
        //private static readonly HttpContextService _httpContextService;
        public DiscordService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _session = httpContextAccessor.HttpContext.Session;
        }
        public void SetSession()
        {
            _session.SetString("userDiscordID", userDiscordID);
        }
    }
}
