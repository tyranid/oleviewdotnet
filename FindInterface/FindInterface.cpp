#define _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <stdio.h>

#pragma comment(lib, "Ole32.lib")

MULTI_QI mq[50000];
WCHAR keys[50000][256];
CLSID clsid;
DWORD index;

DWORD WINAPI FindInterface(LPVOID args) {
	DWORD retval;
	__try {
		HANDLE hFile = CreateFile(L"now.local", GENERIC_WRITE, FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == NULL) return GetLastError();
		char line[256];

		SIZE_T convertedSize;
		INT cnt = 0;
		DWORD nBytes;

		HRESULT hr = CoCreateInstanceEx(clsid, NULL, CLSCTX_LOCAL_SERVER, NULL, index, mq);
		if (hr < 0) {
			printf("CoCreateInstanceEx(LOCAL) Failed. hr: 0x%X\n", hr);
			retval = hr;
		}
		else {
			for (INT i = 0; i < index; i++) {

				if (mq[i].hr == S_OK) {
					memset(line, 0, 256);
					wcstombs_s(&convertedSize, line, keys[i], 256);
					strcat(line, "\n");
					if (!WriteFile(hFile, line, strlen(line), &nBytes, NULL)) return GetLastError();
					cnt++;
					mq[i].pItf->Release();
				}
			}
		}
		if (cnt != 0) return 0;
		else return 0x80004002;
	}
	__except (EXCEPTION_EXECUTE_HANDLER) {
		printf("Exception!");
		return GetExceptionCode();
	}
	return retval;
}

INT wmain(INT argc, WCHAR **argv) {
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	HRESULT hr;

	hr = CLSIDFromString(argv[1], &clsid);
	if (hr < 0) {
		printf("CLSIDFromString() Failed. hr: 0x%X\n", hr);
		return hr;
	}

	HKEY hKey;
	LSTATUS status;
	status = RegOpenKeyExW(HKEY_CLASSES_ROOT, L"Interface", 0, KEY_READ, &hKey);
	if (status != ERROR_SUCCESS) {
		printf("RegOpenKeyExW() Failed. LastError: %lu\n", GetLastError());
	}
	FILETIME lastWriteTime;
	WCHAR keyName[256];
	DWORD keyNameSize;
	while (true) {
		keyNameSize = sizeof(keyName) / sizeof(keyName[0]);
		status = RegEnumKeyExW(hKey, index, keyName, &keyNameSize, NULL, NULL, NULL, &lastWriteTime);
		lstrcpy(keys[index], keyName);
		if (status == ERROR_NO_MORE_ITEMS) break;

		IID* iid = (IID*)malloc(sizeof(IID));
		IIDFromString(keyName, iid);

		mq[index].pIID = iid;
		index++;
	}

	DWORD tid;
	HANDLE hThread = CreateThread(NULL, 0, FindInterface, NULL, 0, &tid);

	DWORD waitResult = WaitForSingleObject(hThread, 10000);


	DWORD retval;
	if (waitResult == WAIT_OBJECT_0) {
		GetExitCodeThread(hThread, &retval);
	}
	else if (waitResult == WAIT_TIMEOUT) {
		retval = 0xFFFFFFFF;
	}
	else retval = 0xEEEEEEEE;

	CoUninitialize();
	return retval;
}