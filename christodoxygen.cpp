#include <iostream>
#include <string>
#include <experimental/filesystem> 
#include <list>
#include <iostream>
#include <fstream>
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

    /**
     * Scans a given directory and store each .cs file into a linked list
    **/
    //directory of the files to scan
    string path = "C:/Users/Nicholas/Unity LPK/Assets/Scripts/LPK";
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

    /*
    Var definitions for shit
    */
   for(int i = 0; i < listOfFiles.size(); i++)
   {
       string fileName = accessItem(listOfFiles, i);


   }


    return 0;
}