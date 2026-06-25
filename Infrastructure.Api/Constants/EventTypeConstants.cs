namespace Infrastructure.Api.Constants;

public static class EventTypeConstants
{
    public static class Account
    {
        public const string AccountCreated = "AccountService.Command.Domain.Events.AccountCreatedEvent";
        public const string AccountUpdated = "AccountService.Command.Domain.Events.AccountUpdatedEvent";
        public const string AccountRoleUpdated = "AccountService.Command.Domain.Events.AccountRoleUpdatedEvent";
        public const string AccountPasswordChanged = "AccountService.Command.Domain.Events.AccountPasswordChangedEvent";
        public const string AccountDeleted = "AccountService.Command.Domain.Events.AccountDeletedEvent";

        public static readonly string[] QuerySubscriptions =
        [
            AccountCreated,
            AccountUpdated,
            AccountRoleUpdated,
            AccountPasswordChanged,
            AccountDeleted
        ];
    }

    public static class Profile
    {
        public const string ProfileCreated = "ProfileService.Command.Domain.Events.ProfileCreatedEvent";
        public const string ProfileUpdated = "ProfileService.Command.Domain.Events.ProfileUpdatedEvent";
        public const string ProfileDeleted = "ProfileService.Command.Domain.Events.ProfileDeletedEvent";

        public static readonly string[] QuerySubscriptions =
        [
            ProfileCreated,
            ProfileUpdated,
            ProfileDeleted
        ];
    }

    public static class Time
    {
        public const string TimeEntryCreated = "TimeService.Command.Domain.Events.TimeEntryCreatedEvent";
        public const string TimesheetCreated = "TimeService.Command.Domain.Events.TimesheetCreatedEvent";
        public const string LeaveRequestCreated = "TimeService.Command.Domain.Events.LeaveRequestCreatedEvent";
        public const string TimesheetStatusChanged = "TimeService.Command.Domain.Events.TimesheetStatusChangedEvent";
        public const string LeaveRequestStatusChanged = "TimeService.Command.Domain.Events.LeaveRequestStatusChangedEvent";
        public const string LeaveBalanceAdjusted = "TimeService.Command.Domain.Events.LeaveBalanceAdjustedEvent";

        public static readonly string[] QuerySubscriptions =
        [
            TimeEntryCreated,
            TimesheetCreated,
            LeaveRequestCreated,
            TimesheetStatusChanged,
            LeaveRequestStatusChanged,
            LeaveBalanceAdjusted
        ];
    }

    public static class Evolution
    {
        public const string JobMovementUpserted = "EvolutionService.Command.Domain.Events.JobMovementUpsertedEvent";
        public const string JobMovementDeleted = "EvolutionService.Command.Domain.Events.JobMovementDeletedEvent";
        public const string SalaryChangeUpserted = "EvolutionService.Command.Domain.Events.SalaryChangeUpsertedEvent";
        public const string SalaryChangeDeleted = "EvolutionService.Command.Domain.Events.SalaryChangeDeletedEvent";
        public const string TrainingUpserted = "EvolutionService.Command.Domain.Events.TrainingUpsertedEvent";
        public const string TrainingDeleted = "EvolutionService.Command.Domain.Events.TrainingDeletedEvent";
        public const string RewardUpserted = "EvolutionService.Command.Domain.Events.RewardUpsertedEvent";
        public const string RewardDeleted = "EvolutionService.Command.Domain.Events.RewardDeletedEvent";

        public static readonly string[] QuerySubscriptions =
        [
            JobMovementUpserted,
            JobMovementDeleted,
            SalaryChangeUpserted,
            SalaryChangeDeleted,
            TrainingUpserted,
            TrainingDeleted,
            RewardUpserted,
            RewardDeleted
        ];
    }
}
