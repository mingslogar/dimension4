// Win32Window.cpp
//#include <atlbase.h>
//#include <direct.h>
#include "stdafx.h"
#include <afxwin.h>
//#include <UrlMon.h>	// URLDownloadToFile
#include "resource.h"
#include "ProgressDialog.h"
#include "NETerror.h"

bool extractResource(WORD resourceID, LPCSTR outputFilename, LPCSTR resName)
{
	bool success = false;

	try
	{
		// Locate the resource
		HRSRC hResource = FindResourceA(NULL, MAKEINTRESOURCEA(resourceID), resName);
		if (hResource == 0) { return false; } // Checking for errors

		// Load the resource
		HGLOBAL hFileResource = LoadResource(NULL, hResource);
		if (hFileResource == 0) { return false; } // Checking for errors

		// Lock the resource so other processes can't use it
		LPVOID lpFile = LockResource(hFileResource);
		if (lpFile == 0) { return false; } // Checking for errors

		// and then get the size on disk of the file.
		DWORD dwSize = SizeofResource(NULL, hResource);
		if (dwSize == 0) { return false; } // Checking for errors

		HANDLE hFile = CreateFileA(outputFilename, GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

		HANDLE hFilemap = CreateFileMapping(hFile, NULL, PAGE_READWRITE, 0, dwSize, NULL);
		if (hFilemap == 0) { return false; } // Checking for errors

		LPVOID lpBaseAddress = MapViewOfFile(hFilemap, FILE_MAP_WRITE, 0, 0, 0);

		CopyMemory(lpBaseAddress, lpFile, dwSize);

		// Unmap the file and close the handles
		UnmapViewOfFile(lpBaseAddress);
		CloseHandle(hFilemap);
		CloseHandle(hFile);

		// Everything should have gone well. Returning true.
		return true;
	}
	catch (...)
	{
		// Catch all errors
	}

	return success;
}

void exit(LPCSTR message, LPCSTR shelldll, LPCSTR setuploc, LPCSTR depend, LPCSTR tmp)
{
	MessageBoxA(NULL, message, "Dimension 4 Setup", NULL);

	if (shelldll != NULL)
	{
		// Clean up
		DeleteFileA(shelldll);
		DeleteFileA(setuploc);
		DeleteFileA(depend);
		RemoveDirectoryA(tmp);
	}

	exit(1);
}

//void installDotNet(char * tempDir)
//{
//	LPUNKNOWN pCaller = NULL;
//	LPCTSTR szURL = "http://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe";
//
//	tempDir = strcat(tempDir, "\\dotNetFx40_Full_x86_x64.exe");
//
//	LPCTSTR szFileName = tempDir;
//	DWORD dwReserved = 0;
//	LPBINDSTATUSCALLBACK lpfnCb = NULL;
//
//	URLDownloadToFile(pCaller, szURL, szFileName, dwReserved, lpfnCb);
//}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	DisableProcessWindowsGhosting();

	CRegKey reg = CRegKey(HKEY_LOCAL_MACHINE);
	LONG value = reg.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full"), KEY_READ);

	// Get the %TEMP% directory
	char tmp[512];
	DWORD nLength = 512;
	GetTempPathA(nLength, tmp);

	// Files directory
	strcat_s(tmp, "\\Dimension4Setup");

	while (PathIsDirectoryA(tmp))
		strcat_s(tmp, "_");

	//// Lang directory
	//char enUS[512];
	//strcpy_s(enUS, tmp);
	//strcat_s(enUS, "\\en-US");

	if (value == ERROR_SUCCESS)
	{
		ProgressDialog d;// = ProgressDialog();

		LPSECURITY_ATTRIBUTES dirAttribs = OF_READ;
		CreateDirectoryA(tmp, dirAttribs);
		//CreateDirectoryA(enUS, dirAttribs);

		// Increment progress
		d.ProgressBar.StepIt();

		// Declare everything needed for resource extraction
		WORD *resourceID = new WORD;
		LPCSTR *outputFilename = new LPCSTR;
		LPCSTR	*resName = new LPCSTR;

		// Microsoft.Windows.Shell.dll
		char shelldll[512];
		strcpy_s(shelldll, tmp);
		strcat_s(shelldll, "\\Microsoft.Windows.Shell.dll");

		// Setup.exe
		char setuploc[512];
		strcpy_s(setuploc, tmp);
		strcat_s(setuploc, "\\Setup.exe");

		//// Setup.resources.dll
		//char setupresources[512];
		//strcpy_s(setupresources, enUS);
		//strcat_s(setupresources, "\\Setup.resources.dll");

		// SetupDependencies.dll
		char depend[512];
		strcpy_s(depend, tmp);
		strcat_s(depend, "\\SetupDependencies.dll");

		// Give them some values
		*resourceID = 110;
		*outputFilename = shelldll;
		*resName = "FILE";

		// Call resource extraction function
		bool eSuccess = extractResource(*resourceID, *outputFilename, *resName);

		if (!eSuccess)
			exit("Error while extracting files. Setup will now exit.", shelldll, setuploc, depend, tmp);

		// Increment progress
		d.ProgressBar.StepIt();

		*resourceID = 111;
		*outputFilename = setuploc;

		eSuccess = extractResource(*resourceID, *outputFilename, *resName);

		if (!eSuccess)
			exit("Error while extracting files. Setup will now exit.", shelldll, setuploc, depend, tmp);

		// Increment progress
		d.ProgressBar.StepIt();

		*resourceID = 112;
		*outputFilename = depend;

		eSuccess = extractResource(*resourceID, *outputFilename, *resName);

		if (!eSuccess)
			exit("Error while extracting files. Setup will now exit.", shelldll, setuploc, depend, tmp);

		// Increment progress
		d.ProgressBar.StepIt();

		/**resourceID = 113;
		*outputFilename = setupresources;

		eSuccess = extractResource(*resourceID, *outputFilename, *resName);
		if (eSuccess == false)
		{
		exit("Error while extracting files. Setup will now exit.", shelldll, setuploc, depend, tmp);
		}*/

		if (!PathFileExistsA(setuploc))
			exit("Required setup files are missing. Setup will now exit.", shelldll, setuploc, depend, tmp);

		// Increment progress
		d.ProgressBar.StepIt();

		// Start the main setup program
		STARTUPINFOA startupInfo = STARTUPINFOA();
		PROCESS_INFORMATION processInfo = PROCESS_INFORMATION();
		CreateProcessA(setuploc, setuploc, NULL, NULL, FALSE, NORMAL_PRIORITY_CLASS, NULL, NULL, &startupInfo, &processInfo);

		// Change the progress bar to a marquee.
		//d.ProgressBar.SetMarquee(true, 1);

		// Wait for setup dialog to stabilize.
		//WaitForInputIdle(processInfo.hProcess, INFINITE);

		Sleep(100);

		// Close the progress dialog
		d.EndDialog(0);

		// Wait for Setup to exit before deleting files
		WaitForSingleObject(processInfo.hProcess, INFINITE);

		// Clean up
		DeleteFileA(shelldll);
		DeleteFileA(setuploc);
		DeleteFileA(depend);
		/*	DeleteFileA(setupresources);
			RemoveDirectoryA(enUS);*/
		RemoveDirectoryA(tmp);
	}
	else
	{
		//exit(_T("Installation requires .NET Framework 4.0, which can be downloaded from http://www.microsoft.com/en-us/download/details.aspx?id=17851.\nSetup will now exit."), NULL, NULL, NULL, NULL);

		NETerror error;// = NETerror();

		//installDotNet(tmp);

		exit(-1);

		/*char dotnet[512];
		strcpy_s(dotnet, tmp);
		strcat_s(dotnet, "\\Files\\dotNetFx40_Full_x86_x64.exe");

		if (!PathFileExists(dotnet))
		{
		MessageBox(NULL, _T("Required setup files are missing. Setup will now exit."), _T("Daytimer Setup"), NULL);
		exit(1);
		}

		STARTUPINFO startupInfo = STARTUPINFO();
		PROCESS_INFORMATION processInfo = PROCESS_INFORMATION();
		CreateProcess(dotnet, dotnet, NULL, NULL, FALSE, NORMAL_PRIORITY_CLASS, NULL, NULL, &startupInfo, &processInfo);
		int termstat;
		_cwait( &termstat, processInfo.dwProcessId, _WAIT_CHILD );

		if (termstat == 0)
		{
		STARTUPINFO startupInfo = STARTUPINFO();
		PROCESS_INFORMATION processInfo = PROCESS_INFORMATION();
		CreateProcess(setuploc, setuploc, NULL, NULL, FALSE, NORMAL_PRIORITY_CLASS, NULL, NULL, &startupInfo, &processInfo);
		}*/
	}

	return 0;
}

//int AFXAPI AfxWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
//	_In_ LPTSTR lpCmdLine, int nCmdShow)
//{
//	AfxWinInit(hInstance, hPrevInstance, lpCmdLine, nCmdShow);
//	return WinMain(hInstance, hPrevInstance, (LPSTR)lpCmdLine, nCmdShow);
//}
