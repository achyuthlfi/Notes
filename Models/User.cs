namespace NotesPOC.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // TBD: hash passwords 
        public string ProfilePic { get; set; }

        public string Status { get; set; } // CREATE, UPDATE, DELETE
        public long CreatedAt { get; set; }
        public long LastModifiedAt { get; set; }
    }

    public class PullAddUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePic { get; set; }
    }

    public class UserUpdateRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePic { get; set; }
    }

    public class PullUsers
    {
        public List<PullAddUser> Created { get; set; }
        public List<UserUpdateRequest> Updated { get; set; }
        public List<int> Deleted { get; set; }
    }

    public class PullUserResponse
    {
        public PullUserChanges Changes { get; set; }
        public long LastPulledAt { get; set; }
    }



    public class PullUserChanges
    {
        public PullUsers Users { get; set; }
    }

    //Push

    public class UserAddRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePic { get; set; }

    }
    public class PushUsers
    {
        public List<UserAddRequest> created { get; set; }
        public List<UserUpdateRequest> updated { get; set; }
        public List<int> deleted { get; set; }
    }

    public class PushUserReq
    {
        public PushUsers users { get; set; }
    }

    public class PushUsersRequest
    {
        public PushUserReq Changes { get; set; }

    }
}
