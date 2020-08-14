using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Sistema_JYR.Models;
using Sistema_JYR.Models.Session;
using Image = iText.Layout.Element.Image;
using Rectangle = iText.Kernel.Geom.Rectangle;
using System.Web;
using System.Web.Helpers;
using System.Data.Entity.Validation;

namespace Sistema_JYR.Controllers
{
    public class ProformasController : Controller
    {
        private readonly SistemaJYREntities db = new SistemaJYREntities();

        #region Metodos de Admin - Vendedor
        // GET: Proformas
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult Index()
        {
            var proformas = db.Proformas.Where(x => x.AspNetUsers.Rol == 2 && x.IdEstado == 2 || x.AspNetUsers.Rol == 1 && x.IdEstado == 2).OrderByDescending(x => x.Id);
            return View(proformas.ToList());
        }

        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult SeleccionarDocumento(int id)
        {
            try
            {
                Proformas proformas = db.Proformas.Find(id);
            }
            catch (Exception)
            {

                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Proforma, NumerosDocumento = id };
            Session["Proforma"] = "Seleccionado";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult CancelarProforma(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proforma = db.Proformas.Find(id);
            if (proforma == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            proforma.IdEstado = 1;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            Session["Proforma"] = "Cancelada";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult DuplicarProforma(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Proformas proformaDuplicado = new Proformas
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = proformas.IdEstado,
                Fecha = proformas.Fecha,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombreProforma = proformas.NombreProforma
            };
            db.Proformas.Add(proformaDuplicado);
            db.SaveChanges();
            ProformaDetalle detalleDuplicado = new ProformaDetalle();
            foreach (var item in detalles)
            {
                detalleDuplicado.IdProforma = proformaDuplicado.Id;
                detalleDuplicado.IdProducto = item.IdProducto;
                detalleDuplicado.Cantidad = item.Cantidad;
                detalleDuplicado.PrecioUnitario = item.PrecioUnitario;
                detalleDuplicado.Descuento = item.Descuento;
                db.ProformaDetalle.Add(detalleDuplicado);
                db.SaveChanges();
            }
            Session["Proforma"] = "¡Proforma duplicada correctamente!";
            return RedirectToAction("Index");
        }

        // GET: Proformas/Details/5
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }

            Proformas proformas = db.Proformas.Find(id);
            if (proformas != null)
            {
                List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
                proformas.ProformaDetalle = detalles;
                ViewBag.Id = proformas.Id;
            }
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            return View(proformas);
        }

        public ActionResult Pdf(int id)
        {
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdfDocument = new PdfDocument(pw);
            Document doc = new Document(pdfDocument, PageSize.LETTER);
            doc.SetMargins(122, 35, 70, 35);
            string pathLogo = Server.MapPath("~/Content/imagenes/LOGO2.png");
            Image img = new Image(ImageDataFactory.Create(pathLogo));
            pdfDocument.AddEventHandler(PdfDocumentEvent.START_PAGE, new HeaderEventHandler(img));
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler());
            List<Proformas> model = db.Proformas.Where(x => x.Id == id).ToList();

            foreach (var item in model)
            {
                List<AspNetUsers> user = db.AspNetUsers.Where(x => x.Id == item.IdCliente).ToList();
                List<Telefonos> tel = db.Telefonos.Where(x => x.IdUsuario == item.IdCliente).ToList();
                Table enc = new Table(4).UseAllAvailableWidth();
                Cell cellenc = new Cell().Add(new Paragraph("Proforma No." + id).SetFontSize(11)).
                    SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Día").SetFontSize(9)).
                            SetTextAlignment(TextAlignment.CENTER).SetWidth(25);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Mes").SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(40);
                enc.AddCell(cellenc);
                cellenc = new Cell().Add(new Paragraph("Año").SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(30);
                enc.AddCell(cellenc);
                doc.Add(enc);
                Table enc2 = new Table(4).UseAllAvailableWidth();
                Cell cellenc2 = new Cell().Add(new Paragraph(item.NombreProforma).SetFontSize(9)).
                    SetTextAlignment(TextAlignment.LEFT).SetBorder(Border.NO_BORDER);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Day.ToString()).SetFontSize(9)).
                            SetTextAlignment(TextAlignment.CENTER).SetWidth(25).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Month.ToString()).SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(40).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                cellenc2 = new Cell().Add(new Paragraph(item.Fecha.Year.ToString()).SetFontSize(9)).
                           SetTextAlignment(TextAlignment.CENTER).SetWidth(30).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                enc2.AddCell(cellenc2);
                doc.Add(enc2);
                Table _enc = new Table(2).UseAllAvailableWidth();
                Cell _cellEnc = new Cell(2, 1).Add(new Paragraph("Información de Cliente").SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                _enc.AddCell(_cellEnc);
                _cellEnc = new Cell(2, 1).Add(new Paragraph("Detalles").SetFontSize(9)).
                      SetTextAlignment(TextAlignment.LEFT).SetWidth(100).SetBorderLeft(Border.NO_BORDER);
                _enc.AddCell(_cellEnc);
                doc.Add(_enc);
                Table _det = new Table(2).UseAllAvailableWidth();
                Cell _cellDet = new Cell(2, 1).Add(new Paragraph(item.NombreCliente).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                _enc.AddCell(_cellDet);
                _det.AddHeaderCell(_cellDet);
                _cellDet = new Cell(2, 1).Add(new Paragraph("Vendedor: " + item.AspNetUsers.Nombre + " " + item.AspNetUsers.Apellido1).SetFontSize(9)).
                       SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetWidth(116).SetBorderLeft(Border.NO_BORDER);
                _enc.AddCell(_cellDet);
                _det.AddHeaderCell(_cellDet);
                doc.Add(_det);
                Table _det2 = new Table(2).UseAllAvailableWidth();
                Cell _cellDet2 = new Cell(2, 1).Add(new Paragraph(item.DireccionEntrega).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER)
                     .SetBorderBottom(Border.NO_BORDER).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                _enc.AddCell(_cellDet2);
                _det2.AddHeaderCell(_cellDet2);
                _cellDet2 = new Cell(2, 1).Add(new Paragraph("Estado:" + item.EstadoProforma.Descripcion).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetWidth(125)
                     .SetBorderLeft(Border.NO_BORDER);
                _enc.AddCell(_cellDet2);
                _det2.AddHeaderCell(_cellDet2);
                doc.Add(_det2);
                Table _det3 = new Table(2).UseAllAvailableWidth();
                if (tel.Count() == 0)
                {
                    Cell _cellDet3 = new Cell(2, 1).Add(new Paragraph("Tel: N/A").SetFontSize(9)).
                                    SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER);
                    _enc.AddCell(_cellDet3);
                    _det3.AddHeaderCell(_cellDet3);
                    doc.Add(_det3);
                }
                else
                {
                    foreach (var t in tel)
                    {

                        Cell _cellDet3 = new Cell(2, 1).Add(new Paragraph("Tel:" + t.Telefono).SetFontSize(9)).
                   SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER);
                        _enc.AddCell(_cellDet3);
                        _det3.AddHeaderCell(_cellDet3);
                        doc.Add(_det3);
                    }
                }
            }
            //Productos
            Style styleCell = new Style()
                .SetBackgroundColor(WebColors.GetRGBColor("#042c3c"))
                .SetFontColor(WebColors.GetRGBColor("#f68c25"))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10);
            Table _table = new Table(5).UseAllAvailableWidth();
            Cell _cell = new Cell(2, 1).Add(new Paragraph("Id"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Producto"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Cantidad"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Precio Unitario"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            _cell = new Cell(2, 1).Add(new Paragraph("Descuento"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));
            Style sty = new Style()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(9);
            List<ProformaDetalle> det = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            foreach (var item in det)
            {
                List<Productos> prod = db.Productos.Where(p => p.Id == item.IdProducto).ToList();
                foreach (var p in prod)
                {
                    _cell = new Cell().Add(new Paragraph(item.IdProducto.ToString())).SetBorderRight(Border.NO_BORDER).
                        SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Productos.Nombre)).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Cantidad.ToString())).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.PrecioUnitario.ToString("₡0,#.00"))).SetBorder(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.Descuento.ToString("₡" + "0,#.00"))).SetBorderLeft(Border.NO_BORDER).
                        SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                }
            }
            doc.Add(_table);
            foreach (var item in model)
            {
                float[] anchos = { 300f, 70f, 80f };
                Table _footer = new Table(anchos).UseAllAvailableWidth();
                Cell _foot = new Cell().Add(new Paragraph("Términos y Condiciones").SetFontSize(9)).
             SetTextAlignment(TextAlignment.LEFT).SetBorderRight(Border.NO_BORDER);
                _footer.AddCell(_foot);

                _foot = new Cell().Add(new Paragraph("Total Descuento").SetFontSize(9)).
                     SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(Border.NO_BORDER);
                _footer.AddCell(_foot);
                _foot = new Cell().Add(new Paragraph(item.TotalDescuento.ToString("₡0,#.00")).SetFontSize(9)).
              SetTextAlignment(TextAlignment.CENTER);
                _footer.AddCell(_foot);
                doc.Add(_footer);
                Table _footer2 = new Table(anchos).UseAllAvailableWidth();
                Cell _foot2 = new Cell().Add(new Paragraph("El precio de los productos en una proforma estará vigente por los próximos 10 días").SetFontSize(9)).
             SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER);
                _footer2.AddCell(_foot2);
                _foot2 = new Cell().Add(new Paragraph("Total Impuesto").SetFontSize(9)).
                     SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(Border.NO_BORDER);
                _footer2.AddCell(_foot2);
                _foot2 = new Cell().Add(new Paragraph(item.TotalImpuesto.ToString("₡0,#.00")).SetFontSize(9)).
              SetTextAlignment(TextAlignment.CENTER);
                _footer2.AddCell(_foot2);
                doc.Add(_footer2);
                Table _footer3 = new Table(anchos).UseAllAvailableWidth();
                Cell _foot3 = new Cell().Add(new Paragraph("después de su creación (" + item.Fecha.ToShortDateString() + ") ,posterior a este tiempo los mismos podrían variar.").SetFontSize(9)).
             SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER);
                _footer3.AddCell(_foot3);
                _foot3 = new Cell().Add(new Paragraph("Total Pagar").SetFontSize(9)).
                     SetTextAlignment(TextAlignment.RIGHT).SetBorderRight(Border.NO_BORDER);
                _footer3.AddCell(_foot3);
                _foot3 = new Cell().Add(new Paragraph(item.TotalPagar.ToString("₡0,#.00")).SetFontSize(9)).
              SetTextAlignment(TextAlignment.CENTER);
                _footer3.AddCell(_foot3);
                doc.Add(_footer3);
            }
            doc.Close();
            byte[] bytesStream = ms.ToArray();
            ms = new MemoryStream();
            ms.Write(bytesStream, 0, bytesStream.Length);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/pdf");
        }

        // GET: Proformas/Create
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult Create()
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View();
        }

        // POST: Proformas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Vendedor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            proformas.Fecha = DateTime.Now;
            proformas.TotalDescuento = 0;
            proformas.TotalImpuesto = 0;
            proformas.TotalPagar = 0;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Proformas.Add(proformas);
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma creada con éxito!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }

        // GET: Proformas/Edit/5
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            ViewBag.Id = proformas.Id;
            ViewBag.TotalPagar = proformas.TotalPagar;
            ViewBag.TotalDescuento = proformas.TotalDescuento;
            ViewBag.TotalImpuesto = proformas.TotalImpuesto;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 2 || x.Rol == 1 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }

        // POST: Proformas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Vendedor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == proformas.Id).ToList();
            foreach (var item in detalles)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proformas.TotalDescuento = desc;
            proformas.TotalImpuesto = imp;
            proformas.TotalPagar = totalPagar;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma actualizada correctamente!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 2 || x.Rol == 1 && x.Estado == true), "Id", "Nombre", proformas.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoProforma, "Id", "Descripcion", proformas.IdEstado);
            return View(proformas);
        }
        #endregion

        #region Metodos de Cliente
        [Authorize(Roles = "Cliente")]
        public ActionResult ListaProformas(string idUser)
        {
            var proformas = db.Proformas.Where(x => x.IdUsuario == idUser && x.IdEstado == 2).OrderByDescending(x => x.Id);
            return View(proformas);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult ProformasRevaloracion(string idUser)
        {
            if (idUser == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", User.Identity.GetUserId());
            }

            var lista = db.Proformas.Where(x => x.IdUsuario == idUser && x.IdEstado == 5).ToList();
            return View(lista);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult SeleccionarDocumentoCliente(int id)
        {
            try
            {
                Proformas proformas = db.Proformas.Find(id);
            }
            catch (Exception)
            {

                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Proforma, NumerosDocumento = id };
            Session["Proforma"] = "Seleccionado";
            return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult CancelarProformaCliente(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", User.Identity.GetUserId());
            }
            Proformas proforma = db.Proformas.Find(id);
            if (proforma == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas", User.Identity.GetUserId());
            }
            proforma.IdEstado = 1;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            Session["Proforma"] = "Cancelada";
            return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
        }

        // GET: Proformas/Edit/5
        [Authorize(Roles = "Cliente")]
        public ActionResult EditProformaCliente(int? id, string idUser)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = idUser });
            }
            var usuario = db.AspNetUsers.Where(x => x.Id == idUser).First();
            if (usuario.Rol != 3)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = idUser });
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = idUser });
            }
            ViewBag.Id = proformas.Id;
            ViewBag.TotalPagar = proformas.TotalPagar;
            ViewBag.TotalDescuento = proformas.TotalDescuento;
            ViewBag.TotalImpuesto = proformas.TotalImpuesto;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == idUser), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }

        // POST: Proformas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProformaCliente([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == proformas.Id).ToList();
            foreach (var item in detalles)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proformas.TotalDescuento = desc;
            proformas.TotalImpuesto = imp;
            proformas.TotalPagar = totalPagar;
            proformas.IdUsuario = proformas.IdCliente;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma actualizada correctamente!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = proformas.IdCliente });
            }
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == proformas.IdCliente), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return RedirectToAction("ListaProformas", "Proformas", new { idUser = proformas.IdCliente });
        }

        public ActionResult EnviarRevaloracion(int id)
        {
            if (id == 0)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }
            var proforma = db.Proformas.Find(id);
            if (proforma == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }
            if (proforma.IdUsuario != User.Identity.GetUserId())
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }

            ViewBag.Id = proforma.Id;
            return View(proforma);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarRevaloracion([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma,Comentario")] Proformas proformas)
        {
            int id = proformas.Id;
            string comentario = proformas.Comentario;
            if (id == 0)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }
            var proforma = db.Proformas.Find(id);
            ViewBag.Id = proforma.Id;
            proforma.IdEstado = 5;
            proforma.Comentario = comentario;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            Session["Proforma"] = "Revaloracion";
            return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
        }

        [HttpPost]
        public ActionResult agregardocumento(int idProforma, HttpPostedFileBase file)
        {
            try
            {
                if (Request.Files.Count != 0)
                {

                    HttpPostedFileBase FileBase = Request.Files[0];
                    Stream fs = FileBase.InputStream;
                    BinaryReader br = new BinaryReader(fs);
                    Byte[] bytes = br.ReadBytes((Int32)fs.Length);

                    string descrip = "";
                    try
                    {
                        descrip = FileBase.FileName.Substring(0, 50);
                    }
                    catch (Exception)
                    {

                        descrip = FileBase.FileName;
                    }

                    Documentos doc = new Documentos()
                    {
                        IdProforma = idProforma,
                        Documento = bytes,
                        Descripcion = descrip
                    };
                    db.Documentos.Add(doc);
                    db.SaveChanges();
                    return PartialView("_ListaDocumentos", idProforma);
                }
                else
                {
                    return PartialView("_ListaDocumentos", idProforma);
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public ActionResult EliminarDocumentos(int id)
        {
            Documentos doc = db.Documentos.Find(id);
            db.Documentos.Remove(doc);
            db.SaveChanges();

            return PartialView("_ListaDocumentos", doc.IdProforma);
        }


        [Authorize(Roles = "Cliente")]
        public ActionResult ProformasCanceladas(string idUser)
        {
            var canceladas = db.Proformas.Where(x => x.IdUsuario == idUser && x.IdEstado == 1).ToList();
            return View(canceladas);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult DuplicarProformaCliente(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", new { idUser = User.Identity.GetUserId() });
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas", new { idUser = proformas.IdCliente });
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Proformas proformaDuplicado = new Proformas
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = proformas.IdEstado,
                Fecha = proformas.Fecha,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombreProforma = proformas.NombreProforma
            };
            db.Proformas.Add(proformaDuplicado);
            db.SaveChanges();
            ProformaDetalle detalleDuplicado = new ProformaDetalle();
            foreach (var item in detalles)
            {
                detalleDuplicado.IdProforma = proformaDuplicado.Id;
                detalleDuplicado.IdProducto = item.IdProducto;
                detalleDuplicado.Cantidad = item.Cantidad;
                detalleDuplicado.PrecioUnitario = item.PrecioUnitario;
                detalleDuplicado.Descuento = item.Descuento;
                db.ProformaDetalle.Add(detalleDuplicado);
                db.SaveChanges();
            }
            Session["Proforma"] = "¡Proforma duplicada correctamente!";
            return RedirectToAction("ListaProformas", new { idUser = proformas.IdCliente });
        }

        /// <summary>
        /// Detalles de Proforma con usuario cliente
        /// </summary>
        /// <param name="id">Id de la Proforma</param>
        /// <param name="idCliente">Id del usuario en session</param>
        /// <returns></returns>
        [Authorize(Roles = "Cliente")]
        public ActionResult DetailsProformaCliente(int? id, string idCliente)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas");
            }
            var cliente = db.AspNetUsers.Where(x => x.Id == idCliente).First();
            if (cliente.Rol != 3)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas");
            }

            Proformas proformas = db.Proformas.Find(id);
            if (proformas != null)
            {
                if (proformas.IdUsuario == idCliente)
                {
                    List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
                    proformas.ProformaDetalle = detalles;
                    ViewBag.Id = proformas.Id;
                }
                else
                {
                    Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                    return RedirectToAction("ListaProformas");
                }
            }
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas");
            }
            return View(proformas);
        }

        // GET: Proformas/Create
        [Authorize(Roles = "Cliente")]
        public ActionResult CreateProfromaCliente(string id)
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == id), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View();
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProfromaCliente([Bind(Include = "IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombreProforma")] Proformas proformas)
        {
            proformas.Fecha = DateTime.Now;
            proformas.TotalDescuento = 0;
            proformas.TotalImpuesto = 0;
            proformas.TotalPagar = 0;
            if (proformas.DireccionEntrega == null)
            {
                proformas.DireccionEntrega = "Retirar en la ferretería";
            }
            if (proformas.NombreProforma == null)
            {
                proformas.NombreProforma = "Proforma #";
            }
            if (ModelState.IsValid)
            {
                db.Proformas.Add(proformas);
                db.SaveChanges();
                Session["Proforma"] = "¡Proforma creada con éxito!";
                if (proformas.NombreProforma == "Proforma #")
                {
                    proformas.NombreProforma += proformas.Id;
                }
                db.Entry(proformas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListaProformas", new { idUser = proformas.IdCliente });
            }
            ViewBag.Id = proformas.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Id == proformas.IdUsuario), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoProforma.Where(x => x.Descripcion.Equals("Nueva")), "Id", "Descripcion");
            return View(proformas);
        }
        #endregion

        [Authorize(Roles = "Cliente")]
        public ActionResult DetallesProformaCanceladaCliente(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ProformasCanceladas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }

            return View(proformas);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult DetallesProformasRevaloracion(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ProformasRevaloracion", "Proformas", new { idUser = User.Identity.GetUserId() });
            }
            return View(proformas);
        }

        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult RestaurarProforma(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return View("Index");
            }

            proformas.IdEstado = 2;
            db.Entry(proformas).State = EntityState.Modified;
            db.SaveChanges();
            Session["Proforma"] = "Restaurada";
            return View("Index");
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult RestaurarProformaCliente(int id)
        {
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ProformasCanceladas", "Proformas", new { idUser = User.Identity.GetUserId() });
            }

            proformas.IdEstado = 2;
            db.Entry(proformas).State = EntityState.Modified;
            db.SaveChanges();
            Session["Proforma"] = "Restaurada";
            return RedirectToAction("ProformasCanceladas", "Proformas", new { idUser = User.Identity.GetUserId() });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public FileResult PDFD(int id)
        {
            var arch = db.Documentos.Where(x => x.Id == id).First();
            return File(arch.Documento, "application/pdf", arch.Descripcion + ".pdf");
        }

        public ActionResult EliminarProdProforma(int? idD, int IdProforma)
        {
            Proformas proforma = db.Proformas.Find(IdProforma);
            ProformaDetalle detalles = db.ProformaDetalle.Find(idD);
            var prod = db.Productos.Where(x => x.Id == detalles.IdProducto);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            db.ProformaDetalle.Remove(detalles);
            db.SaveChanges();
            List<ProformaDetalle> detallesPedido = db.ProformaDetalle.Where(x => x.IdProforma == IdProforma).ToList();
            proforma.Id = proforma.Id;
            proforma.IdUsuario = proforma.IdUsuario;
            proforma.IdEstado = proforma.IdEstado;
            proforma.Fecha = proforma.Fecha;
            foreach (var item in detallesPedido)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proforma.TotalDescuento = desc;
            proforma.TotalImpuesto = imp;
            proforma.TotalPagar = totalPagar;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.TotalPagar = proforma.TotalPagar;
            ViewBag.TotalDescuento = proforma.TotalDescuento;
            ViewBag.TotalImpuesto = proforma.TotalImpuesto;
            proforma.ProformaDetalle = detallesPedido;
            return PartialView("_ListaProformaCarrito", proforma);
        }

        /// <summary>
        /// Cambiar la cantidad en una proforma
        /// </summary>
        /// <returns></returns>
        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {
            int idProforma = Convert.ToInt32(objeto.idProforma);
            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = 0;
            try
            {
                cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);

                if(cantidadCambio < 0)
                {
                    cantidadCambio = 1;
                }
            }
            catch (Exception)
            {
                cantidadCambio = 2000000000;
                if (cantidadCambio < 0)
                {
                    cantidadCambio = 1;
                }
            }

            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            Proformas proforma = db.Proformas.Find(idProforma);
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == idProforma).ToList();
            foreach (var item in detalles)
            {
                if (item.IdProducto == id)
                {
                    ProformaDetalle detalle = db.ProformaDetalle.Find(item.Id);
                    if (cantidadCambio == 0)
                    {
                        proforma.TotalDescuento = desc;
                        proforma.TotalImpuesto = imp;
                        proforma.TotalPagar = totalPagar;
                        db.Entry(proforma).State = EntityState.Modified;
                        db.SaveChanges();
                        proforma.ProformaDetalle = detalles;
                        ViewBag.TotalPagar = proforma.TotalPagar;
                        ViewBag.TotalDescuento = proforma.TotalDescuento;
                        ViewBag.TotalImpuesto = proforma.TotalImpuesto;
                        db.ProformaDetalle.Remove(detalle);
                        db.SaveChanges();
                        return PartialView("_ListaProformaCarrito", proforma);
                    }
                    detalle.Id = item.Id;
                    detalle.Cantidad = cantidadCambio;
                    detalle.IdProforma = item.IdProforma;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();
                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            proforma.TotalDescuento = desc;
            proforma.TotalImpuesto = imp;
            proforma.TotalPagar = totalPagar;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            proforma.ProformaDetalle = detalles;
            ViewBag.TotalPagar = proforma.TotalPagar;
            ViewBag.TotalDescuento = proforma.TotalDescuento;
            ViewBag.TotalImpuesto = proforma.TotalImpuesto;
            return PartialView("_ListaProformaCarrito", proforma);
        }


        public ActionResult CambiarDescuento(AjaxDescuento objet)
        {

            int proId = Convert.ToInt32(objet.proformaId);
            int id = Convert.ToInt32(objet.productoId);
            int descuentoP = Convert.ToInt32(objet.descuento);
            if(descuentoP < 0)
            {
                descuentoP = 0;
            }
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            double total = 0;
            double impuestoVenta = 0;
            double descuentoCant = 0;
            double impuestoC = 0;
            double descCant = 0;
            Proformas prof = db.Proformas.Find(proId);
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == proId).ToList();

            foreach (var item in detalles)
            {
                descuentoCant = (item.PrecioUnitario * item.Cantidad) * (item.Descuento/100);
                descCant += descuentoCant;
                impuestoVenta = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                impuestoC += impuestoVenta;
                total += ((item.PrecioUnitario * item.Cantidad) + impuestoVenta) - descuentoCant;

            }
            foreach (var item in detalles)
            {
                ProformaDetalle detalle = db.ProformaDetalle.Find(item.Id);


                if (item.IdProducto == id)
                {
                   

                    detalle.Id = item.Id;
                    detalle.Cantidad = item.Cantidad;
                    detalle.IdProforma = item.IdProforma;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = descuentoP;
                    
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();


                }
                {
                }
                descuento = (item.PrecioUnitario * item.Cantidad) * (item.Descuento/100);
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;

            }

            prof.TotalDescuento = desc;
            prof.TotalImpuesto = imp;
            prof.TotalPagar = totalPagar;
            db.Entry(prof).State = EntityState.Modified;
            db.SaveChanges();
       

            prof.ProformaDetalle = detalles;
            ViewBag.TotalPagar = prof.TotalPagar;
            ViewBag.TotalDescuento = prof.TotalDescuento;
            ViewBag.TotalImpuesto = prof.TotalImpuesto;
            return PartialView("_ListaProformaCarrito", prof);
        }

        /// <summary>
        /// Agrega un nuevo producto a la proforma
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult AgregarDetalle(AjaxDetalle objeto)
        {
            int idProforma = Convert.ToInt32(objeto.idProforma);
            int nuevoId = Convert.ToInt32(objeto.idProducto);
            int cant = Convert.ToInt32(objeto.cantidad);
            Proformas ped = db.Proformas.Find(idProforma);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;
            if (cant == 0)
            {
                ViewBag.TotalPagar = ped.TotalPagar;
                ViewBag.TotalDescuento = ped.TotalDescuento;
                ViewBag.TotalImpuesto = ped.TotalImpuesto;
                Session["Proforma"] = "Debe digitar una cantidad mayor a 0";
                return PartialView("_ListaProformaCarrito", ped);
            }
            ProformaDetalle validacion = null;
            try
            {
                validacion = db.ProformaDetalle.Where(x => x.IdProforma == idProforma && x.IdProducto == nuevoId).First();
            }
            catch (Exception)
            {
                validacion = null;
            }
            if (validacion != null)
            {
                validacion.Cantidad += cant;
                db.Entry(validacion).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                try
                {
                    ProformaDetalle detalle = new ProformaDetalle
                    {
                        IdProforma = idProforma,
                        IdProducto = nuevoId
                    };
                    detalle.Productos = db.Productos.Where(x => x.Id == detalle.IdProducto).First();
                    detalle.Cantidad = cant;
                    detalle.PrecioUnitario = detalle.Productos.Precio;
                    detalle.Descuento = 0;
                    db.ProformaDetalle.Add(detalle);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    ViewBag.TotalPagar = ped.TotalPagar;
                    ViewBag.TotalDescuento = ped.TotalDescuento;
                    ViewBag.TotalImpuesto = ped.TotalImpuesto;
                    Session["Proforma"] = "Producto es inválido. Digite un código de producto nuevamente";
                    return PartialView("_ListaProformaCarrito", ped);
                }
            }
            List<ProformaDetalle> detallesProforma = db.ProformaDetalle.Where(x => x.IdProforma == idProforma).ToList();
            foreach (var item in detallesProforma)
            {
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }
            ped.TotalDescuento = desc;
            ped.TotalImpuesto = imp;
            ped.TotalPagar = totalPagar;
            db.Entry(ped).State = EntityState.Modified;
            db.SaveChanges();
            ped.ProformaDetalle = detallesProforma;
            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaProformaCarrito", ped);
        }

        /// <summary>
        /// Filtra por nombre de proforma 
        /// </summary>
        /// <returns></returns>
        public ActionResult filtrarProformasAjax(Ajax objeto)
        {
            string terminoBusqueda = Convert.ToString(objeto.terminoBusqueda);
            int idProforma = Convert.ToInt32(objeto.idProforma);

            List<Proformas> proformas = new List<Proformas>();
            List<Proformas> proformasFiltradasNombre = new List<Proformas>();
            List<Proformas> proformasFiltradaId = new List<Proformas>();
            proformas = db.Proformas.Where(x => x.AspNetUsers.Rol == 2 && x.IdEstado == 2 || x.AspNetUsers.Rol == 1 && x.IdEstado == 2).ToList();

            if (terminoBusqueda != null)
            {
                foreach (var item in proformas)
                {
                    if (item.NombreProforma.ToLower().Contains(terminoBusqueda.ToLower()))
                    {
                        proformasFiltradasNombre.Add(item);
                    }
                }
                proformas = proformasFiltradasNombre;
            }


            if (idProforma != 0)
            {
                foreach (var item in proformas)
                {
                    if (item.Id == idProforma)
                    {
                        proformasFiltradaId = new List<Proformas>
                        {
                            item
                        };
                        break;
                    }
                }
                proformas = proformasFiltradaId;
            }

            return PartialView("_ListaProformas", proformas);
        }

        /// <summary>
        /// Busqueda avanzada de Usuario para nueva proforma
        /// </summary>
        /// <param name="cedula">Identicación del usuario</param>
        /// <returns></returns>
        public ActionResult filtrarCliente(string cedula)
        {
            try
            {
                int ced = Convert.ToInt32(cedula);
                AspNetUsers usuario = db.AspNetUsers.Where(x => x.Cedula == ced && x.Rol == 3).First();
                return PartialView("_Cliente", usuario);
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Filtra por Numero Proforma
        /// </summary>
        /// <returns></returns>
        public ActionResult filtrarNumeroProforma(string numeroProforma)
        {


            if (numeroProforma != null)
            {
                int id = Convert.ToInt32(numeroProforma);
                var lista = db.Proformas.Where(x => x.Id == id);
                return PartialView("_ListaProformas", lista.ToList().Where(x => x.AspNetUsers.Rol == 2 || x.AspNetUsers.Rol == 1));
            }


            return View();
        }


        /// <summary>
        /// Retorna la vista para clientes no registrados creando una nueva proforma
        /// </summary>
        /// <returns></returns>
        public ActionResult ClienteSinCuenta()
        {
            return PartialView("_ClienteSinCuenta");
        }

        /// <summary>
        /// Convierte una proforma en un pedido
        /// </summary>
        /// <returns></returns>

        public ActionResult ConvertiraPedido(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("Index");
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("Index");
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Pedidos pedido = new Pedidos
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = 7,
                Fecha = DateTime.Now,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombrePedido = proformas.NombreProforma,
                NumeroProforma = proformas.Id
            };
            db.Pedidos.Add(pedido);
            db.SaveChanges();
            PedidoDetalle pedidoDetalle = new PedidoDetalle();
            foreach (var item in detalles)
            {
                pedidoDetalle.IdPedido = pedido.Id;
                pedidoDetalle.IdProducto = item.IdProducto;
                pedidoDetalle.Cantidad = item.Cantidad;
                pedidoDetalle.PrecioUnitario = item.PrecioUnitario;
                pedidoDetalle.Descuento = item.Descuento;
                db.PedidoDetalle.Add(pedidoDetalle);
                db.SaveChanges();
            }
            Session["Proforma"] = "¡Proforma convertida en Pedido exitosamente!";
            Session["NumPedido"] = pedido.Id;

            proformas.IdEstado = 4;
            db.Entry(proformas).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult ConvertiraPedidoCliente(int? id)
        {
            if (id == null)
            {
                Session["Proforma"] = "Proforma inválida. Especifique una proforma";
                return RedirectToAction("ListaProformas", "Profromas", new { idUser = User.Identity.GetUserId() });
            }
            Proformas proformas = db.Proformas.Find(id);
            if (proformas == null)
            {
                Session["Proforma"] = "No existe la proforma";
                return RedirectToAction("ListaProformas", "Profromas", new { idUser = User.Identity.GetUserId() });
            }
            List<ProformaDetalle> detalles = db.ProformaDetalle.Where(x => x.IdProforma == id).ToList();
            proformas.ProformaDetalle = detalles;
            Pedidos pedido = new Pedidos
            {
                IdUsuario = proformas.IdUsuario,
                IdEstado = 7,
                Fecha = DateTime.Now,
                TotalPagar = proformas.TotalPagar,
                TotalDescuento = proformas.TotalDescuento,
                TotalImpuesto = proformas.TotalImpuesto,
                IdCliente = proformas.IdCliente,
                NombreCliente = proformas.NombreCliente,
                DireccionEntrega = proformas.DireccionEntrega,
                NombrePedido = proformas.NombreProforma,
                NumeroProforma = proformas.Id
            };
            db.Pedidos.Add(pedido);
            db.SaveChanges();
            PedidoDetalle pedidoDetalle = new PedidoDetalle();
            foreach (var item in detalles)
            {
                pedidoDetalle.CantidadEnviada = 0;
                pedidoDetalle.IdPedido = pedido.Id;
                pedidoDetalle.IdProducto = item.IdProducto;
                pedidoDetalle.Cantidad = item.Cantidad;
                pedidoDetalle.PrecioUnitario = item.PrecioUnitario;
                pedidoDetalle.Descuento = item.Descuento;
                db.PedidoDetalle.Add(pedidoDetalle);
                db.SaveChanges();
            }
            
            Session["NumPedido"] = pedido.Id;

            proformas.IdEstado = 4;
            db.Entry(proformas).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ListaProformas", "Proformas", new { idUser = User.Identity.GetUserId() });
        }

        public class Ajax
        {
            public string terminoBusqueda
            {
                get;
                set;
            }
            public string id
            {
                get;
                set;
            }

            public string idProforma
            {
                get;
                set;
            }
        }

        public class AjaxDescuento
        {
            public string descuento
            {
                get;
                set;
            }
            public string productoId
            {
                get;
                set;
            }

            public string proformaId
            {
                get;
                set;
            }
        }
        public class AjaxDetalle
        {
            public string cantidad
            {
                get;
                set;
            }
            public string idProforma
            {
                get;
                set;
            }

            public string idProducto
            {
                get;
                set;
            }
        }

        public class AjaxFiltro
        {
            public string numeroPedido
            {
                get;
                set;
            }
        }

        [Authorize(Roles = "Admin,Vendedor")]
        public ActionResult reporteProformaAPedido(string fechaAnterior, string fechaActual)
        {
            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Proforma"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Proforma"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Proforma"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Proforma"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {
                                var querys = from p in db.Proformas
                                             join e in db.EstadoProforma on p.IdEstado equals e.Id
                                             where p.Fecha >= anterior && p.Fecha <= actual
                                             group new { p.IdEstado, p.TotalPagar, p.TotalImpuesto, p.TotalDescuento }
                                             by new { e.Descripcion, e.Id, p.IdEstado }
                                             into g
                                             orderby g.Sum(x => x.TotalPagar)

                                             select new
                                             {
                                                 TotalPagar = g.Sum(x => x.TotalPagar),
                                                 TotalDescuento = g.Sum(x => x.TotalDescuento),
                                                 TotalImpuesto = g.Sum(x => x.TotalImpuesto),
                                                 Cantidad = g.Count(),
                                                 Estado = g.Key.Descripcion,
                                                 Porcentaje = g.Count() * 100 / db.Proformas.Count()
                                             };
                                ViewBag.ReportViewer = Reporte.reporte(querys.ToList(), "", "ReporteProformaAPedido.rdlc");
                                return PartialView("_reporteProformaAPedido", querys.ToList());
                            }
                        }
                    }
                }
                if (TempData.ContainsKey("mensajeReporte"))
                {

                    return PartialView("_aviso");

                }

            }

            return View();
        }

        public class HeaderEventHandler : IEventHandler
        {
            Image Img;
            public HeaderEventHandler(Image img)
            {
                Img = img;
            }

            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                Rectangle rootArea = new iText.Kernel.Geom.Rectangle(35, page.GetPageSize().GetTop() - 130, page.GetPageSize().GetRight() - 70, 100);
                iText.Kernel.Pdf.Canvas.PdfCanvas canvas1 = new iText.Kernel.Pdf.Canvas.PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                new Canvas(canvas1, pdfDoc, rootArea)
                    .Add(getTable(docEvent));
            }


            public Table getTable(PdfDocumentEvent docEvent)
            {

                Table tableEvent = new Table(2).UseAllAvailableWidth().SetHeight(600);

                Style styleCell = new Style()
                    .SetBorder(Border.NO_BORDER);

                Style styleText = new Style()
                    .SetTextAlignment(TextAlignment.LEFT);

                PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                Cell cell = new Cell()
                    .Add(new Paragraph("Ferretería y Materiales JYR S.A\n").SetFont(bold).SetFontSize(12))
                    .Add(new Paragraph("Guapalupe de Alajuela. 300 metros norte\n").SetFont(bold).SetFontSize(8))
                    .Add(new Paragraph("de la iglesia Santa Eduviges,\n").SetFont(bold).SetFontSize(8))
                     .Add(new Paragraph("carretera a Carrizal\n").SetFont(bold).SetFontSize(8))
                     .Add(new Paragraph("Tel: 2430-1131 / 2430-6876\n").SetFont(bold).SetFontSize(8))
                        .Add(new Paragraph("Cédula Jurídica: 3-101-659718\n").SetFont(bold).SetFontSize(8))
                           .Add(new Paragraph("Email: ferreteriaymaterialesjyr@gmail.com\n").SetFont(bold).SetFontSize(8))
                    .AddStyle(styleText).AddStyle(styleCell)
                    .SetBorder(Border.NO_BORDER).SetWidth(450);


                tableEvent.AddCell(cell);

                cell = new Cell().Add(Img.SetAutoScale(true)).SetBorder(Border.NO_BORDER)
                     .SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetTextAlignment(TextAlignment.RIGHT);

                tableEvent.AddCell(cell).
                    SetTextAlignment(TextAlignment.RIGHT);

                return tableEvent;
            }

        }

        public class FooterEventHandler : IEventHandler
        {
            public void HandleEvent(Event @event)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                Rectangle rootArea = new iText.Kernel.Geom.Rectangle(32, 20, page.GetPageSize().GetWidth() - 70, 50);
                iText.Kernel.Pdf.Canvas.PdfCanvas canvas1 = new iText.Kernel.Pdf.Canvas.PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
                new Canvas(canvas1, pdfDoc, rootArea)
                    .Add(getTable(docEvent));
            }

            public Table getTable(PdfDocumentEvent docEvent)
            {
                float[] cellWidth = { 92f, 8f };
                Table tableEvent = new Table(UnitValue.CreatePercentArray(cellWidth)).UseAllAvailableWidth();

                PdfPage page = docEvent.GetPage();
                int pageNum = docEvent.GetDocument().GetPageNumber(page);
                int pageAll = docEvent.GetDocument().GetNumberOfPages();

                Style styleCell = new Style()
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(5)
                    .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 2));

                Style styleText = new Style()
                    .SetTextAlignment(TextAlignment.RIGHT);

                PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                Cell cell = new Cell()
                    .Add(new Paragraph("Página " + pageNum + " de " + pageAll).SetFont(bold).SetFontSize(8))
                    .Add(new Paragraph("Proforma emitida el: " + DateTime.Now).SetFont(bold).SetFontSize(8))
                    .AddStyle(styleText).AddStyle(styleCell)
                    .SetBorder(Border.NO_BORDER).SetWidth(450);


                tableEvent.AddCell(cell);

                return tableEvent;
            }

            public class Ajax
            {
                public string TerminoBusqueda
                {
                    get;
                    set;
                }
                public string IdProforma
                {
                    get;
                    set;
                }
            }
        }
    }
}
