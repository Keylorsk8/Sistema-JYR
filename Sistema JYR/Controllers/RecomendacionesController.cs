using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema_JYR.Controllers
{
    public class RecomendacionesController : Controller
    {
        // GET: Recomendaciones
        public ActionResult RecomendarProfuctos()
        {

            return View();
        }

        public List<string> RecomendarBusquedas()
        {
            return null;
        }
    }
}