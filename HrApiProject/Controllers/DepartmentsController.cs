using HrApiProject.Domain;
using HrApiProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using HrApiProject.Services;
using Microsoft.AspNetCore.Authorization;

namespace HrApiProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {

        // crear una funcionalidad que le permita avisar mediante correo electronico al manager cuando tiene más de 5 empleados asignados.

        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 0)
            {
                pageNumber = 1;
            }
            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var departments = await _departmentService.GetDepartments(pageNumber,pageSize);

            return Ok(departments);
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<DepartmentDto?>> GetDeptoById([FromRoute] int id)
        {
            var deptoid = await _departmentService.GetDepartmentByIdAsync(id);

            if(deptoid == null)
            {
                return NotFound(new { message = "No existe un departamento con dicho id" });
            }

            return Ok(deptoid);
        }


        [HttpPost]

        public async Task<ActionResult<DepartmentDto>> CreateDepartments(DepartmentCreateDto departmentCreateDto)
        {
            var Creatdepto = await _departmentService.CreateDepartments(departmentCreateDto);

            return CreatedAtAction(
                   nameof(GetDeptoById),
                   new { id = Creatdepto.DepartmentId },
                   Creatdepto);
        }

        [HttpGet("departamentos")]
        public async Task<ActionResult<IEnumerable<Departments>>> GetDepartmentss()
        {
            var departments = await _departmentService.GetDepartmentss();

            return Ok(departments);
        }

        [HttpPut("{id}")]

        public async Task<ActionResult> UpdateDepto([FromRoute] int id, [FromBody]  UpdateDepartmentDto updateDepartmentDto)
        {
            var succes = await _departmentService.UpdateInfoDepto(id, updateDepartmentDto);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró un departamento con el {id} para actualizarlo" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteDepto([FromRoute] int id)
        {
            var succes = await _departmentService.EliminarDepto(id);

            if (!succes)
            {
                return NotFound(new { message = $"No se encontró un departamento con el {id} para eliminarlo" });
            }

            return NoContent();
        }

        [HttpPatch("ChangeManager/{id}")]
        public async Task<ActionResult> CambiarManager([FromRoute] int id, [FromBody] int manager_id)
        {
            var ChangeManager = await _departmentService.ChangeManager(id, manager_id);

            if(!ChangeManager)
            {
                return NotFound(new { message = $"No se encontró el departamento {id} o el empleado {manager_id}." });
            }

            return NoContent();
        }
        
    }
}
