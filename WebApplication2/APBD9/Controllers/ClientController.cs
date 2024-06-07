using APBD9.Data;
using APBD9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace APBD9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var clientsTrips = await _context.ClientTrips.Where(c => c.IdClient == id).ToListAsync();
        
        if (clientsTrips.Any())
        {
            return StatusCode(403, "Unacceptable: The client has assigned trips and cannot be deleted.");
        }
        else
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdClient == id);
            if (client == null)
            {
                return NotFound();
            }
            
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return Ok("Client deletet succesfully");
        }
    }
}