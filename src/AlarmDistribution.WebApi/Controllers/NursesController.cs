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
    public async Task<List<Nurse>> GetNursesAsync(CancellationToken cancellationToken)
    {
        return await _nurseRepository.GetAllAsync(true, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Nurse>> GetNurseByIdAsync(int id, CancellationToken cancellationToken)
    {
        var nurse = await _nurseRepository.GetByIdAsync(id, true, cancellationToken);

        if (nurse == null)
            return NotFound();

        return nurse;
    }
}

