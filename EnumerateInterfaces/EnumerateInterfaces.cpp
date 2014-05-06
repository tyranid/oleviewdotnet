//    This file is part of OleViewDotNet.
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

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

			IInspectable* pInspect;

			hr = pObj->QueryInterface(&pInspect);
			if (SUCCEEDED(hr))
			{
				IID* pIids;
				ULONG iidCount;

				hr = pInspect->GetIids(&iidCount, &pIids);
				if (SUCCEEDED(hr))
				{
					for (ULONG i = 0; i < iidCount; ++i)
					{
						IUnknown* pUnk;
						hr = pObj->QueryInterface(pIids[i], (void**)&pUnk);
						if (SUCCEEDED(hr))
						{
							LPOLESTR iidString;
							hr = StringFromIID(pIids[i], &iidString);
							if (SUCCEEDED(hr))
							{
								_tprintf(_T("%ls\n"), iidString);
								CoTaskMemFree(iidString);
							}
							pUnk->Release();
						}
					}
				}

				CoTaskMemFree(pIids);

				pInspect->Release();
			}

			/* Query for any extra interface IIDs */
			for (iExtras = 4; iExtras < argc; iExtras++)
			{
				if (QueryForInterface(pObj, argv[iExtras]))
				{
					_tprintf(_T("%ls\n"), argv[iExtras]);
				}
			}

			if (RegOpenKeyEx(HKEY_CLASSES_ROOT, _T("Interface"), 0, KEY_ENUMERATE_SUB_KEYS, &hKey) == ERROR_SUCCESS)
			{
				TCHAR szName[MAX_PATH];
				DWORD dwNameSize = MAX_PATH;
				DWORD dwIndex = 0;

				while (RegEnumKeyEx(hKey, dwIndex, szName, &dwNameSize, NULL, NULL, NULL, NULL) == ERROR_SUCCESS)
				{
					if (QueryForInterface(pObj, szName))
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

