using GoogleDirectoryInfoObjects;
using GoogleDirectoryUtils;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Admin.Directory.directory_v1;

namespace GoogleDirectoryDataAccess
{

    class OrgUnitDataAccess
    {
        OrgUnitUtils orgUtils;
        public OrgUnitDataAccess()
        {
            orgUtils = new OrgUnitUtils();
        }

        public OrgUnit checkGradeExistence(OUInfo ouInfo)
        {
            OrgUnit grade = orgUtils.GetOrgUnit(ouInfo);
            return grade;
        }

        public OrgUnit addGrade(OUInfo ouInfo)
        {
            OrgUnit added = orgUtils.AddOrgUnit(ouInfo);
            return added;
        }

        public OrgUnits getGrades(OUInfo ouInfo)
        {
            return orgUtils.ListOrgUnits(ouInfo);
        }




    }

    class UserDataAccess
    {
        UserUtils userUtils;
        public UserDataAccess()
        {

            userUtils = new UserUtils();
        }

        public string GetUserSchool(UserInfo userInfo)
        {

            User current = userUtils.GetUser(userInfo);
            return current.OrgUnitPath;

        }

        public Users getMembersInGrade(OUInfo ouInfo)
        {
            string query = "orgUnitPath='/" + ouInfo.orgUnitPath + "'";
            UserInfo userInfo = new UserInfo() { query = query };
            return userUtils.SearchForUsers(userInfo);
        }

        public User addUserToGrade(UserInfo userInfo)
        {
            return userUtils.UpdateUser(userInfo);
        }





    }
}