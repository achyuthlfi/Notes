namespace NotesPOC.Models
{
    public class PushNotes
    {
        public List<NoteAddRequest> created { get; set; }
        public List<NoteUpdateRequest> updated { get; set; }
        public List<int> deleted { get; set; } // Assuming deletion is identified by the Note ID
    }

    public class AddOrEditNotes
    {
        public List<Note> created { get; set; }
        public List<Note> updated { get; set; }
        public List<int> deleted { get; set; } // Assuming deletion is identified by the Note ID
    }


    public class SyncResponse
    {
        public ChangesContainer Changes { get; set; }
        public long Timestamp { get; set; }
    }

    public class PushNotesRequest
    {
        public PushNotes Changes { get; set; }

    }
}
