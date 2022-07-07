# Google Directory API and Google Drive API Access Methods

This repository provides utility methods and sample usage for the Google Directory API and for the Google Drive API. Proper credentials are needed (and must be saved to a file called 'credentials.json' in the same directory as the class accessing them). Credentials are generated through the project's Google Cloud Console project page.

In this repository:
- GoogleDirectory.cs: sample usage of accessing Directory API provided methods/classes
- GoogleDirectoryUtils.cs: general, abstracted methods to access directory API, including sample usage in Testing class
- GoogleDirectoryInfoObjects.cs: classes that hold information that would be necessary for the Util methods
- GoogleDirectoryDataAccess.cs & GoogleDirectoryServices.cs: sample service-oriented flow for using GoogleDirectory API util methods (starting with service layer, then data access layer, then utility layer)
- GoogleDrive.cs: sample usage of accessing Drive API provided methods/classes
- Validation.cs: sample external layer that can be injected into service-oriented flow for validation of user input

Documentation:

- Directory API (Google Documentation): https://developers.google.com/admin-sdk/directory/v1/guides
- SDK Directory API Method Documentation: https://googleapis.dev/dotnet/Google.Apis.Admin.Directory.directory_v1/latest/api/Google.Apis.Admin.Directory.directory_v1.html
- SDK Drive API Method Documentation: https://googleapis.dev/dotnet/Google.Apis.Drive.v3/latest/api/Google.Apis.Drive.v3.html 
