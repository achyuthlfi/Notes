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

    public class PushChangeReq
    {
        public PushNotes notes { get; set; }
    }

    /*public class SyncResponse
    {
        public PullChange Changes { get; set; }
        public long Timestamp { get; set; }
    }*/

    public class PushNotesRequest
    {
        public PushChangeReq Changes { get; set; }

    }
}
