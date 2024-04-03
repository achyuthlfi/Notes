using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using System.Threading.Tasks;
using NotesPOC.Services;


namespace NotesPOC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly NoteContext _context;
        private readonly INoteService _noteService;
        private readonly IUserService _userService;

        //DB context
        public CollectionController(NoteContext context, INoteService noteService, IUserService userService)
        {
            _context = context;
            _noteService = noteService;
            _userService = userService;
        }


        [HttpPost("push/{lastPulledAt}")]
        public async Task<ActionResult<collectionPullResponse>> PushCollection(long lastPulledAt, [FromBody] CollectionPushRequest request)
        {
            Console.WriteLine($"Payload: {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Console.WriteLine("Collection push API called at: "+ currentTimestamp);

            PullNoteResponse notesRes = await _noteService.ProcessPushedNotes(lastPulledAt, request.Changes.notes);
            PullUserResponse userRes = await _userService.ProcessPushedUsers(lastPulledAt, request.Changes.users);

            //await _context.SaveChangesAsync();

            var response = new collectionPullResponse
            {
                changes = new ColleectionPulls
                {
                    notes = notesRes.Changes.Notes,
                    users = userRes.Changes.Users
                },
                LastPulledAt = currentTimestamp
            };

            return Ok(response);
        }

        [HttpGet("pull/{lastPulledAt}")]
        public async Task<ActionResult<collectionPullResponse>> PullCollection(long lastPulledAt)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Console.WriteLine("Collection pull API called at: " + currentTimestamp);
            PullNoteResponse notesRes = await _noteService.FetchNotesByLastSync(lastPulledAt);
            PullUserResponse userRes = await _userService.FetchUsersByLastSync(lastPulledAt);

            var response = new collectionPullResponse
            {
                changes = new ColleectionPulls
                {
                    notes = notesRes.Changes.Notes,
                    users = userRes.Changes.Users
                },
                LastPulledAt = currentTimestamp
            };

            return Ok(response);
        }

    }
}



