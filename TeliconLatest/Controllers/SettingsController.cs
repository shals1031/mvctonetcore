using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public SettingsController(IWebHostEnvironment env)
        {
            _env = env;
        }
        //
        // GET: /Settings/
        [HttpPost]
        public ActionResult Index()
        {
            ViewBag.Parishes = DataDictionaries.Parishes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Value
            }).ToList();
            XDocument doc = XDocument.Load(Path.Combine(_env.WebRootPath, "settings.xml"));
            XmlSerializer serializer = new XmlSerializer(typeof(Settings), new XmlRootAttribute("Settings"));
            using (StringReader reader = new StringReader(doc.ToString()))
            {
                Settings settings = (Settings)(serializer.Deserialize(reader));
                return PartialView("SettingsPartial", settings);
            }
        }
        [HttpPost]
        public JsonResult Save(Settings model)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(Settings));
                TextWriter WriteFileStream = new StreamWriter(Path.Combine(_env.WebRootPath, "settings.xml"));
                x.Serialize(WriteFileStream, model);
                WriteFileStream.Close();
                return Json(new JsonReturnParams
                {
                    Additional = 1,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
    }
}