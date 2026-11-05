using HrApiProject.Controllers;
using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace HrApiProyect.Tests
{
    public class EmployeeControllerTest
    {
        [Fact]
        public async Task GetEmployees_CuandoExistenEmpleados_DebeRetornarOk()
        {
            // Arrange
            var empleados = new List<EmployeeDetailsDto>
            {
                new()
                {
                    Id = 1,
                    FullName = "Juan Pérez",
                    Email = "juan.perez@empresa.com"
                },
                new()
                {
                    Id = 2,
                    FullName = "Ana López",
                    Email = "ana.lopez@empresa.com"
                }
            };

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.GetAllEmployees(1, 10))
                .ReturnsAsync(empleados);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.GetEmployees(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var empleadosDevueltos =
                Assert.IsAssignableFrom<IEnumerable<EmployeeDetailsDto>>(okResult.Value);

            Assert.Equal(2, empleadosDevueltos.Count());
            Assert.Equal("Juan Pérez", empleadosDevueltos.First().FullName);

            mockService.Verify(
                service => service.GetAllEmployees(1, 10),
                Times.Once);
        }

        [Fact]
        public async Task GetEmployeeById_CuandoIdExiste_DebeRetornarOkConElEmpleado()
        {
            // Arrange
            const int employeeId = 1;

            var empleado = new EmployeeDetailsDto
            {
                Id = employeeId,
                FullName = "Juan Pérez",
                Email = "juan.perez@empresa.com",
                Phone = "555-1234",
                JobTitle = "Developer"
            };

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(empleado);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.GetEmployeeById(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var empleadoDevuelto = Assert.IsType<EmployeeDetailsDto>(okResult.Value);

            Assert.Equal(employeeId, empleadoDevuelto.Id);
            Assert.Equal("Juan Pérez", empleadoDevuelto.FullName);
            Assert.Equal("juan.perez@empresa.com", empleadoDevuelto.Email);
        }

        [Fact]
        public async Task GetEmployeeById_CuandoIdNoExiste_DebeRetornarNotFound()
        {
            // Arrange
            const int employeeId = 99;

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((EmployeeDetailsDto?)null);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.GetEmployeeById(employeeId);

            // Assert
            var notFoundResult =
                Assert.IsType<NotFoundObjectResult>(resultado.Result);

            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateEmployees_ConDatosValidos_DebeRetornarCreated201()
        {
            // Arrange
            var empleadoParaCrear = new EmployeeCreateDto
            {
                EmployeeId = 10,
                FirstName = "Carlos",
                LastName = "García",
                Email = "carlos.garcia@empresa.com",
                PhoneNumber = "555-9876",
                HireDate = new DateTime(2026, 1, 15),
                Salary = 2500,
                JobId = "DEV"
            };

            var empleadoCreado = new EmployeeDetailsDto
            {
                Id = 10,
                FullName = "Carlos García",
                Email = "carlos.garcia@empresa.com"
            };

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.CreateEmployeesAsync(empleadoParaCrear))
                .ReturnsAsync(empleadoCreado);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.CreateEmployees(empleadoParaCrear);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(resultado.Result);

            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(
                nameof(EmployeeController.GetEmployeeById),
                createdResult.ActionName);

            var empleadoDevuelto =
                Assert.IsType<EmployeeDetailsDto>(createdResult.Value);

            Assert.Equal(10, empleadoDevuelto.Id);
            Assert.Equal("Carlos García", empleadoDevuelto.FullName);
            Assert.Equal("carlos.garcia@empresa.com", empleadoDevuelto.Email);

            mockService.Verify(
                service => service.CreateEmployeesAsync(empleadoParaCrear),
                Times.Once);
        }

        [Fact]
        public async Task UpdatdatosEmpleado_CuandoEmpleadoExiste_DebeRetornarNoContent()
        {
            // Arrange
            const int employeeId = 1;

            var datosActualizados = new UpdateDataEmpDTO
            {
                FirstName = "Juan",
                LastName = "Pérez Actualizado",
                Email = "juan.actualizado@empresa.com",
                PhoneNumber = "555-0000",
                Salary = 3000,
                CommissionPct = 5
            };

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.UpdateDatosEmployee(employeeId, datosActualizados))
                .ReturnsAsync(true);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado =
                await controller.UpdatdatosEmpleado(employeeId, datosActualizados);

            // Assert
            Assert.IsType<NoContentResult>(resultado);

            mockService.Verify(
                service => service.UpdateDatosEmployee(employeeId, datosActualizados),
                Times.Once);
        }

        [Fact]
        public async Task UpdatdatosEmpleado_CuandoEmpleadoNoExiste_DebeRetornarNotFound()
        {
            // Arrange
            const int employeeId = 99;

            var datosActualizados = new UpdateDataEmpDTO
            {
                FirstName = "Empleado",
                LastName = "Inexistente",
                Email = "inexistente@empresa.com"
            };

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.UpdateDatosEmployee(employeeId, datosActualizados))
                .ReturnsAsync(false);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado =
                await controller.UpdatdatosEmpleado(employeeId, datosActualizados);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);

            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteEmployee_CuandoEmpleadoExiste_DebeRetornarNoContent()
        {
            // Arrange
            const int employeeId = 1;

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.DeleteEmpleado(employeeId))
                .ReturnsAsync(true);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<NoContentResult>(resultado);

            mockService.Verify(
                service => service.DeleteEmpleado(employeeId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEmployee_CuandoEmpleadoNoExiste_DebeRetornarNotFound()
        {
            // Arrange
            const int employeeId = 99;

            var mockService = new Mock<IEmployeeService>();

            mockService
                .Setup(service => service.DeleteEmpleado(employeeId))
                .ReturnsAsync(false);

            var controller = new EmployeeController(mockService.Object);

            // Act
            var resultado = await controller.DeleteEmployee(employeeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado);

            Assert.Equal(404, notFoundResult.StatusCode);
        }

    }
}
