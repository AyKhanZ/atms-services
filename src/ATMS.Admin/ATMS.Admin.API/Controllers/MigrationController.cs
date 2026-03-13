using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[Authorize]
[Route("api/migrations")]
public class MigrationController(IMediator mediator) : AdminControllerBase
{
    
    /// <summary>
    /// Applies all pending migrations.
    /// </summary>
    /// <remarks>
    /// Executes migrations up to the latest version. Use carefully in production environments.
    /// </remarks>
    /// <param name="command">Migration command containing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Migration applied successfully.</response>
    /// <response code="400">Validation error or invalid command.</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPost("up")]
    [ProducesResponseType(typeof(MigrationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MigrationModel>> Up([FromQuery] ApplyMigrationsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Rolls back a migration.
    /// </summary>
    /// <remarks>
    /// Rolls back the specified migration version.
    /// </remarks>
    /// <param name="command">Command containing the migration version to revert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Migration rolled back successfully.</response>
    /// <response code="400">Validation error or invalid command.</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPost("down")]
    [ProducesResponseType(typeof(MigrationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MigrationModel>> Down([FromQuery] DownMigrationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
