namespace NotesPOC.Models
{
    public class PullAddNote
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    
    public class PullNotes
    {
        public List<PullAddNote> Created { get; set; }
        public List<NoteUpdateRequest> Updated { get; set; }
        public List<int> Deleted { get; set; }
    }


    public class PullNoteResponse
    {
        public PullChange Changes { get; set; }
        public long LastPulledAt { get; set; }
    }

    public class PullChange    {
        public PullNotes Notes { get; set; }
    }
}
