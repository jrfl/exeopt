I don't generally comment source that I'm not intending to release, except for comments that I need myself, so this code contains very few usful comments. (A note to remind me of something probably wont mean much to you.)

Project files are from sharp develop Agugest 2005 beta refresh. I use VS 2005 for development and only switch to sharp develop to compile to save people downloading .NET 2.0, but since sharp develop is an 8 mb download and VS 2005 is over 100, I'll distruibute this version.

Before you can build exeopt, you must run file packer to generate the Files.cs source file.

There are a number of defines available to aid in debugging. 'UseConsole' will cause exeopt to not create a gui, and will automatically start searching for patches in morrowind. It requires that all necessary files already be in the executable directory

To test individual patches, you can undefine 'fulltest', as well as defining 'partialtest' for when you want to test multiple, but not all, patches. Defining clean has no meaning in this distribution, as it requires files to exist in specific  directories.