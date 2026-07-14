using JsConsulting.Web.Models;
using JsConsulting.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JsConsulting.Web.Pages;

[IgnoreAntiforgeryToken]
public sealed class IndexModel(SqlWorkspaceService workspaceService, LocalHistoryStore historyStore) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostTestAsync([FromBody] ConnectionInput input, CancellationToken cancellationToken)
    {
        var message = await workspaceService.TestAsync(input, cancellationToken);
        return new JsonResult(new { ok = true, message });
    }

    public async Task<IActionResult> OnPostObjectsAsync([FromBody] ConnectionInput input, CancellationToken cancellationToken)
    {
        var objects = await workspaceService.LoadObjectsAsync(input, cancellationToken);
        return new JsonResult(objects);
    }

    public async Task<IActionResult> OnPostExecuteAsync([FromBody] QueryRequest request, CancellationToken cancellationToken)
    {
        var result = await workspaceService.ExecuteAsync(request, cancellationToken);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnGetHistoryAsync()
    {
        return new JsonResult(await historyStore.ListAsync());
    }
}
