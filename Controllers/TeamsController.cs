using FormulaOneApp.Data;
using FormulaOneApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormulaOneApp.Controllers
{
    // It specifies that it is actually controller is authenticated through utilizing of jwt better tokens
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // It needs user to have some kind of authorization (JWT_Token)
    [Route("api/[controller]")] //api/Teams if we change words before Controller in (TeamsController)
    //As ControllerBase does not contain view => this controller will return API (Http Status: 200, 400, ... ) instead of views like usual
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private static AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        //private static List<Team> teams = new List<Team>()
        //{
        //    new Team()
        //    {
        //        Id = 1,
        //        Name = "Mercesdes AMG F1",
        //        Country = "Germany",
        //        TeamPrinciple = "Toto Wolf"
        //    },
        //    new Team() 
        //    {
        //        Id = 2,
        //        Name = "Ferrari",
        //        Country = "Italy",
        //        TeamPrinciple = "Mattia Binotto"
        //    },
        //    new Team() 
        //    { 
        //        Id = 3,
        //        Name = "Alpha Romeo",
        //        Country = "Swiss",
        //        TeamPrinciple = "Frédéric Vasseur"
        //    }
        //};

        [HttpGet]
        //[Route("GetBestTeam")] // To make it like /api/Teams/GetBestTeam
        public async Task<IActionResult> Get()
        {
            var teams = await _context.Teams.ToListAsync();
            return Ok(teams);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            //var team = teams.FirstOrDefault (x => x.Id == id);
            var team = await _context.Teams.FirstOrDefaultAsync();

            if (team == null)
            {
                return BadRequest("Invalid Id");
            }

            return Ok(team);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Team team)
        {
            //teams.Add(team);
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            //// Check how many teams
            //Console.WriteLine(teams.Count);

            return CreatedAtAction("Get", team.Id, team);
        }

        // Patch <> Put : Partial Update <> Full Update
        [HttpPatch]
        public async Task<IActionResult> Patch(int id, string country)
        {
            //var team = teams.FirstOrDefault(x =>  x.Id == id);

            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);

            if (team == null)
            {
                return BadRequest("Invalid Id");
            }

            team.Country = country;
            await _context.SaveChangesAsync();

            return NoContent(); // Return http status 204 instead of 200
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            //var team = teams.FirstOrDefault(x => x.Id == id);

            var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);

            if (team == null)
            {
                return BadRequest("Invalid Id");
            }

            //teams.Remove(team);
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent(); // Return http status 204 instead of 200
        }
    }
}
