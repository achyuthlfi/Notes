using NotesPOC.Models;
using System.Threading.Tasks;

namespace NotesPOC.Services
{
    public interface INoteService
    {
        Task<PullNoteResponse> ProcessPushedNotes(long lastPulledAt, PushNotes changes);
        Task<PullNoteResponse> FetchNotesByLastSync(long? lastPulledAt);
    }
}
