using FreelancerAssignment;
using FreelancerAssignment.Extensions;
using FreelancerAssignment.Presistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFreelancerAssignment(builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseRouting();

app.UseCors(PolicyContracts.FrontEnd);
app.UseHttpsRedirection();

await app.MigrateDbAsync();

app.UseAuthorization();

app.MapEndpoints();

app.Run();