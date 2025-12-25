using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Text.Json;

namespace NZWalks.API.Controllers
{
    //https://localhost:xxxx/api/regions
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // Apply authorization at the controller level
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;
        private readonly IRegionRepository regionRepository;
        private readonly IMapper mapper;
        private readonly ILogger<RegionsController> logger;

        public RegionsController(NZWalksDbContext dbContext, 
            IRegionRepository regionRepository, 
            IMapper mapper,
            ILogger<RegionsController> logger)
        {
            this.dbContext = dbContext;
            this.regionRepository = regionRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        //GET ALL REGIONS
        //GET : https://localhost:xxxx/api/regions
        [HttpGet]
        //[Authorize(Roles ="Reader")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //throw new Exception("This is a test exception for logging");
                //Get Data from Database - Domains Models 
                var regionsDomain = await regionRepository.GetAllAsync();

                //Map Domain Models to DTOs
                var regionDto = mapper.Map<List<RegionDto>>(regionsDomain);
                logger.LogInformation($"Finished Get all regions request with data: {JsonSerializer.Serialize(regionsDomain)}");
                return Ok(regionDto);

                //var regionsDto = new List<RegionDto>();
                //foreach (var regionDomain in regionsDomain)
                //{
                //    regionsDto.Add(new RegionDto()
                //    {
                //        Id = regionDomain.Id,
                //        Code = regionDomain.Code,
                //        Name = regionDomain.Name,
                //        RegionImageUrl = regionDomain.RegionImageUrl
                //    });
                //}

                ////Return DTO's
                /////return Ok(regionsDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }                        
        }

        //GET SINGLE REGION (Get REgion By Id)
        //GET: https://localhost:xxxx/api/regions/{id}
        [HttpGet]
        [Route("{id:guid}")]
        //[Authorize(Roles ="Reader")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            //Get Region Domain model from database
            //var region = dbContext.Regions.Find(id);
            var regionDomain = await regionRepository.GetByIdAsync(id);
            if (regionDomain == null)
            {
                return NotFound();
            }

            //Map Region Domain model to Region Dto
            var regionDto = mapper.Map<RegionDto>(regionDomain);
            return Ok(regionDto);

            //var regionDto = new RegionDto
            //{
            //    Id = regionDomain.Id,
            //    Name= regionDomain.Name,
            //    Code= regionDomain.Code,
            //    RegionImageUrl= regionDomain.RegionImageUrl
            //};
            //return Ok(regionDto);
        }

        //POST to create new Region
        //POST: https://localhost:xxxx/api/regions
        [HttpPost]
        [ValidateModel]
        //[Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateRegion([FromBody] AddRegionRequestDto addRegionRequestDto)
        {
            //Validate the Request (Optionally can use Fluent Validation)
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            //Map or Convert DTO to Domain Model
            var regionDomain = mapper.Map<Region>(addRegionRequestDto);
            //var regionDomain = new Region
            //{
            //    Name = addRegionRequestDto.Name,
            //    Code = addRegionRequestDto.Code,
            //    RegionImageUrl = addRegionRequestDto.RegionImageUrl
            //};

            //Use Domain Model to create Region in Database
            regionDomain = await regionRepository.CreateRegionAsync(regionDomain);
            //await dbContext.SaveChangesAsync();

            //Map Domain Model back to DTO
            var regionDto = mapper.Map<RegionDto>(regionDomain);
            //var regionDto = new RegionDto
            //{
            //    Id = regionDomain.Id,
            //    Code = regionDomain.Code,
            //    Name = regionDomain.Name,
            //    RegionImageUrl = regionDomain.RegionImageUrl
            //};

            return CreatedAtAction(nameof(GetById), new { id = regionDto.Id }, regionDto);
        }

        //Update Region
        //PUT: https://localhost:xxxx/api/regions/{id}
        [HttpPut]
        [Route("{id:guid}")]
        [ValidateModel]
        //[Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateRegion([FromRoute] Guid id, [FromBody] UpdateRegionRequestDto updateRegionRequestDto)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            //Map DTO to Domain Model
            var regionDomainModel = mapper.Map<Region>(updateRegionRequestDto);
            //var regionDomainModel = new Region
            //{
            //    Code = updateRegionRequestDto.Code,
            //    Name = updateRegionRequestDto.Name,
            //    RegionImageUrl = updateRegionRequestDto.RegionImageUrl
            //};

            //Check if region exists in database
            regionDomainModel = await regionRepository.UpdateRegionAsync(id, regionDomainModel);

            if (regionDomainModel == null)
            {
                return NotFound();
            }

            //Map Domain Model back to DTO
            var updatedRegionDto = mapper.Map<RegionDto>(regionDomainModel);
            //var UpdatedRegionDto = new RegionDto
            //{
            //    Id = regionDomainModel.Id,
            //    Code = regionDomainModel.Code,
            //    Name = regionDomainModel.Name,
            //    RegionImageUrl = regionDomainModel.RegionImageUrl
            //};

            return Ok(updatedRegionDto);
            
        }

        //Delete Region
        //DELETE: https://localhost:xxxx/api/regions/{id}
        [HttpDelete]
        [Route("{id:guid}")]
        //[Authorize(Roles = "Reader,Writer")]
        public async Task<IActionResult> DeleteRegion([FromRoute] Guid id)
        {
            //Check if region exists in database
            var existingRegionDomain = await regionRepository.DeleteRegionAsync(id);
            if (existingRegionDomain == null)
            {
                return NotFound();
            }

            //Map Domain Model back to DTO
            var deletedRegionDto = mapper.Map<RegionDto>(existingRegionDomain);
            //var deletedRegionDto = new RegionDto
            //{
            //    Id = existingRegionDomain.Id,
            //    Code = existingRegionDomain.Code,
            //    Name = existingRegionDomain.Name,
            //    RegionImageUrl = existingRegionDomain.RegionImageUrl
            //};

            return Ok(deletedRegionDto);
        }
    }
}
