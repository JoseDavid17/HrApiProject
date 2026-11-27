using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrApiProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("GeneralPolicy")]
    public class EmployeeController : ControllerBase
    {

        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDetailsDto>>> GetEmployees([FromQuery] int pageNumber=1, [FromQuery] int pageSize = 10)
        {
            if(pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var employees = await _employeeService.GetAllEmployees(pageNumber,pageSize);

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<EmployeeDetailsDto>>> GetEmployeeById([FromRoute] int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if(employee == null)
            {
                return NotFound(new{Message = $"No se encontró un empleado para el id: {id}"});
            }

            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDetailsDto>> CreateEmployees(EmployeeCreateDto employeeCreateDto) 
        {
          var Createdemployees = await _employeeService.CreateEmployeesAsync(employeeCreateDto);

          return CreatedAtAction(nameof(GetEmployeeById), new { id = Createdemployees.Id }, Createdemployees);

        }

        [HttpPatch("{id}")]

        public async Task<ActionResult> UpdatdatosEmpleado([FromRoute] int id, [FromBody] UpdateDataEmpDTO updateDataEmpDTO)
        {

            var succes = await _employeeService.UpdateDatosEmployee(id, updateDataEmpDTO);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró el empleado {id}" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteEmployee([FromRoute] int id)
        {
            var succes = await _employeeService.DeleteEmpleado(id);

            if (!succes)
            {
                return NotFound( new { message = $"No se encontró el empleado {id}" } ) ;
            }

            return NoContent();

        }

        [HttpGet("Supervisores")]
        public async Task<ActionResult> ObtenerSupervisores([FromQuery] int pagenumber = 1, [FromQuery] int pagesize = 50)
        {
            if (pagenumber < 0)
            {
                pagenumber = 1;
            }

            if(pagesize > 50)
            {
                pagesize = 50;
            }

            var supervisores = await _employeeService.Supervisores(pagenumber,pagesize);

            return Ok(supervisores);
        }

        [HttpPatch("ChangeSupervisor/{id}")]
        public async Task<ActionResult> CambiarSupervisor([FromRoute(Name = "id")] int employee, [FromBody] int SupervisorId, [FromQuery] int DepartmentId)
        {
            var updated = await _employeeService.ChangeSupervisor(
                employee, SupervisorId, DepartmentId);

            if (!updated)
            {
                return NotFound(new
                {
                    message = $"No se encontró el empleado {employee} o el supervisor {SupervisorId}."
                });
            }

            return NoContent();
        }

        [HttpGet("OrganizationChart")]
        public async Task<ActionResult> GetOrganizationChart([FromQuery] int? employeeid = null)
        {
            if (employeeid is <= 0)
            {
                return BadRequest(new
                {
                    message = "El ID del empleado debe ser mayor que cero."
                });
            }

            var organigrama = await _employeeService.Organigrama(employeeid);

            if (employeeid.HasValue && !organigrama.Any())
            {
                return NotFound(new
                {
                    message = $"No se encontró al empleado con ID {employeeid}."
                });
            }

            return Ok(organigrama);
        }

        [HttpPatch("ChangeDepartment/{id}")]

        public async Task<ActionResult> ChangeDepartment([FromRoute] int id, [FromQuery] int departmentid, [FromQuery] int supervisorid, [FromQuery] string jobid, [FromQuery] decimal? salary = null)
        {
            var success = await _employeeService.ChangeDepartment(id, departmentid,supervisorid, jobid, salary);

            if (!success)
            {
                return NotFound(new{message = $"No se encontró el empleado con ID {id}."});
            }

            return NoContent();
        }

        [HttpGet("SameBirthday")]
        public async Task<ActionResult> SameBirthday([FromQuery] int pageNumber = 1, [FromQuery] int pageSize=50)
        {
            if(pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageSize > 50)
            {
                pageNumber = 50;
            }

            var employees = await _employeeService.SameHireDate(pageNumber,pageSize);
            return Ok(employees);
        }

    }
}
