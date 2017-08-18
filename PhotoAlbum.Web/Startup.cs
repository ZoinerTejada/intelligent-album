using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PhotoAlbum.Web.Startup))]
namespace PhotoAlbum.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
