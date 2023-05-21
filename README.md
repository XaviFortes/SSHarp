# SSHarp
## Welcome
This is an SSH / SFTP Terminal made in C# and WPF.
I wanted some substitute to common terminal emulators, so I made my own. As of right now it's pretty uncooked, so don't expect much from it.

The images are from 22/05/2023 the same day I thinked of making the project so all these functionalities were made in only one day. Pretty good isn't it?

So I'll show first how the app works

After you install it [Latest Release](https://github.com/XaviFortes/SSHarp/releases/latest)

You have the app installed

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/edabc366-e5f4-452a-8b84-be474716f9d7)

you open it and...

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/62cd4b04-15f0-41cb-83c1-b76df31f3626)

This shows up!

Here the sessions will be empty just a rectangular box with blank space. So add your own SSH session and save it

If you ask before saving it, it's saved at `%appdata%/XaviFortes/Terminal` also more files like updates, and temporal files.

After you save it, it shows on the list view of the left so click it!

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/a31e785a-2387-4edf-a303-805e0d687df5)

Nice now you have the terminal, there you have an SFTP tree view on the left where you can see all the files of the root directory of the server if you have the perms to do so.

You can open to see more deeper files

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/b7968884-e960-4180-8855-a2a94e4c76e0)

And also edit the files with right click!

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/ce6a3e5d-683b-41b9-88ff-a6beb14ee7e7)

When you click edit, it downloads the file from the SFTP session into the temporal folder of appdata and opens it with Notepad++.

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/9449d82a-c155-485e-937d-488794e940a7)

If you change something, save it and close Notepad++ so the process has exited it will check the before hash of the file and the actual hash, if it isn't the same then it changed something so...

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/06ab32a3-adfe-4b1e-98bf-bc826bb3de0d)

It asks you if you want to upload the new file to the server!

If you say Yes then it uploads the new version

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/9f82a6b5-2080-4549-b80a-2f60b0755982)

And if you edit it again then there's the change

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/bbc220f8-6374-4ee0-93b0-a1f1aac54ca7)

You can also click download and downloads the file

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/200223a7-eb45-4ab9-9820-f90c069aef3a)

Right now it leaves the file in the Desktop by default, gotta change that in the future.

Now if you want to upload a file it shows the file explorer menu so you can select the file to upload

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/d560516d-f852-4b06-8d8e-4cae8c66ceb5)

And now the main feature

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/5e251825-1749-4102-9aff-f52fb2cb71aa)


Sending commands via SSH. Just put the command and press enter or Send button

And there's the output

![imagen](https://github.com/XaviFortes/SSHarp/assets/53091080/f5617357-18a1-4f8e-b23c-01fb90f621ab)

Gotta improve the terminal emulator but it actually works in just one day. Gotta update the software asap! 

Hope you can understand how everything works rn. If you want a feature or see a bug open an issue in this repo!

Have a nice day.
