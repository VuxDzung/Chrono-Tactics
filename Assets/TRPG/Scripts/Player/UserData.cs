namespace TRPG
{
    public class UserData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string[] ObtainedUnitIds { get; set; }
        public string[] InTeamUnitIds { get; set; }

        public UserData() { }


        public UserData(string userId, string userName, string email, string password, string[] obtainedUnitIds, string[] inTeamUnitIds)
        {
            UserId = userId;
            UserName = userName;
            UserEmail = email;
            Password = password;
            ObtainedUnitIds = obtainedUnitIds;
            InTeamUnitIds = inTeamUnitIds;
        }
    }
}