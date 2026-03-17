using AlarmDistribution.WebApi.Application.Commands.AckAlarm;
using AlarmDistribution.WebApi.Application.Commands.PublishAlarm;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AlarmDistribution.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlarmsController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);

        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> PublishAlarmAsync(PublishAlarmCommand command)
    {
        await _mediator.Send(command);

        return Accepted();
    }

    [HttpPut("{id}/ack")]
    public async Task<ActionResult> AckAlarmAsync(int id, [FromBody] AckAlarmCommand command)
    {
        command.SetAlarmId(id);
        await _mediator.Send(command);

        return NoContent();
    }
}
