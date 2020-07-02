using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Sistema_JYR.Models;

namespace Sistema_JYR.Controllers
{
    public class ProformasController : Controller
    {
        private SistemaJYREntities db = new SistemaJYREntities();

        // GET: Proformas
        public ActionResult Index()
        {
            if (TempData.ContainsKey("mensaje"))
            {
                ViewBag.Mensaje = TempData["mensaje"].ToString();
            }
            var proformas = db.Proformas.Include(p => p.AspNetUsers).Include(p => p.EstadoProforma).Where(x => x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1);
            return View(proformas.ToList());
        }

        // GET: Proformas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["mensaje"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }


            Proformas proformas = db.Proformas.Find(id);
            if (proformas != null)
            {
                List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
                proformas.ProformaDetalle = detalles;
            }
             
            if (proformas == null)
            {
                TempData["mensaje"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            return View(proformas);
        }

        // GET: Proformas/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion");
            return View();
        }

        // POST: Proformas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto")] Proformas proformas)
        {
            if (ModelState.IsValid)
            {
                db.Proformas.Add(proformas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }

        // GET: Proformas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["mensaje"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                TempData["mensaje"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }

        // POST: Proformas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto")] Proformas proformas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers, "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }

        // GET: Proformas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                return HttpNotFound();
            }
            return View(proformas);
        }

        // POST: Proformas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            db.Proformas.Remove(proformas);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
       

    protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        public ActionResult filtrarProformasAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.Proformas.Where(x => x.AspNetUsers.Nombre.Contains(terminoBusqueda)
                || x.EstadoProforma.Descripcion.Contains(terminoBusqueda) && x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1);
                return PartialView("_ListaProformas", lista.ToList());
            }


            return View();
        }
    }
}
