namespace BackgroundTasks.Kid;

public interface IKidUpdateService
{
    public Task<int> UpdateKids(int daysLookBack, int batchSize);
}