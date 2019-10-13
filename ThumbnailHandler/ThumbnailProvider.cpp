#include "pch.h"
#include "ThumbnailProvider.h"
#include <Shlwapi.h>
#include <Wincrypt.h>   // For CryptStringToBinary.
#include <msxml6.h>
#include <vector>

#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Crypt32.lib")
#pragma comment(lib, "msxml6.lib")

extern HINSTANCE g_hInst;
extern long g_cDllRef;

ThumbnailProvider::ThumbnailProvider()
	:
m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);
}

ThumbnailProvider::~ThumbnailProvider()
{
	InterlockedDecrement(&g_cDllRef);
}

#pragma region IUnknown

// Query to the interface the component supported.
IFACEMETHODIMP ThumbnailProvider::QueryInterface(REFIID riid, void** ppv)
{
	static const QITAB qit[] =
	{
		QITABENT(ThumbnailProvider, IThumbnailProvider),
		QITABENT(ThumbnailProvider, IInitializeWithFile),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

// Increase the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) ThumbnailProvider::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

// Decrease the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) ThumbnailProvider::Release()
{
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if (0 == cRef)
	{
		delete this;
	}

	return cRef;
}

#pragma endregion

HRESULT ThumbnailProvider::Initialize(LPCWSTR pszFilePath, DWORD grfMode)
{
	//std::wstring wpath(pszFilePath);
	//std::string spath;
	//spath.resize(wpath.size());
	//for (size_t i = 0; i < wpath.size(); ++i)
	//	spath[i] = char(wpath[i]);

	//m_model.ClearImages();
	//m_model.OpenImage(spath);
	//m_model.OpenImage("F:\\git\\ImageViewer\\FrameworkTests\\TestData\\checkers.dds");
	//auto size = m_model.GetSize(0);
	//m_width = size.x;
	//m_height = size.y;

	return S_OK;
}

#pragma region IThumbnailProvider

// Gets a thumbnail image and alpha type. The GetThumbnail is called with the 
// largest desired size of the image, in pixels. Although the parameter is 
// called cx, this is used as the maximum size of both the x and y dimensions. 
// If the retrieved thumbnail is not square, then the longer axis is limited 
// by cx and the aspect ratio of the original image respected. On exit, 
// GetThumbnail provides a handle to the retrieved image. It also provides a 
// value that indicates the color format of the image and whether it has 
// valid alpha information.
IFACEMETHODIMP ThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp,
	WTS_ALPHATYPE* pdwAlpha)
{
	//int width = 0, height = 0;
	//auto data = m_model.GenThumbnail(cx, width, height);
	
	//*phbmp = CreateBitmap(width, height, 1, 32, data.data());
	
	std::vector<uint8_t> data;
	data.resize(cx * cx * 4, 55);
	*phbmp = CreateBitmap(cx, cx, 1, 32, data.data());
	*pdwAlpha = WTSAT_ARGB;

	return S_OK;
}

#pragma endregion