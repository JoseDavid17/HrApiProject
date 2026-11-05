using HrApiProject.Controllers;
using HrApiProject.DTOs;
using HrApiProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HrApiProyect.Tests
{
    public class RegionsControllerTests
    {
        [Fact]
        public async Task GetRegionById_CuandoIdExiste_DebeRetornarOkConLaRegion()
        {
            // 1. ARRANGE 
            int regionIdPrueba = 1;
            var regionFalsa = new RegionDto { Id = 1, Name = "Europe" };

            // Creamos un clon falso de la interfaz de nuestro servicio
            var mockService = new Mock<IRegionService>();

            mockService
                .Setup(servicio => servicio.GetRegionById(regionIdPrueba))
                .ReturnsAsync(regionFalsa);

            // Instanciamos el controlador inyectándole el clon
            var controller = new RegionController(mockService.Object);

            // 2. ACT 
            var resultado = await controller.GetRegionById(regionIdPrueba);

            // 3. ASSERT 
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);

            var regionDevuelta = Assert.IsType<RegionDto>(okResult.Value);

            Assert.Equal(1, regionDevuelta.Id);
            Assert.Equal("Europe", regionDevuelta.Name);
        }

        [Fact]

        public async Task GetRegionById_CuandoIdNoExiste_DebeRetornarNotFound()
        {

            // arrange
            var regionIdInexistente = 99;

            var mockservice = new Mock<IRegionService>();

            mockservice.Setup(servicio => servicio.GetRegionById(regionIdInexistente))
                .ReturnsAsync((RegionDto?) null);

            var controller = new RegionController(mockservice.Object);

            //act

            var resultado = await controller.GetRegionById(regionIdInexistente);

            //assert

            var notfounddResulto = Assert.IsType<NotFoundObjectResult>(resultado.Result);

            Assert.Equal(404,notfounddResulto.StatusCode);
        }

        [Fact]
        public async Task CreateRegion_ConDatosValidos_DebeRetornarCreated201()
        {
            // 1. ARRANGE
            var regionParaCrear = new RegionCreateDto 
            { 
                RegionId = 5, 
                RegionName = "Oceania" 
            };

            var regionCreadaEsperada = new RegionDto 
            { 
                Id = 5, 
                Name = "Oceania" 
            };

            var mockService = new Mock<IRegionService>();
            
            mockService
                .Setup(servicio => servicio.CreateRegion(It.IsAny<RegionCreateDto>()))
                .ReturnsAsync(regionCreadaEsperada);

            var controller = new RegionController(mockService.Object);

            // 2. ACT
            var resultado = await controller.CreateRegion(regionParaCrear);

            // 3. ASSERT 
            var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);

            Assert.Equal(201, createdResult.StatusCode);

            Assert.Equal(nameof(RegionController.GetRegionById), createdResult.ActionName);

            var regionDevuelta = Assert.IsType<RegionDto>(createdResult.Value);
            Assert.Equal(5, regionDevuelta.Id);
            Assert.Equal("Oceania", regionDevuelta.Name);
        }

    }
}
