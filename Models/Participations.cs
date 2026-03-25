namespace Calendar.Models
{
    public class Participation
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
        public int? InvitedByUserId { get; set; }

        public bool IsCreator { get; set; } = false;

        public User User { get; set; } = null!;
        public Event Event { get; set; } = null!;

        public DateTime? RespondedAt { get; set; }

        public ParticipationStatus Status { get; set; } = ParticipationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool Active { get; set; } = true;
    }

    public enum ParticipationStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}