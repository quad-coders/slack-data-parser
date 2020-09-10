# What
.Net Core 3 project for parsing Slack exported conversation data into viewable HTML on browser and clickable attachments.

# Environment
- .Net Core 3
- Newtonsoft.JSON 12.0.1
- External codes from:
  - [Deserialize timestamp](https://stackoverflow.com/questions/19971494/how-to-deserialize-a-unix-timestamp-%ce%bcs-to-a-datetime-from-json/19972214#19972214)
  - [Serializing objects to CSV](https://medium.com/@utterbbq/c-serializing-list-of-objects-to-csv-9dce02519f6b)

# Instructions
1. Export data from your Slack organization, see [Slack](https://slack.com/help/articles/201658943-Export-your-workspace-data).
2. The exported data will look something like this:
```
  {Organization name}
  - channels.json
  - integration_logs.json
  - users.json
  - {channel name}
    - 2020-07-20.json
    - 2020-07-21.json
```

3. Open `users.json` from the exported data. Make note of all users' id.
4. Clone `slack-data-parser` repository.
5. Add **Data** folder in the project root.
6. Copy exported channel folder to **Data** folder. **Data** folder will looks like this:
```
  Data
  - {channel name}
    - 2020-07-20.json
    - 2020-07-21.json
```

7. Open `Program.cs`, add new entry to `Users` object to map available user id with real name, for example:
```
  static string[,] Users = {
      {"USLACKBOT", "SlackBot"},
      {"UJF8JK52", "TestUser"}
  };
```

8. On project root, run following CLI commands:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet run`

9. When finish, the app will generate new folders and files in the **Data** folder.
```
  Data
  - {channel name}
    - 2020-07-20.json
    - 2020-07-21.json
    - Full                  // Contain full (resolution) version of the attachments
    - Thumb                 // Contain thumb version of the attachments
    - {channel name}.html   // The html of all conversation categorized by channel name
```

10. Open the HTML file with any browser to view conversation for that channel. Click on attachments will open new tab in a browser to show full size of the attachments.
