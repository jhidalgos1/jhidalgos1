namespace JH.QueryStudio.Api.Middleware;
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger){
 public async Task Invoke(HttpContext ctx){try{await next(ctx);}catch(Exception ex){logger.LogError(ex,"Unhandled API error");ctx.Response.StatusCode=500;await ctx.Response.WriteAsJsonAsync(new{error="Se produjo un error interno.",traceId=ctx.TraceIdentifier});}}
}
