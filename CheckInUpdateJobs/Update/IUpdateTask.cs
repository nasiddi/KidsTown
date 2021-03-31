namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public interface IUpdateTask
    {
        public void ActivateTask();
        public void DeactivateTask();
        public bool IsTaskActive();
        public int GetExecutionCount();
    }
}