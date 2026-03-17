using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using Microsoft.AspNetCore.Mvc;

namespace AlarmDistribution.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class NursesController : ControllerBase
{
    private readonly INurseRepository _nurseRepository;

    public NursesController(INurseRepository nurseRepository)
    {
        ArgumentNullException.ThrowIfNull(nurseRepository);

        _nurseRepository = nurseRepository;
    }

    [HttpGet]
    public async Task<List<NurseResponse>> GetNursesAsync(CancellationToken cancellationToken)
    {
        var nurses = await _nurseRepository.GetAllAsync(true, cancellationToken);

        return nurses.ConvertAll(nurse => new NurseResponse
        {
            Id = nurse.Id,
            Name = nurse.Name,
            PendingAlarms = nurse.PendingAlarms
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NurseResponse>> GetNurseByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var nurse = await _nurseRepository.GetByIdAsync(id, true, cancellationToken);

        if (nurse == null)
            return NotFound();

        var response = new NurseResponse
        {
            Id = nurse.Id,
            Name = nurse.Name,
            PendingAlarms = nurse.PendingAlarms
        };

        return response;
    }
}

