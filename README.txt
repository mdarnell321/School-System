_______________________________________________________________________________________________
**************************************SOURCE SETUP GUIDE***************************************
_______________________________________________________________________________________________
 
- No need to manually import anything for DB, just have a MySQL connection ready and you are
  good to go.

- To modify source have Visual Studio installed.

_______________________________________________________________________________________________
Web Server - requires MySQL & PHP
_______________________________________________________________________________________________

- Drag 'schoolproj' directory into your web server public directory so that people may access it
  because these contain the sql queries.
	-to make a localhost webserver download XAMPP - https://www.apachefriends.org/
		-includes phpmyadmin (mysql db manager)
		-public directory is 'C:\xampp\htdocs'
			- to enable web server, turn on Apache in XAMPP
_______________________________________________________________________________________________
Watchdog - requires .NET 6.0 & NuGET package 'MySql.Data'
_______________________________________________________________________________________________

- On your server or local PC, run the 'Watchdog' program. This will automatically give zeros to people who
  dont turn in assignments on time. It also starts new semester cycles, and posts final grades.
- Adjust connection settings for your server

_______________________________________________________________________________________________
Client - requires .NET 6.0 & uses WPF for UI
_______________________________________________________________________________________________

- In DatabaseManager.cs change 'host' to your server address


_______________________________________________________________________________________________
****************************************OTHER TIPS*********************************************
_______________________________________________________________________________________________

-When creating an exam, put a '*' as the first character in an option to make it the answer
-When creating an exam, new lines are considered dividers between options
-Exams are automatically graded, but teachers must grade assignments manually.
-Once a semester ends, teachers never lose their modules (Yes that would be annoying to recreate
 them everytime). If they choose to teach that class again, the modules will be reloaded with 
 adjusted due date times based off the offset of semester begin date.
-Students may not access modules during any period not of a semester, however they may register
 for said course.

