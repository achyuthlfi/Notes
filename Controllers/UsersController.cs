using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesPOC.Data;
using NotesPOC.Models;
using NotesPOC.Services;
using System.Threading.Tasks;
using NotesPOC.Utilities;

namespace NotesPOC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly NoteContext _context;
        private readonly IUserService _userService;
        public UsersController(NoteContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        //Push users
        [HttpPost("push/{lastPulledAt}")]
        public async Task<ActionResult<PullUserResponse>> PushUsers(long lastPulledAt, [FromBody] PushUsersRequest request)
        {
            try
            {
                var processResult = await _userService.ProcessPushedUsers(lastPulledAt, request.Changes.users);
                return Ok(processResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the push user process." + ex.Message + " :: " + ex.ToString());
            }
        }


        //Pull users by timestamp
        [HttpGet("pull/{lastPulledAt}")]
        public async Task<ActionResult<PullUserResponse>> PullUsers(long lastPulledAt)
        {
            try
            {
                var users = await _userService.FetchUsersByLastSync(lastPulledAt);
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during the pulling the user process." + ex.Message + " :: " + ex.ToString());
            }

        }

        
        //Add User
        [HttpPost("create")]
        public async Task<ActionResult<List<User>>> Add(UserAddRequest user)
        {
            var creatTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var addUser = new User()
            {
                ReferenceId = user.Id,
                Email= user.Email,
                Password= user.Password,
                ProfilePic = user.ProfilePic,
                Status = AppConstants.Created,
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
            var getUser = await _context.Users.Where(n => n.ReferenceId == user.Id).FirstOrDefaultAsync();
            if (getUser is null)
            {
                return NotFound("User not found");
            }

            getUser.Email = user.Email;
            getUser.Password = user.Password;
            getUser.ProfilePic = user.ProfilePic;
            getUser.Status = AppConstants.Updated;
            getUser.LastModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _context.SaveChangesAsync();
            return Ok(await _context.Users.ToListAsync());
        }

        //Get User by ID
        [HttpGet("getById/{id}")]
        public async Task<ActionResult<User>> GetById(string id)
        {
            var user = await _context.Users.Where(n => n.ReferenceId == id).FirstOrDefaultAsync(); ;
            if (user is null)
            {
                string notFoundRes = "User didn't found with id: " + id;
                return NotFound(notFoundRes);
            }
            return Ok(user);

        }

        //Delete 
        [HttpDelete("delete")]
        public async Task<ActionResult<List<User>>> Delete(string id)
        {
            var user = await _context.Users.Where(n => n.ReferenceId == id).FirstOrDefaultAsync();
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
