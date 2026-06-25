using EvolutionService.Query.Domain;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EvolutionService.Query.Infrastructure;

public class ReadDbContext
{
    private readonly IMongoDatabase _evolutionDatabase;
    private readonly IMongoDatabase _profileDatabase;

    public ReadDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReadDatabase")
            ?? "mongodb://root:root@localhost:27017/admin?authSource=admin";

        var client = new MongoClient(connectionString);
        _evolutionDatabase = client.GetDatabase("evolution_read");
        _profileDatabase = client.GetDatabase("profile_read");

        CreateIndexes();
    }

    public IMongoCollection<JobMovementReadModel> JobMovements => _evolutionDatabase.GetCollection<JobMovementReadModel>("jobMovements");
    public IMongoCollection<SalaryChangeReadModel> SalaryChanges => _evolutionDatabase.GetCollection<SalaryChangeReadModel>("salaryChanges");
    public IMongoCollection<TrainingReadModel> Trainings => _evolutionDatabase.GetCollection<TrainingReadModel>("trainings");
    public IMongoCollection<RewardReadModel> Rewards => _evolutionDatabase.GetCollection<RewardReadModel>("rewards");
    public IMongoCollection<ProfileLookupReadModel> Profiles => _profileDatabase.GetCollection<ProfileLookupReadModel>("profiles");

    private void CreateIndexes()
    {
        try
        {
            JobMovements.Indexes.CreateMany(
            [
                new CreateIndexModel<JobMovementReadModel>(Builders<JobMovementReadModel>.IndexKeys.Ascending(x => x.EmployeeId)),
                new CreateIndexModel<JobMovementReadModel>(Builders<JobMovementReadModel>.IndexKeys.Descending(x => x.EffectiveDate))
            ]);

            SalaryChanges.Indexes.CreateMany(
            [
                new CreateIndexModel<SalaryChangeReadModel>(Builders<SalaryChangeReadModel>.IndexKeys.Ascending(x => x.EmployeeId)),
                new CreateIndexModel<SalaryChangeReadModel>(Builders<SalaryChangeReadModel>.IndexKeys.Descending(x => x.EffectiveDate))
            ]);

            Trainings.Indexes.CreateMany(
            [
                new CreateIndexModel<TrainingReadModel>(Builders<TrainingReadModel>.IndexKeys.Ascending(x => x.EmployeeId)),
                new CreateIndexModel<TrainingReadModel>(Builders<TrainingReadModel>.IndexKeys.Descending(x => x.StartDate)),
                new CreateIndexModel<TrainingReadModel>(Builders<TrainingReadModel>.IndexKeys.Ascending(x => x.Status))
            ]);

            Rewards.Indexes.CreateMany(
            [
                new CreateIndexModel<RewardReadModel>(Builders<RewardReadModel>.IndexKeys.Ascending(x => x.EmployeeId)),
                new CreateIndexModel<RewardReadModel>(Builders<RewardReadModel>.IndexKeys.Descending(x => x.GrantedAt))
            ]);
        }
        catch (MongoCommandException)
        {
        }
    }
}
