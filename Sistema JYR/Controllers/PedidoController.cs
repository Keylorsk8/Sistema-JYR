using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml.Schema;
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
using Microsoft.AspNet.Identity;
using Sistema_JYR.Models;
using Sistema_JYR.Models.Session;

namespace Sistema_JYR.Controllers
{
    public class PedidoController : Controller
    {
        private readonly SistemaJYREntities db = new SistemaJYREntities();

        // GET: Pedido

        public ActionResult Index()
        {

            var pedidos = db.Pedidos.Where(x => x.IdEstado == 7).ToList();
            return View(pedidos.ToList());
        }

        // GET: Pedido/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";

                return RedirectToAction("Index");
            }

            Pedidos pedido = db.Pedidos.Find(id);
            if (pedido != null)
            {
                List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
                pedido.PedidoDetalle = detalles;
                ViewBag.Id = pedido.Id;
            }


            if (pedido == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }


            return View(pedido);
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

            List<Pedidos> model = db.Pedidos.Where(x => x.Id == id).ToList();

            foreach (var item in model)
            {
                List<AspNetUsers> user = db.AspNetUsers.Where(x => x.Id == item.IdCliente).ToList();
                List<Telefonos> tel = db.Telefonos.Where(x => x.IdUsuario == item.IdCliente).ToList();
                Table enc = new Table(4).UseAllAvailableWidth();
                Cell cellenc = new Cell().Add(new Paragraph("Pedido No." + id).SetFontSize(11)).
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
                Cell cellenc2 = new Cell().Add(new Paragraph(item.NombrePedido).SetFontSize(9)).
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
                       SetTextAlignment(TextAlignment.LEFT).SetBorderBottom(Border.NO_BORDER).SetWidth(114).SetBorderLeft(Border.NO_BORDER);
                _enc.AddCell(_cellDet);
                _det.AddHeaderCell(_cellDet);


                doc.Add(_det);

                Table _det2 = new Table(2).UseAllAvailableWidth();
                Cell _cellDet2 = new Cell(2, 1).Add(new Paragraph(item.DireccionEntrega).SetFontSize(9)).
                     SetTextAlignment(TextAlignment.LEFT).SetBorderTop(Border.NO_BORDER)
                     .SetBorderBottom(Border.NO_BORDER).SetWidth(100).SetBorderRight(Border.NO_BORDER);
                _enc.AddCell(_cellDet2);
                _det2.AddHeaderCell(_cellDet2);

                _cellDet2 = new Cell(2, 1).Add(new Paragraph("Estado:" + item.EstadoPedido.Descripcion).SetFontSize(9)).
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


            Table _table = new Table(6).UseAllAvailableWidth();
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
            _cell = new Cell(2, 1).Add(new Paragraph("Cantidad Enviada"));
            _table.AddHeaderCell(_cell.AddStyle(styleCell));


            Style sty = new Style()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(9);

            List<PedidoDetalle> det = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
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
                    _cell = new Cell().Add(new Paragraph(item.Descuento.ToString("₡0,#.00"))).SetBorderLeft(Border.NO_BORDER).
                        SetBorderTop(Border.NO_BORDER).SetBorderBottom(Border.NO_BORDER).SetBorderRight(Border.NO_BORDER);
                    _table.AddCell(_cell.AddStyle(sty));
                    _cell = new Cell().Add(new Paragraph(item.CantidadEnviada.ToString())).SetBorderLeft(Border.NO_BORDER).
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
                Cell _foot2 = new Cell(2, 1).Add(new Paragraph("La devolución de inventario debe realizarse con la factura,").SetFontSize(9)).
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
                Cell _foot3 = new Cell().Add(new Paragraph("se contará con 15 días de plazo a partir de su compra (" + item.Fecha.ToShortDateString() + ").").SetFontSize(9)).
             SetTextAlignment(TextAlignment.LEFT).SetWidth(187).SetBorderTop(Border.NO_BORDER);
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







        // GET: Pedido/Create
        public ActionResult Create()
        {
            ViewBag.Fecha = DateTime.Now.ToShortDateString();
            ViewBag.NumeroProforma = "N/A";
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre");
            ViewBag.IdEstado = new SelectList(db.EstadoPedido.Where(x => x.Descripcion.Equals("Nuevo")), "Id", "Descripcion");
            return View();
        }

        // POST: Pedido/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,IdCliente,NombreCliente,DireccionEntrega,NombrePedido,NumeroProforma")] Pedidos pedidos)
        {
            pedidos.Fecha = DateTime.Now;
            pedidos.TotalDescuento = 0;
            pedidos.TotalImpuesto = 0;
            pedidos.TotalPagar = 0;
            if (pedidos.DireccionEntrega == null)
            {
                pedidos.DireccionEntrega = "Retirar en la ferretería";
            }
            if (pedidos.NombrePedido == null)
            {
                pedidos.NombrePedido = "Pedido #";
            }
            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedidos);
                db.SaveChanges();
                Session["Pedido"] = "¡Pedido creado con éxito!";
                if (pedidos.NombrePedido == "Pedido #")
                {
                    pedidos.NombrePedido += pedidos.Id;
                }
                db.Entry(pedidos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido.Where(x => x.Descripcion.Equals("Nuevo")), "Id", "Descripcion");
            return View(pedidos);
        }



        // GET: Pedido/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";
                return RedirectToAction("Index");
            }

            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }



            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            ViewBag.NumeroProforma = pedidos.NumeroProforma;
            ViewBag.Id = pedidos.Id;
            ViewBag.Fecha = pedidos.Fecha.ToShortDateString();
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedidos.PedidoDetalle = detalles;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        // POST: Pedido/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdUsuario,IdEstado,Fecha,TotalPagar,TotalDescuento,TotalImpuesto,NumeroProforma,IdCliente,NombreCliente,DireccionEntrega,NombrePedido")] Pedidos pedidos)
        {

            double descuento = 0;
            double desc = 0;
            double impuesto = 0;
            double imp = 0;
            double totalPagar = 0;
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidos.Id).ToList();

            foreach (var item in detalles)
            {

                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;

            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;

            if (ModelState.IsValid)
            {

                db.Entry(pedidos).State = EntityState.Modified;

                db.SaveChanges();
                Session["Pedido"] = "¡Pedido actualizado correctamente!";
                return RedirectToAction("Index");
            }

            ViewBag.Id = pedidos.Id;
            ViewBag.IdUsuario = new SelectList(db.AspNetUsers.Where(x => x.Rol == 1 || x.Rol == 2 && x.Estado == true), "Id", "Nombre", pedidos.IdUsuario);
            ViewBag.IdEstado = new SelectList(db.EstadoPedido, "Id", "Descripcion", pedidos.IdEstado);
            return View(pedidos);
        }

        public ActionResult SeleccionarDocumento(int id)
        {
            try
            {
                Pedidos pedido = db.Pedidos.Find(id);
            }
            catch (Exception)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Pedido, NumerosDocumento = id };
            Session["Pedido"] = "Seleccionado";
            return RedirectToAction("Index");
        }

        public ActionResult CancelarPedido(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un Pedido";
                return RedirectToAction("Index");
            }
            Pedidos pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }
            pedido.IdEstado = 4;
            db.Entry(pedido).State = EntityState.Modified;
            db.SaveChanges();
            Session["Pedido"] = "Cancelado";
            return RedirectToAction("Index");
        }

        public ActionResult SeleccionarDocumentoCliente(int id)
        {
            try
            {
                Pedidos pedido = db.Pedidos.Find(id);
            }
            catch (Exception)
            {

                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("ListaPedidos",User.Identity.GetUserId());
            }
            Session["Documento"] = new Documento() { TipoDocumento = TipoDocumento.Proforma, NumerosDocumento = id };
            Session["Pedido"] = "Seleccionado";
            return RedirectToAction("ListaPedidos", HttpContext.User.Identity.GetUserId());
        }

        public ActionResult CancelarPedidoCliente(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";
                return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
            }
            Proformas proforma = db.Proformas.Find(id);
            if (proforma == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
            }
            proforma.IdEstado = 4;
            db.Entry(proforma).State = EntityState.Modified;
            db.SaveChanges();
            Session["Pedido"] = "Cancelado";
            return RedirectToAction("ListaPedidos", User.Identity.GetUserId());
        }



        public ActionResult DuplicarPedido(int? id)
        {
            if (id == null)
            {
                Session["Pedido"] = "Pedido inválido. Especifique un pedido";
                return RedirectToAction("Index");
            }

            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                Session["Pedido"] = "No existe el pedido";
                return RedirectToAction("Index");
            }

            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == id).ToList();
            pedidos.PedidoDetalle = detalles;


            Pedidos pedidoDuplicado = new Pedidos
            {
                IdUsuario = pedidos.IdUsuario,
                IdEstado = pedidos.IdEstado,
                Fecha = pedidos.Fecha,
                TotalPagar = pedidos.TotalPagar,
                TotalDescuento = pedidos.TotalDescuento,
                TotalImpuesto = pedidos.TotalImpuesto,
                NumeroProforma = pedidos.NumeroProforma,
                IdCliente = pedidos.IdCliente,
                NombreCliente = pedidos.NombreCliente,
                NombrePedido = pedidos.NombrePedido,
                DireccionEntrega = pedidos.DireccionEntrega 
            };
            db.Pedidos.Add(pedidoDuplicado);
            db.SaveChanges();

            PedidoDetalle detalleDuplicado = new PedidoDetalle();

            foreach (var item in detalles)
            {
                detalleDuplicado.IdPedido = pedidoDuplicado.Id;
                detalleDuplicado.IdProducto = item.IdProducto;
                detalleDuplicado.Cantidad = item.Cantidad;
                detalleDuplicado.PrecioUnitario = item.PrecioUnitario;
                detalleDuplicado.Descuento = item.Descuento;
                detalleDuplicado.CantidadEnviada = item.CantidadEnviada;
                db.PedidoDetalle.Add(detalleDuplicado);
                db.SaveChanges();
                Session["Pedido"] = "¡Pedido duplicado correctamente!";
            }

            return RedirectToAction("Index");
        }

        // GET: Pedido/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedidos pedidos = db.Pedidos.Find(id);
            if (pedidos == null)
            {
                return HttpNotFound();
            }
            return View(pedidos);
        }

        // POST: Pedido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pedidos pedidos = db.Pedidos.Find(id);
            db.Pedidos.Remove(pedidos);
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

        public ActionResult FiltrarPedidosAjax(string terminoBusqueda)
        {
            if (terminoBusqueda != null)
            {
                var lista = db.Pedidos.Where(x => x.NombrePedido.Contains(terminoBusqueda));
                return PartialView("_ListaPedidos", lista.ToList());
            }


            return View();
        }

        public ActionResult filtrarNumeroPedido(string numeroPedido)
        {
            

            if (numeroPedido != null)
            {
                int id = Convert.ToInt32(numeroPedido);
                var lista = db.Pedidos.Where(x => x.Id == id);
                return PartialView("_ListaPedidos", lista.ToList());
            }


            return View();
        }

        

        public ActionResult EliminarCarrito(int? idD, int IdPedido)
        {
            Pedidos pedidos = db.Pedidos.Find(IdPedido);
            PedidoDetalle detalles = db.PedidoDetalle.Find(idD);
            var prod = db.Productos.Where(x => x.Id == detalles.IdProducto);
            double totalPagar = 0;
            double impuesto = 0;
            double descuento = 0;
            double desc = 0;
            double imp = 0;

            db.PedidoDetalle.Remove(detalles);
            db.SaveChanges();


            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == IdPedido).ToList();

            pedidos.Id = pedidos.Id;
            pedidos.IdUsuario = pedidos.IdUsuario;
            pedidos.IdEstado = pedidos.IdEstado;
            pedidos.Fecha = pedidos.Fecha;

            foreach (var item in detallesPedido)
            {

                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;


            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            pedidos.PedidoDetalle = detallesPedido;
            return PartialView("_ListaPedidoCarrito", pedidos);
        }



        public ActionResult CambiarCantidadEnviada(AjaxCambio objet)
        {

            int pedidoId = Convert.ToInt32(objet.pedidoId);
            int id = Convert.ToInt32(objet.productoId);
            int cantidad = Convert.ToInt32(objet.cantidadEnviada);
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
            Pedidos ped = db.Pedidos.Find(pedidoId);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == pedidoId).ToList();

            foreach (var item in detalles)
            {
                descuentoCant = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                descCant += descuentoCant;
                impuestoVenta = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                impuestoC += impuestoVenta;
                total += ((item.PrecioUnitario * item.Cantidad) + impuestoVenta) - descuentoCant;

            }
            foreach (var item in detalles)
            {
                PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);


                if (item.IdProducto == id)
                {
                    if (cantidad > item.Cantidad)
                    {

                        ViewBag.TotalPagar = total;
                        ViewBag.TotalDescuento = descCant;
                        ViewBag.TotalImpuesto = impuestoC;
                        Session["Pedido"] = "Debe digitar una cantidad menor a la cantidad";
                        return PartialView("_ListaPedidoCarrito", ped);
                    }

                    detalle.Id = item.Id;
                    detalle.Cantidad = item.Cantidad;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = cantidad;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();


                }
                {
                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;

            }

            ped.PedidoDetalle = detalles;
            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", ped);
        }


        public ActionResult CambiarCantidadPedida(Ajax objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int id = Convert.ToInt32(objeto.id);
            int cantidadCambio = Convert.ToInt32(objeto.terminoBusqueda);

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
            Pedidos pedidos = db.Pedidos.Find(idPedido);
            List<PedidoDetalle> detalles = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();

            foreach (var item in detalles)
            {
                descuentoCant = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                descCant += descuentoCant;
                impuestoVenta = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                impuestoC += impuestoVenta;
                total += ((item.PrecioUnitario * item.Cantidad) + impuestoVenta) - descuentoCant;
            }
            foreach (var item in detalles)
            {

                if (item.IdProducto == id)
                {
                    PedidoDetalle detalle = db.PedidoDetalle.Find(item.Id);

                    if (cantidadCambio == 0)
                    {
                        pedidos.TotalDescuento = desc;
                        pedidos.TotalImpuesto = imp;
                        pedidos.TotalPagar = totalPagar;
                        db.Entry(pedidos).State = EntityState.Modified;
                        db.SaveChanges();
                        pedidos.PedidoDetalle = detalles;
                        ViewBag.TotalPagar = pedidos.TotalPagar;
                        ViewBag.TotalDescuento = pedidos.TotalDescuento;
                        ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
                        db.PedidoDetalle.Remove(detalle);
                        db.SaveChanges();
                        return PartialView("_ListaPedidoCarrito", pedidos);
                    }

                    if (cantidadCambio < item.CantidadEnviada)
                    {

                        ViewBag.TotalPagar = total;
                        ViewBag.TotalDescuento = descCant;
                        ViewBag.TotalImpuesto = impuestoC;
                        Session["Pedido"] = "Debe digitar una cantidad mayor a la cantidad enviada";

                        return PartialView("_ListaPedidoCarrito", pedidos);
                    }

                    detalle.Id = item.Id;
                    detalle.Cantidad = cantidadCambio;
                    detalle.IdPedido = item.IdPedido;
                    detalle.IdProducto = item.IdProducto;
                    detalle.PrecioUnitario = item.PrecioUnitario;
                    detalle.Descuento = item.Descuento;
                    detalle.CantidadEnviada = item.CantidadEnviada;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();

                }
                descuento = (item.PrecioUnitario * item.Cantidad) * item.Descuento;
                desc += descuento;
                impuesto = (item.PrecioUnitario * item.Cantidad) * (double)item.Productos.Impuesto / 100;
                imp += impuesto;
                totalPagar += ((item.PrecioUnitario * item.Cantidad) + impuesto) - descuento;
            }

            pedidos.TotalDescuento = desc;
            pedidos.TotalImpuesto = imp;
            pedidos.TotalPagar = totalPagar;
            db.Entry(pedidos).State = EntityState.Modified;
            db.SaveChanges();
            pedidos.PedidoDetalle = detalles;

            ViewBag.TotalPagar = pedidos.TotalPagar;
            ViewBag.TotalDescuento = pedidos.TotalDescuento;
            ViewBag.TotalImpuesto = pedidos.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", pedidos);
        }


        public ActionResult AgregarDetalle(AjaxDetalle objeto)
        {
            int idPedido = Convert.ToInt32(objeto.idPedido);
            int nuevoId = Convert.ToInt32(objeto.idProducto);
            int cant = Convert.ToInt32(objeto.cantidad);



            Pedidos ped = db.Pedidos.Find(idPedido);
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
                Session["Pedido"] = "Debe digitar una cantidad mayor a 0";
                return PartialView("_ListaPedidoCarrito", ped);
            }

            PedidoDetalle validacion = null;

            try
            {
                validacion = db.PedidoDetalle.Where(x => x.IdPedido == idPedido && x.IdProducto == nuevoId).First();
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

                    PedidoDetalle detalle = new PedidoDetalle
                    {
                        IdPedido = idPedido,
                        IdProducto = nuevoId
                    };
                    detalle.Productos = db.Productos.Where(x => x.Id == detalle.IdProducto).First();
                    detalle.Cantidad = cant;
                    detalle.CantidadEnviada = 0;
                    detalle.PrecioUnitario = detalle.Productos.Precio;
                    detalle.Descuento = 0;
                    db.PedidoDetalle.Add(detalle);
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    ViewBag.TotalPagar = ped.TotalPagar;
                    ViewBag.TotalDescuento = ped.TotalDescuento;
                    ViewBag.TotalImpuesto = ped.TotalImpuesto;
                    Session["Pedido"] = "Producto es inválido. Digite un código de producto nuevamente";
                    return PartialView("_ListaPedidoCarrito", ped);

                }

            }

            List<PedidoDetalle> detallesPedido = db.PedidoDetalle.Where(x => x.IdPedido == idPedido).ToList();
            ped.PedidoDetalle = detallesPedido;

            foreach (var item in detallesPedido)
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

            ViewBag.TotalPagar = ped.TotalPagar;
            ViewBag.TotalDescuento = ped.TotalDescuento;
            ViewBag.TotalImpuesto = ped.TotalImpuesto;
            return PartialView("_ListaPedidoCarrito", ped);

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

            public string idPedido
            {
                get;
                set;
            }


        }

        public class AjaxCambio
        {
            public string productoId
            {
                get;
                set;
            }
            public string cantidadEnviada
            {
                get;
                set;
            }

            public string pedidoId
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
            public string idPedido
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

        public ActionResult reporteTopProducto(string fechaAnterior, string fechaActual, string IdCategoria)
        {
            var list = db.CategoriasProducto.ToList();
            CategoriasProducto cat = new CategoriasProducto();
            cat.Descripcion = "Todas";
            cat.Id = 90000;
            list.Insert(0, cat);
            SelectList listaCat = new SelectList(list, "Id", "Descripcion");
            ViewBag.IdCategoria = listaCat;


            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Pedido"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Pedido"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Pedido"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Pedido"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {
                                int categoria = Convert.ToInt32(IdCategoria);
                                if (categoria == 90000)
                                {
                                    var query = from d in db.PedidoDetalle
                                                join p in db.Productos on d.IdProducto equals p.Id
                                                join c in db.CategoriasProducto on p.IdCategoria equals c.Id
                                                join o in db.Pedidos on d.IdPedido equals o.Id
                                                where o.Fecha >= anterior && o.Fecha <= actual
                                                group new { d.Cantidad, d.PrecioUnitario }
                                                by new { p.Id, p.Nombre, c.Descripcion }
                                                into g
                                                 orderby g.Sum(x=> x.PrecioUnitario * x.Cantidad) descending

                                                select new
                                                {
                                                    Id = g.Key.Id,
                                                    Producto = g.Key.Nombre,
                                                    Categoria = g.Key.Descripcion,
                                                    cantidadTotal = g.Sum(x => x.Cantidad),
                                                    Total = g.Sum(x => x.PrecioUnitario * x.Cantidad)

                                                };

                                    ViewBag.ReportViewer = Reporte.reporte(query.ToList(), "", "ReporteTopProducto.rdlc");
                                    return PartialView("_reporteTopProducto", query.ToList());
                                }

                                var querys = from d in db.PedidoDetalle
                                            join p in db.Productos on d.IdProducto equals p.Id
                                            join c in db.CategoriasProducto on p.IdCategoria equals c.Id
                                            join o in db.Pedidos on d.IdPedido equals o.Id
                                             where o.Fecha >= anterior && o.Fecha <= actual && p.IdCategoria == categoria
                                             group new { d.Cantidad, d.PrecioUnitario }
                                            by new { p.Id, p.Nombre, c.Descripcion }
                                               into g
                                            orderby g.Sum(x => x.PrecioUnitario * x.Cantidad) descending

                                            select new
                                            {
                                                Id = g.Key.Id,
                                                Producto = g.Key.Nombre,
                                                Categoria = g.Key.Descripcion,
                                                cantidadTotal = g.Sum(x => x.Cantidad),
                                                Total = g.Sum(x => x.PrecioUnitario * x.Cantidad)

                                            };



                                ViewBag.ReportViewer = Reporte.reporte(querys.ToList(), "", "ReporteTopProducto.rdlc");



                                return PartialView("_reporteTopProducto", querys.ToList());
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

        public ActionResult reporteVentasPorEmpleado(string fechaAnterior, string fechaActual)
        {
            if (fechaAnterior != null && fechaActual != null)
            {
                DateTime anterior = Convert.ToDateTime(fechaAnterior);
                DateTime actual = Convert.ToDateTime(fechaActual);

                if (anterior.Year > actual.Year)
                {
                    Session["Pedido"] = "Seleccione una fecha válida";
                    TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                }
                else
                {
                    if (anterior.Year == actual.Year && anterior.Month > actual.Month)
                    {
                        Session["Pedido"] = "Seleccione una fecha válida";
                        TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                    }
                    else
                    {
                        if (anterior.Year == actual.Year && anterior.Month == actual.Month && anterior.Day > actual.Day)
                        {
                            Session["Pedido"] = "Seleccione una fecha válida";
                            TempData["mensajeReporte"] = "Seleccione una fecha inválida";

                        }

                        else
                        {
                            if (actual > DateTime.Now)
                            {
                                Session["Pedido"] = "Seleccione una fecha válida";
                                TempData["mensajeReporte"] = "Seleccione una fecha inválida";
                            }

                            else
                            {

                                var query = from u in db.AspNetUsers
                                            join p in db.Pedidos on u.Id equals p.IdUsuario
                                            where u.Rol == 1 || u.Rol == 2 && p.Fecha >= anterior && p.Fecha <= actual
                                            group new { p.TotalPagar, u.Id }
                                            by new { u.Nombre, u.Apellido1, u.Apellido2, u.Cedula, u.Email }
                                           into g
                                            orderby g.Sum(x => x.TotalPagar) descending

                                            select new
                                            {
                                                g.Key.Cedula,
                                                g.Key.Nombre,
                                                g.Key.Apellido1,
                                                g.Key.Apellido2,
                                                g.Key.Email,
                                                Producto = g.Key.Nombre,
                                                CantidadPedidos = g.Count(),
                                                Acumulado = g.Sum(x => x.TotalPagar)
                                            };

                                ViewBag.ReportViewer = Reporte.reporte(query.ToList(), "", "ReporteUsuario.rdlc");

                                return PartialView("_ReporteUsuario", query.ToList());
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
                    .Add(new Paragraph("Pedido emitido el: " + DateTime.Now).SetFont(bold).SetFontSize(8))
                    .AddStyle(styleText).AddStyle(styleCell)
                    .SetBorder(Border.NO_BORDER).SetWidth(450);


                tableEvent.AddCell(cell);

                return tableEvent;
            }
        }
    }
}
