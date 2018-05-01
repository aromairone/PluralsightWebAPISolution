using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using Microsoft.Extensions.Logging;
using AutoMapper;
using PluralsightWebAPI.Models;

namespace PluralsightWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class CampController : BaseController
    {
        private ICampRepository _camp;
        private ILogger<CampController> _logger;
        private IMapper _mapper;

        public CampController(ICampRepository repo, ILogger<CampController> logger,
            IMapper mapper)
        {
            _camp = repo;
            _logger = logger;
            _mapper = mapper;
        }
        [HttpGet()]
        public IActionResult Get()
        {
            var camps = _camp.GetAllCamps();
            //With automapper, convert Camps to IEnumerable of CampsModel objects
            //automatically maps properties and flattens inner objects like Location and Speakers
            //Automapper is injected in the comptroller and the startup class
            //Need to create a profile class to map entities

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }
        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp;
                if (includeSpeakers) camp = _camp.GetCampWithSpeakers(id);
                else
                camp = _camp.GetCamp(id);

                if (camp == null) return NotFound($"Camp {id} was not found in Azure new build/deployment");
                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {

            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");
                _camp.Add(model);
                if(await _camp.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { id = model.Id });
                    _logger.LogInformation("New Code Camp created with ID " + model.Id);
                    return Created(newUri,model);
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to DB");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Threw exception while saving Camp: {ex}");
                return BadRequest(ex.Message);
            }           
            return BadRequest();
        }

        //Patch and PUT are both valid methods to update an object
        [HttpPatch("{id}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id,[FromBody] Camp model)
        {
            try
            {
                var oldCamp = _camp.GetCamp(id);
                if (oldCamp == null) return NotFound($"Could not find Camp with an ID of {id}");

                //Map model to the oldCamp
                oldCamp.Name = model.Name ?? oldCamp.Name; //update oldModel with modelName but if null then use oldCamp value
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;
                //Save Changes to the database
                if(await _camp.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("Couldn't Update Camp");
        }
        //Implement Delete Command
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldCamp = _camp.GetCamp(id);
                if (oldCamp == null) return NotFound($"Could not find Camp with ID of {id}");

                _camp.Delete(oldCamp);
                if(await _camp.SaveAllAsync())
                {
                    return Ok("Camp Deleted Azure");
                } 
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("Could not find Camp in Azure");
        }
    }
}
