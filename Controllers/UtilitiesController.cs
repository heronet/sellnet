using Microsoft.AspNetCore.Mvc;
using sellnet.Services;

namespace sellnet.Controllers
{
    public class UtilitiesController : DefaultController
    {
        private readonly UtilityService _utilityService;
        public UtilitiesController(UtilityService utilityService)
        {
            _utilityService = utilityService;
        }
        [HttpGet("locations")]
        public ActionResult GetCitiesAndDivisions()
        {
            var data = new
            {
                Cities = _utilityService.Cities,
                Divisions = _utilityService.Divisions
            };
            return Ok(data);
        }
    }
}