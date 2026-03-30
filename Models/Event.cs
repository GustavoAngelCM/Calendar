namespace Calendar.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string BgColor { get; set; } = string.Empty;
        public TypeEvent TypeEvent { get; set; } = TypeEvent.Shared;
        public DateTime DateEvent { get; set; }
        public DateTime DateEndEvent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool Active { get; set; } = true;

        public List<Participation> Participations { get; set; } = new();
    }

    public enum TypeEvent
    {
        Exclusive,
        Shared,
    }
}
