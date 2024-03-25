namespace NotesPOC.Models
{
    public class PullNotes
    {
        public List<NoteAddRequest> Created { get; set; }
        public List<NoteUpdateRequest> Updated { get; set; }
        public List<int> Deleted { get; set; }
    }


    public class PullNoteResponse
    {
        public ChangesContainer Changes { get; set; }
        public long LastPulledAt { get; set; }
    }

    public class ChangesContainer
    {
        public PullNotes Notes { get; set; }
    }
}
