using Google.Apis.Auth.OAuth2;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;

using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.Json;


namespace GoogleApiTesting
{
    class Authentication
    {

        /* Global instance of the scopes.
                If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { DirectoryService.Scope.AdminDirectoryOrgunit, DirectoryService.Scope.AdminDirectoryGroup, DirectoryService.Scope.AdminDirectoryUser, DirectoryService.Scope.AdminDirectoryUserschema };
        static string ApplicationName = "Google Directory API Testing";

        public static DirectoryService authenticate()
        {
            UserCredential credential;
            // Load client secrets.
            using (var stream =
                   new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                /* The file token.json stores the user's access and refresh tokens, and is created
                 automatically when the authorization flow completes for the first time. */
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Directory API service.
            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            return service;

        }
    }
    // Class to demonstrate the use of Directory users list API
    class OUTesting
    {
        public static void testOUs()
        {

            var service = Authentication.authenticate();


            //Adding, Modifying, Deleting data

            // Add new OU under root of directory (school name)
            OrgUnit dhahranHighSchool = new OrgUnit { Name = "Dhahran High School", ParentOrgUnitPath = "/" };
            OrgunitsResource.InsertRequest request = service.Orgunits.Insert(dhahranHighSchool, "my_customer");
            var org = request.Execute();
            Console.WriteLine("Added Organizational Unit Details: " + JsonSerializer.Serialize(org));

            // Add new OU under school name for specific grade
            OrgUnit classOf2022 = new OrgUnit { Name = "2022", ParentOrgUnitPath = "/Dhahran High School" };
            OrgunitsResource.InsertRequest insertRequest = service.Orgunits.Insert(classOf2022, "my_customer");
            var subOrg = insertRequest.Execute();
            Console.WriteLine("Sub Organizational Unit Details: " + JsonSerializer.Serialize(subOrg));

            // Retrieve OU information for class of 2022
            // Update Description of OU 
            OrgUnit toUpdate = service.Orgunits.Get("my_customer", "Dhahran High School/2022").Execute();
            string orgUnitPath = toUpdate.OrgUnitPath.Substring(1);
            toUpdate.Description = "Seniors";
            OrgunitsResource.UpdateRequest updateRequest = service.Orgunits.Update(toUpdate, "my_customer", orgUnitPath);
            var updatedOrg = updateRequest.Execute();
            Console.WriteLine("Sub Organizational Update Details: " + JsonSerializer.Serialize(updatedOrg));

            // Retrieve OU information for class of 2022
            // Delete OU
            OrgUnit toDelete = service.Orgunits.Get("my_customer", "Dhahran High School/2022").Execute();
            string orgUnitPath2 = toDelete.OrgUnitPath.Substring(1);
            OrgunitsResource.DeleteRequest deleteRequest = service.Orgunits.Delete("my_customer", orgUnitPath2);
            var result = deleteRequest.Execute();
            Console.WriteLine("Deleted class of 2022 from Dhahran High School.");


            //Retrieving Data


            //Retrieve all immediate children of root of directory (schools)
            OrgunitsResource.ListRequest listRequest = service.Orgunits.List("my_customer");
            OrgUnits orgList = listRequest.Execute();
            Console.WriteLine("Listing main OUs in directory:");
            foreach (OrgUnit ou in orgList.OrganizationUnits)
            {
                Console.WriteLine("OU " + ou.Name + ": " + JsonSerializer.Serialize(ou));
            }

            //Retrieve all directories and sub-directories
            OrgunitsResource.ListRequest listRequest2 = new OrgunitsResource.ListRequest(service, "my_customer") { Type = OrgunitsResource.ListRequest.TypeEnum.All };
            OrgUnits allUnits = listRequest2.Execute();
            Console.WriteLine("Listing all OUs in directory:");
            foreach (OrgUnit ou in allUnits.OrganizationUnits)
            {
                Console.WriteLine("OU " + ou.Name + ": " + JsonSerializer.Serialize(ou));
            }

            //Retrieve all directories under one school 
            OrgunitsResource.ListRequest listRequest3 = new OrgunitsResource.ListRequest(service, "my_customer") { OrgUnitPath = "Washington HS" };
            OrgUnits subUnits = listRequest3.Execute();
            Console.WriteLine("Listing all OUs under Washington HS:");
            foreach (OrgUnit ou in subUnits.OrganizationUnits)
            {
                Console.WriteLine("OU " + ou.Name + ": " + JsonSerializer.Serialize(ou));
            }



            //Comments 


            //If multiple requests are attempted within one execution of the code, it errors and says that 
            //Login is required (i.e. adding an OU and sub OU at the same time);

            //From what I've seen so far, there are no methods to query for specific OUs except in terms of their path like there is for groups


        }


    }

    class GroupTesting
    {

        public static void testGroups()
        {
            var service = Authentication.authenticate();


            //Adding, Modifying, Deleting groups


            //Create a new group
            Group APCSA = new Group() { Name = "AP Computer Science A", Description = "Morning section", Email = "apcsa@oneuseredu.com" };
            GroupsResource.InsertRequest request = service.Groups.Insert(APCSA);
            var group = request.Execute();
            Console.WriteLine("Added Group for class: " + JsonSerializer.Serialize(group));

            //Edit description of group
            Group toUpdate = service.Groups.Get("apcsa@oneuseredu.com").Execute();
            string id = toUpdate.Id;
            toUpdate.Description = "Afternoon section";
            GroupsResource.UpdateRequest updateRequest = service.Groups.Update(toUpdate, id);
            var updatedGroup = updateRequest.Execute();
            Console.WriteLine("Updated group: " + JsonSerializer.Serialize(updatedGroup));

            //Add user as member of group
            Group toUpdate2 = service.Groups.Get("apcsa@oneuseredu.com").Execute();
            User toAdd = service.Users.Get("isabelle@oneuseredu.com").Execute();
            Member newMember = new Member() { Email = toAdd.Emails[0].Address, Role = "MEMBER" };
            MembersResource.InsertRequest addMember = service.Members.Insert(newMember, toUpdate2.Id);
            var addedMember = addMember.Execute();
            Console.WriteLine("Added member to group: " + JsonSerializer.Serialize(addedMember));

            //Add group as member of another group
            Member newGroupmember = new Member() { Email = "apcsa@oneuseredu.com", Role = "MEMBER" };
            Member addedGroupMember = service.Members.Insert(newGroupmember, "tech@oneuseredu.com").Execute();
            Console.WriteLine("Added group APCSA to group Tech: " + JsonSerializer.Serialize(addedGroupMember));

            //Delete group
            service.Groups.Delete("apcsa@oneuseredu.com").Execute();
            Console.WriteLine("APCSA deleted!");


            //Group Aliases

            //Add alias to group
            Alias newAlias = new Alias() { PrimaryEmail = "apcsa@oneuseredu.com", AliasValue = "alias@oneuseredu.com" };
            GroupsResource.AliasesResource.InsertRequest addAlias = new GroupsResource.AliasesResource.InsertRequest(service, newAlias, "apcsa@oneuseredu.com");
            var addedAlias = addAlias.Execute();
            Console.WriteLine("Added alias to AP CSA: " + JsonSerializer.Serialize(addedAlias));

            //Get aliases of group
            GroupsResource.AliasesResource.ListRequest csaAliasesRequest = new GroupsResource.AliasesResource.ListRequest(service, "apcsa@oneuseredu.com");
            Aliases csaAliases = csaAliasesRequest.Execute();
            Console.WriteLine("AP CSA aliases: " + JsonSerializer.Serialize(csaAliases));

            //Delete alias of group
            GroupsResource.AliasesResource.DeleteRequest deleteAliasRequest = new GroupsResource.AliasesResource.DeleteRequest(service, "apcsa@oneuseredu.com", "alias2@oneuseredu.com");
            deleteAliasRequest.Execute();
            Console.WriteLine("Deleted alias!");



            //Retrieving groups (GET and LIST)

            //Retrieve groups of one member
            GroupsResource.ListRequest isabelleGroupRequest = new GroupsResource.ListRequest(service) { UserKey = "isabelle@oneuseredu.com" };
            Groups isabelleGroups = isabelleGroupRequest.Execute();
            Console.WriteLine(JsonSerializer.Serialize(isabelleGroups)); //will give groups that user is DIRECT member of, not if user is member of group that is member of group

            //Retrieve all groups in domain, max number is 3, and ordering by email (descending)
            GroupsResource.ListRequest sortedGroupsRequest = new GroupsResource.ListRequest(service) { Domain = "oneuseredu.com", MaxResults = 3, OrderBy = GroupsResource.ListRequest.OrderByEnum.Email, SortOrder = GroupsResource.ListRequest.SortOrderEnum.DESCENDING };
            // Only value available to sort by is email
            Groups sortedGroups = sortedGroupsRequest.Execute();
            Console.WriteLine("Sorted list of groups: ");
            foreach (Group g in sortedGroups.GroupsValue)
            {
                Console.WriteLine(JsonSerializer.Serialize(g));
            }

            //Retrieve groups separately in domain using page tokens (used for large responses)
            GroupsResource.ListRequest largeRequest = new GroupsResource.ListRequest(service) { Customer = "my_customer", MaxResults = 1 }; //if neither Customer or Domain properties included, 400 bad request thrown
            Groups groups1 = largeRequest.Execute();
            Console.WriteLine("First set of groups: " + JsonSerializer.Serialize(groups1));
            Console.WriteLine("Next page token: " + groups1.NextPageToken);
            //Get next page
            largeRequest.PageToken = groups1.NextPageToken;
            Groups groups2 = largeRequest.Execute();
            Console.WriteLine("Second set of groups: " + JsonSerializer.Serialize(groups2));

            //Search for groups: email & name cannot be used in same query as memberKey
            string query = "email=apcsa@oneuseredu.com"; //if group is not found, returns object with "GroupsValue" equal to null
            GroupsResource.ListRequest searchRequest = new GroupsResource.ListRequest(service) { Customer = "my_customer", Query = query };
            Groups response = searchRequest.Execute();
            Console.WriteLine("Searching for group's email: " + JsonSerializer.Serialize(response));
            query = "name:{Test}* email:{test}*"; //multiple search terms separated by whitespace
            searchRequest.Query = query;
            response = searchRequest.Execute();
            Console.WriteLine("Searching for groups using prefix of name and email: " + JsonSerializer.Serialize(response));
            query = "memberKey=isabelle@oneuseredu.com"; //only = operator can be used with memberKey
            searchRequest.Query = query;
            response = searchRequest.Execute();
            Console.WriteLine("Searching for groups using email of member: " + JsonSerializer.Serialize(response));

            //Retrieve members of one group
            Members techMembers = service.Members.List("tech@oneuseredu.com").Execute();
            Console.WriteLine("Tech group members: " + JsonSerializer.Serialize(techMembers)); //only gives direct members (not members of group that is a member)
            MembersResource.ListRequest apcsaMembersRequest = new MembersResource.ListRequest(service, "apcsa@oneuseredu.com") { Roles = "MEMBER" };
            Members apcsaMembers = apcsaMembersRequest.Execute();
            Console.WriteLine("APCSA members: " + JsonSerializer.Serialize(apcsaMembers));

        }
    }

    class userTesting
    {
        public static void testUsers()
        {
            var service = Authentication.authenticate();

            //Retrieve 3 users, sorting by last name in descending order, only including non-custom fields
            UsersResource.ListRequest getAllUsers = new UsersResource.ListRequest(service) { Customer = "my_customer", Projection = UsersResource.ListRequest.ProjectionEnum.Basic, MaxResults = 3, SortOrder = UsersResource.ListRequest.SortOrderEnum.DESCENDING, OrderBy = UsersResource.ListRequest.OrderByEnum.FamilyName };
            Users allUsers = getAllUsers.Execute();
            Console.WriteLine("All users: ");
            foreach (User u in allUsers.UsersValue)
            {
                Console.WriteLine(JsonSerializer.Serialize(u.Name));
            }

            //Searching for users

            //Users that contain string in name
            UsersResource.ListRequest searchRequest1 = new UsersResource.ListRequest(service) { Customer = "my_customer", Query = "name:isabelle email:{isabelle}*" }; //givenName + familyName contains 'Isabelle', email begins with 'isabelle'
            Users search1 = searchRequest1.Execute();
            Console.WriteLine("Users that include 'Isabelle' in name and email: " + JsonSerializer.Serialize(search1));

            // //Get all super administrators
            UsersResource.ListRequest searchRequest2 = new UsersResource.ListRequest(service) { Customer = "my_customer", Query = "isAdmin=true" }; //user is superadmin
            Users search2 = searchRequest2.Execute();
            Console.WriteLine("Super administrators: " + JsonSerializer.Serialize(search2));

            //Get all users in Dhahran High School, should be empty
            UsersResource.ListRequest searchRequest3 = new UsersResource.ListRequest(service) { Customer = "my_customer", Query = "orgUnitPath='/Dhahran High School'" }; //gets all users PAST this point
            Users search3 = searchRequest3.Execute();
            Console.WriteLine("All users in Dhahran High School: " + JsonSerializer.Serialize(search3));

            // //Get all users in organiation
            UsersResource.ListRequest searchRequest4 = new UsersResource.ListRequest(service) { Customer = "my_customer", Query = "orgUnitPath='/'" }; //all users in organization
            Users search4 = searchRequest4.Execute();
            Console.WriteLine("All users in organization: " + JsonSerializer.Serialize(search4));
            foreach (User u in search4.UsersValue)
            {
                Console.WriteLine(JsonSerializer.Serialize(u.Name));
            }

            //Get user and find OU
            User isabelle = new UsersResource.GetRequest(service, "isabelle@oneuseredu.com").Execute();
            Console.WriteLine("Isabelle's org unit path: " + isabelle.OrgUnitPath);
            //Get all groups user is a part of
            GroupsResource.ListRequest isabelleGroupRequest = new GroupsResource.ListRequest(service) { Customer = "my_customer", Query = "memberKey='isabelle@oneuseredu.com'" };
            Groups isabelleGroups = isabelleGroupRequest.Execute();
            Console.WriteLine("Isabelle's groups: ");
            foreach (Group g in isabelleGroups.GroupsValue)
            {
                Console.WriteLine(g.Name);
            }

            //Get all schemas
            Schemas schemas = service.Schemas.List("my_customer").Execute();
            Console.WriteLine("All schemas: " + JsonSerializer.Serialize(schemas));

            //Add custom schema 
            SchemaFieldSpec field1 = new SchemaFieldSpec() { DisplayName = "location", FieldName = "location", FieldType = "STRING" };
            IList<SchemaFieldSpec> fields = new List<SchemaFieldSpec>();
            fields.Add(field1);
            Schema info = new Schema() { DisplayName = "Information", SchemaName = "Information", Fields = fields };
            SchemasResource.InsertRequest addSchemaRequest = new SchemasResource.InsertRequest(service, info, "my_customer");
            var addedSchema = addSchemaRequest.Execute();
            Console.WriteLine("Added schema: " + JsonSerializer.Serialize(addedSchema));


            //Creating and updating users


            //Create new user
            List<UserEmail> userEmails = new List<UserEmail>() { new UserEmail() { Address = "newUser@oneuseredu.com", Primary = true } };
            UserGender userGender = new UserGender() { Type = "female" };
            UserKeyword userKeyword = new UserKeyword() { Type = "occupation", Value = "teacher" };
            UserLanguage userLanguage = new UserLanguage() { LanguageCode = "en" };
            UserLocation userLocation = new UserLocation() { Area = "Mountain View, CA", BuildingId = "23", DeskCode = "5" };
            UserName userName = new UserName() { GivenName = "Jane", FamilyName = "Doe" };
            User newUser = new User()
            {
                Emails = userEmails,
                Gender = userGender,
                Keywords = new List<UserKeyword>() { userKeyword },
                Languages = new List<UserLanguage>() { userLanguage },
                Locations = new List<UserLocation>() { userLocation },
                Name = userName,
                OrgUnitPath = "/Dhahran High School/2022",
                Password = "SamplePassword123",
                PrimaryEmail = "newUser@oneuseredu.com",
            };
            UsersResource.InsertRequest newUserRequest = new UsersResource.InsertRequest(service, newUser);
            var addedUser = newUserRequest.Execute();
            Console.WriteLine("Added user: " + JsonSerializer.Serialize(addedUser));

            //Move OU of user
            User toUpdate = service.Users.Get("newuser@oneuseredu.com").Execute();
            toUpdate.OrgUnitPath = "/Washington HS/2022";
            UsersResource.UpdateRequest moveUserRequest = new UsersResource.UpdateRequest(service, toUpdate, "newuser@oneuseredu.com");
            var movedUser = moveUserRequest.Execute();
            Console.WriteLine("Moved Jane Doe to Washington HS: " + JsonSerializer.Serialize(movedUser));

            //Add as member of group
            Group toUpdate2 = service.Groups.Get("apcsa@oneuseredu.com").Execute();
            User toAdd = service.Users.Get("newuser@oneuseredu.com").Execute();
            Member newMember = new Member() { Email = toAdd.Emails[0].Address, Role = "MEMBER" };
            MembersResource.InsertRequest addMember = service.Members.Insert(newMember, toUpdate2.Id);
            var addedMember = addMember.Execute();
            Console.WriteLine("Added member to group: " + JsonSerializer.Serialize(addedMember));

            //Move member to different group
            Member toMove = service.Members.Get("apcsa@oneuseredu.com", "newuser@oneuseredu.com").Execute();
            var added = service.Members.Insert(toMove, "tech@oneuseredu.com").Execute(); //do this first
            Console.WriteLine("Added member to new group: " + JsonSerializer.Serialize(added));
            var removed = service.Members.Delete("apcsa@oneuseredu.com", "newuser@oneuseredu.com").Execute();
            Console.WriteLine("Removed member from old group: " + JsonSerializer.Serialize(removed));
        }
    }
    class Testing
    {
        static void Main(String[] args)
        {
            //Directory API
            // OUTesting.testOUs(); //calls test methods for OUs
            // GroupTesting.testGroups(); //calls test methods for groups;
            // userTesting.testUsers(); //calls test methods for users

            //Drive API
            // GoogleDriveTesting.driveTesting.testDrive(); //tests google drive methods
        }
    }

}
