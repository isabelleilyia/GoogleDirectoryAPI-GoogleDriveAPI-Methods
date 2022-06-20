using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;

using System;
using System.Linq;


namespace GoogleDirectoryInfoObjects
{
    class OUInfo
    {
        public string orgName { get; set; }
        public string parentOrgUnitPath { get; set; }
        public string description
        { get; set; }

        //OrgPath should not include leading "/"
        private string orgUnitPathValue; //backing variable
        public string orgUnitPath
        {
            get
            {
                return orgUnitPathValue;
            }
            set
            {
                if (value.Substring(0, 1) == "/")
                {
                    value = value.Substring(1);
                }
                orgUnitPathValue = value;

            }
        }
        //fieldToEdit should match property name (case sensitive, start with capital letter)
        private string fieldToEditValue; //backing variable
        public string fieldToEdit
        {
            get { return fieldToEditValue; }
            set
            {

                string[] validFields = { "Name", "Description", "OrgUnitPath", "ParentOrgUnitId", "ParentOrgUnitPath" };
                if (validFields.Contains(value))
                {
                    fieldToEditValue = value;
                }
                else
                {
                    throw new ArgumentException("Invalid field name for OU");
                }
            }
        }
        public string newFieldVal { get; set; }

        //Passing in OrgunitsResource.ListRequest.TypeEnum.Children for type gets only direct children, OrgunitsResource.ListRequest.TypeEnum.All gets directories and all subdirectories
        public OrgunitsResource.ListRequest.TypeEnum queryType { get; set; }

        public OUInfo()
        {
            parentOrgUnitPath = "/";

        }


    }

    class GroupInfo
    {
        public string name { get; set; }
        public string email { get; set; }
        public string description { get; set; }

        //Can be unique id or email
        public string groupId { get; set; }

        //fieldToEdit should match property name (case sensitive, start with capital letter)
        private string fieldToEditValue; //backing variable
        public string fieldToEdit
        {
            get { return fieldToEditValue; }
            set
            {

                string[] validFields = { "Name", "Description", "Email" };
                if (validFields.Contains(value))
                {
                    fieldToEditValue = value;
                }
                else
                {
                    throw new ArgumentException("Invalid field name for Group");
                }
            }
        }
        public string newFieldVal { get; set; }
        public string newMemberEmail { get; set; }

        //Role is "MEMBER" or "OWNER" or "MANAGER" (https://developers.google.com/admin-sdk/directory/v1/guides/manage-group-members) for more info
        private string memberRoleValue; //backing variable
        public string memberRole
        {
            get { return memberRoleValue; }
            set
            {
                string[] validRoles = { "MEMBER", "OWNER", "MANAGER" };
                if (validRoles.Contains(value.ToUpper()))
                {
                    memberRoleValue = value.ToUpper();
                }
                else
                {
                    throw new ArgumentException("Invalid member role given.");
                }
            }
        }

        //Can be unique ID or email of user
        public string userKey { get; set; }
        public int maxResults { get; set; }

        // Passing in GroupsResource.ListRequest.SortOrderEnum.ASCENDING sorts in ascending order, GroupsResource.ListRequest.SortOrderEnum.DESCENDING sorts in descending order
        public GroupsResource.ListRequest.SortOrderEnum sortOrder { get; set; }

        // Next page token passed in if more results are available (accessed from the result of the previous query)
        public string pageToken { get; set; }

        // query is to query based on name or email ex: "name:{Test}* email:{test}*" -> name and email are prefixed by "text"
        // multiple search terms in query separated by whitespace
        // email & name cannot be used in same query as memberKey
        // more information on queries: https://developers.google.com/admin-sdk/directory/v1/guides/search-groups
        public string query { get; set; }
        public bool indirectMembersIncluded { get; set; }
        public string aliasValue { get; set; }

        public GroupInfo()
        {
            indirectMembersIncluded = false;
            sortOrder = GroupsResource.ListRequest.SortOrderEnum.ASCENDING;
            maxResults = System.Int32.MaxValue;
            memberRole = "MEMBER";
        }



    }

    class UserInfo
    {

        // Search information: info on each field https://googleapis.dev/dotnet/Google.Apis.Admin.Directory.directory_v1/latest/api/Google.Apis.Admin.Directory.directory_v1.UsersResource.ListRequest.html 
        // info on querying for "query" field: https://developers.google.com/admin-sdk/directory/v1/guides/search-users 
        public UsersResource.ListRequest.ViewTypeEnum viewType { get; set; }
        public UsersResource.ListRequest.SortOrderEnum sortOrder { get; set; }
        public string showDeleted { get; set; }
        public UsersResource.ListRequest.ProjectionEnum projection { get; set; }
        public string query { get; set; }
        public UsersResource.ListRequest.OrderByEnum orderBy { get; set; }
        public int maxResults { get; set; }
        public UsersResource.ListRequest.EventEnum eventIntended { get; set; }
        public string customFieldMask { get; set; }
        public string pageToken { get; set; }
        private string fieldToEditValue; //backing variable
        public string fieldToEdit
        {
            get { return fieldToEditValue; }
            set
            {

                // string[] validFields = { "Name", "Email",  };
                User user = new User();
                if (user.GetType().GetProperty(value) != null)
                {
                    fieldToEditValue = value;
                }
                else
                {
                    throw new ArgumentException("Invalid field name for User");
                }
            }
        }
        public string newFieldVal { get; set; }

        //User information
        public string userKey { get; set; }
        public UserInfo()
        {
            maxResults = System.Int32.MaxValue;
            showDeleted = "false";
        }

    }
}