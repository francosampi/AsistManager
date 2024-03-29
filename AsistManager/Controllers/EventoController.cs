using AsistManager.Helpers;
using AsistManager.Models;
using AsistManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AsistManager.Controllers
{
    [Authorize]
    public class EventoController : Controller
    {
        private readonly AsistManagerContext _context;

        public EventoController(AsistManagerContext context)
        {
            _context = context;
        }

        //Traer eventos as�ncronamente e ir a la vista del listado
        public async Task<IActionResult> Index()
        {
            var eventos = _context.Eventos;

            //Si existe una b�squeda por filtro, devolver esa lista
            if (TempData["Filtro"] != null)
            {
                //Eliminar espacios en blanco, convertir a minusculas y remover tildes
                var filtro = TempData["Filtro"] as string;
                filtro = Utilities.PrepareFilter(filtro);

                var resultados = await eventos.ToListAsync();

                resultados = resultados.Where(vm => Utilities.PrepareFilter(vm.Nombre).Contains(filtro) ||
                                                    vm.FechaInicio.ToString("dd/MM/yyyy").Contains(filtro)).ToList();

                return View(resultados);
            }

            return View(await eventos.ToListAsync());
        }

        public IActionResult Filter(int id, string filtro)
        {
            var filtroInicial = filtro;

            if (string.IsNullOrEmpty(filtro))
            {
                TempData["AlertaTipo"] = "warning";
                TempData["AlertaMensaje"] = "El filtro de b�squeda est� vac�o.";
            }
            else
            {
                TempData["Filtro"] = filtro;

                //Para rellenar el campo b�squeda con lo �ltimo buscado
                TempData["PalabraBuscada"] = filtroInicial.Trim();
            }

            return RedirectToAction(nameof(Index), new { id = id });
        }

        //Ir a la vista para las operaciones del Evento
        public IActionResult Menu(int id)
        {
            var evento = _context.Eventos.Find(id);

            //Verificar si el evento existe
            if (evento == null)
            {
                return NotFound();
            }

            //Traer informacion de la base para generar el Chart
            var acreditados = _context.Acreditados;

            var chartRegistros = acreditados.Where(i => i.IdEvento == id).Count();
            var chartIngreso = acreditados.Where(i => i.IdEvento == id && _context.Ingresos.Any(ingreso => ingreso.IdAcreditado == i.Id)).Count();
            var chartEgreso = acreditados.Where(i => i.IdEvento == id && _context.Egresos.Any(egreso => egreso.IdAcreditado == i.Id)).Count();

            ViewData["ChartRegistros"] = chartRegistros;
            ViewData["ChartIngreso"] = chartIngreso;
            ViewData["ChartEgreso"] = chartEgreso;

            return View(evento);
        }

        //Ir a vista para insertar un Evento
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Crear un evento nuevo
        public async Task<IActionResult> Create(EventoViewModel model)
        {
            //Verifico si el modelo pasado desde la vista es v�lido (validaciones de campos)
            if (ModelState.IsValid)
            {
                //Creo instancia de la clase Evento y cargo los datos del ViewModel
                var evento = new Evento()
                {
                    Nombre = model.Nombre,
                    FechaInicio = model.FechaInicio
                };

                //Agrego y guardo los cambios en la base de datos
                _context.Add(evento);

                await _context.SaveChangesAsync();

                //Redirecciono al Index y muestro alerta
                TempData["AlertaMensaje"] = "El evento '<b>" + model.Nombre + "</b>' se agreg� exitosamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        //Ir a vista para editar Evento
        public IActionResult Update(int id)
        {
            var evento = _context.Eventos.Find(id);

            //Verificar si el evento existe
            if (evento == null)
            {
                return NotFound();
            }

            ViewData["NombreEvento"] = evento.Nombre;
            ViewData["FechaEvento"] = evento.FechaInicio;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Editar el Evento
        public IActionResult Update(int id, EventoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var evento = _context.Eventos.Find(id);

                //Verificar si el evento existe
                if (evento != null)
                {
                    //Actualizar propiedades del evento
                    evento.Nombre = model.Nombre;
                    evento.FechaInicio = model.FechaInicio;

                    //Guardo los cambios en la base de datos
                    _context.SaveChanges();

                    //Redirecciono al Index del controlador y muestro alerta
                    TempData["AlertaMensaje"] = "El evento '<b>" + model.Nombre + "</b>' se modific� exitosamente.";

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    //Lanzar error si no existe el evento
                    return NotFound();
                }
            }

            //Si hay errores de validaci�n, regresar a la vista de edici�n
            return View(model);
        }

        //Ir a vista para borrar Evento
        public IActionResult Delete(int id)
        {
            //Hago Select del Evento e incluyo en la consulta tanto sus acreditados como sus ingresos y egresos
            var evento = _context.Eventos
                .Include(e => e.Acreditados)
                    .ThenInclude(a => a.Ingresos)
                .Include(e => e.Acreditados)
                    .ThenInclude(a => a.Egresos)
                .FirstOrDefault(e => e.Id == id);

            //Verificar si el evento existe
            if (evento == null)
            {
                return NotFound();
            }

            ViewData["AcreditadosABorrar"] = evento.Acreditados.Count();
            ViewData["IngresosABorrar"] = evento.Acreditados.Sum(a => a.Ingresos.Count());
            ViewData["EgresosABorrar"] = evento.Acreditados.Sum(a => a.Egresos.Count());

            return View(evento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Borrar el Evento
        public IActionResult Delete(int id, int a)
        {
            //Hago Select del Evento e incluyo en la consulta tanto sus acreditados como sus ingresos y egresos
            var evento = _context.Eventos
                .Include(e => e.Acreditados)
                    .ThenInclude(a => a.Ingresos)
                .Include(e => e.Acreditados)
                    .ThenInclude(a=> a.Egresos)
                .FirstOrDefault(e => e.Id == id);

            //Verificar que el evento existe
            if (evento != null)
            {
                //Obtener los acreditados del evento y la cantidad a borrar
                var acreditados = evento.Acreditados;

                //Eliminar los ingresos y egresos del acreditado
                foreach (var acreditado in acreditados)
                {
                    _context.Ingresos.RemoveRange(acreditado.Ingresos);
                    _context.Egresos.RemoveRange(acreditado.Egresos);
                }

                _context.Acreditados.RemoveRange(acreditados);

                //Una vez eliminados los acreditados, ingresos y egresos, elimino el evento
                _context.Eventos.Remove(evento);
                _context.SaveChanges();

                //Redireccionar al Index y mostrar alerta
                TempData["AlertaMensaje"] = $"El evento '<b>{evento.Nombre}</b>' y su informaci�n fue eliminada exitosamente.";

                return RedirectToAction(nameof(Index));
            }

            //Lanzar error si no existe el evento
            return NotFound();
        }
    }
}
