using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Route("v1/api/roles")]
public class RolesController(IMediator mediator) : AdminControllerBase
{
    
    /// <summary>
    /// Returns a list of all roles.
    /// </summary>
    /// <remarks>
    /// Retrieves all roles in the system. 
    /// Supports optional query parameters for filtering or paging via <see cref="GetRolesRequest"/>.
    /// </remarks>
    /// <param name="request">Query request containing filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns an array of roles.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RoleModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleModel[]>> Index([FromQuery] GetRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Returns a single role by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a role given its ID.
    /// </remarks>
    /// <param name="request">Request containing the role ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the role data.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Role with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet(":id")]
    [ProducesResponseType(typeof(RoleModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleModel>> Get([FromQuery] GetRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <remarks>
    /// Adds a new role to the system with the specified details.
    /// Returns the created role and a route to retrieve it.
    /// </remarks>
    /// <param name="command">Command containing role details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Role successfully created.</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoleModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(Get),
            controllerName: "Roles",
            routeValues: new { id = result.Id },
            value: result);
    }

    
    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <remarks>
    /// Modifies the details of an existing role. All fields are updated based on the command.
    /// </remarks>
    /// <param name="command">Command containing updated role details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Role successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    
    /// <summary>
    /// Deletes an existing role.
    /// </summary>
    /// <remarks>
    /// Removes a role from the system by ID. 
    /// Use with caution, as deleting a role may affect user permissions.
    /// </remarks>
    /// <param name="command">Command containing the role ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Role successfully deleted.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromQuery] DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
