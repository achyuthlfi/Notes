using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NotesPOC.Utilities;
using Azure.Core;

namespace NotesPOC.Services
{
    public class NoteService : INoteService
    {
        private readonly NoteContext _context;

        public NoteService(NoteContext context)
        {
            _context = context;
        }

        public async Task<PullNoteResponse> ProcessPushedNotes(long lastPulledAt, PushNotes changes)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Add new notes
            if (changes.created != null && changes.created.Any())
            {
                foreach (var note in changes.created)
                {
                    var noteExited = await _context.Notes.Where(n => n.ReferenceId == note.Id).FirstOrDefaultAsync();
                    //Console.WriteLine("noteExited", noteExited);
                    Console.WriteLine($"create note: {Newtonsoft.Json.JsonConvert.SerializeObject(noteExited)}");
                    if (noteExited == null) {
                        var addNote = new Note
                        {
                            ReferenceId = note.Id,
                            Title = note.Title,
                            Description = note.Description,
                            Status = AppConstants.Created,
                            LastModifiedAt = currentTimestamp,
                            CreatedAt = currentTimestamp
                        };
                        _context.Notes.Add(addNote);
                    } 
                }
            }

            // Update the existing note with the new values
            if (changes.updated != null && changes.updated.Any())
            {
                foreach (var note in changes.updated)
                {
                    var existingNote = await _context.Notes.Where(n => n.ReferenceId == note.Id).FirstOrDefaultAsync();
                    if (existingNote != null)
                    {
                        existingNote.Title = note.Title;
                        existingNote.Description = note.Description;
                        existingNote.Status = AppConstants.Updated;
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
                    var noteToDelete = await _context.Notes.Where(n => n.ReferenceId == noteId).FirstOrDefaultAsync();
                    if (noteToDelete != null)
                    {
                        noteToDelete.Status = AppConstants.Deleted;
                        _context.Entry(noteToDelete).CurrentValues.SetValues(noteToDelete);
                        noteToDelete.LastModifiedAt = currentTimestamp;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Call PullNotes and handle its result to extract PullNoteResponse
            var pullNoteResponse = await FetchNotesByLastSync(lastPulledAt);
            return pullNoteResponse;

            // Handle other cases or errors appropriately. 
            //throw new InvalidOperationException("Failed to retrieve notes after push operation.");
        }

        // Example method to fetch updated notes - replace with actual implementation
        public async Task<PullNoteResponse> FetchNotesByLastSync(long? lastPulledAt)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var changedNotes = await _context.Notes
                                            .Where(n => lastPulledAt == null || n.LastModifiedAt > lastPulledAt.Value)
                                            .ToListAsync();

            var createdNotes = changedNotes
                               .Where(n => n.Status == AppConstants.Created)
                               .Select(n => new PullAddNote
                               {
                                   Id = n.ReferenceId,
                                   Title = n.Title,
                                   Description = n.Description,
                                   //CreatedAt = n.CreatedAt
                               }).ToList();

            var updatedNotes = changedNotes
                               .Where(n => n.Status == AppConstants.Updated)
                               .Select(n => new NoteUpdateRequest
                               {
                                   Id = n.ReferenceId,
                                   Title = n.Title,
                                   Description = n.Description,
                                   //CreatedAt = n.CreatedAt
                               }).ToList();

            var deletedNotesIds = changedNotes
                                  .Where(n => n.Status == AppConstants.Deleted)
                                  .Select(n => n.ReferenceId)
                                  .ToList();

            var response = new PullNoteResponse
            {
                Changes = new PullChange
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

            return response;
        }
    }
}
