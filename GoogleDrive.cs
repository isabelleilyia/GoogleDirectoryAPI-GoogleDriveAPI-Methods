using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.Json;

namespace GoogleDriveTesting
{
    class Authentication
    {

        /* Global instance of the scopes.
                If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Drive API Testing";

        public static DriveService authenticate()
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
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            return service;

        }
    }
    public class driveTesting
    {

        public static void testDrive()
        {
            var service = Authentication.authenticate();

            //Creating and searching for a folder


            // Create a new folder on drive.
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Sample Folder",
                MimeType = "application/vnd.google-apps.folder"
            };
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();
            // Prints the created folder id.
            Console.WriteLine("Folder ID: " + file.Id);

            //Create a folder inside the folder created
            var file2MetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Nested folder",
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>(),
            };
            file2MetaData.Parents.Add("1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6");
            var request2 = service.Files.Create(file2MetaData);
            request.Fields = "id";
            var newFolder = request2.Execute();
            Console.WriteLine("Created nested folder: " + newFolder.Id);

            //Search for folder
            FilesResource.ListRequest fileSearchRequest = new FilesResource.ListRequest(service) { Q = "name contains 'Sample'" };
            Google.Apis.Drive.v3.Data.FileList fileSearch = fileSearchRequest.Execute();
            Console.WriteLine("Search results: " + JsonSerializer.Serialize(fileSearch.Files[0].Name));


            //Permissions


            //Create a new permission for everyone in a group to be a writer to the file 
            //https://developers.google.com/drive/api/guides/ref-roles (what each role can do)
            Google.Apis.Drive.v3.Data.File file1 = service.Files.Get("1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6").Execute();
            Google.Apis.Drive.v3.Data.Permission newPermissions = new Google.Apis.Drive.v3.Data.Permission() { Type = "group", EmailAddress = "apcsa@oneuseredu.com", Role = "writer" };
            PermissionsResource.CreateRequest createPermissionRequest = new PermissionsResource.CreateRequest(service, newPermissions, file1.Id);
            // FilesResource.UpdateRequest updateFileRequest = new FilesResource.UpdateRequest(service, file.Id);
            var permissionRequest = createPermissionRequest.Execute();
            Console.WriteLine("Created Permission: " + JsonSerializer.Serialize(permissionRequest));

            //Create multiple permissions using a batch request
            var ids = new List<String>();
            var batch = new BatchRequest(service);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    // Handle error
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            };
            Permission groupPermission1 = new Permission()
            {
                Type = "group",
                Role = "writer",
                EmailAddress = "tech@oneuseredu.com"
            };
            var request3 = service.Permissions.Create(groupPermission1, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6");
            request.Fields = "id";
            batch.Queue(request3, callback);
            Permission groupPermission2 = new Permission()
            {
                Type = "group",
                Role = "reader",
                EmailAddress = "testgroup@oneuseredu.com"
            };
            request3 = service.Permissions.Create(groupPermission2, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6");
            request3.Fields = "id";
            batch.Queue(request, callback);
            var task = batch.ExecuteAsync();
            task.Wait();


            //Get permissions of folder
            Google.Apis.Drive.v3.Data.PermissionList permissions = service.Permissions.List("1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6").Execute();
            Console.WriteLine("Permissions of sample folder: " + JsonSerializer.Serialize(permissions));

            //Get capabilities of folder (abilities that current user has on file)
            FilesResource.GetRequest fileRequest = new FilesResource.GetRequest(service, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6") { Fields = "permissionIds" };
            var capabilitiesList = fileRequest.Execute();
            Console.WriteLine("Capabiities of folder: " + JsonSerializer.Serialize(capabilitiesList));

            //Change permissions of folder
            //Get old permissions
            Google.Apis.Drive.v3.Data.Permission oldPermission = new PermissionsResource.GetRequest(service, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6", "05050847964769348749") { Fields = "emailAddress, id" }.Execute();
            Console.WriteLine("Old permission: " + JsonSerializer.Serialize(oldPermission));
            Google.Apis.Drive.v3.Data.Permission newPermission = new Google.Apis.Drive.v3.Data.Permission() { Role = "reader" };
            PermissionsResource.UpdateRequest updatePermissionsRequest = new PermissionsResource.UpdateRequest(service, newPermission, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6", oldPermission.Id);
            var updatedPermissions = updatePermissionsRequest.Execute();
            Console.WriteLine("New permissions: " + JsonSerializer.Serialize(updatedPermissions));

            //Delete permissions of a folder
            PermissionsResource.DeleteRequest deletePermissionsRequest = new PermissionsResource.DeleteRequest(service, "1-uVnLRRtkmgBS5gAqKyvNz5VlvcjAfz6", "05050847964769348749");
            deletePermissionsRequest.Execute();
            Console.WriteLine("Deleted APCSA access to folder!");




            //Comments
            //For certain fields to show in get/list request i.e. capabilities or email address, they must be specified using Fields property in request object
            //all fields of File object: https://developers.google.com/drive/api/v3/reference/files

            //Capabilities vs Permissions: kinda confusing but here's what I understood
            //capabilities are in general what you want to be done to the file; for example, you don't want anyone to be able to delete it, regardless of permission
            //permissions are what you want one specific user/group to be able to do; 
            //permissions and capabilities must match for a specific action in order for it to be executed.
        }
    }
}