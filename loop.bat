@echo off
set path=%windir%\Microsoft.NET\Framework\v4.0.30319;%path%
:loop
 notepad main.cs
 cls
 echo ------------------COMPILING-----------------
 csc /nologo /platform:x86 main.cs >m.txt 
 if %errorlevel% GEQ 1 (echo ---------------------ERROR----------------- & type m.txt & goto loop)
 echo -------------------OUTPUT-------------------
 main.exe
 goto loop