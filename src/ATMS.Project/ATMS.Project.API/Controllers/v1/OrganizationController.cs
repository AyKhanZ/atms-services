using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Requests.Organizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/organization")]
public class OrganizationController(IMediator mediator) : ControllerBase
{
    
    /// <summary>
    /// Returns paginated and filtered list of all organizations.
    /// </summary>
    /// <remarks>
    /// Retrieves paginated and filtered organizations in the system. 
    /// Supports optional query parameters for filtering or paging via <see cref="GetOrganizationsRequest"/>.
    /// </remarks>
    /// <param name="request">Query parameters for filtering, paging, etc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns an array of organizations.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrganizationItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<OrganizationItemModel>>> Index(
        [FromQuery] GetOrganizationsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Returns a single organization by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about an organization given its ID.
    /// </remarks>
    /// <param name="id">Organization ID (Guid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the organization data.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Organization with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationModel>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrganizationRequest { Id = id }, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <remarks>
    /// Adds a new organization to the system with the specified details.
    /// Returns the created organization and a route to retrieve it.
    /// </remarks>
    /// <param name="command">Command containing organization details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Organization successfully created.</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromForm] CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(Get),
            controllerName: "Organization",
            routeValues: new { id },
            value: id);
    }


    /// <summary>
    /// Updates an existing organization.
    /// </summary>
    /// <remarks>
    /// Modifies the details of an existing organization. All fields are updated based on the command.
    /// </remarks>
    /// <param name="command">Command containing updated organization details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Organization successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Organization with specified ID not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromForm] UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }


    /// <summary>
    /// Deletes an existing organization.
    /// </summary>
    /// <remarks>
    /// Removes an organization from the system by ID. 
    /// Use with caution, as deleting an organization may affect user permissions.
    /// </remarks>
    /// <param name="id">Organization ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Organization successfully deleted.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Organization with specified ID not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteOrganizationCommand{ Id = id }, cancellationToken);

        return NoContent();
    }
}
