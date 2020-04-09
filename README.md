# Implemented custom file logging provider according following requirements:

You will create a new project called "FileLogger" (ASP Core Console Library) which will contain three class files: a custom logger file, a custom log provider file, and an extension method file.
While many standard loggers exists (e.g., logging to the console) there are certainly cases where you want your own logging entity (e.g., you want to put log messages on a GUI).  In such a case you will need to create a custom logging setup.  Fortunately, it is not too complicated.
For our purposes, we will be creating a custom FILE logger which will output the information that we usually send to the console, to a file!
Logging Infrastructure 
Note: in order to get logging to work, you will need to install several packages (e.g., use NuGet).  For example, when you create your log provider class (which implements the ILoggerProvider interace) you will have to install the Microsoft.Extensions.Logging.Abstractions package.  This can be done either using NuGet or, as seen here, using the magic "Fix Code" button:
 
To review all installed packages (or add new ones) use the NuGet manager.  You will want to, at the very least, install:
1.	Microsoft.Extensions.Configuration.Abstractions
2.	Microsoft.Extensions.DependencyInjection
3.	Microsoft.Extensions.Hosting.Abstractions
4.	Microsoft.Extensions.Logging
5.	Microsoft.Extensions.Logging.Abstractions
6.	Microsoft.Extensions.Logging.Console
7.	Microsoft.Extensions.Logging.Debug
8.	Microsoft.Extensions.Logging.EventLog
Warning: Do not use an external file logging package, such as NLog or SeriLog.
Custom Log Provider (CustomFileLogProvider.cs)
In order for logging and the services model (discussed above) to work, you need to write two classes.  One is a "wrapper" class called a provider.  The seconds is the actual working class.  In our case, we need a log provider class and a custom logger class.
The log provider class must implement the ILoggerProvider interface.
Notes:
•	this class must create and store a custom file logger object.
•	this class should be less than a page of code
•	disposing of the custom log provider must first tell the stored logger to close its file, and then simply "give the object to the garbage collector"
Custom Logger (CustomFileLogger.cs)
The custom logger class is the place where the actual interesting "logging" code will go.  It should implement the ILogger interface. 
1.	BeginScope: you can leaves this throwing a NotImplementedException.  We won't be using it.  You should comment this method to describe what scoping is used for!  This will require a (tiny) bit of Googling.
2.	IsEnabled: you can leave this as throwing the NotImplementedException as well.  We won't be using it.  You don't need to comment it.
3.	Log: this is the core of the functionality.  In this case you should write the message to the log  file. (Note: this file should be opened in the constructor, and kept open until the Close method is called. )  The message should contain the time, the thread id, a 5 character definition of the level of log message and the message itself in the exact format as shown below.  For example:
o	2020-03-20 1:38:35 PM (1) - Infor - This message was produced by a LogTrace call
2020-03-20 1:38:35 PM (1) - Debug - This message was produced by a LogDebug call
2020-03-20 1:38:35 PM (1) - Error - This message was produced by a LogError call
4.	Constructor: open the file (in the current directory). Name the file Log_{category_name}.txt.  There should also be a flag that informs the constructor if it is to append to the file (if it exists) or replace it.
5.	Close: close the file.
Show Time And Thread String Extension
You will note that the above example file output contains the date and thread id.  You should write an extension method that appends this information to a string.  (This information can be found in Date Time - Now and the Thread - CurrenThread - ManagedThreadId).  This extension should sit in its own class (ShowTimeAndThreadClass) and be named ShowTimeAndThread.
