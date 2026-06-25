using EvolutionService.Command.Application.Commands;
using EvolutionService.Command.Application.DTOs;
using EvolutionService.Command.Application.Handlers;
using EvolutionService.Command.Domain;
using EvolutionService.Command.Infrastructure;
using EvolutionService.Command.Infrastructure.Persistence;
using Infrastructure.Api.Authentication;
using Infrastructure.Api.Extensions;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.Configure<MvcOptions>(_ => { });
builder.Services.AddWorkForceHubTracing(builder.Configuration, "EvolutionService.Command");
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EvolutionCommandDbContext>("postgresql");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Evolution Command API");
builder.Services.AddEvolutionCommandInfrastructure(builder.Configuration);
builder.Services.AddHandlersFromAssemblies(typeof(CreateJobMovementHandler).Assembly);
builder.Services.AddScoped<ICommandHandler<CreateJobMovementCommand, CommandAcceptedResponse>, CreateJobMovementHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateJobMovementCommand, CommandAcceptedResponse>, UpdateJobMovementHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteJobMovementCommand, bool>, DeleteJobMovementHandler>();
builder.Services.AddScoped<ICommandHandler<CreateSalaryChangeCommand, CommandAcceptedResponse>, CreateSalaryChangeHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateSalaryChangeCommand, CommandAcceptedResponse>, UpdateSalaryChangeHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteSalaryChangeCommand, bool>, DeleteSalaryChangeHandler>();
builder.Services.AddScoped<ICommandHandler<CreateTrainingCommand, CommandAcceptedResponse>, CreateTrainingHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTrainingCommand, CommandAcceptedResponse>, UpdateTrainingHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteTrainingCommand, bool>, DeleteTrainingHandler>();
builder.Services.AddScoped<ICommandHandler<CreateRewardCommand, CommandAcceptedResponse>, CreateRewardHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateRewardCommand, CommandAcceptedResponse>, UpdateRewardHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteRewardCommand, bool>, DeleteRewardHandler>();
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

var app = builder.Build();

await app.ApplyMigrationsAsync<EvolutionCommandDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalErrorHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckExtensions.DefaultOptions);
app.MapControllers();

app.Run();
