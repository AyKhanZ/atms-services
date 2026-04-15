using System.Net;
using System.Net.Http.Json;
using ATMS.Admin.Contracts.Models.Organizations;
using ATMS.Admin.Service.Providers.Interfaces;

namespace ATMS.Admin.Service.Providers;

public class OrganizationsProvider(HttpClient http) : IOrganizationProvider
{
    public async Task<OrganizationModel?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await http.GetAsync($"/organizations/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OrganizationModel>(cancellationToken);
    }
}