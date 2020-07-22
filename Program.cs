using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace SlackDataParser
{
    class Program
    {
        const string FullFolder = @"Full/";
        const string ThumbFolder = @"Thumb/";
        static string[,] Users = {
            {"USLACKBOT","SlackBot"},
            {"UTestUser","TestUser"}
        };

        static void Main(string[] args)
        {
            GenerateConversationThread();
        }

        private static void GenerateConversationThread()
        {
            var directories = Directory.GetDirectories("Data");

            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(directory);
                var projectName = directoryInfo.Name;
                var outputFolder = directory + "/";

                var outputFilePath = outputFolder + projectName + ".html";
                var fullOutputFolder = outputFolder + FullFolder;
                var thumbOutputFolder = outputFolder + ThumbFolder;

                Console.WriteLine("-----------------");
                Console.WriteLine("Start processing " + outputFolder);

                // Delete existing file and directories
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                var fullOutputFolderInfo = new DirectoryInfo(fullOutputFolder);
                if (Directory.Exists(fullOutputFolder))
                {
                    foreach (FileInfo file in fullOutputFolderInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in fullOutputFolderInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    Directory.Delete(fullOutputFolder);
                }

                var thumbOutputFolderInfo = new DirectoryInfo(thumbOutputFolder);
                if (Directory.Exists(thumbOutputFolder))
                {
                    foreach (FileInfo file in thumbOutputFolderInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in thumbOutputFolderInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    Directory.Delete(thumbOutputFolder);
                }

                var files = Directory.GetFiles(outputFolder);

                // Add users to dictionary
                var users = new Dictionary<string, string>();
                for (int i = 0; i < Users.GetLength(0); i++)
                {
                    users.Add(Users[i, 0], Users[i, 1]);
                }

                List<Message> messages = new List<Message>();

                // Loop through all files
                foreach (var file in files)
                {
                    // Only process json file
                    if (Path.GetExtension(file) != ".json")
                    {
                        continue;
                    }

                    Console.WriteLine("Parsing: " + file);

                    // Read file and escape newline character
                    var json = File.ReadAllText(file)
                        .Replace(@"\n", "<br>");

                    // Convert json to POCO
                    messages.AddRange(JsonConvert.DeserializeObject<Message[]>(json));
                }

                // Sort messages by date
                var sortedMessages = messages.OrderBy(x => x.ts);

                // Compile all text
                using (var outputFile = new StreamWriter(outputFilePath, true))
                {
                    // Write html style and open html body
                    var htmlStyle = "<style>body {font-size: 15px;font-family: NotoSansJP, Slack-Lato, appleLogo, sans-serif;}.message {margin-bottom: 15px;}.message-sender {font-weight: 900;margin-right: 5px;}.message-time,.attachment-metadata {color: #616061;font-size: 12px;}.message-body {display: inline-block}</style>";
                    outputFile.Write(htmlStyle);
                    outputFile.WriteLine("<body>");

                    // Loop through messages
                    foreach (var message in sortedMessages)
                    {
                        var htmlMessage = "<div class='message'><div><span class='message-sender'>{0}</span><span class='message-time'>{1}</span></div>";
                        outputFile.WriteLine(string.Format(htmlMessage, users[message.user], message.ts));

                        // Show messages without attachment
                        if (message.files == null || !message.files.Any())
                        {
                            var htmlMessageText = "<span class='message-body'>{0}</span>";
                            outputFile.WriteLine(string.Format(htmlMessageText, message.text));

                            // Close html message
                            outputFile.WriteLine("</div>");
                            continue;
                        }

                        // Show messages with attachment
                        foreach (var fileAttachment in message.files)
                        {
                            // Construct unique file name
                            var guid = Guid.NewGuid().ToString();
                            var uniqueId = guid.Substring(guid.Length - 12);
                            var fileName = fileAttachment.title + " - " + uniqueId + "." + fileAttachment.filetype;

                            // Dowload full
                            DownloadAttachment(fileAttachment.url_private_download, fullOutputFolder, fileName);

                            // Attachment is movie
                            if (fileAttachment.filetype == "mov" ||
                                fileAttachment.filetype == "mp4")
                            {
                                var htmlMessageAttachmentMov = "<div><span class='attachment-metadata'>{0}</span><div><video controls='controls' width='800' height='600' src='{1}'></video></div></div>";
                                outputFile.WriteLine(string.Format(htmlMessageAttachmentMov, fileAttachment.title, FullFolder + fileName));
                                continue;
                            }

                            // Attachment is pdf
                            if (fileAttachment.filetype == "pdf")
                            {
                                var htmlMessageAttachmentPdf = "<div><span class='attachment-metadata'>{0}</span><div><a target='_blank' href='{1}'>Full size PDF</a></div><embed type='application/pdf' width='800px' height='600px' src='{1}' /></div>";
                                outputFile.WriteLine(string.Format(htmlMessageAttachmentPdf, fileAttachment.title, FullFolder + fileName));
                                continue;
                            }

                            // Attachment is picture
                            if (fileAttachment.filetype == "png" ||
                                fileAttachment.filetype == "jpg" ||
                                fileAttachment.filetype == "gif")
                            {
                                // Download thumb
                                DownloadAttachment(fileAttachment.thumb_360, thumbOutputFolder, fileName);

                                var htmlMessageAttachmentPic = "<div><span class='attachment-metadata'>{0}</span><div><a target='_blank' href='{1}'><img src='{2}'></a></div></div>";
                                outputFile.WriteLine(string.Format(htmlMessageAttachmentPic, fileAttachment.title, FullFolder + fileName, ThumbFolder + fileName));
                                continue;
                            }

                            // All other files
                            var htmlMessageAttachmentOther = "<div><span class='attachment-metadata'>{0}</span><div><a target='_blank' href='{1}'>{0}</a></div></div>";
                            outputFile.WriteLine(string.Format(htmlMessageAttachmentOther, fileAttachment.title, FullFolder + fileName));
                        }

                        // Close html message
                        outputFile.WriteLine("</div>");
                    };

                    // Close html body
                    outputFile.Write("</body>");
                }

                Console.WriteLine("Finished processing " + outputFolder);
                Console.WriteLine("--------------------");
            }
        }

        private static void DownloadAttachment(string url, string outputFolder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            // Create output folder if it doesn't exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Download url
            var downloadUrl = url;

            // Attachement file path
            var attachementFilePath = outputFolder + fileName;

            // Download attachment
            Console.WriteLine("Downloading from: " + downloadUrl);
            WebClient Client = new WebClient();
            Client.DownloadFile(downloadUrl, attachementFilePath);
        }
    }
}
