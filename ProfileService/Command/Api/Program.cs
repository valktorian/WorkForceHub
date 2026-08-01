using Infrastructure.Api.Authentication;
using Infrastructure.Api.Extensions;
using Infrastructure.Api.Filters;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Command.Application.Commands;
using ProfileService.Command.Application.DTOs;
using ProfileService.Command.Application.Handlers;
using ProfileService.Command.Infrastructure;
using ProfileService.Command.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.Configure<MvcOptions>(options => options.Filters.Add<IdempotencyFilter>());
builder.Services.AddWorkForceHubTracing(builder.Configuration, "ProfileService.Command");
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProfileCommandDbContext>("postgresql");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Profile Command API");

builder.Services.AddProfileCommandInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddHandlersFromAssemblies(typeof(CreateProfileHandler).Assembly);
builder.Services.AddScoped<ICommandHandler<CreateProfileCommand, ProfileResponse>, CreateProfileHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProfileCommand, ProfileResponse>, UpdateProfileHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProfileEmploymentCommand, ProfileResponse>, UpdateProfileEmploymentHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProfileStatusCommand, ProfileResponse>, UpdateProfileStatusHandler>();
builder.Services.AddScoped<ICommandHandler<LinkProfileAccountCommand, ProfileResponse>, LinkProfileAccountHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateSelfPersonalInfoCommand, ProfileResponse>, UpdateSelfPersonalInfoHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProfilePictureCommand, ProfileResponse>, UpdateProfilePictureHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteProfileCommand, bool>, DeleteProfileHandler>();
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

var app = builder.Build();

await app.ApplyMigrationsAsync<ProfileCommandDbContext>();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseGlobalErrorHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckExtensions.DefaultOptions);
app.MapControllers();

app.Run();
