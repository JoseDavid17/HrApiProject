using Microsoft.AspNetCore.Mvc;
using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace HrApiProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
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

    }
}
