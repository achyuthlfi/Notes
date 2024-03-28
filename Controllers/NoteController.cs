using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using NotesPOC.Services;
using System.Threading.Channels;
using System.Threading.Tasks;
using NotesPOC.Utilities;

namespace NotesPOC.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        
        private readonly NoteContext _context;
        private readonly INoteService _noteService;

        //DB context
        public NoteController(NoteContext context, INoteService noteService)
        {
            _context = context;
            _noteService = noteService;
        }

        [HttpPost("push/{lastPulledAt}")]
        public async Task<ActionResult<PullNoteResponse>> PushNotes(long lastPulledAt, [FromBody] PushNotesRequest request)
        {
            try
            {
                var processResult = await _noteService.ProcessPushedNotes(lastPulledAt, request.Changes.notes);
                return Ok(processResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the push process." + ex.Message + " :: " + ex.ToString());
            }
        }

        //Pull notes by timestamp
        [HttpGet("pull/{lastPulledAt}")]
        public async Task<ActionResult<PullNoteResponse>> PullNotes(long lastPulledAt)
        {
            try
            {
                var processResult = await _noteService.FetchNotesByLastSync(lastPulledAt);
                return Ok(processResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the push process." + ex.Message + " :: " + ex.ToString());
            }
        }

        //Add Note
        [HttpPost("create")]
        public async Task<ActionResult<List<Note>>> Add(NoteAddRequest note)
        {
            var creatTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var addNote = new Note()
            {
                Title = note.Title,
                Description = note.Description,
                Status=AppConstants.Created,
                LastModifiedAt= creatTime,
                CreatedAt= creatTime,
            };
            _context.Notes.Add(addNote);
            await _context.SaveChangesAsync();
            return Ok(await _context.Notes.ToListAsync());
        }

        //Get All Notes
        [HttpGet("getAll")]
        public async Task<ActionResult<List<Note>>> GetAll()
        {
            var notes = await _context.Notes.ToListAsync();
            return Ok(notes);
        }

        //Note update
        [HttpPut("update")]
        public async Task<ActionResult<List<Note>>> Update(NoteUpdateRequest note)
        {
            var getNote = await _context.Notes.FindAsync(note.Id);
            if (getNote is null)
            {
                return NotFound("Note not found");
            }

            getNote.Title = note.Title;
            getNote.Description = note.Description;
            getNote.Status = AppConstants.Updated;
            getNote.LastModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _context.SaveChangesAsync();
            return Ok(await _context.Notes.ToListAsync());
        }

        //Get Note by ID
        [HttpGet("getById/{id}")]
        public async Task<ActionResult<Note>> GetById(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note is null)
            {
                string notFoundRes = "Note didn't found with id: " + id;
                return NotFound(notFoundRes);
            }
            return Ok(note);

        }

        //Delete note
        [HttpDelete("delete")]
        public async Task<ActionResult<List<Note>>> Delete(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note is null)
            {
                return NotFound("Note found");
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return Ok(await _context.Notes.ToListAsync());
        }
    }
}
