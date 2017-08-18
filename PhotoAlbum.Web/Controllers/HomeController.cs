using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoAlbum.Data;
using PhotoAlbum.Web.Models;

namespace PhotoAlbum.Web.Controllers
{
    public class HomeController : Controller
    {
        private ImageRepository _imageRepo;
        private ImagePersonRepository _imagePersonRepo;
        private ImageTagRepository _imageTagRepo;

        public HomeController()
        {
            _imageRepo = new ImageRepository(SettingsHelper.DatabaseConnectionString());
            _imagePersonRepo = new ImagePersonRepository(SettingsHelper.DatabaseConnectionString());
            _imageTagRepo = new ImageTagRepository(SettingsHelper.DatabaseConnectionString());
        }

        public ActionResult Index()
        {
            const int maxImages = 20;
            var vm = new AlbumViewModel
            {
                Images = _imageRepo.GetAll(maxImages).ToList(),
                MaxNumberImages = maxImages
            };
            return View(vm);
        }

        public ActionResult Tags(string tag)
        {
            var vm = new AlbumViewModel
            {
                Tags = _imageTagRepo.GetUniqueTagList().ToList(),
                Images = !string.IsNullOrWhiteSpace(tag) ? _imageRepo.GetByTag(tag).ToList() : null,
                SelectedTag = tag
            };
            return View(vm);
        }

        public ActionResult Friends(string friend)
        {
            var vm = new AlbumViewModel
            {
                Names = _imagePersonRepo.GetUniqueNameList().ToList(),
                Images = !string.IsNullOrWhiteSpace(friend) ? _imageRepo.GetByPersonName(friend).ToList() : null,
                SelectedName = friend
            };
            return View(vm);
        }

        public ActionResult About()
        {
            return View(new AlbumViewModel());
        }

    }
}