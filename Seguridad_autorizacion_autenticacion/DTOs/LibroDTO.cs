using Seguridad_autorizacion_autenticacion.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Seguridad_autorizacion_autenticacion.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public List<AutorDTO> Autores { get; set; }
        public List<ComentarioDTO> Comentarios  { get; set; }
    }
}
