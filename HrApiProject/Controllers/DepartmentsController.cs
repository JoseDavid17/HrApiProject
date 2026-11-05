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


    }
}
