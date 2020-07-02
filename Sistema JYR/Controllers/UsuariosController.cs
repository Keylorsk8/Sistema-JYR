using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data;
using System.Data.Entity;
using System.Net;
using Sistema_JYR.Models;
using Microsoft.Owin.Security.Provider;

namespace Sistema_JYR.Controllers
{
    public class UsuariosController : Controller
    {
        private SistemaJYREntities db = new SistemaJYREntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Usuarios
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.AspNetUsers.Where(x => x.Rol == 2).ToList());
        }

        // GET: Usuarios/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        // GET: Usuarios/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        // [Bind(Include = "Id,Nombre,Apellido1,Apellido2,Cedula,Rol,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AspNetUsers model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Cedula = model.Cedula, Nombre = model.Nombre, Apellido1 = model.Apellido1, Apellido2 = model.Apellido2, Rol = 2, Estado = true };
                var result = await UserManager.CreateAsync(user, model.PasswordHash);
                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user.Id, "Vendedor");

                    // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar correo electrónico con este vínculo
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                    Session["Vendedor"] = "¡Vendedor Creado Correctamente!";
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // GET: Usuarios/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        // POST: Usuarios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Apellido1,Apellido2,Cedula,Rol,Email,EmailConfirmed,Estado,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUsers aspNetUsers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetUsers).State = EntityState.Modified;
                db.SaveChanges();
                Session["Vendedor"] = "¡Información del vendedor actualizada correctamente!";
                return RedirectToAction("Index");
            }
            return View(aspNetUsers);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ChangePassword(string id)
        {
            var user = db.AspNetUsers.Where(x => x.Id == id).First();
            user.PasswordHash = "";
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword([Bind(Include = "Id,Nombre,Apellido1,Apellido2,Cedula,Rol,Email,EmailConfirmed,Estado,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUsers model)
        {
            var code = await UserManager.GeneratePasswordResetTokenAsync(model.Id);
            var result = await UserManager.ResetPasswordAsync(model.Id,code,model.PasswordHash);
            if (result.Succeeded)
            {
                Session["Vendedor"] = "¡La contraseña del vendedor ha sido actualizada con éxito!";
                return RedirectToAction("Index", "Usuarios");
            }
            AddErrors(result);
            Session["Errores"] = "";
            foreach (var item in result.Errors)
            {
                Session["Errores"] += item + "\n";
            }
            Session["Errores"] = result.Errors;
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
