using Microsoft.Extensions.Logging;
using Supabase;

namespace Periods;

public interface IPeriodsRepository
{
    Task Add(DateOnly date);
    Task<DateOnly> LastDate();
}

public class PeriodsRepository : IPeriodsRepository
{
    private readonly Client _db;

    public PeriodsRepository(Supabase.Client db)
    {
        _db = db;
    }

    public async Task Add(DateOnly date)
    {
        var result = await _db.From<Periods>().Upsert(new Periods(){Date = date});
    }

    public async Task<DateOnly> LastDate()
    {
        var result = await _db.From<Periods>().Get();
            
        return result.Models.Max(c => c.Date);
    }
}

public class PeriodsRepositoryDecorator : IPeriodsRepository
{
    private IPeriodsRepository _repository;
    private readonly ILogger _logger;

    public PeriodsRepositoryDecorator(IPeriodsRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Add(DateOnly date)
    {
        await _repository.Add(date);
    }

    public async Task<DateOnly> LastDate()
    {
        return await _repository.LastDate();
    }
}