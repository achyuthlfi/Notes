namespace NotesPOC.Models
{


    public class Collections
    {
        public PushNotes notes { get; set; }
        public PushUsers users { get; set; }
    }

    public class CollectionPushRequest
    {
        public Collections Changes { get; set; }
    }

    public class ColleectionPulls
    {
        public PullNotes notes { get; set; }
        public PullUsers users { get; set; }
    }
    public class collectionPullResponse
    {
        public ColleectionPulls changes { get; set; }
        public long LastPulledAt { get; set; }
    }

}
