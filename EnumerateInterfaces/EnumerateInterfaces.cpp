// EnumerateInterfaces.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#define MAX_TIMEOUT (10000)

bool QueryForInterface(IUnknown* pUnk, LPCTSTR lpIID)
{
	HRESULT hr;
	IID iid;
	IUnknown* pQuery;
	bool ret = false;

	if(SUCCEEDED(IIDFromString(lpIID, &iid)))
	{						
		hr = pUnk->QueryInterface(iid, (void**)&pQuery);
		if(SUCCEEDED(hr))
		{
			pQuery->Release();
			ret = true;
		}					
	}

	return ret;
}

DWORD CALLBACK TimeOutThread(LPVOID /* arg */)
{
	Sleep(MAX_TIMEOUT);

	ExitProcess(1);
}

int _tmain(int argc, _TCHAR* argv[])
{
	HKEY hKey;
	CLSID clsid;
	DWORD dwClsCtx;
	DWORD dwCoInit;
	HRESULT hr;
	IUnknown* pObj;
	int iRet = 1;

	if(argc < 4)
	{
		/* Arguments are CLSID s|m ctlctx */
		return 1;
	}

	if(FAILED(CLSIDFromString(argv[1], &clsid)))
	{
		return 1;
	}

	if(argv[2][0] == 'm')
	{
		dwCoInit = COINIT_MULTITHREADED;
	}
	else if(argv[2][0] == 's')
	{
		dwCoInit = COINIT_APARTMENTTHREADED;
	}
	else
	{
		return 1;
	}

	dwClsCtx = _tcstoul(argv[3], NULL, 0);
	
	hr = CoInitializeEx(NULL, dwCoInit);
	if(SUCCEEDED(hr))
	{
		/* Start timeout thread */
		CreateThread(NULL, 0, TimeOutThread, NULL, 0, NULL);

		hr = CoCreateInstance(clsid, NULL, dwClsCtx, IID_PPV_ARGS(&pObj));
		if(SUCCEEDED(hr))
		{
			int iExtras;

			/* Query for any extra interface IIDs */
			for(iExtras = 4; iExtras < argc; iExtras++)
			{
				if(QueryForInterface(pObj, argv[iExtras]))
				{
					_tprintf(_T("%ls\n"), argv[iExtras]);
				}
			}

			if(RegOpenKeyEx(HKEY_CLASSES_ROOT, _T("Interface"), 0, KEY_ENUMERATE_SUB_KEYS, &hKey) == ERROR_SUCCESS)
			{
				TCHAR szName[MAX_PATH];
				DWORD dwNameSize = MAX_PATH;
				DWORD dwIndex = 0;

				while(RegEnumKeyEx(hKey, dwIndex, szName, &dwNameSize, NULL, NULL, NULL, NULL) == ERROR_SUCCESS)
				{
					if(QueryForInterface(pObj, szName))
					{
						_tprintf(_T("%ls\n"), szName);
					}
					
					dwNameSize = MAX_PATH;
					dwIndex++;
				}
				iRet = 0;
			}	
			pObj->Release();
		}
		else
		{
			printf("ERROR:%08X\n", hr);
		}

		CoUninitialize();
	}
	else
	{
		printf("ERROR:%08X\n", hr);
	}

	return iRet;
}

