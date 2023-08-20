using System;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.DTO;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/VillaAPI")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		private readonly ILogger<VillaAPIController> _logger;
		public VillaAPIController(ILogger<VillaAPIController> logger)
		{
			_logger = logger;
		}

		//private readonly ILogging _logger;
		//public VillaAPIController(ILogging logger)
		//{
		//	_logger = logger;
		//}


		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<IEnumerable<VillaDTO>> GetVillas()
		{
			_logger.LogInformation("Getting all villas");

			return Ok(VillaStore.villaList);
		}


		[HttpGet("{id:int}", Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<VillaDTO> GetVilla(int id)
		{
			_logger.LogInformation("Getting the single villa with id: " + id);
			if (id == 0)
			{
				return BadRequest();
			}
			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}
			return Ok(villa);
		}


		[HttpPost(Name = "CreateVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villa)
		{
			if (VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == villa.Name.ToLower()) != null)
			{
				ModelState.AddModelError("Custom Error", "This Name already exists");
				return BadRequest(ModelState);
			}
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}
			if (villa == null)
			{
				return BadRequest(villa);
			}
			if (villa.Id > 0)
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
			villa.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
			VillaStore.villaList.Add(villa);
			_logger.LogInformation($"new Villa created with an id of {villa.Id}");
			return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);
		}


		[HttpDelete("{id:int}", Name ="Delete Villa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
		{
			if(id == 0)
			{
				return BadRequest();
			}
			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}
			VillaStore.villaList.Remove(villa);
			return Ok();
			//can also use return NoContent() -- //returns 204 code which is still successful
		}


		[HttpPut("{id:int}", Name = "UpdateVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villa)
		{
			if (id != villa.Id || villa == null)
			{
				return BadRequest();
			}
			var updatedVilla = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			if(updatedVilla == null)
			{
				return NotFound();
			}
			updatedVilla.Name = villa.Name;
			updatedVilla.Occupancy = villa.Occupancy;
			updatedVilla.Sqft = villa.Sqft;

			return NoContent(); ;

		}


		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchVilla)
		{
			if (patchVilla == null || id == 0)
			{
				return BadRequest();
			}
			var villaBeingUpdated = VillaStore.villaList.FirstOrDefault(u => u.Id == id);

			if (villaBeingUpdated == null)
			{
				return BadRequest();
			}
			patchVilla.ApplyTo(villaBeingUpdated, ModelState);
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			return NoContent();
		}
	}
}

