namespace System.Threading.Tasks
{
    public static class TaskStatusExtensions
    {
        public static bool IsActive(this TaskStatus taskStatus)
        {
            switch (taskStatus)
            {
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                case TaskStatus.RanToCompletion:
                    return false;

                default:
                    return true;
            }
        }
    }
}
