
using Calendar.Models;

namespace Calendar.DTOs
{
    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string BgColor { get; set; } = string.Empty;
        public TypeEvent TypeEvent { get; set; } = TypeEvent.Shared;
        public DateTime DateEvent { get; set; }
        public DateTime DateEndEvent { get; set; }
        public bool ForcedNametag { get; set; } = false;

        public List<int> ParticipantsIds { get; set; } = new();
    }

    public class UpdateEventDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string BgColor { get; set; } = string.Empty;
        public TypeEvent TypeEvent { get; set; }
        public DateTime DateEvent { get; set; }
        public DateTime DateEntEvent { get; set; }

        public List<int> ParticipantsIds { get; set; } = new();
    }

    public class DeleteEventDto
    {
        public int Id { get; set; }
    }
}
