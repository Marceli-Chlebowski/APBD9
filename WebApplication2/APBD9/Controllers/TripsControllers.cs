using APBD9.Data;
using APBD9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsControllers: ControllerBase
{
    private readonly ApbdContext _context;

    public TripsControllers(ApbdContext _context)
    {
        this._context = _context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _context.Trips.ToListAsync();
        return Ok(trips);
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetTripsSortedAndPaged(int? page, int pageSize = 10)
    {
        int pageNumber = page ?? 1;
        int skipAmount = (pageNumber - 1) * pageSize;

        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip(skipAmount)
            .Take(pageSize)
            .ToListAsync();

        return Ok(trips);
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, ClientTripDTO clientTripDTO)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip && t.DateFrom > DateTime.Now);
        if (trip == null)
        {
            return NotFound("Trip does not exist or has already started.");
        }

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientTripDTO.Pesel);
        if (client == null)
        {
            return NotFound("Client does not exist.");
        }

        bool isAlreadyAssigned = await _context.ClientTrips.AnyAsync(ct => ct.IdTrip == idTrip && ct.IdClient == client.IdClient);
        if (isAlreadyAssigned)
        {
            return BadRequest("Client is already assigned to this trip.");
        }

        var clientTrip = new ClientTrip()
        {
            IdTrip = idTrip,
            IdClient = client.IdClient,
            RegisteredAt = DateTime.UtcNow,
            PaymentDate = clientTripDTO.PaymentDate
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return Ok("Client assigned to trip successfully.");
    }


}