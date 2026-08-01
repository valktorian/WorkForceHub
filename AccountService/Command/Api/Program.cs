using AccountService.Command.Api.Authentication;
using AccountService.Command.Application.Abstractions;
using AccountService.Command.Application.Commands;
using AccountService.Command.Application.DTOs;
using AccountService.Command.Application.Handlers;
using AccountService.Command.Domain;
using AccountService.Command.Infrastructure;
using AccountService.Command.Infrastructure.Persistence;
using Infrastructure.Api.Authentication;
using Infrastructure.Api.Extensions;
using Infrastructure.Api.Filters;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.Configure<MvcOptions>(options => options.Filters.Add<IdempotencyFilter>());
builder.Services.AddWorkForceHubTracing(builder.Configuration, "AccountService.Command");
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccountCommandDbContext>("postgresql");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Account Command API");
builder.Services.AddAccountCommandInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddHandlersFromAssemblies(typeof(CreateAccountHandler).Assembly);
builder.Services.AddScoped<ICommandHandler<CreateAccountCommand, CreateAccountResponse>, CreateAccountHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAccountCommand, AccountResponse>, UpdateAccountHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAccountRoleCommand, AccountResponse>, UpdateAccountRoleHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeAccountPasswordCommand, bool>, ChangeAccountPasswordHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteAccountCommand, bool>, DeleteAccountHandler>();
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

var app = builder.Build();

await app.ApplyMigrationsAsync<AccountCommandDbContext>();

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
