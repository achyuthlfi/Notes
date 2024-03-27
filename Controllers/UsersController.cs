using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using System.Threading.Tasks;

namespace NotesPOC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private const string Created = "created";
        private const string Updated = "updated";
        private const string Deleted = "deleted";

        private readonly NoteContext _context;
        public UsersController(NoteContext context)
        {
            _context = context;
        }

        //Push users
        [HttpPost("push/{lastPulledAt}")]
        public async Task<ActionResult<PullUserResponse>> PushUsers(long lastPulledAt, [FromBody] PushUsersRequest request)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var changes = request.Changes;

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
                        Status = Created,
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
                        existingUser.Status = Updated;
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
                        userToDelete.Status = Deleted;
                        _context.Entry(userToDelete).CurrentValues.SetValues(userToDelete);
                        userToDelete.LastModifiedAt = currentTimestamp;
                    }
                }
            }


            await _context.SaveChangesAsync();

            var getUsers = await PullUsers(lastPulledAt);

            if (getUsers.Result is OkObjectResult okResult)
            {
                // If the result is OkObjectResult, extract its value (which is the actual data)
                // Return this data wrapped in an Ok() to match the response type
                var pullResponse = okResult.Value as PullUserResponse;
                return Ok(pullResponse);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred, While getting users");
            }
        }


        //Pull users by timestamp
        [HttpGet("pull/{lastPulledAt}")]
        public async Task<ActionResult<PullUserResponse>> PullUsers(long? lastPulledAt)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var getUSers = await _context.Users
                                            .Where(n => lastPulledAt == null || n.LastModifiedAt > lastPulledAt.Value)
                                            .ToListAsync();

            var created = getUSers
                               .Where(n => n.Status == Created)
                               .Select(n => new PullAddUser
                               {
                                   Id = n.Id,
                                   Email = n.Email,
                                   Password = n.Password,
                                   ProfilePic = n.ProfilePic
                               }).ToList();

            var updated = getUSers
                               .Where(n => n.Status == Updated)
                               .Select(n => new UserUpdateRequest
                               {
                                   Id = n.Id,
                                   Email = n.Email,
                                   Password = n.Password,
                                   ProfilePic = n.ProfilePic
                               }).ToList();

            var deleted = getUSers
                                  .Where(n => n.Status == Deleted)
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

            return Ok(response);

        }

        
        //Add User
        [HttpPost("create")]
        public async Task<ActionResult<List<User>>> Add(UserAddRequest user)
        {
            var creatTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var addUser = new User()
            {
                Email= user.Email,
                Password= user.Password,
                ProfilePic = user.ProfilePic,
                Status = Created,
                LastModifiedAt = creatTime,
                CreatedAt = creatTime,
            };
            _context.Users.Add(addUser);
            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToListAsync());
        }

        //Get All Users
        [HttpGet("getAll")]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        //User update
        [HttpPut("update")]
        public async Task<ActionResult<List<User>>> Update(UserUpdateRequest user)
        {
            var getUser = await _context.Users.FindAsync(user.Id);
            if (getUser is null)
            {
                return NotFound("User not found");
            }

            getUser.Email = user.Email;
            getUser.Password = user.Password;
            getUser.ProfilePic = user.ProfilePic;
            getUser.Status = Updated;
            getUser.LastModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToListAsync());
        }

        //Get User by ID
        [HttpGet("getById/{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                string notFoundRes = "User didn't found with id: " + id;
                return NotFound(notFoundRes);
            }
            return Ok(user);

        }

        //Delete 
        [HttpDelete("delete")]
        public async Task<ActionResult<List<User>>> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound("User found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToListAsync());
        }
    }
}
