var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc(option => option.EnableEndpointRouting = true);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v1",
        Title = "Инструмента"
    });
    string path = Path.Combine(AppContext.BaseDirectory, "OrderAPI.xml");
    options.IncludeXmlComments(path);
});
var app = builder.Build();
app.UseSwagger();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Инструментарий");
});
app.Run();
