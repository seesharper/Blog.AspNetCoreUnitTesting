using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Controllers
{
    [Route("api/[controller]")]
    public class ValueController : Controller
    {
        private readonly IService _service;

        public ValueController(IService service) => _service = service;

        // GET api/values
        [HttpGet]
        public string Get()
        {
            return _service.GetValue();
        }        
    }
}
