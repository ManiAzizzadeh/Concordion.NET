@echo off
echo !!! You must have Gallio installed and have Gallio.Echo.exe in the PATH to run this batch file !!!
Gallio.Echo.exe /pd:Gallio-Concordion-Adapter /runner:Local /wd:. Specifications\Concordion.Spec.dll
pause