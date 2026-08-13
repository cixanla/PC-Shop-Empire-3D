using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    public enum DomainEventEnqueueStatus
    {
        Accepted = 1,
        Duplicate = 2,
        Rejected = 3
    }

    public readonly struct DomainEventEnqueueResult
    {
        private DomainEventEnqueueResult(DomainEventEnqueueStatus status, Failure error)
        {
            Status = status;
            Error = error;
        }

        public DomainEventEnqueueStatus Status { get; }

        public Failure Error { get; }

        public bool IsAccepted => Status == DomainEventEnqueueStatus.Accepted;

        public bool IsDuplicate => Status == DomainEventEnqueueStatus.Duplicate;

        public bool IsRejected => Status == DomainEventEnqueueStatus.Rejected;

        internal static DomainEventEnqueueResult Accepted()
        {
            return new DomainEventEnqueueResult(DomainEventEnqueueStatus.Accepted, Failure.None);
        }

        internal static DomainEventEnqueueResult Duplicate()
        {
            return new DomainEventEnqueueResult(DomainEventEnqueueStatus.Duplicate, Failure.None);
        }

        internal static DomainEventEnqueueResult Rejected(Failure error)
        {
            return new DomainEventEnqueueResult(DomainEventEnqueueStatus.Rejected, error);
        }
    }
}
