using Seguridad_autorizacion_autenticacion.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Seguridad_autorizacion_autenticacion.DTOs
{
    public class LibroPatchDTO
    {
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
        [Required]
        public string Titulo { get; set; }
        public DateTime fechaPublicacion { get; set; }
    }
}
