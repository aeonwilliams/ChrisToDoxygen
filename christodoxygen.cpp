#include <iostream>
#include <string>
#include <experimental/filesystem> 
#include <list>
#include <fstream>
#include <locale>
#include <windows.h>
using namespace std;
namespace fs = experimental::filesystem;

//function for printing the elements in a list 
void showlist(list <string> g) 
{ 
     list <string> :: iterator it; 
    for(it = g.begin(); it != g.end(); ++it) 
        cout << *it << endl; 
    cout << '\n'; 
} 

//function for returning a specific element in a string list, starting at the 0th position
string accessItem(list <string> g, int pos)
{
    if(pos > g.size() || pos < 0)
    {
        return "null";
    }
    list <string> :: iterator it;
    int i = 0;
    for(it = g.begin(); it != g.end(); ++it)
    {
        if(i == pos)
        {
            return *it;
        }
        i++;
    }

    return "null";
}

string betterReplace(string line, string toReplace, string key)
{
    int location = line.find(toReplace);
    if(location != string::npos)
    {
        line.replace(location, toReplace.size(), key);
    }

    return line;
}

int main(void)
{
    /*
    Var definitions for the directory searching loop
    */
    list <string> listOfFiles;
    int slashPos;
    string nameOfCurrFile;
    string metaKey = ".meta";
    string csKey = ".cs";
    //INCLUDE FORWARD SLASH AT THE END, IDIOT
    string lpkDirectory = "C:/Users/aeon.williams/Documents/LPK/Assets/Scripts/LPK/";

    /**
     * Scans a given directory and store each .cs file into a linked list
    **/
    //directory of the files to scan
    string path = lpkDirectory;
    //scan the directory, file by file
    for(const auto & entry : fs::directory_iterator(path))
    {
        //store the name of the file or directory found
        nameOfCurrFile = entry.path();
        //store the position of the last '/' found
        slashPos = nameOfCurrFile.find_last_of('/');
        //truncate the path down to the name of the file and whatever comes after it
        nameOfCurrFile = nameOfCurrFile.substr(slashPos + 1);
        //check if the file is a simple .cs file - if it is, push it into the list
        if (nameOfCurrFile.find(metaKey) == string::npos && nameOfCurrFile.find(csKey) != string::npos)
        {
            listOfFiles.push_back(nameOfCurrFile);
        }
    }

    /**
     * For each .cs file in the created list, make a copy, and edit the syntax of Chris' comments to be doxygen style comments
    **/
   ifstream currFile;
   string line;
   string newLine;
   string key;
   int replaceEnd;
   int replaceStart;
   //loop through the list and edit each file
   for(int i = 0; i < listOfFiles.size(); i++)
   {  
       line = "";
       string fileName = accessItem(listOfFiles, i);
       string filePath = lpkDirectory + fileName;
       string tempFileName = (fileName.substr(0,fileName.size() -3)) + "_temp.cs" ;
       string doxyFileName = "doxygenFiles/" + (fileName.substr(0,fileName.size() - 3)) + "_doxy.cs";

       cout << fileName << endl;
       cout << filePath << endl;
       cout << tempFileName << endl;
       cout << doxyFileName << endl;
       cout << "\n";

       //create a temp copy of the file for safety
       CopyFile(filePath.c_str(), tempFileName.c_str(), TRUE);
       currFile.open(tempFileName.c_str());

       //create a file to store the doxygen-ized version of the current file
       ofstream doxyFile;
       remove(doxyFileName.c_str());
       doxyFile.open(doxyFileName.c_str());

       //parse through each line of the file to make changes as needed
       if(currFile.is_open())
       {
           while(getline (currFile, line))
           {
               line = betterReplace(line, "File:", "\\file");
               line = betterReplace(line, "Authors:", "\\author");
               line = betterReplace(line, "Last Updated:", "\\date");
               line = betterReplace(line, "Last Version:", "\\version");
               line = betterReplace(line, "Description:", "\\brief");
               line = betterReplace(line, "DESCRIPTION  :", "\\brief");
               line = betterReplace(line, "Copyright", "\\copyright");
               line = betterReplace(line, "CLASS NAME  :", "\\class");
               line = betterReplace(line, "DESCRIPTION :", "\\brief");
               line = betterReplace(line, "FUNCTION NAME:", "\\fn");
               line = betterReplace(line, "INPUTS       : None", "");
               line = betterReplace(line, "OUTPUTS      : None", "");
               line = betterReplace(line, "INPUTS       :", "\\param");
               line = betterReplace(line, "OUTPUTS      :", "\\return");

               doxyFile << line << endl;
           }
       }

        currFile.close();
       //delete the temp file at the end
       remove(tempFileName.c_str());
   }

    return 0;
}