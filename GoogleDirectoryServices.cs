using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System;
using GoogleDirectoryInfoObjects;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using GoogleDirectoryUtils;
using GoogleDirectoryDataAccess;
using ValidationMethods;
using System.Text.Json;



namespace GoogleDirectoryServices
{
    class OrgUnitServices
    {
        public OrgUnitDataAccess orgDataAccess;

        public OrgUnitServices()
        {
            orgDataAccess = new OrgUnitDataAccess();
        }

        public OrgUnit addGradeLevel(OUInfo ouInfo)
        {
            string gradYearStr = ouInfo.orgName;
            int gradYear = int.Parse(gradYearStr);
            bool valid = Validation.validateGradeNumber(gradYear);
            if (valid)
            {
                string orgUnitPath = ouInfo.parentOrgUnitPath + "/" + gradYearStr;
                ouInfo.orgUnitPath = orgUnitPath;
                OrgUnit grade = null;
                try
                {
                    grade = orgDataAccess.checkGradeExistence(ouInfo);
                    Console.WriteLine("Grade already exists!");
                    return grade;
                }
                catch
                {
                    Console.WriteLine("Grade is valid!");
                }
                OrgUnit added = orgDataAccess.addGrade(ouInfo);
                Console.WriteLine("Added grade level!");
                return added;


            }
            else
            {
                Console.WriteLine("Invalid grade level!");
                return null;
            }

        }

        public OrgUnits GetGrades(OUInfo OUInfo)
        {
            OrgUnits grades = orgDataAccess.getGrades(new OUInfo() { orgUnitPath = OUInfo.orgUnitPath });
            return grades;

        }



    }


    class UserServices
    {
        public UserDataAccess userDataAccess;

        public UserServices()
        {
            userDataAccess = new UserDataAccess();
        }

        public string GetUserSchool(UserInfo userInfo)
        {
            UserDataAccess userDataAccess = new UserDataAccess();
            string schoolName = userDataAccess.GetUserSchool(new UserInfo() { userKey = userInfo.userKey });
            return schoolName;
        }

        public int GetNumStudentsInGrade(OUInfo OUInfo)
        {
            UserDataAccess userDataAccess = new UserDataAccess();
            OUInfo ouInfo = new OUInfo() { orgUnitPath = OUInfo.orgUnitPath };
            return userDataAccess.getMembersInGrade(ouInfo).UsersValue.Count;
        }
    }

    class Testing
    {
        public static void Main(string[] args)
        {
            OUInfo newGrade = new OUInfo() { orgName = "2023", parentOrgUnitPath = "/Dhahran High School" };
            OrgUnitServices orgUnitServices = new OrgUnitServices();
            OrgUnit returned = orgUnitServices.addGradeLevel(newGrade);
            Console.WriteLine(returned);

        }
    }
}