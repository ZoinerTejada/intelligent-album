using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoAlbum.Web.Models;
using PhotoAlbum.Data;

namespace PhotoAlbum.Web.Controllers
{
    public class BotController : Controller
    {
        private ImagePersonRepository _imagePersonRepo;
        private ImageTagRepository _imageTagRepo;

        public BotController()
        {
            _imagePersonRepo = new ImagePersonRepository(SettingsHelper.DatabaseConnectionString());
            _imageTagRepo = new ImageTagRepository(SettingsHelper.DatabaseConnectionString());
        }

        // GET: Bot
        public ActionResult Index()
        {
            var vm = new AlbumViewModel
            {
                Tags = _imageTagRepo.GetUniqueTagList().ToList(),
                Names = _imagePersonRepo.GetUniqueNameList().ToList()
            };
            return View(vm);
        }
    }
}