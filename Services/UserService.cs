using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NotesPOC.Utilities;

namespace NotesPOC.Services
{
    public class UserService : IUserService
    {
        private readonly NoteContext _context;

        public UserService(NoteContext context)
        {
            _context = context;
        }

        public async Task<PullUserResponse> ProcessPushedUsers(long lastPulledAt, PushUsers changes)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Add new users
            if (changes.created != null && changes.created.Any())
            {
                foreach (var u in changes.created)
                {
                    var addUser = new User
                    {
                        Email = u.Email,
                        Password = u.Password,
                        ProfilePic = u.ProfilePic,
                        Status = AppConstants.Created,
                        LastModifiedAt = currentTimestamp,
                        CreatedAt = currentTimestamp
                    };
                    _context.Users.Add(addUser);
                }
            }

            // Update the existing user with the new values
            if (changes.updated != null && changes.updated.Any())
            {
                foreach (var u in changes.updated)
                {
                    var existingUser = await _context.Users.FindAsync(u.Id);
                    if (existingUser != null)
                    {
                        existingUser.Email = u.Email;
                        existingUser.Password = u.Password;
                        existingUser.ProfilePic = u.ProfilePic;
                        existingUser.Status = AppConstants.Updated;
                        existingUser.LastModifiedAt = currentTimestamp;
                    }
                }
            }

            // Handle Deletions
            if (changes.deleted != null && changes.deleted.Any())
            {
                foreach (var uId in changes.deleted)
                {
                    var userToDelete = await _context.Users.FindAsync(uId);
                    if (userToDelete != null)
                    {
                        userToDelete.Status = AppConstants.Deleted;
                        _context.Entry(userToDelete).CurrentValues.SetValues(userToDelete);
                        userToDelete.LastModifiedAt = currentTimestamp;
                    }
                }
            }

            //Save changes
            await _context.SaveChangesAsync();


            // Call PullNotes and handle its result to extract PullNoteResponse
            var pullUserResponse = await FetchUsersByLastSync(lastPulledAt);
            return pullUserResponse;

            // Handle other cases or errors appropriately. 
            //throw new InvalidOperationException("Failed to retrieve notes after push operation.");
        }

        // Example method to fetch updated notes - replace with actual implementation
        public async Task<PullUserResponse> FetchUsersByLastSync(long? lastPulledAt)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


            var getUSers = await _context.Users
                                            .Where(n => lastPulledAt == null || n.LastModifiedAt > lastPulledAt.Value)
                                            .ToListAsync();

            var created = getUSers
                               .Where(n => n.Status == AppConstants.Created)
                               .Select(n => new PullAddUser
                               {
                                   Id = n.Id,
                                   Email = n.Email,
                                   Password = n.Password,
                                   ProfilePic = n.ProfilePic
                               }).ToList();

            var updated = getUSers
                               .Where(n => n.Status == AppConstants.Updated)
                               .Select(n => new UserUpdateRequest
                               {
                                   Id = n.Id,
                                   Email = n.Email,
                                   Password = n.Password,
                                   ProfilePic = n.ProfilePic
                               }).ToList();

            var deleted = getUSers
                                  .Where(n => n.Status == AppConstants.Deleted)
                                  .Select(n => n.Id)
                                  .ToList();

            var response = new PullUserResponse
            {
                Changes = new PullUserChanges
                {
                    Users = new PullUsers
                    {
                        Created = created,
                        Updated = updated,
                        Deleted = deleted
                    }
                },
                LastPulledAt = currentTimestamp
            };
            return response;
        }
    }
}
