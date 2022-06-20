using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System;
using GoogleDirectoryInfoObjects;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;


namespace GoogleDirectoryUtils
{
    class Authentication
    {
        private string[] Scopes;
        private string ApplicationName = "Google Utils";
        public DirectoryService service { get; set; }


        public Authentication(string[] Scopes)
        {
            this.Scopes = Scopes;

            authenticate();
        }

        private void authenticate()
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

            this.service = service;
        }
    }
    class OrgUnitUtils
    {
        DirectoryService service;

        public OrgUnitUtils()
        {
            service = (new Authentication(new string[] { DirectoryService.Scope.AdminDirectoryOrgunit })).service;
        }


        //required fields: orgName, parentOrgUnitPath
        //optional fields: description
        public OrgUnit AddOrgUnit(OUInfo ouInfo)
        {
            OrgUnit toAdd = new OrgUnit { Name = ouInfo.orgName, ParentOrgUnitPath = ouInfo.parentOrgUnitPath };
            if (ouInfo.description != null)
            {
                toAdd.Description = ouInfo.description;
            }
            return service.Orgunits.Insert(toAdd, "my_customer").Execute();


        }



        //Required fields: orgUnitPath, fieldToEdit, newFieldVal
        public OrgUnit EditOrgUnit(OUInfo ouInfo)
        {
            OrgUnit toUpdate = service.Orgunits.Get("my_customer", ouInfo.orgUnitPath).Execute();
            System.Reflection.PropertyInfo propertyInfo = toUpdate.GetType().GetProperty(ouInfo.fieldToEdit);
            propertyInfo.SetValue(toUpdate, ouInfo.newFieldVal);
            return service.Orgunits.Update(toUpdate, "my_customer", ouInfo.orgUnitPath).Execute();

        }

        //Required fields: orgUnitPath
        public string DeleteOrgUnit(OUInfo ouInfo)
        {
            return service.Orgunits.Delete("my_customer", ouInfo.orgUnitPath).Execute();


        }


        //Required fields: orgUnitPath
        //Optional fields: queryType
        public OrgUnits ListOrgUnits(OUInfo ouInfo)
        {
            OrgunitsResource.ListRequest listRequest = new OrgunitsResource.ListRequest(service, "my_customer") { OrgUnitPath = ouInfo.orgUnitPath, Type = ouInfo.queryType };
            return listRequest.Execute();

        }

        //Required fields: orgUnitPath
        public OrgUnit GetOrgUnit(OUInfo ouInfo)
        {
            return service.Orgunits.Get("my_customer", ouInfo.orgUnitPath).Execute();

        }


    }



    class GroupUtils
    {
        DirectoryService service;

        public GroupUtils()
        {
            service = (new Authentication(new string[] { DirectoryService.Scope.AdminDirectoryGroup })).service;
        }

        //Required fields: name, email
        //Optional fields: description
        public Group AddGroup(GroupInfo groupInfo)
        {
            Group newGroup = new Group() { Name = groupInfo.name, Email = groupInfo.email };
            if (groupInfo.description != null)
            {
                newGroup.Description = groupInfo.description;
            }
            Group group = service.Groups.Insert(newGroup).Execute();
            return group;
        }

        //Required fields: groupId, fieldToEdit, newFieldVal
        public Group EditGroup(GroupInfo groupInfo)
        {
            Group toUpdate = service.Groups.Get(groupInfo.groupId).Execute();
            System.Reflection.PropertyInfo propertyInfo = toUpdate.GetType().GetProperty(groupInfo.fieldToEdit);
            propertyInfo.SetValue(toUpdate, groupInfo.newFieldVal);
            return service.Groups.Update(toUpdate, groupInfo.groupId).Execute();

        }

        //Required fields: groupId, newMemberEmail
        //Optional fields: memberRole
        public Member AddMemberToGroup(GroupInfo groupInfo)
        {
            Group toUpdate = service.Groups.Get(groupInfo.groupId).Execute();
            Member newMember = new Member() { Email = groupInfo.newMemberEmail, Role = groupInfo.memberRole };
            return service.Members.Insert(newMember, toUpdate.Id).Execute();
        }

        //Required fields: groupId
        public string DeleteGroup(GroupInfo groupInfo)
        {
            return service.Groups.Delete(groupInfo.groupId).Execute();
        }

        //will give groups that user is DIRECT member of, not if user is member of group that is member of group
        //Required fields: userKey
        public Groups GetGroupsOfMember(GroupInfo groupInfo)
        {
            return SearchForGroups(groupInfo);
        }

        // Returns groups values sorted by Email
        // Optional Fields: maxResults, sortOrder, query, pageToken, userKey
        public Groups SearchForGroups(GroupInfo groupInfo)
        {

            GroupsResource.ListRequest sortedGroupsRequest = new GroupsResource.ListRequest(service) { Domain = "oneuseredu.com", MaxResults = groupInfo.maxResults, OrderBy = GroupsResource.ListRequest.OrderByEnum.Email, SortOrder = groupInfo.sortOrder };
            if (groupInfo.query != "")
            {
                sortedGroupsRequest.Query = groupInfo.query;
            }
            if (groupInfo.pageToken != null)
            {
                sortedGroupsRequest.PageToken = groupInfo.pageToken;
            }
            if (groupInfo.userKey != null)
            {
                sortedGroupsRequest.UserKey = groupInfo.userKey;
            }
            return sortedGroupsRequest.Execute();

        }

        //Only includes direct members of group (not members of group that is member of group)
        //Required fields: groupId
        //Optional fields: memberRole, indirectMembersIncluded, maxResults, pageToken
        public Members GetMembersOfGroup(GroupInfo groupInfo)
        {
            MembersResource.ListRequest membersRequest = new MembersResource.ListRequest(service, groupInfo.groupId) { Roles = groupInfo.memberRole, MaxResults = groupInfo.maxResults, PageToken = groupInfo.pageToken, IncludeDerivedMembership = groupInfo.indirectMembersIncluded };
            return membersRequest.Execute();

        }

        //Aliases

        //Required Fields: email, aliasValue
        public Alias AddAliasToGroup(GroupInfo groupInfo)
        {
            Alias newAlias = new Alias() { PrimaryEmail = groupInfo.email, AliasValue = groupInfo.aliasValue };
            return service.Groups.Aliases.Insert(newAlias, groupInfo.email).Execute();
        }

        //Required Fields: groupId
        public Aliases GetAliasesOfGroup(GroupInfo groupInfo)
        {
            return service.Groups.Aliases.List(groupInfo.groupId).Execute();
        }

        //Required Fields: groupId, aliasValue
        public string DeleteAliasOfGroup(GroupInfo groupInfo)
        {
            return service.Groups.Aliases.Delete(groupInfo.groupId, groupInfo.aliasValue).Execute();
        }

    }

    class UserUtils
    {
        DirectoryService service;

        public UserUtils()
        {
            service = (new Authentication(new string[] { DirectoryService.Scope.AdminDirectoryUser })).service;
        }


        //Optional fields: projection, maxResults, sortOrder, orderBy, viewType, eventIntended, showDeleted, query, customFieldMask, pageToken
        public Users SearchForUsers(UserInfo userInfo)
        {
            UsersResource.ListRequest getUsersRequest = new UsersResource.ListRequest(service)
            {
                Customer = "my_customer",
                Projection = userInfo.projection,
                MaxResults = userInfo.maxResults,
                SortOrder = userInfo.sortOrder,
                OrderBy = userInfo.orderBy,
                ViewType = userInfo.viewType,
                Event = userInfo.eventIntended,
                ShowDeleted = userInfo.showDeleted,

            };
            if (userInfo.query != null)
            {
                getUsersRequest.Query = userInfo.query;
            }
            if (userInfo.customFieldMask != null)
            {
                getUsersRequest.CustomFieldMask = userInfo.customFieldMask;
            }
            if (userInfo.pageToken != null)
            {
                getUsersRequest.PageToken = userInfo.pageToken;
            }
            return getUsersRequest.Execute();

        }

        //Required fields: userKey
        public User GetUser(UserInfo userInfo)
        {
            return service.Users.Get(userInfo.userKey).Execute();

        }

        //Required fields: userKey, fieldToEdit, newFieldVal
        public User UpdateUser(UserInfo userInfo)
        {
            User toUpdate = service.Users.Get(userInfo.userKey).Execute();
            System.Reflection.PropertyInfo propertyInfo = toUpdate.GetType().GetProperty(userInfo.fieldToEdit);
            propertyInfo.SetValue(toUpdate, userInfo.newFieldVal);
            return service.Users.Update(toUpdate, userInfo.userKey).Execute();
        }



    }

    class Testing
    {

        // static void Main(String[] args)
        // {
        //Example usage 

        // //ORGUNITUTILS
        // OrgUnitUtils utils = new OrgUnitUtils();

        // //AddOrgUnit
        // OUInfo ouInfo = new OUInfo() { orgName = "2020", parentOrgUnitPath = "/Dhahran High School", description = "Juniors" };
        // OrgUnit newOrg = utils.AddOrgUnit(ouInfo);

        // //EditOrgUnit
        // ouInfo = new OUInfo() { orgUnitPath = "Dhahran High School/2020", fieldToEdit = "Description", newFieldVal = "Graduated" };
        // OrgUnit editedOrg = utils.EditOrgUnit(ouInfo);

        // //DeleteOrgUnit
        // ouInfo = new OUInfo() { orgUnitPath = "Dhahran High School/2020" };
        // string result = utils.DeleteOrgUnit(ouInfo);

        // //ListOrgUnits, orgUnits list accessible under list.OrganizationUnits
        // ouInfo = new OUInfo() { orgUnitPath = "", queryType = OrgunitsResource.ListRequest.TypeEnum.Children };
        // OrgUnits list = utils.ListOrgUnits(ouInfo);

        // //GetOrgUnit
        // ouInfo = new OUInfo() { orgUnitPath = "Dhahran High School/2022" };
        // OrgUnit one = utils.GetOrgUnit(ouInfo);




        //GROUPUTILS
        // GroupUtils groupUtils = new GroupUtils();

        //Add Group
        // GroupInfo groupInfo = new GroupInfo() { name = "Calculus", email = "calc@oneuseredu.com", description = "Calc sections" };
        // Group newGroup = groupUtils.AddGroup(groupInfo);

        //Edit Group
        // groupInfo = new GroupInfo() { groupId = "calc@oneuseredu.com", fieldToEdit = "Description", newFieldVal = "Morning sections" };
        // Group editedGroup = groupUtils.EditGroup(groupInfo);

        //Add member to group
        // groupInfo = new GroupInfo() { groupId = "calc@oneuseredu.com", newMemberEmail = "isabelle@oneuseredu.com", memberRole = "Owner" };
        // Member newMember = groupUtils.AddMemberToGroup(groupInfo);

        //Delete group
        // groupInfo = new GroupInfo() { groupId = "calc@oneuseredu.com" };
        // string deleteGroupResult = groupUtils.DeleteGroup(groupInfo);

        //Add alias to group
        // groupInfo = new GroupInfo() { email = "calc@oneuseredu.com", aliasValue = "aliastest@oneuseredu.com" };
        // Alias addedAlias = groupUtils.AddAliasToGroup(groupInfo);

        //Get aliases of group
        // groupInfo = new GroupInfo() { groupId = "calc@oneuseredu.com" };
        // Aliases groupAliases = groupUtils.GetAliasesOfGroup(groupInfo);

        //Delete alias of group
        // groupInfo = new GroupInfo() { groupId = "calc@oneuseredu.com", aliasValue = "aliastest@oneuseredu.com" };
        // string deleteAliasResult = groupUtils.DeleteAliasOfGroup(groupInfo);

        //Get groups of one member
        // groupInfo = new GroupInfo() { userKey = "isabelle@oneuseredu.com" };
        // Groups isabelleGroups = groupUtils.GetGroupsOfMember(groupInfo);

        //Search for groups
        // groupInfo = new GroupInfo() { maxResults = 1, sortOrder = GroupsResource.ListRequest.SortOrderEnum.DESCENDING, query = "memberKey=isabelle@oneuseredu.com" };
        // Groups searchedGroups = groupUtils.SearchForGroups(groupInfo);
        // //Get second set of results
        // groupInfo.pageToken = searchedGroups.NextPageToken;
        // Groups searchedGroups2 = groupUtils.SearchForGroups(groupInfo);

        //Get members of group
        // groupInfo = new GroupInfo() { groupId = "apcsa@oneuseredu.com", memberRole = "Member", indirectMembersIncluded = true, maxResults = 2 };
        // Members groupMembers = groupUtils.GetMembersOfGroup(groupInfo);

        // }
    }
}