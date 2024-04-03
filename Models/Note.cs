namespace NotesPOC.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Status { get; set; } // CREATE, UPDATE, DELETE
        public long LastModifiedAt { get; set; } //Unix timestamp

        public long CreatedAt { get; set; } //Unix timestamp
    }

    public class NoteAddRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }

    public class NoteUpdateRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
