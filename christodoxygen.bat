@echo off

DEL C:\Users\aeon.williams\Documents\ChrisToDoxygen\doxygenFiles /s /f /q >NUL
CALL g++ christodoxygen.cpp -lstdc++fs -o christodoxygen.exe
CALL christodoxygen.exe