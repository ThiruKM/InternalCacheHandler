using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InternalCacheMVCDemo.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 120, VaryByParam = "None")]
        public ActionResult Index()
        {
            ViewBag.Message = "This is cached for 120 seconds. Currdet Date and Time is " + DateTime.Now.ToString();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
