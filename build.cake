using System.Net;
using System.Linq;

#tool nuget:?package=Wyam
#addin nuget:?package=Cake.Wyam
//#addin nuget:?package=Cake.Ftp

var target = Argument("target", "Build");
var ftpUsername = Argument("username", "");
var ftpPassword = Argument("password", "");
var ftpPathRoot = Argument("path", "");
var onlyUploadPosts = HasArgument("only_posts");

var postsOnlyExtensions = new string[] { ".html", ".xml", ".rss", ".atom" };

private void RecurseDirectories(DirectoryPath root, DirectoryPath dir) 
{
    foreach(var subDir in GetSubDirectories(dir)) 
    {
        // no need to reupload CSS, JS, and other theme files
        if (onlyUploadPosts && subDir.GetDirectoryName() == "assets") 
        {
            continue;
        }
        var relativePath = root.GetRelativePath(subDir);
        var ftpTargetPath = ftpPathRoot + relativePath;

        Information(String.Format("Creating FTP directory: {0}", ftpTargetPath));
        CreateFTPDirectory(ftpTargetPath);

        RecurseDirectories(root, subDir);
    }

    var dirPattern = String.Format("{0}/*.*", dir.FullPath);
    
    foreach(var file in GetFiles(dirPattern)) 
    {
        if(onlyUploadPosts)
        {
            if(!postsOnlyExtensions.Contains(file.GetExtension()))
            {
                continue;
            }
        }
        var relativePath = root.GetRelativePath(dir);
        var ftpTargetPath = ftpPathRoot + relativePath + "/" + file.GetFilename();
        Information(String.Format("Uploading file: {0}", ftpTargetPath));
        UploadFTPFile(ftpTargetPath, file.FullPath);
    }
}

private void UploadFTPFile(string ftpTargetPath, string localFilePath)
{
    try 
    {
        using (WebClient client = new WebClient())
        {
            client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            client.UploadFile(ftpTargetPath, WebRequestMethods.Ftp.UploadFile, localFilePath);
        }
    }
    catch(WebException wex)
    {
        var response = (FtpWebResponse)wex.Response;
        Error(response.StatusCode);
        Error(response.StatusDescription);
    }
}

private void CreateFTPDirectory(string ftpTargetPath)
{
    try 
    {
        WebRequest request = WebRequest.Create(ftpTargetPath);
        request.Method = WebRequestMethods.Ftp.MakeDirectory;
        request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
        var response = (FtpWebResponse)request.GetResponse();
        if(response.StatusCode == FtpStatusCode.PathnameCreated) 
        {
            Information("Directory created");
        }
    }
    catch(WebException wex)
    {
        var response = (FtpWebResponse)wex.Response;

        // Need to handle status code if directory already exists
        // This assumes the server returns error 550
        // Exact FTP code may vary depending on your server's implementation
        if(response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) 
        {
            Warning(response.StatusDescription);
        }
        else 
        {
            Error(response.StatusCode);
            Error(response.StatusDescription);
        }
    }
}

Task("Build")
    .Does(() =>
    {
        Wyam();        
    });
    
Task("Preview")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            Preview = true,
            Watch = true
        });        
    });

Task("Deploy")
    .IsDependentOn("Build")
    .WithCriteria(DirectoryExists("output"))
    .Does(() => 
    {
        RecurseDirectories(MakeAbsolute(Directory("output")), MakeAbsolute(Directory("output")));
    });

RunTarget(target);