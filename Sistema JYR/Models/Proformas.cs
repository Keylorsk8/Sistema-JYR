//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sistema_JYR.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public partial class Proformas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Proformas()
        {
            this.ProformaDetalle = new HashSet<ProformaDetalle>();
            this.Documentos = new HashSet<Documentos>();
        }
    
        public int Id { get; set; }
        public string IdUsuario { get; set; }
        public int IdEstado { get; set; }
        public System.DateTime Fecha { get; set; }

        [DisplayName("Total Pagar")]
        public double TotalPagar { get; set; }


        [DisplayName("Total Descuento")]
        public double TotalDescuento { get; set; }


        [DisplayName("Total Impuesto")]
        public double TotalImpuesto { get; set; }
        public string IdCliente { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Name = "Dirección de Entrega")]
        public string DireccionEntrega { get; set; }
        public string NombreProforma { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual EstadoProforma EstadoProforma { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProformaDetalle> ProformaDetalle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Documentos> Documentos { get; set; }
    }
}
