using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using DAP.API.Models;

namespace DAP.API.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost("separate")]
        public void Post(int a, int b, int c)
        {
            Debug.WriteLine(a + " " + b + " " + c);
        }

        [HttpPost("together")]
        public void Post(Value Value)
        {
            Debug.WriteLine(Value.A + " " + Value.B);
        }

        [HttpPost("mix")]
        public void Post(Value value, int c)
        {
            Debug.WriteLine(value.A + " " + value.B + " " + c);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
