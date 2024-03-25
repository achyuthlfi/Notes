using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using System.Threading.Tasks;

namespace NotesPOC.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private const string Created = "created";
        private const string Updated = "updated";
        private const string Deleted = "deleted";
        
        private readonly NoteContext _context;

        //DB context
        public NoteController(NoteContext context)
        {
            _context = context;
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
                Status=Created,
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
            getNote.Status = Updated;
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

        //Pull notes by timestamp
        [HttpGet("pull/{lastPulledAt}")]
        public async Task<ActionResult<PullNoteResponse>> PullNotes(long? lastPulledAt)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var changedNotes = await _context.Notes
                                            .Where(n => lastPulledAt == null || n.LastModifiedAt > lastPulledAt.Value)
                                            .ToListAsync();

            var createdNotes = changedNotes
                               .Where(n => n.Status == Created)
                               .Select(n => new NoteAddRequest
                               {
                                   Title = n.Title,
                                   Description = n.Description,
                                   //CreatedAt = n.CreatedAt
                               }).ToList();

            var updatedNotes = changedNotes
                               .Where(n => n.Status == Updated)
                               .Select(n => new NoteUpdateRequest
                               {
                                   Id = n.Id,
                                   Title = n.Title,
                                   Description = n.Description,
                                   //CreatedAt = n.CreatedAt
                               }).ToList();

            var deletedNotesIds = changedNotes
                                  .Where(n => n.Status == Deleted)
                                  .Select(n => n.Id)
                                  .ToList();

            /*var response = new PullNotes
            {
                Created = createdNotes.Any() ? createdNotes : new List<NoteAddRequest>(),
                Updated = updatedNotes.Any() ? updatedNotes : new List<NoteUpdateRequest>(),
                Deleted = deletedNotesIds.Any() ? deletedNotesIds : new List<int>()
            };*/

            /*var response = new PullNotes
            {
                Created = createdNotes,
                Updated = updatedNotes,
                Deleted = deletedNotesIds
            };*/

            var response = new PullNoteResponse
            {
                Changes = new ChangesContainer
                {
                    Notes = new PullNotes
                    {
                        Created = createdNotes,
                        Updated = updatedNotes,
                        Deleted = deletedNotesIds
                    }
                },
                LastPulledAt = currentTimestamp
            };

            return Ok(response);

        }

        //Push notes
        [HttpPost("push/{lastPulledAt}")]
        public async Task<ActionResult<PullNoteResponse>> PushNotes(long lastPulledAt, [FromBody] PushNotesRequest request)
        {
            // Assuming you want to return changes up to the current moment after pushing changes
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var changes = request.Changes;

            // Add new notes
            if (changes.created != null && changes.created.Any())
            {
                foreach (var note in changes.created)
                {
                    var addNote = new Note
                    {
                        Title = note.Title,
                        Description = note.Description,
                        Status = Created,
                        LastModifiedAt = currentTimestamp,
                        CreatedAt = currentTimestamp
                    };
                    _context.Notes.Add(addNote);
                }
            }


            // Update the existing note with the new values
            if (changes.updated != null && changes.updated.Any())
            {
                foreach (var note in changes.updated)
                {
                    var existingNote = await _context.Notes.FindAsync(note.Id);
                    if (existingNote != null)
                    {
                        existingNote.Title = note.Title;
                        existingNote.Description = note.Description;
                        existingNote.Status = Updated;
                        existingNote.LastModifiedAt = currentTimestamp;
                    }
                }
            }


            // Handle Deletions
            if (changes.deleted != null && changes.deleted.Any())
            {
                Console.WriteLine("In side delete");
                foreach (var noteId in changes.deleted)
                {
                    Console.WriteLine("In side delete {0}", noteId);
                    var noteToDelete = await _context.Notes.FindAsync(noteId);
                    if (noteToDelete != null)
                    {
                        noteToDelete.Status = Deleted;
                        _context.Entry(noteToDelete).CurrentValues.SetValues(noteToDelete);
                        noteToDelete.LastModifiedAt = currentTimestamp;
                    }
                }
            }


            await _context.SaveChangesAsync();

            // Directly calling PullChanges and returning its result
            var getNotes = await PullNotes(lastPulledAt);

            //return pullChangesResult.Result;
            //return Ok(getNotes);

            if (getNotes.Result is OkObjectResult okResult)
            {
                // If the result is OkObjectResult, extract its value (which is the actual data)
                var pullResponse = okResult.Value as PullNoteResponse;
                // Return this data wrapped in an Ok() to match the response type
                return Ok(pullResponse);
            }
            else
            {
                // Handle other cases (e.g., if PullNotes returned a different type of result)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred, While getting notes");
            }
        }


    }
}
