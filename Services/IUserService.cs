using NotesPOC.Models;
using System.Threading.Tasks;

namespace NotesPOC.Services
{
    public interface IUserService
    {
        Task<PullUserResponse> ProcessPushedUsers(long lastPulledAt, PushUsers changes);
        Task<PullUserResponse> FetchUsersByLastSync(long? lastPulledAt);
    }
}
