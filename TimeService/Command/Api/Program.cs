using Infrastructure.Api.Authentication;
using Infrastructure.Api.Extensions;
using Infrastructure.Api.Filters;
using Infrastructure.Api.HealthChecks;
using Infrastructure.Api.Messaging;
using Infrastructure.Api.Middleware;
using Infrastructure.Api.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeService.Command.Application.Commands;
using TimeService.Command.Application.DTOs;
using TimeService.Command.Application.Handlers;
using TimeService.Command.Infrastructure;
using TimeService.Command.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.Configure<MvcOptions>(options => options.Filters.Add<IdempotencyFilter>());
builder.Services.AddWorkForceHubTracing(builder.Configuration, "TimeService.Command");
builder.Services.AddTimeCommandInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TimeCommandDbContext>("postgresql");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddWorkForceHubJwtAuthentication(builder.Configuration);
builder.Services.AddWorkForceHubSwagger("WorkForceHub Time Command API");
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddHandlersFromAssemblies(typeof(CreateTimeEntryHandler).Assembly);
builder.Services.AddScoped<ICommandHandler<CreateTimeEntryCommand, CommandAcceptedResponse>, CreateTimeEntryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTimeEntryCommand, CommandAcceptedResponse>, UpdateTimeEntryHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteTimeEntryCommand, CommandAcceptedResponse>, DeleteTimeEntryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateTimesheetCommand, CommandAcceptedResponse>, CreateTimesheetHandler>();
builder.Services.AddScoped<ICommandHandler<SubmitTimesheetCommand, CommandAcceptedResponse>, SubmitTimesheetHandler>();
builder.Services.AddScoped<ICommandHandler<ApproveTimesheetCommand, CommandAcceptedResponse>, ApproveTimesheetHandler>();
builder.Services.AddScoped<ICommandHandler<RejectTimesheetCommand, CommandAcceptedResponse>, RejectTimesheetHandler>();
builder.Services.AddScoped<ICommandHandler<ReopenTimesheetCommand, CommandAcceptedResponse>, ReopenTimesheetHandler>();
builder.Services.AddScoped<ICommandHandler<CreateLeaveRequestCommand, CommandAcceptedResponse>, CreateLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateLeaveRequestCommand, CommandAcceptedResponse>, UpdateLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<SubmitLeaveRequestCommand, CommandAcceptedResponse>, SubmitLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<ApproveLeaveRequestCommand, CommandAcceptedResponse>, ApproveLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<RejectLeaveRequestCommand, CommandAcceptedResponse>, RejectLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<CancelLeaveRequestCommand, CommandAcceptedResponse>, CancelLeaveRequestHandler>();
builder.Services.AddScoped<ICommandHandler<AdjustLeaveBalanceCommand, CommandAcceptedResponse>, AdjustLeaveBalanceHandler>();
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

var app = builder.Build();
await app.ApplyMigrationsAsync<TimeCommandDbContext>();

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
